using Mystira.Application.Interfaces;
using Mystira.Contracts.App.Responses.Attribution;

namespace Mystira.Application.CQRS.Attribution.Queries;

/// <summary>
/// Query to retrieve IP registration status for a content bundle.
/// Cached for 10 minutes as IP status changes infrequently.
/// </summary>
/// <param name="BundleId">The unique identifier of the content bundle.</param>
public record GetBundleIpStatusQuery(string BundleId) : IQuery<IpVerificationResponse?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"BundleIpStatus:{BundleId}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 600; // 10 minutes
}
