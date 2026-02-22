namespace Mystira.App.Application.Ports.Storage;

/// <summary>
/// Port interface for blob storage operations (platform-agnostic).
/// Implementations can use Azure Blob Storage, AWS S3, local file system, etc.
/// </summary>
public interface IBlobService
{
    Task<string> UploadMediaAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<string> GetMediaUrlAsync(string blobName, CancellationToken ct = default);
    Task<bool> DeleteMediaAsync(string blobName, CancellationToken ct = default);
    Task<List<string>> ListMediaAsync(string prefix = "", CancellationToken ct = default);
    Task<Stream?> DownloadMediaAsync(string blobName, CancellationToken ct = default);
}
