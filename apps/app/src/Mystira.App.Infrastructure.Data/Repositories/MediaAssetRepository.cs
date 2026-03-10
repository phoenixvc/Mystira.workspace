using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for MediaAsset entity
/// </summary>
public class MediaAssetRepository : Repository<MediaAsset>, IMediaAssetRepository
{
    public MediaAssetRepository(DbContext context) : base(context)
    {
    }

    public async Task<MediaAsset?> GetByMediaIdAsync(string mediaId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.MediaId == mediaId, ct);
    }

    public async Task<bool> ExistsByMediaIdAsync(string mediaId, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(m => m.MediaId == mediaId, ct);
    }

    public async Task<IEnumerable<string>> GetMediaIdsAsync(IEnumerable<string> mediaIds, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(m => mediaIds.Contains(m.MediaId))
            .Select(m => m.MediaId)
            .ToListAsync(ct);
    }

    public IQueryable<MediaAsset> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
}
