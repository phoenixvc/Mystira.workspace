using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CharacterMap entity
/// </summary>
public class CharacterMapRepository : Repository<CharacterMap>, ICharacterMapRepository
{
    public CharacterMapRepository(DbContext context) : base(context)
    {
    }

    public async Task<CharacterMap?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(c => c.Name == name, ct);
    }
}
