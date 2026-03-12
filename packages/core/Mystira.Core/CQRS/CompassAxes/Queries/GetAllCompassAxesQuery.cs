using Mystira.Core.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.CompassAxes.Queries;

/// <summary>
/// Query to retrieve all compass axes.
/// </summary>
public record GetAllCompassAxesQuery : IQuery<List<CompassAxis>>, ICacheableQuery
{
    public string CacheKey => "MasterData:CompassAxes:All";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
