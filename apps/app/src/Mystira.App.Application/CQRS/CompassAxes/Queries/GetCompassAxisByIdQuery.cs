using Mystira.App.Application.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.CompassAxes.Queries;

/// <summary>
/// Query to retrieve a compass axis by ID.
/// </summary>
public record GetCompassAxisByIdQuery(string Id) : IQuery<CompassAxis?>, ICacheableQuery
{
    public string CacheKey => $"MasterData:CompassAxes:{Id}";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
