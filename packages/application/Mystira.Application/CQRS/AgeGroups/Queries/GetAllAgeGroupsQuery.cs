using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Query to retrieve all age groups.
/// </summary>
public record GetAllAgeGroupsQuery : IQuery<List<AgeGroupDefinition>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "MasterData:AgeGroups:All";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
