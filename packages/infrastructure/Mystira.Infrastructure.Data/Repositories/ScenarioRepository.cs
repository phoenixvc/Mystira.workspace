using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Scenario entity
/// </summary>
public class ScenarioRepository : Repository<Scenario>, IScenarioRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ScenarioRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Scenario>> GetByAgeGroupAsync(string ageGroup)
    {
        // Compare against AgeGroupId (the string property), not the computed AgeGroup value object
        return await _dbSet.Where(s => s.AgeGroupId == ageGroup).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Scenario?> GetByTitleAsync(string title)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Title == title);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsByTitleAsync(string title)
    {
        return await _dbSet.AnyAsync(s => s.Title == title);
    }

    /// <inheritdoc/>
    public IQueryable<Scenario> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(Expression<Func<Scenario, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync();
        }
        return await _dbSet.CountAsync(predicate);
    }
}

