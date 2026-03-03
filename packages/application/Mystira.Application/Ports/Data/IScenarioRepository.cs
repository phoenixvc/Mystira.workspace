using System.Linq.Expressions;
using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for Scenario entity with domain-specific queries
/// </summary>
public interface IScenarioRepository : IRepository<Scenario>
{
    /// <summary>
    /// Gets all scenarios for a specific age group.
    /// </summary>
    /// <param name="ageGroup">The age group identifier.</param>
    /// <returns>A collection of scenarios for the specified age group.</returns>
    Task<IEnumerable<Scenario>> GetByAgeGroupAsync(string ageGroup);

    /// <summary>
    /// Gets a scenario by its title.
    /// </summary>
    /// <param name="title">The scenario title.</param>
    /// <returns>The scenario if found; otherwise, null.</returns>
    Task<Scenario?> GetByTitleAsync(string title);

    /// <summary>
    /// Checks whether a scenario exists with the specified title.
    /// </summary>
    /// <param name="title">The scenario title.</param>
    /// <returns>True if a scenario with the title exists; otherwise, false.</returns>
    Task<bool> ExistsByTitleAsync(string title);

    /// <summary>
    /// Gets a queryable collection of scenarios for advanced querying.
    /// </summary>
    /// <returns>An IQueryable of scenarios.</returns>
    IQueryable<Scenario> GetQueryable();

    /// <summary>
    /// Counts the number of scenarios that match the optional predicate.
    /// </summary>
    /// <param name="predicate">The optional predicate expression to filter scenarios. If null, counts all scenarios.</param>
    /// <returns>The count of scenarios matching the predicate, or total count if predicate is null.</returns>
    Task<int> CountAsync(Expression<Func<Scenario, bool>>? predicate = null);
}

