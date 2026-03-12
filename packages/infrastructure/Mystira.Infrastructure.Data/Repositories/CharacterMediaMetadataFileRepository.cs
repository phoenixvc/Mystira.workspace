using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CharacterMediaMetadataFile singleton entity
/// </summary>
public class CharacterMediaMetadataFileRepository : ICharacterMediaMetadataFileRepository
{
    private readonly MystiraAppDbContext _appContext;
    private readonly DbSet<CharacterMediaMetadataFile> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterMediaMetadataFileRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CharacterMediaMetadataFileRepository(MystiraAppDbContext context)
    {
        _appContext = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<CharacterMediaMetadataFile>();
    }

    /// <inheritdoc/>
    public async Task<CharacterMediaMetadataFile?> GetAsync(CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<CharacterMediaMetadataFile> AddOrUpdateAsync(CharacterMediaMetadataFile entity, CancellationToken ct = default)
    {
        var existing = await GetAsync(ct);
        if (existing != null)
        {
            existing.Entries = entity.Entries;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.Version = entity.Version;
            DbSet.Update(existing);
            return existing;
        }

        await DbSet.AddAsync(entity, ct);
        return entity;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(CancellationToken ct = default)
    {
        var existing = await GetAsync(ct);
        if (existing != null)
        {
            DbSet.Remove(existing);
        }
    }
}
