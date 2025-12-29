using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Caching;

/// <summary>
/// Wolverine middleware that caches query results for queries implementing ICacheableQuery.
/// Uses in-memory caching with configurable expiration per query.
/// This is invoked via Wolverine's handler chain middleware.
/// </summary>
/// <remarks>
/// To use this middleware, register it in your Wolverine configuration:
/// <code>
/// opts.Policies.Add&lt;QueryCachingMiddlewarePolicy&gt;();
/// </code>
/// </remarks>
public class QueryCachingMiddleware
{
    private readonly IMemoryCache _cache;
    private readonly IQueryCacheInvalidationService _cacheInvalidation;
    private readonly ILogger<QueryCachingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCachingMiddleware"/> class.
    /// </summary>
    /// <param name="cache">The memory cache instance.</param>
    /// <param name="cacheInvalidation">The cache invalidation service.</param>
    /// <param name="logger">Logger instance.</param>
    public QueryCachingMiddleware(
        IMemoryCache cache,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger<QueryCachingMiddleware> logger)
    {
        _cache = cache;
        _cacheInvalidation = cacheInvalidation;
        _logger = logger;
    }

    /// <summary>
    /// Wolverine middleware method that wraps handler execution with caching logic.
    /// Called Before in the handler chain for cacheable queries.
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="query">The cacheable query</param>
    /// <returns>Cached response if found, otherwise default</returns>
    public Task<T?> TryGetFromCache<T>(ICacheableQuery query)
    {
        var cacheKey = query.CacheKey;

        if (_cache.TryGetValue(cacheKey, out T? cachedResponse) && cachedResponse != null)
        {
            _logger.LogDebug("Cache hit for query with key {CacheKey}", cacheKey);
            return Task.FromResult<T?>(cachedResponse);
        }

        _logger.LogDebug("Cache miss for query with key {CacheKey}", cacheKey);
        return Task.FromResult<T?>(default);
    }

    /// <summary>
    /// Caches the response after handler execution.
    /// Called After in the handler chain for cacheable queries.
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="query">The cacheable query</param>
    /// <param name="response">The response to cache</param>
    public void CacheResponse<T>(ICacheableQuery query, T response)
    {
        var cacheKey = query.CacheKey;

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(query.CacheDurationSeconds),
            Size = 1 // For size-limited caches
        };

        _cache.Set(cacheKey, response, cacheOptions);

        // Track cache key for prefix-based invalidation
        _cacheInvalidation.TrackCacheKey(cacheKey);

        _logger.LogDebug("Cached query with key {CacheKey} for {Duration} seconds",
            cacheKey, query.CacheDurationSeconds);
    }
}

/// <summary>
/// Static helper methods for query caching that can be used directly in handlers.
/// Alternative to middleware when direct control is needed.
/// </summary>
public static class QueryCacheHelper
{
    /// <summary>
    /// Executes a query with caching support.
    /// Use this in handlers for cacheable queries.
    /// </summary>
    /// <typeparam name="T">The response type</typeparam>
    /// <param name="query">The cacheable query</param>
    /// <param name="cache">The memory cache</param>
    /// <param name="cacheInvalidation">The cache invalidation service</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="executeQuery">Function to execute the query if not cached</param>
    /// <returns>Cached or freshly executed result</returns>
    public static async Task<T> ExecuteWithCache<T>(
        ICacheableQuery query,
        IMemoryCache cache,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        Func<Task<T>> executeQuery)
    {
        var cacheKey = query.CacheKey;

        // Try to get from cache
        if (cache.TryGetValue(cacheKey, out T? cachedResponse) && cachedResponse != null)
        {
            logger.LogDebug("Cache hit for query with key {CacheKey}", cacheKey);
            return cachedResponse;
        }

        logger.LogDebug("Cache miss for query with key {CacheKey}", cacheKey);

        // Execute query
        var response = await executeQuery();

        // Cache the response
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(query.CacheDurationSeconds),
            Size = 1 // For size-limited caches
        };

        cache.Set(cacheKey, response, cacheOptions);

        // Track cache key for prefix-based invalidation
        cacheInvalidation.TrackCacheKey(cacheKey);

        logger.LogDebug("Cached query with key {CacheKey} for {Duration} seconds",
            cacheKey, query.CacheDurationSeconds);

        return response;
    }
}
