using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CharacterMaps.Queries;

/// <summary>
/// Query to retrieve all character maps.
/// Initializes with default character data if empty.
/// </summary>
public record GetAllCharacterMapsQuery : IQuery<List<CharacterMap>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "AllCharacterMaps";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 300; // 5 minutes
}
