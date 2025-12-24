using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Mystira.Shared.Locking;

/// <summary>
/// Redis-based distributed lock implementation using the SET NX command
/// with automatic expiry for safety.
/// </summary>
/// <remarks>
/// This implementation uses the RedLock algorithm principles:
/// - SET with NX (only if not exists) and PX (expiry in milliseconds)
/// - Unique lock value for safe release (only release if we own the lock)
/// - Lua scripts for atomic operations
/// </remarks>
public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly DistributedLockOptions _options;
    private readonly ILogger<RedisDistributedLockService> _logger;

    // Lua script for atomic release (only release if we own the lock)
    private const string ReleaseLockScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end
    ";

    // Lua script for atomic extend (only extend if we own the lock)
    private const string ExtendLockScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('pexpire', KEYS[1], ARGV[2])
        else
            return 0
        end
    ";

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisDistributedLockService"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <param name="options">Distributed lock configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public RedisDistributedLockService(
        IConnectionMultiplexer redis,
        IOptions<DistributedLockOptions> options,
        ILogger<RedisDistributedLockService> logger)
    {
        _redis = redis;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IDistributedLockHandle?> TryAcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var lockId = Guid.NewGuid().ToString("N");
        var key = GetLockKey(resource);
        var db = _redis.GetDatabase();

        var acquired = await db.StringSetAsync(
            key,
            lockId,
            expiry,
            When.NotExists);

        if (acquired)
        {
            if (_options.EnableDetailedLogging)
            {
                _logger.LogDebug(
                    "Lock acquired on {Resource} with ID {LockId}, expires in {Expiry}",
                    resource, lockId, expiry);
            }

            return new RedisDistributedLockHandle(
                lockId,
                resource,
                key,
                expiry,
                db,
                _logger,
                _options.EnableDetailedLogging);
        }

        if (_options.EnableDetailedLogging)
        {
            _logger.LogDebug("Failed to acquire lock on {Resource}", resource);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<IDistributedLockHandle> AcquireAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan wait,
        TimeSpan? retry = null,
        CancellationToken cancellationToken = default)
    {
        var retryInterval = retry ?? TimeSpan.FromMilliseconds(_options.RetryIntervalMs);
        var deadline = DateTimeOffset.UtcNow.Add(wait);

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var handle = await TryAcquireAsync(resource, expiry, cancellationToken);
            if (handle != null)
            {
                return handle;
            }

            await Task.Delay(retryInterval, cancellationToken);
        }

        _logger.LogWarning(
            "Failed to acquire lock on {Resource} after waiting {Wait}",
            resource, wait);

        throw new DistributedLockException(
            resource,
            $"Could not acquire lock on resource '{resource}' within {wait.TotalSeconds} seconds");
    }

    /// <inheritdoc />
    public async Task<bool> IsLockedAsync(string resource, CancellationToken cancellationToken = default)
    {
        var key = GetLockKey(resource);
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(key);
    }

    /// <inheritdoc />
    public async Task<T> ExecuteWithLockAsync<T>(
        string resource,
        Func<CancellationToken, Task<T>> action,
        TimeSpan expiry,
        TimeSpan wait,
        CancellationToken cancellationToken = default)
    {
        await using var lockHandle = await AcquireAsync(resource, expiry, wait, cancellationToken: cancellationToken);

        try
        {
            return await action(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action with lock on {Resource}", resource);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task ExecuteWithLockAsync(
        string resource,
        Func<CancellationToken, Task> action,
        TimeSpan expiry,
        TimeSpan wait,
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithLockAsync(
            resource,
            async ct =>
            {
                await action(ct);
                return true;
            },
            expiry,
            wait,
            cancellationToken);
    }

    private string GetLockKey(string resource)
    {
        return $"{_options.KeyPrefix}{resource}";
    }

    /// <summary>
    /// Internal lock handle implementation for Redis locks.
    /// </summary>
    private sealed class RedisDistributedLockHandle : IDistributedLockHandle
    {
        private readonly IDatabase _db;
        private readonly ILogger _logger;
        private readonly bool _enableLogging;
        private readonly string _key;
        private bool _isReleased;

        public string LockId { get; }
        public string Resource { get; }
        public bool IsAcquired => !_isReleased;
        public DateTimeOffset? AcquiredAt { get; }
        public DateTimeOffset? ExpiresAt { get; private set; }

        public RedisDistributedLockHandle(
            string lockId,
            string resource,
            string key,
            TimeSpan expiry,
            IDatabase db,
            ILogger logger,
            bool enableLogging)
        {
            LockId = lockId;
            Resource = resource;
            _key = key;
            _db = db;
            _logger = logger;
            _enableLogging = enableLogging;
            AcquiredAt = DateTimeOffset.UtcNow;
            ExpiresAt = AcquiredAt.Value.Add(expiry);
        }

        public async Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default)
        {
            if (_isReleased)
            {
                return false;
            }

            var result = await _db.ScriptEvaluateAsync(
                ExtendLockScript,
                new RedisKey[] { _key },
                new RedisValue[] { LockId, (long)extension.TotalMilliseconds });

            var extended = (long)result! == 1;

            if (extended)
            {
                ExpiresAt = DateTimeOffset.UtcNow.Add(extension);

                if (_enableLogging)
                {
                    _logger.LogDebug(
                        "Lock {LockId} on {Resource} extended until {ExpiresAt}",
                        LockId, Resource, ExpiresAt);
                }
            }

            return extended;
        }

        public async Task ReleaseAsync(CancellationToken cancellationToken = default)
        {
            if (_isReleased)
            {
                return;
            }

            var result = await _db.ScriptEvaluateAsync(
                ReleaseLockScript,
                new RedisKey[] { _key },
                new RedisValue[] { LockId });

            _isReleased = true;

            if (_enableLogging)
            {
                var released = (long)result! == 1;
                if (released)
                {
                    _logger.LogDebug("Lock {LockId} on {Resource} released", LockId, Resource);
                }
                else
                {
                    _logger.LogWarning(
                        "Lock {LockId} on {Resource} was not released (already expired or owned by another)",
                        LockId, Resource);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_isReleased)
            {
                await ReleaseAsync();
            }
        }
    }
}
