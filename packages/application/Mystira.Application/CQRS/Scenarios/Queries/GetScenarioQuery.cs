using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Scenarios.Queries;

/// <summary>
/// Query to retrieve a single scenario by ID (read operation).
/// Cached for 5 minutes as scenarios are relatively static.
/// </summary>
/// <param name="ScenarioId">The unique identifier of the scenario to retrieve.</param>
public record GetScenarioQuery(string ScenarioId) : IQuery<Scenario?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"Scenario:{ScenarioId}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 300; // 5 minutes
};
