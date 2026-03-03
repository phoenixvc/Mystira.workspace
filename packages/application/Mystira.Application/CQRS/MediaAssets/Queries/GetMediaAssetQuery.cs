using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.MediaAssets.Queries;

/// <summary>
/// Query to retrieve media asset metadata by ID.
/// Cached for 5 minutes as media metadata is relatively static.
/// </summary>
/// <param name="MediaId">The unique identifier of the media asset.</param>
public record GetMediaAssetQuery(string MediaId) : IQuery<MediaAsset?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"MediaAsset:{MediaId}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 300; // 5 minutes
};
