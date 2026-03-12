using Mystira.Core.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.CharacterMaps.Queries;

/// <summary>
/// Query to retrieve all character maps.
/// Initializes with default character data if empty.
/// </summary>
public record GetAllCharacterMapsQuery : IQuery<List<CharacterMap>>, ICacheableQuery
{
    public string CacheKey => "AllCharacterMaps";
    public int CacheDurationSeconds => CacheDefaults.ShortSeconds;
}
