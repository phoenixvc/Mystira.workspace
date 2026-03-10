using Mystira.App.Application.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.CharacterMaps.Queries;

/// <summary>
/// Query to retrieve a specific character map by ID.
/// </summary>
public record GetCharacterMapQuery(string Id) : IQuery<CharacterMap?>, ICacheableQuery
{
    public string CacheKey => $"CharacterMap:{Id}";
    public int CacheDurationSeconds => CacheDefaults.MediumSeconds;
}
