using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CharacterMapFile singleton entity
/// </summary>
public class CharacterMapFileRepository : ICharacterMapFileRepository
{
    private readonly MystiraAppDbContext _context;
    private readonly DbSet<CharacterMapFile> _dbSet;

    public CharacterMapFileRepository(MystiraAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<CharacterMapFile>();
    }

    public async Task<CharacterMapFile?> GetAsync(CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(ct);
    }

    public async Task<CharacterMapFile> AddOrUpdateAsync(CharacterMapFile entity, CancellationToken ct = default)
    {
        var existing = await GetAsync(ct);
        if (existing != null)
        {
            existing.Characters = entity.Characters;
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
