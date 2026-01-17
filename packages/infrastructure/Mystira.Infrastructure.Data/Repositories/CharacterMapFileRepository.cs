using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CharacterMapFile singleton entity
/// </summary>
public class CharacterMapFileRepository : ICharacterMapFileRepository
{
    private readonly MystiraAppDbContext _appContext;
    private readonly DbSet<CharacterMapFile> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterMapFileRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CharacterMapFileRepository(MystiraAppDbContext context)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<CharacterMapFile>();
    }

    /// <inheritdoc/>
    public async Task<CharacterMapFile?> GetAsync()
    {
        return await DbSet.FirstOrDefaultAsync();
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

