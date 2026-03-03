using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.EchoTypes.Queries;

/// <summary>
/// Query to retrieve all echo types.
/// </summary>
public record GetAllEchoTypesQuery : IQuery<List<EchoTypeDefinition>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "MasterData:EchoTypes:All";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
