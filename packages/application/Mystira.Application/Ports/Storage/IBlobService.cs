namespace Mystira.Application.Ports.Storage;

/// <summary>
/// Port interface for blob storage operations (platform-agnostic).
/// Implementations can use Azure Blob Storage, AWS S3, local file system, etc.
/// </summary>
public interface IBlobService
{
    /// <summary>
    /// Uploads a media file to blob storage.
    /// </summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The name for the file.</param>
    /// <param name="contentType">The MIME content type of the file.</param>
    /// <returns>The URL or identifier of the uploaded blob.</returns>
    Task<string> UploadMediaAsync(Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Gets the URL for accessing a media file.
    /// </summary>
    /// <param name="blobName">The blob name or identifier.</param>
    /// <returns>The URL to access the media.</returns>
    Task<string> GetMediaUrlAsync(string blobName);

    /// <summary>
    /// Deletes a media file from blob storage.
    /// </summary>
    /// <param name="blobName">The blob name or identifier to delete.</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    Task<bool> DeleteMediaAsync(string blobName);

    /// <summary>
    /// Lists media files in blob storage with an optional prefix filter.
    /// </summary>
    /// <param name="prefix">Optional prefix to filter results.</param>
    /// <returns>A list of blob names matching the prefix.</returns>
    Task<List<string>> ListMediaAsync(string prefix = "");

    /// <summary>
    /// Downloads a media file from blob storage.
    /// </summary>
    /// <param name="blobName">The blob name or identifier to download.</param>
    /// <returns>The file stream, or null if not found.</returns>
    Task<Stream?> DownloadMediaAsync(string blobName);
}
