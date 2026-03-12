using Mystira.Core.Interfaces;
using Mystira.Contracts.App.Responses.Attribution;

namespace Mystira.Core.CQRS.Attribution.Queries;

/// <summary>
/// Query to retrieve attribution/creator credits for a content bundle.
/// Cached for 10 minutes as attribution data changes infrequently.
/// </summary>
public record GetBundleAttributionQuery(string BundleId) : IQuery<ContentAttributionResponse?>, ICacheableQuery
{
    public string CacheKey => $"BundleAttribution:{BundleId}";
    public int CacheDurationSeconds => CacheDefaults.MediumSeconds;
}
