using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CompassAxes.Queries;

/// <summary>
/// Query to retrieve all compass axes.
/// </summary>
public record GetAllCompassAxesQuery : IQuery<List<CompassAxis>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "MasterData:CompassAxes:All";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
