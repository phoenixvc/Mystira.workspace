using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Mystira.App.Application.Interfaces;
using Mystira.App.Application.Services;
using Wolverine;

namespace Mystira.App.Application.Behaviors;

/// <summary>
/// Wolverine middleware that caches query results for queries implementing ICacheableQuery.
/// Uses distributed caching (Redis) with configurable expiration per query.
/// This is invoked via Wolverine's handler chain middleware.
/// </summary>
public class QueryCachingMiddleware
{
    private readonly IDistributedCache _cache;
    private readonly IQueryCacheInvalidationService _cacheInvalidation;
    private readonly ILogger<QueryCachingMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public QueryCachingMiddleware(
        IDistributedCache cache,
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
    public async Task<T?> TryGetFromCache<T>(ICacheableQuery query)
    {
        var cacheKey = query.CacheKey;

        try
        {
            var cachedJson = await _cache.GetStringAsync(cacheKey);
            if (cachedJson != null)
            {
                var cachedResponse = JsonSerializer.Deserialize<T>(cachedJson, _jsonOptions);
                if (cachedResponse != null)
                {
                    _logger.LogDebug("Cache hit for query with key {CacheKey}", cacheKey);
                    return cachedResponse;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read from distributed cache for key {CacheKey}", cacheKey);
        }

        _logger.LogDebug("Cache miss for query with key {CacheKey}", cacheKey);
        return default;
    }

    /// <summary>
    /// Caches the response after handler execution.
    /// Called After in the handler chain for cacheable queries.
    /// </summary>
    public async Task CacheResponse<T>(ICacheableQuery query, T response)
    {
        var cacheKey = query.CacheKey;

        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(query.CacheDurationSeconds)
            };

            var serialized = JsonSerializer.Serialize(response, _jsonOptions);
            await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            // Track cache key for prefix-based invalidation
            _cacheInvalidation.TrackCacheKey(cacheKey);

            _logger.LogDebug("Cached query with key {CacheKey} for {Duration} seconds",
                cacheKey, query.CacheDurationSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write to distributed cache for key {CacheKey}", cacheKey);
        }
    }
}

/// <summary>
/// Static helper methods for query caching that can be used directly in handlers.
/// Alternative to middleware when direct control is needed.
/// </summary>
public static class QueryCacheHelper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Executes a query with caching support.
    /// Use this in handlers for cacheable queries.
    /// </summary>
    public static async Task<T> ExecuteWithCache<T>(
        ICacheableQuery query,
        IDistributedCache cache,
        IQueryCacheInvalidationService cacheInvalidation,
        ILogger logger,
        Func<Task<T>> executeQuery)
    {
        var cacheKey = query.CacheKey;

        // Try to get from cache
        try
        {
            var cachedJson = await cache.GetStringAsync(cacheKey);
            if (cachedJson != null)
            {
                var cachedResponse = JsonSerializer.Deserialize<T>(cachedJson, _jsonOptions);
                if (cachedResponse != null)
                {
                    logger.LogDebug("Cache hit for query with key {CacheKey}", cacheKey);
                    return cachedResponse;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read from distributed cache for key {CacheKey}", cacheKey);
        }

        logger.LogDebug("Cache miss for query with key {CacheKey}", cacheKey);

        // Execute query
        var response = await executeQuery();

        // Cache the response
        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(query.CacheDurationSeconds)
            };

            var serialized = JsonSerializer.Serialize(response, _jsonOptions);
            await cache.SetStringAsync(cacheKey, serialized, cacheOptions);

            // Track cache key for prefix-based invalidation
            cacheInvalidation.TrackCacheKey(cacheKey);

            logger.LogDebug("Cached query with key {CacheKey} for {Duration} seconds",
                cacheKey, query.CacheDurationSeconds);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write to distributed cache for key {CacheKey}", cacheKey);
        }

        return response;
    }
}
