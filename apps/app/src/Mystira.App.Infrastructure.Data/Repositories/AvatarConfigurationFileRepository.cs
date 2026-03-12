using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for AvatarConfigurationFile singleton entity
/// </summary>
public class AvatarConfigurationFileRepository : IAvatarConfigurationFileRepository
{
    private readonly DbContext _context;
    private readonly DbSet<AvatarConfigurationFile> _dbSet;

    public AvatarConfigurationFileRepository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<AvatarConfigurationFile>();
    }

    public async Task<AvatarConfigurationFile?> GetAsync(CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(ct);
    }

    public async Task<AvatarConfigurationFile> AddOrUpdateAsync(AvatarConfigurationFile entity, CancellationToken ct = default)
    {
        var existing = await GetAsync(ct);
        if (existing != null)
        {
            existing.AgeGroupAvatars = entity.AgeGroupAvatars;
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
