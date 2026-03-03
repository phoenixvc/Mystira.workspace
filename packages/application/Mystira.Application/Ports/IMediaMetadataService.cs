using Mystira.Domain.Models;

namespace Mystira.Application.Ports;

/// <summary>
/// Port interface for media metadata service (file-based metadata management)
/// Uses Domain models as per Hexagonal Architecture - Application layer depends on Domain, not Contracts
/// </summary>
public interface IMediaMetadataService
{
    /// <summary>
    /// Gets the media metadata file containing all metadata entries.
    /// </summary>
    /// <returns>The media metadata file if it exists; otherwise, null.</returns>
    Task<MediaMetadataFile?> GetMediaMetadataFileAsync();

    /// <summary>
    /// Updates the entire media metadata file.
    /// </summary>
    /// <param name="metadataFile">The updated media metadata file.</param>
    /// <returns>The updated media metadata file.</returns>
    Task<MediaMetadataFile> UpdateMediaMetadataFileAsync(MediaMetadataFile metadataFile);

    /// <summary>
    /// Adds a new media metadata entry to the metadata file.
    /// </summary>
    /// <param name="entry">The media metadata entry to add.</param>
    /// <returns>The updated media metadata file.</returns>
    Task<MediaMetadataFile> AddMediaMetadataEntryAsync(MediaMetadataEntry entry);

    /// <summary>
    /// Updates an existing media metadata entry.
    /// </summary>
    /// <param name="entryId">The identifier of the entry to update.</param>
    /// <param name="entry">The updated media metadata entry.</param>
    /// <returns>The updated media metadata file.</returns>
    Task<MediaMetadataFile> UpdateMediaMetadataEntryAsync(string entryId, MediaMetadataEntry entry);

    /// <summary>
    /// Removes a media metadata entry from the metadata file.
    /// </summary>
    /// <param name="entryId">The identifier of the entry to remove.</param>
    /// <returns>The updated media metadata file.</returns>
    Task<MediaMetadataFile> RemoveMediaMetadataEntryAsync(string entryId);

    /// <summary>
    /// Gets a specific media metadata entry by its identifier.
    /// </summary>
    /// <param name="entryId">The entry identifier.</param>
    /// <returns>The media metadata entry if found; otherwise, null.</returns>
    Task<MediaMetadataEntry?> GetMediaMetadataEntryAsync(string entryId);

    /// <summary>
    /// Imports media metadata entries from JSON data.
    /// </summary>
    /// <param name="jsonData">The JSON data containing metadata entries to import.</param>
    /// <param name="overwriteExisting">Whether to overwrite existing entries with the same identifier.</param>
    /// <returns>The updated media metadata file.</returns>
    Task<MediaMetadataFile> ImportMediaMetadataEntriesAsync(string jsonData, bool overwriteExisting = false);
}

