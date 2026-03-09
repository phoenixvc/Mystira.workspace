using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Mystira.App.Application.Services;

/// <summary>
/// Service for invalidating query caches.
/// Use this after commands that modify data to ensure cache consistency.
/// </summary>
public interface IQueryCacheInvalidationService
{
    /// <summary>
    /// Removes a specific cache entry by key.
    /// </summary>
    Task InvalidateCacheAsync(string cacheKey);

    /// <summary>
    /// Removes all cache entries matching a prefix.
    /// Example: InvalidateCacheByPrefixAsync("Scenario") removes all scenario-related caches.
    /// </summary>
    Task InvalidateCacheByPrefixAsync(string prefix);

    /// <summary>
    /// Tracks a cache key for prefix-based invalidation.
    /// </summary>
    void TrackCacheKey(string cacheKey);

    /// <summary>
    /// Clears all tracked cache keys.
    /// </summary>
    Task ClearTrackedKeysAsync();

    // Synchronous overloads for backward compatibility
    void InvalidateCache(string cacheKey);
    void InvalidateCacheByPrefix(string prefix);
    void ClearTrackedKeys();
}

public class QueryCacheInvalidationService : IQueryCacheInvalidationService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<QueryCacheInvalidationService> _logger;
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lock = new();

    public QueryCacheInvalidationService(
        IDistributedCache cache,
        ILogger<QueryCacheInvalidationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task InvalidateCacheAsync(string cacheKey)
    {
        try
        {
            await _cache.RemoveAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache entry: {CacheKey}", cacheKey);
        }

        lock (_lock)
        {
            _cacheKeys.Remove(cacheKey);
        }

        _logger.LogDebug("Invalidated cache entry: {CacheKey}", cacheKey);
    }

    public async Task InvalidateCacheByPrefixAsync(string prefix)
    {
        HashSet<string> keysToRemove;

        lock (_lock)
        {
            keysToRemove = _cacheKeys
                .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToHashSet();
        }

        foreach (var key in keysToRemove)
        {
            await InvalidateCacheAsync(key);
        }

        _logger.LogDebug("Invalidated {Count} cache entries with prefix: {Prefix}",
            keysToRemove.Count, prefix);
    }

    public void TrackCacheKey(string cacheKey)
    {
        lock (_lock)
        {
            _cacheKeys.Add(cacheKey);
        }
    }

    public async Task ClearTrackedKeysAsync()
    {
        HashSet<string> allKeys;
        lock (_lock)
        {
            allKeys = new HashSet<string>(_cacheKeys);
            _cacheKeys.Clear();
        }

        foreach (var key in allKeys)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove cache entry during clear: {CacheKey}", key);
            }
        }
    }

    // Synchronous overloads for backward compatibility
    public void InvalidateCache(string cacheKey)
    {
        InvalidateCacheAsync(cacheKey).GetAwaiter().GetResult();
    }

    public void InvalidateCacheByPrefix(string prefix)
    {
        InvalidateCacheByPrefixAsync(prefix).GetAwaiter().GetResult();
    }

    public void ClearTrackedKeys()
    {
        ClearTrackedKeysAsync().GetAwaiter().GetResult();
    }
}
