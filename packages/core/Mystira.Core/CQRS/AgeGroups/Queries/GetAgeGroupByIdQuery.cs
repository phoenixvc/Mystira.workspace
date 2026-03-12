using Mystira.Core.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.AgeGroups.Queries;

/// <summary>
/// Query to retrieve an age group by ID.
/// </summary>
public record GetAgeGroupByIdQuery(string Id) : IQuery<AgeGroupDefinition?>, ICacheableQuery
{
    public string CacheKey => $"MasterData:AgeGroups:{Id}";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
