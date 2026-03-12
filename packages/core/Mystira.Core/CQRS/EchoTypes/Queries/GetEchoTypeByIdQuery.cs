using Mystira.Core.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.EchoTypes.Queries;

/// <summary>
/// Query to retrieve an echo type by ID.
/// </summary>
public record GetEchoTypeByIdQuery(string Id) : IQuery<EchoTypeDefinition?>, ICacheableQuery
{
    public string CacheKey => $"MasterData:EchoTypes:{Id}";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
