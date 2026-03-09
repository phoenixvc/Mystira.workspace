using Mystira.Admin.Api.Models;

namespace Mystira.Admin.Api.Services;

/// <summary>
/// Service for managing media metadata files
/// </summary>
public interface IMediaMetadataService
{
    /// <summary>
    /// Gets the media metadata file
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The media metadata file</returns>
    Task<MediaMetadataFile?> GetMediaMetadataFileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the media metadata file
    /// </summary>
    /// <param name="metadataFile">The updated media metadata file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated media metadata file</returns>
    Task<MediaMetadataFile> UpdateMediaMetadataFileAsync(MediaMetadataFile metadataFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new media metadata entry
    /// </summary>
    /// <param name="entry">The media metadata entry to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated media metadata file</returns>
    Task<MediaMetadataFile> AddMediaMetadataEntryAsync(MediaMetadataEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing media metadata entry
    /// </summary>
    /// <param name="entryId">The ID of the entry to update</param>
    /// <param name="entry">The updated media metadata entry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated media metadata file</returns>
    Task<MediaMetadataFile> UpdateMediaMetadataEntryAsync(string entryId, MediaMetadataEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a media metadata entry
    /// </summary>
    /// <param name="entryId">The ID of the entry to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated media metadata file</returns>
    Task<MediaMetadataFile> RemoveMediaMetadataEntryAsync(string entryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific media metadata entry by ID
    /// </summary>
    /// <param name="entryId">The ID of the entry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The media metadata entry or null if not found</returns>
    Task<MediaMetadataEntry?> GetMediaMetadataEntryAsync(string entryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports media metadata entries from a JSON file
    /// </summary>
    /// <param name="jsonData">The JSON data containing media metadata entries</param>
    /// <param name="overwriteExisting">Whether to overwrite existing entries</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated media metadata file</returns>
    Task<MediaMetadataFile> ImportMediaMetadataEntriesAsync(string jsonData, bool overwriteExisting = false, CancellationToken cancellationToken = default);
}
