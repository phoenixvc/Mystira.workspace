using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for MediaMetadataFile singleton entity
/// </summary>
public class MediaMetadataFileRepository : IMediaMetadataFileRepository
{
    private readonly MystiraAppDbContext _context;
    private readonly DbSet<MediaMetadataFile> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaMetadataFileRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public MediaMetadataFileRepository(MystiraAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<MediaMetadataFile>();
    }

    /// <inheritdoc/>
    public async Task<MediaMetadataFile?> GetAsync()
    {
        return await _dbSet.FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<MediaMetadataFile> AddOrUpdateAsync(MediaMetadataFile entity)
    {
        var existing = await GetAsync();
        if (existing != null)
        {
            existing.Entries = entity.Entries;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Version = entity.Version;
            _dbSet.Update(existing);
            return existing;
        }

        await _dbSet.AddAsync(entity);
        return entity;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync()
    {
        var existing = await GetAsync();
        if (existing != null)
        {
            _dbSet.Remove(existing);
        }
    }
}

