using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository interface for MediaMetadataFile singleton entity
/// </summary>
public interface IMediaMetadataFileRepository
{
    /// <summary>
    /// Gets the singleton media metadata file.
    /// </summary>
    Task<MediaMetadataFile?> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds or updates the singleton media metadata file.
    /// </summary>
    Task<MediaMetadataFile> AddOrUpdateAsync(MediaMetadataFile entity, CancellationToken ct = default);

    /// <summary>
    /// Deletes the singleton media metadata file.
    /// </summary>
    Task DeleteAsync(CancellationToken ct = default);
}
