using Mystira.Core.Interfaces;
using Mystira.Contracts.App.Responses.Scenarios;

namespace Mystira.Core.CQRS.Scenarios.Queries;

/// <summary>
/// Query to retrieve featured scenarios for the home page.
/// Featured scenarios are those marked with IsFeatured = true.
/// Cached for 10 minutes as featured content changes infrequently.
/// </summary>
public record GetFeaturedScenariosQuery : IQuery<List<ScenarioSummary>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "FeaturedScenarios";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 600; // 10 minutes
}
