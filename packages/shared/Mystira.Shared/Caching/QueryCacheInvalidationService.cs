using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Caching;

/// <summary>
/// Implementation of IQueryCacheInvalidationService using in-memory cache.
/// Tracks cache keys to enable prefix-based invalidation.
/// </summary>
public class QueryCacheInvalidationService : IQueryCacheInvalidationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<QueryCacheInvalidationService> _logger;
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCacheInvalidationService"/> class.
    /// </summary>
    /// <param name="cache">The memory cache instance.</param>
    /// <param name="logger">Logger instance.</param>
    public QueryCacheInvalidationService(
        IMemoryCache cache,
        ILogger<QueryCacheInvalidationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public void InvalidateCache(string cacheKey)
    {
        _cache.Remove(cacheKey);

        lock (_lock)
        {
            _cacheKeys.Remove(cacheKey);
        }

        _logger.LogDebug("Invalidated cache entry: {CacheKey}", cacheKey);
    }

    /// <inheritdoc />
    public void InvalidateCacheByPrefix(string prefix)
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
            InvalidateCache(key);
        }

        _logger.LogDebug("Invalidated {Count} cache entries with prefix: {Prefix}",
            keysToRemove.Count, prefix);
    }

    /// <inheritdoc />
    public void TrackCacheKey(string cacheKey)
    {
        lock (_lock)
        {
            _cacheKeys.Add(cacheKey);
        }
    }

    /// <inheritdoc />
    public void ClearTrackedKeys()
    {
        lock (_lock)
        {
            _cacheKeys.Clear();
        }
    }
}
