using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for MediaAsset entity with domain-specific queries
/// </summary>
public interface IMediaAssetRepository : IRepository<MediaAsset, string>
{
    Task<MediaAsset?> GetByMediaIdAsync(string mediaId, CancellationToken ct = default);
    Task<bool> ExistsByMediaIdAsync(string mediaId, CancellationToken ct = default);
    Task<IEnumerable<string>> GetMediaIdsAsync(IEnumerable<string> mediaIds, CancellationToken ct = default);
    IQueryable<MediaAsset> GetQueryable();
}

