using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for MediaMetadataFile singleton entity
/// </summary>
public class MediaMetadataFileRepository : IMediaMetadataFileRepository
{
    private readonly MystiraAppDbContext _appContext;
    private readonly DbSet<MediaMetadataFile> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaMetadataFileRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public MediaMetadataFileRepository(MystiraAppDbContext context)
    {
        _appContext = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<MediaMetadataFile>();
    }

    /// <inheritdoc/>
    public async Task<MediaMetadataFile?> GetAsync()
    {
        return await DbSet.FirstOrDefaultAsync();
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
            DbSet.Update(existing);
            return existing;
        }

        await DbSet.AddAsync(entity);
        return entity;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync()
    {
        var existing = await GetAsync();
        if (existing != null)
        {
            DbSet.Remove(existing);
        }
    }
}

