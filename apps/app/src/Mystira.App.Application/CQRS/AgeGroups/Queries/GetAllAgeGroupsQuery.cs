using Mystira.App.Application.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Query to retrieve all age groups.
/// </summary>
public record GetAllAgeGroupsQuery : IQuery<List<AgeGroupDefinition>>, ICacheableQuery
{
    public string CacheKey => "MasterData:AgeGroups:All";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
