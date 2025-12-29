namespace Mystira.Shared.Caching;

/// <summary>
/// Service for invalidating query caches.
/// Use this after commands that modify data to ensure cache consistency.
/// </summary>
/// <remarks>
/// This service tracks cache keys to enable prefix-based invalidation.
/// When data is modified, call InvalidateCacheByPrefix with the entity type
/// to clear all related cached queries.
/// </remarks>
/// <example>
/// <code>
/// // In a command handler after updating a scenario:
/// _cacheInvalidation.InvalidateCacheByPrefix("Scenario");
///
/// // Or invalidate a specific cache entry:
/// _cacheInvalidation.InvalidateCache($"Scenario:{scenarioId}");
/// </code>
/// </example>
public interface IQueryCacheInvalidationService
{
    /// <summary>
    /// Removes a specific cache entry by key.
    /// </summary>
    /// <param name="cacheKey">The exact cache key to remove</param>
    void InvalidateCache(string cacheKey);

    /// <summary>
    /// Removes all cache entries matching a prefix.
    /// </summary>
    /// <param name="prefix">The prefix to match (e.g., "Scenario" removes all scenario-related caches)</param>
    /// <example>
    /// InvalidateCacheByPrefix("Scenario") removes keys like:
    /// - "Scenario:123"
    /// - "Scenarios:Page:1"
    /// - "Scenario:Featured"
    /// </example>
    void InvalidateCacheByPrefix(string prefix);

    /// <summary>
    /// Tracks a cache key for prefix-based invalidation.
    /// Called automatically by QueryCachingMiddleware when caching items.
    /// </summary>
    /// <param name="cacheKey">The cache key to track</param>
    void TrackCacheKey(string cacheKey);

    /// <summary>
    /// Clears all tracked cache keys.
    /// Primarily used for testing purposes.
    /// </summary>
    void ClearTrackedKeys();
}
