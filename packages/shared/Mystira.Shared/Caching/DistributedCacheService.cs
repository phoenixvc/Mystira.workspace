using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mystira.Shared.Caching;

/// <summary>
/// Implementation of ICacheService using IDistributedCache.
/// Works with both Redis and in-memory cache providers.
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public DistributedCacheService(
        IDistributedCache cache,
        IOptions<CacheOptions> options,
        ILogger<DistributedCacheService> logger)
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

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return default;
        }

        try
        {
            var data = await _cache.GetStringAsync(key, cancellationToken);
            if (data == null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(data, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cache key {Key}", key);
            return default;
        }
    }

    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return SetAsync(key, value, TimeSpan.FromMinutes(_options.DefaultExpirationMinutes), cancellationToken);
    }

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
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cache key {Key}", key);
        }
    }

    public Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken cancellationToken = default)
    {
        return GetOrCreateAsync(key, factory,
            TimeSpan.FromMinutes(_options.DefaultExpirationMinutes), cancellationToken);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for key {Key}", key);
            return cached;
        }

        _logger.LogDebug("Cache miss for key {Key}, executing factory", key);
        var value = await factory(cancellationToken);

        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache key {Key}", key);
        }
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Pattern-based removal requires Redis SCAN command
        // This is a no-op for in-memory cache
        _logger.LogWarning(
            "RemoveByPatternAsync is not supported with standard IDistributedCache. " +
            "Use Redis-specific implementation for pattern-based removal.");
        return Task.CompletedTask;
    }

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
