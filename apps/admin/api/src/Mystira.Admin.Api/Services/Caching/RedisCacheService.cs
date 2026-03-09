using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Mystira.Admin.Api.Configuration;
using StackExchange.Redis;

namespace Mystira.Admin.Api.Services.Caching;

/// <summary>
/// Redis-backed distributed cache service.
/// Falls back gracefully when Redis is unavailable.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly RedisCacheOptions _options;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer? redis,
        IOptions<RedisCacheOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = redis;
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            var result = JsonSerializer.Deserialize<T>(data, _jsonOptions);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache get failed for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var data = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? GetDefaultExpiration(key)
            };

            await _cache.SetStringAsync(key, data, options, cancellationToken);
            _logger.LogDebug("Cache set for key: {Key}, expiration: {Expiration}", key, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache set failed for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache remove failed for key: {Key}", key);
        }
    }

    /// <summary>
    /// Removes keys matching the provided pattern from Redis.
    /// </summary>
    /// <param name="pattern">The pattern to match keys against (e.g., "scenario", "character").</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A completed task.</returns>
    /// <remarks>
    /// Uses Redis SCAN for efficient key iteration in large datasets.
    /// Falls back to known key invalidation if Redis is unavailable.
    /// </remarks>
    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogWarning(
                "Pattern-based cache invalidation requested for pattern: {Pattern}. " +
                "Redis connection not available. Falling back to known key invalidation.",
                pattern);

            // Fallback to known key invalidation when Redis is not available
            await InvalidateKnownPatternKeysAsync(pattern, cancellationToken);
            return;
        }

        try
        {
            var database = _redis.GetDatabase();
            var servers = _redis.GetServers().ToList();

            if (servers.Count == 0)
            {
                _logger.LogWarning("No Redis servers available for SCAN operation");
                await InvalidateKnownPatternKeysAsync(pattern, cancellationToken);
                return;
            }

            // Convert simple wildcard pattern to Redis pattern
            // Cache keys typically use ":" as separator, so we need to match accordingly
            var redisPattern = $"*{pattern}*";

            var keysToRemove = new List<RedisKey>();
            var totalKeys = 0;

            // Iterate through all servers to ensure we catch keys in clustered setups
            foreach (var server in servers)
            {
                await foreach (var key in server.KeysAsync(database: database.Database, pattern: redisPattern))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    keysToRemove.Add(key);
                    totalKeys++;
                }
            }

            _logger.LogDebug("SCAN completed, total keys found: {Count}", totalKeys);

            if (keysToRemove.Count == 0)
            {
                _logger.LogInformation(
                    "No cache keys found matching pattern: {Pattern}",
                    pattern);
                return;
            }

            // Remove all matching keys in batches
            const int batchSize = 100;
            var removedCount = 0;

            for (var i = 0; i < keysToRemove.Count; i += batchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = keysToRemove.Skip(i).Take(batchSize).ToArray();
                await database.KeyDeleteAsync(batch);
                removedCount += batch.Length;

                _logger.LogDebug(
                    "Removed batch of {BatchSize} keys for pattern: {Pattern}",
                    batch.Length,
                    pattern);
            }

            _logger.LogInformation(
                "Pattern-based cache invalidation complete: {Pattern}. Removed {Count} keys.",
                pattern,
                removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error during pattern-based cache invalidation for pattern: {Pattern}. " +
                "Falling back to known key invalidation.",
                pattern);

            // Fallback to known key invalidation on error
            await InvalidateKnownPatternKeysAsync(pattern, cancellationToken);
        }
    }

    /// <summary>
    /// Fallback method that invalidates known list keys based on common patterns.
    /// Used when Redis SCAN is not available or fails.
    /// </summary>
    private async Task InvalidateKnownPatternKeysAsync(string pattern, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invalidating known keys for pattern: {Pattern}", pattern);

        // Invalidate known list keys that match common patterns
        if (pattern.Contains("scenario", StringComparison.OrdinalIgnoreCase))
        {
            await RemoveAsync(CacheKeys.ScenariosList, cancellationToken);
        }
        if (pattern.Contains("character", StringComparison.OrdinalIgnoreCase))
        {
            await RemoveAsync(CacheKeys.CharacterMapsList, cancellationToken);
        }
        if (pattern.Contains("bundle", StringComparison.OrdinalIgnoreCase))
        {
            await RemoveAsync(CacheKeys.BundlesList, cancellationToken);
        }
        if (pattern.Contains("badge", StringComparison.OrdinalIgnoreCase))
        {
            await RemoveAsync(CacheKeys.BadgesList, cancellationToken);
        }
        if (pattern.Contains("master", StringComparison.OrdinalIgnoreCase))
        {
            await RemoveAsync(CacheKeys.CompassAxes, cancellationToken);
            await RemoveAsync(CacheKeys.Archetypes, cancellationToken);
            await RemoveAsync(CacheKeys.EchoTypes, cancellationToken);
            await RemoveAsync(CacheKeys.FantasyThemes, cancellationToken);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        // Try to get from cache first
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        // Not in cache, call factory
        var value = await factory(cancellationToken);
        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    private TimeSpan GetDefaultExpiration(string key)
    {
        // Use different expirations based on key type
        if (key.Contains(":master:", StringComparison.OrdinalIgnoreCase))
        {
            return TimeSpan.FromMinutes(_options.MasterDataCacheMinutes);
        }
        if (key.Contains(":account:", StringComparison.OrdinalIgnoreCase) ||
            key.Contains(":profile:", StringComparison.OrdinalIgnoreCase))
        {
            return TimeSpan.FromMinutes(_options.UserCacheMinutes);
        }
        // Default to content cache duration
        return TimeSpan.FromMinutes(_options.ContentCacheMinutes);
    }
}

/// <summary>
/// In-memory cache service for development/testing.
/// Used when Redis is not configured.
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public InMemoryCacheService(
        IDistributedCache cache,
        ILogger<InMemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var data = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(data))
        {
            return null;
        }
        return JsonSerializer.Deserialize<T>(data, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var data = JsonSerializer.Serialize(value, _jsonOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
        };
        await _cache.SetStringAsync(key, data, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Pattern-based removal not supported in in-memory cache: {Pattern}", pattern);
        return Task.CompletedTask;
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        if (value is not null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }
}
