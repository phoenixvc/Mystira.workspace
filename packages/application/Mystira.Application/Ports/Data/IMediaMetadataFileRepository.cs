using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for MediaMetadataFile singleton entity
/// </summary>
public interface IMediaMetadataFileRepository
{
    /// <summary>
    /// Gets the singleton media metadata file.
    /// </summary>
    /// <returns>The media metadata file if it exists; otherwise, null.</returns>
    Task<MediaMetadataFile?> GetAsync();

    /// <summary>
    /// Adds or updates the singleton media metadata file.
    /// </summary>
    /// <param name="entity">The media metadata file entity to add or update.</param>
    /// <returns>The added or updated media metadata file.</returns>
    Task<MediaMetadataFile> AddOrUpdateAsync(MediaMetadataFile entity);

    /// <summary>
    /// Deletes the singleton media metadata file.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync();
}

