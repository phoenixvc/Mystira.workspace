using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for MediaAsset entity with domain-specific queries
/// </summary>
public interface IMediaAssetRepository : IRepository<MediaAsset>
{
    /// <summary>
    /// Gets a media asset by its media identifier.
    /// </summary>
    Task<MediaAsset?> GetByMediaIdAsync(string mediaId, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a media asset exists with the specified media identifier.
    /// </summary>
    Task<bool> ExistsByMediaIdAsync(string mediaId, CancellationToken ct = default);

    /// <summary>
    /// Gets the existing media identifiers from a collection of media identifiers.
    /// </summary>
    Task<IEnumerable<string>> GetMediaIdsAsync(IEnumerable<string> mediaIds, CancellationToken ct = default);

    /// <summary>
    /// Gets a queryable collection of media assets for advanced querying.
    /// </summary>
    IQueryable<MediaAsset> GetQueryable();
}
