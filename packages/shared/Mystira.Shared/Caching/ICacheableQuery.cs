namespace Mystira.Shared.Caching;

/// <summary>
/// Marker interface for queries that should be cached.
/// Implement this interface on queries where caching is appropriate.
/// </summary>
/// <remarks>
/// Queries implementing this interface will be automatically cached
/// by the QueryCachingMiddleware when used with Wolverine handlers.
/// The cache key should be unique based on the query parameters to
/// ensure correct cache isolation.
/// </remarks>
/// <example>
/// <code>
/// public record GetUserByIdQuery(Guid UserId) : IQuery&lt;User?&gt;, ICacheableQuery
/// {
///     public string CacheKey => $"User:{UserId}";
///     public int CacheDurationSeconds => 300; // 5 minutes
/// }
/// </code>
/// </example>
public interface ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for this query.
    /// Should be unique based on query parameters.
    /// </summary>
    /// <remarks>
    /// Best practices for cache keys:
    /// - Use a consistent prefix (e.g., entity type)
    /// - Include all parameters that affect the result
    /// - Keep keys short but descriptive
    /// - Example: "Scenario:123" or "Scenarios:AgeGroup:5:Page:1:Size:20"
    /// </remarks>
    string CacheKey { get; }

    /// <summary>
    /// Gets the cache duration in seconds.
    /// Default is 300 seconds (5 minutes).
    /// </summary>
    /// <remarks>
    /// Consider the data volatility when setting this value:
    /// - Static reference data: 3600+ seconds
    /// - Frequently changing data: 60-300 seconds
    /// - User-specific data: 60-120 seconds
    /// </remarks>
    int CacheDurationSeconds => 300;
}
