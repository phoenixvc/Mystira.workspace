using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CharacterMapFile singleton entity
/// </summary>
public class CharacterMapFileRepository : ICharacterMapFileRepository
{
    private readonly MystiraAppDbContext _context;
    private readonly DbSet<CharacterMapFile> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterMapFileRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CharacterMapFileRepository(MystiraAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<CharacterMapFile>();
    }

    /// <inheritdoc/>
    public async Task<CharacterMapFile?> GetAsync()
    {
        return await _dbSet.FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<CharacterMapFile> AddOrUpdateAsync(CharacterMapFile entity)
    {
        var existing = await GetAsync();
        if (existing != null)
        {
            existing.Characters = entity.Characters;
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

