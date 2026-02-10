using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CharacterMediaMetadataFile singleton entity
/// </summary>
public class CharacterMediaMetadataFileRepository : ICharacterMediaMetadataFileRepository
{
    private readonly MystiraAppDbContext _context;
    private readonly DbSet<CharacterMediaMetadataFile> _dbSet;

    public CharacterMediaMetadataFileRepository(MystiraAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<CharacterMediaMetadataFile>();
    }

    public async Task<CharacterMediaMetadataFile?> GetAsync(CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(ct);
    }

    public async Task<CharacterMediaMetadataFile> AddOrUpdateAsync(CharacterMediaMetadataFile entity, CancellationToken ct = default)
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
