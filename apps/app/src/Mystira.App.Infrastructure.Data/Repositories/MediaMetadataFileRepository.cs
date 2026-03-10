using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for MediaMetadataFile singleton entity
/// </summary>
public class MediaMetadataFileRepository : IMediaMetadataFileRepository
{
    private readonly MystiraAppDbContext _context;
    private readonly DbSet<MediaMetadataFile> _dbSet;

    public MediaMetadataFileRepository(MystiraAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<MediaMetadataFile>();
    }

    public async Task<MediaMetadataFile?> GetAsync(CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(ct);
    }

    public async Task<MediaMetadataFile> AddOrUpdateAsync(MediaMetadataFile entity, CancellationToken ct = default)
    {
        var existing = await GetAsync(ct);
        if (existing != null)
        {
            existing.Entries = entity.Entries;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Version = entity.Version;
            _dbSet.Update(existing);
            return existing;
        }

        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    public async Task DeleteAsync(CancellationToken ct = default)
    {
        var existing = await GetAsync(ct);
        if (existing != null)
        {
            _dbSet.Remove(existing);
        }
    }
}
