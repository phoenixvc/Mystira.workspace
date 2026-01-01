using Mystira.Application.Interfaces;
using Mystira.Contracts.App.Responses.Attribution;

namespace Mystira.Application.CQRS.Attribution.Queries;

/// <summary>
/// Query to retrieve attribution/creator credits for a content bundle.
/// Cached for 10 minutes as attribution data changes infrequently.
/// </summary>
/// <param name="BundleId">The unique identifier of the content bundle.</param>
public record GetBundleAttributionQuery(string BundleId) : IQuery<ContentAttributionResponse?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"BundleAttribution:{BundleId}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 600; // 10 minutes
}
