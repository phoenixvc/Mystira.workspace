using Mystira.Application.Interfaces;
using Mystira.Contracts.App.Responses.Attribution;

namespace Mystira.Application.CQRS.Attribution.Queries;

/// <summary>
/// Query to retrieve IP registration status for a scenario.
/// Cached for 10 minutes as IP status changes infrequently.
/// </summary>
/// <param name="ScenarioId">The unique identifier of the scenario.</param>
public record GetScenarioIpStatusQuery(string ScenarioId) : IQuery<IpVerificationResponse?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"ScenarioIpStatus:{ScenarioId}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 600; // 10 minutes
}
