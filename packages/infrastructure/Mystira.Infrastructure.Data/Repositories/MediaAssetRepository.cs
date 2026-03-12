using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for MediaAsset entity
/// </summary>
public class MediaAssetRepository : Repository<MediaAsset>, IMediaAssetRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaAssetRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public MediaAssetRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<MediaAsset?> GetByMediaIdAsync(string mediaId, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(m => m.MediaId == mediaId, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByMediaIdAsync(string mediaId, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(m => m.MediaId == mediaId, ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetMediaIdsAsync(IEnumerable<string> mediaIds, CancellationToken ct = default)
    {
        return await DbSet
            .Where(m => mediaIds.Contains(m.MediaId))
            .Select(m => m.MediaId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public IQueryable<MediaAsset> GetQueryable()
    {
        return DbSet.AsQueryable();
    }
}
