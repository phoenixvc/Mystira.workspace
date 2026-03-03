using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Queries;

/// <summary>
/// Query to retrieve all archetypes.
/// </summary>
public record GetAllArchetypesQuery : IQuery<List<ArchetypeDefinition>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "MasterData:Archetypes:All";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
