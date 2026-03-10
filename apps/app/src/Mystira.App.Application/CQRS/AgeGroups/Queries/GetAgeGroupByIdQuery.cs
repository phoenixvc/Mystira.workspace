using Mystira.App.Application.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Query to retrieve an age group by ID.
/// </summary>
public record GetAgeGroupByIdQuery(string Id) : IQuery<AgeGroupDefinition?>, ICacheableQuery
{
    public string CacheKey => $"MasterData:AgeGroups:{Id}";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
