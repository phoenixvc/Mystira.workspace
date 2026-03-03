using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CharacterMaps.Queries;

/// <summary>
/// Query to retrieve a specific character map by ID.
/// </summary>
/// <param name="Id">The unique identifier of the character map.</param>
public record GetCharacterMapQuery(string Id) : IQuery<CharacterMap?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"CharacterMap:{Id}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 600; // 10 minutes
}
