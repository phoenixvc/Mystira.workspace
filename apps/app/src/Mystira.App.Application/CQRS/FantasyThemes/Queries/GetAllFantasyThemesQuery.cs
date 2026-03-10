using Mystira.App.Application.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.FantasyThemes.Queries;

/// <summary>
/// Query to retrieve all fantasy themes.
/// </summary>
public record GetAllFantasyThemesQuery : IQuery<List<FantasyThemeDefinition>>, ICacheableQuery
{
    public string CacheKey => "MasterData:FantasyThemes:All";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
