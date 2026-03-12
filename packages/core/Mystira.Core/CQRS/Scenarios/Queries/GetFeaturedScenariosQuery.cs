using Mystira.Core.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Scenarios.Queries;

/// <summary>
/// Query to retrieve featured scenarios for the home page.
/// Featured scenarios are those marked with IsFeatured = true.
/// Cached for 10 minutes as featured content changes infrequently.
/// </summary>
public record GetFeaturedScenariosQuery : IQuery<List<Scenario>>, ICacheableQuery
{
    public string CacheKey => "FeaturedScenarios";
    public int CacheDurationSeconds => CacheDefaults.MediumSeconds;
}
