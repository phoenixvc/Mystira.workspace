using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CompassAxes.Queries;

/// <summary>
/// Query to retrieve a compass axis by ID.
/// </summary>
/// <param name="Id">The unique identifier of the compass axis.</param>
public record GetCompassAxisByIdQuery(string Id) : IQuery<CompassAxis?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"MasterData:CompassAxes:{Id}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
