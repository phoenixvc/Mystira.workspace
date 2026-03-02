using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports;

/// <summary>
/// Port interface for media metadata service (file-based metadata management)
/// Uses Domain models as per Hexagonal Architecture - Application layer depends on Domain, not Contracts
/// </summary>
public interface IMediaMetadataService
{
    Task<MediaMetadataFile?> GetMediaMetadataFileAsync(CancellationToken ct = default);
    Task<MediaMetadataFile> UpdateMediaMetadataFileAsync(MediaMetadataFile metadataFile, CancellationToken ct = default);
    Task<MediaMetadataFile> AddMediaMetadataEntryAsync(MediaMetadataEntry entry, CancellationToken ct = default);
    Task<MediaMetadataFile> UpdateMediaMetadataEntryAsync(string entryId, MediaMetadataEntry entry, CancellationToken ct = default);
    Task<MediaMetadataFile> RemoveMediaMetadataEntryAsync(string entryId, CancellationToken ct = default);
    Task<MediaMetadataEntry?> GetMediaMetadataEntryAsync(string entryId, CancellationToken ct = default);
    Task<MediaMetadataFile> ImportMediaMetadataEntriesAsync(string jsonData, bool overwriteExisting = false, CancellationToken ct = default);
}

