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
    public async Task<MediaAsset?> GetByMediaIdAsync(string mediaId)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.MediaId == mediaId);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByMediaIdAsync(string mediaId)
    {
        return await _dbSet.AnyAsync(m => m.MediaId == mediaId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetMediaIdsAsync(IEnumerable<string> mediaIds)
    {
        return await _dbSet
            .Where(m => mediaIds.Contains(m.MediaId))
            .Select(m => m.MediaId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public IQueryable<MediaAsset> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
}

