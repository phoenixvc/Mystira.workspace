using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Queries;

/// <summary>
/// Query to retrieve an archetype by ID.
/// </summary>
/// <param name="Id">The unique identifier of the archetype.</param>
public record GetArchetypeByIdQuery(string Id) : IQuery<ArchetypeDefinition?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"MasterData:Archetypes:{Id}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
