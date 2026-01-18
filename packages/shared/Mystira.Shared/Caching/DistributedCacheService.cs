using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mystira.Shared.Caching;

/// <summary>
/// Implementation of ICacheService using IDistributedCache.
/// Works with both Redis and in-memory cache providers.
/// Includes observability metrics for cache hit/miss rates.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly CacheMetrics _metrics;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache implementation.</param>
    /// <param name="options">Cache configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public DistributedCacheService(
        IDistributedCache cache,
        IOptions<CacheOptions> options,
        ILogger<DistributedCacheService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
        _metrics = CacheMetrics.Instance;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return default;
        }

        var keyPattern = CacheMetrics.ExtractKeyPattern(key);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);
            stopwatch.Stop();
            _metrics.RecordLatency(stopwatch.Elapsed.TotalMilliseconds, "get", keyPattern);

            if (data == null)
            {
                _metrics.RecordMiss(keyPattern);
                return default;
            }

            _metrics.RecordHit(keyPattern);
            return JsonSerializer.Deserialize<T>(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordError("get", keyPattern);
            _logger.LogWarning(ex, "Failed to get cache key {Key}", key);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task<(bool Found, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return (false, default);
        }

        var keyPattern = CacheMetrics.ExtractKeyPattern(key);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);
            stopwatch.Stop();
            _metrics.RecordLatency(stopwatch.Elapsed.TotalMilliseconds, "get", keyPattern);

            if (data == null)
            {
                _metrics.RecordMiss(keyPattern);
                return (false, default);
            }

            _metrics.RecordHit(keyPattern);
            var value = JsonSerializer.Deserialize<T>(data, _jsonOptions);
            return (true, value);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordError("get", keyPattern);
            _logger.LogWarning(ex, "Failed to get cache key {Key}", key);
            return (false, default);
        }
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return SetAsync(key, value, TimeSpan.FromMinutes(_options.DefaultExpirationMinutes), cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        var keyPattern = CacheMetrics.ExtractKeyPattern(key);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var data = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions();

            if (_options.UseSlidingExpiration)
            {
                options.SlidingExpiration = expiration;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            await _cache.SetStringAsync(key, data, options, cancellationToken);
            stopwatch.Stop();
            _metrics.RecordSet(keyPattern);
            _metrics.RecordLatency(stopwatch.Elapsed.TotalMilliseconds, "set", keyPattern);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordError("set", keyPattern);
            _logger.LogWarning(ex, "Failed to set cache key {Key}", key);
        }
    }

    /// <inheritdoc />
    public Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        return GetOrCreateAsync(key, factory,
            TimeSpan.FromMinutes(_options.DefaultExpirationMinutes), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        // Use TryGetAsync to correctly handle value types
        var (found, cached) = await TryGetAsync<T>(key, cancellationToken);
        if (found)
        {
            _logger.LogDebug("Cache hit for key {Key}", key);
            return cached!;
        }

        _logger.LogDebug("Cache miss for key {Key}, executing factory", key);
        var value = await factory(cancellationToken);

        // Cache even null/default values for reference types to avoid repeated factory calls
        // For value types, always cache the result
        if (value is not null || !typeof(T).IsValueType)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var keyPattern = CacheMetrics.ExtractKeyPattern(key);

        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _metrics.RecordRemove(keyPattern);
        }
        catch (Exception ex)
        {
            _metrics.RecordError("remove", keyPattern);
            _logger.LogWarning(ex, "Failed to remove cache key {Key}", key);
        }
    }

    /// <inheritdoc />
    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Pattern-based removal requires Redis SCAN command
        // This is a no-op for in-memory cache
        _logger.LogWarning(
            "RemoveByPatternAsync is not supported with standard IDistributedCache. " +
            "Use Redis-specific implementation for pattern-based removal.");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return false;
        }

        try
        {
            var data = await _cache.GetAsync(key, cancellationToken);
            return data != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check cache key existence {Key}", key);
            return false;
        }
    }
}
