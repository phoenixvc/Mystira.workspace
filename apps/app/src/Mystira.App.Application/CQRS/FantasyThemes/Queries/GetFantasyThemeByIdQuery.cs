using Mystira.App.Application.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.FantasyThemes.Queries;

/// <summary>
/// Query to retrieve a fantasy theme by ID.
/// </summary>
public record GetFantasyThemeByIdQuery(string Id) : IQuery<FantasyThemeDefinition?>, ICacheableQuery
{
    public string CacheKey => $"MasterData:FantasyThemes:{Id}";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
