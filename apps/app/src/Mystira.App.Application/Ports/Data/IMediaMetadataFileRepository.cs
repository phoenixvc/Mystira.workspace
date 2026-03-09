using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for MediaMetadataFile singleton entity
/// </summary>
public interface IMediaMetadataFileRepository
{
    Task<MediaMetadataFile?> GetAsync(CancellationToken ct = default);
    Task<MediaMetadataFile> AddOrUpdateAsync(MediaMetadataFile entity, CancellationToken ct = default);
    Task DeleteAsync(CancellationToken ct = default);
}

