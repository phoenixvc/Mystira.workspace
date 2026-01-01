using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Query to retrieve an age group by ID.
/// </summary>
/// <param name="Id">The unique identifier of the age group.</param>
public record GetAgeGroupByIdQuery(string Id) : IQuery<AgeGroupDefinition?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"MasterData:AgeGroups:{Id}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
