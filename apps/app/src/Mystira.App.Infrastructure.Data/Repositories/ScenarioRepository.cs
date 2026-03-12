using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for Scenario entity
/// </summary>
public class ScenarioRepository : Repository<Scenario>, IScenarioRepository
{
    public ScenarioRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Scenario>> GetByAgeGroupAsync(string ageGroup, CancellationToken ct = default)
    {
        return await _dbSet.Where(s => s.AgeGroupId == ageGroup).ToListAsync(ct);
    }

    public async Task<Scenario?> GetByTitleAsync(string title, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Title == title, ct);
    }

    public async Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(s => s.Title == title, ct);
    }

    public IQueryable<Scenario> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public async Task<int> CountAsync(Expression<Func<Scenario, bool>>? predicate = null, CancellationToken ct = default)
    {
        if (predicate == null)
        {
            return await _dbSet.CountAsync(ct);
        }
        return await _dbSet.CountAsync(predicate, ct);
    }
}
