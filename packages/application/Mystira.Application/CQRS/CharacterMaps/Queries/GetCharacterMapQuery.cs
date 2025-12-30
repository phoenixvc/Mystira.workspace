using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CharacterMaps.Queries;

/// <summary>
/// Query to retrieve a specific character map by ID.
/// </summary>
public record GetCharacterMapQuery(string Id) : IQuery<CharacterMap?>, ICacheableQuery
{
    public string CacheKey => $"CharacterMap:{Id}";
    public int CacheDurationSeconds => 600; // 10 minutes
}
