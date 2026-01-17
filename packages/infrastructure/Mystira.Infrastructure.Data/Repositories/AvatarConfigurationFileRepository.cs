using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for AvatarConfigurationFile singleton entity
/// </summary>
public class AvatarConfigurationFileRepository : IAvatarConfigurationFileRepository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<AvatarConfigurationFile> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvatarConfigurationFileRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AvatarConfigurationFileRepository(DbContext context)
    {
        _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<AvatarConfigurationFile>();
    }

    /// <inheritdoc/>
    public async Task<AvatarConfigurationFile?> GetAsync()
    {
        return await DbSet.FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<AvatarConfigurationFile> AddOrUpdateAsync(AvatarConfigurationFile entity)
    {
        var existing = await GetAsync();
        if (existing != null)
        {
            existing.AgeGroupAvatars = entity.AgeGroupAvatars;
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

