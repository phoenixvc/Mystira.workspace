using Mystira.App.Application.Interfaces;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.Archetypes.Queries;

/// <summary>
/// Query to retrieve all archetypes.
/// </summary>
public record GetAllArchetypesQuery : IQuery<List<ArchetypeDefinition>>, ICacheableQuery
{
    public string CacheKey => "MasterData:Archetypes:All";
    public int CacheDurationSeconds => CacheDefaults.MasterDataSeconds;
}
