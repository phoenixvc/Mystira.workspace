using Mystira.Core.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.FantasyThemes.Queries;

/// <summary>
/// Query to retrieve all fantasy themes.
/// </summary>
public record GetAllFantasyThemesQuery : IQuery<List<FantasyThemeDefinition>>, ICacheableQuery
{
    public string CacheKey => "MasterData:FantasyThemes:All";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
