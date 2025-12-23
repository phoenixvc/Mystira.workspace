using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Mystira.App.Admin.Api.Configuration;

namespace Mystira.App.Admin.Api.Services.Caching;

/// <summary>
/// Redis-backed distributed cache service.
/// Falls back gracefully when Redis is unavailable.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisCacheOptions _options;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache cache,
        IOptions<RedisCacheOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
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

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Pattern-based removal requires Redis SCAN command
        // IDistributedCache doesn't support this natively
        // For now, log a warning - full implementation requires StackExchange.Redis directly
        _logger.LogWarning(
            "Pattern-based cache invalidation requested for pattern: {Pattern}. " +
            "This requires direct Redis access. Consider invalidating specific keys.",
            pattern);

        // Invalidate known list keys that match common patterns
        if (pattern.Contains("scenario"))
        {
            await RemoveAsync(CacheKeys.ScenariosList, cancellationToken);
        }
        if (pattern.Contains("character"))
        {
            await RemoveAsync(CacheKeys.CharacterMapsList, cancellationToken);
        }
        if (pattern.Contains("bundle"))
        {
            await RemoveAsync(CacheKeys.BundlesList, cancellationToken);
        }
        if (pattern.Contains("badge"))
        {
            await RemoveAsync(CacheKeys.BadgesList, cancellationToken);
        }
        if (pattern.Contains("master"))
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
        if (key.Contains(":master:"))
        {
            return TimeSpan.FromMinutes(_options.MasterDataCacheMinutes);
        }
        if (key.Contains(":account:") || key.Contains(":profile:"))
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
