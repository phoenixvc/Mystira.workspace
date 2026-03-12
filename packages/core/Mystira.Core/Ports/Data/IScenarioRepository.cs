using System.Linq.Expressions;
using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository interface for Scenario entity with domain-specific queries
/// </summary>
public interface IScenarioRepository : IRepository<Scenario>
{
    /// <summary>
    /// Gets all scenarios for a specific age group.
    /// </summary>
    Task<IEnumerable<Scenario>> GetByAgeGroupAsync(string ageGroup, CancellationToken ct = default);

    /// <summary>
    /// Gets a scenario by its title.
    /// </summary>
    Task<Scenario?> GetByTitleAsync(string title, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a scenario exists with the specified title.
    /// </summary>
    Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default);

    /// <summary>
    /// Gets a queryable collection of scenarios for advanced querying.
    /// </summary>
    IQueryable<Scenario> GetQueryable();

    /// <summary>
    /// Counts the number of scenarios that match the optional predicate.
    /// </summary>
    Task<int> CountAsync(Expression<Func<Scenario, bool>>? predicate = null, CancellationToken ct = default);
}
