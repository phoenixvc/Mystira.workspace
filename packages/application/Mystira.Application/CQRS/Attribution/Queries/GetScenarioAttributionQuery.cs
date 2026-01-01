using Mystira.Application.Interfaces;
using Mystira.Contracts.App.Responses.Attribution;

namespace Mystira.Application.CQRS.Attribution.Queries;

/// <summary>
/// Query to retrieve attribution/creator credits for a scenario.
/// Cached for 10 minutes as attribution data changes infrequently.
/// </summary>
/// <param name="ScenarioId">The unique identifier of the scenario.</param>
public record GetScenarioAttributionQuery(string ScenarioId) : IQuery<ContentAttributionResponse?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"ScenarioAttribution:{ScenarioId}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 600; // 10 minutes
}
