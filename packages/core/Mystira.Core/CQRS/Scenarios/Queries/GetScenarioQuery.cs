using Mystira.Core.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Scenarios.Queries;

/// <summary>
/// Query to retrieve a single scenario by ID (read operation).
/// Cached for 5 minutes as scenarios are relatively static.
/// </summary>
public record GetScenarioQuery(string ScenarioId) : IQuery<Scenario?>, ICacheableQuery
{
    public string CacheKey => $"Scenario:{ScenarioId}";
    public int CacheDurationSeconds => CacheDefaults.ShortSeconds;
};
