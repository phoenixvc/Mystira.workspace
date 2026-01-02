using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for CharacterMap entity
/// </summary>
public class CharacterMapRepository : Repository<CharacterMap>, ICharacterMapRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CharacterMapRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CharacterMapRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<CharacterMap?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dbSet.AnyAsync(c => c.Name == name);
    }
}

