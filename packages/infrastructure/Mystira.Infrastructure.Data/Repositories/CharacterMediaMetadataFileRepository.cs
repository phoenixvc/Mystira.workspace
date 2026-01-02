using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CharacterMediaMetadataFile singleton entity
/// </summary>
public class CharacterMediaMetadataFileRepository : ICharacterMediaMetadataFileRepository
{
    private readonly MystiraAppDbContext _context;
    private readonly DbSet<CharacterMediaMetadataFile> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterMediaMetadataFileRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CharacterMediaMetadataFileRepository(MystiraAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<CharacterMediaMetadataFile>();
    }

    /// <inheritdoc/>
    public async Task<CharacterMediaMetadataFile?> GetAsync()
    {
        return await _dbSet.FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<CharacterMediaMetadataFile> AddOrUpdateAsync(CharacterMediaMetadataFile entity)
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

