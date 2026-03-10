using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

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

