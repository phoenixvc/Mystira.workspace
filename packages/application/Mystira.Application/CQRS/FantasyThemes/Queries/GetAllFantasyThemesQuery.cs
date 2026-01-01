using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.FantasyThemes.Queries;

/// <summary>
/// Query to retrieve all fantasy themes.
/// </summary>
public record GetAllFantasyThemesQuery : IQuery<List<FantasyThemeDefinition>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "MasterData:FantasyThemes:All";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
