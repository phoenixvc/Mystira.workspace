using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.FantasyThemes.Queries;

/// <summary>
/// Query to retrieve a fantasy theme by ID.
/// </summary>
/// <param name="Id">The unique identifier of the fantasy theme.</param>
public record GetFantasyThemeByIdQuery(string Id) : IQuery<FantasyThemeDefinition?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"MasterData:FantasyThemes:{Id}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
