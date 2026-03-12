using Mystira.Core.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Archetypes.Queries;

/// <summary>
/// Query to retrieve an archetype by ID.
/// </summary>
public record GetArchetypeByIdQuery(string Id) : IQuery<ArchetypeDefinition?>, ICacheableQuery
{
    public string CacheKey => $"MasterData:Archetypes:{Id}";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
