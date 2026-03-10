using System.Linq.Expressions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for Scenario entity with domain-specific queries
/// </summary>
public interface IScenarioRepository : IRepository<Scenario, string>
{
    Task<IEnumerable<Scenario>> GetByAgeGroupAsync(string ageGroup, CancellationToken ct = default);
    Task<Scenario?> GetByTitleAsync(string title, CancellationToken ct = default);
    Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default);
    IQueryable<Scenario> GetQueryable();
    Task<int> CountAsync(Expression<Func<Scenario, bool>>? predicate = null, CancellationToken ct = default);
}

