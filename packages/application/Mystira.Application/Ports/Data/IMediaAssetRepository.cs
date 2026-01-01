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
    /// <param name="mediaId">The media identifier.</param>
    /// <returns>The media asset if found; otherwise, null.</returns>
    Task<MediaAsset?> GetByMediaIdAsync(string mediaId);

    /// <summary>
    /// Checks whether a media asset exists with the specified media identifier.
    /// </summary>
    /// <param name="mediaId">The media identifier.</param>
    /// <returns>True if the media asset exists; otherwise, false.</returns>
    Task<bool> ExistsByMediaIdAsync(string mediaId);

    /// <summary>
    /// Gets the existing media identifiers from a collection of media identifiers.
    /// </summary>
    /// <param name="mediaIds">The collection of media identifiers to check.</param>
    /// <returns>A collection of media identifiers that exist in the repository.</returns>
    Task<IEnumerable<string>> GetMediaIdsAsync(IEnumerable<string> mediaIds);

    /// <summary>
    /// Gets a queryable collection of media assets for advanced querying.
    /// </summary>
    /// <returns>An IQueryable of media assets.</returns>
    IQueryable<MediaAsset> GetQueryable();
}

