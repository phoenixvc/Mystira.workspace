using Mystira.Application.Interfaces;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.EchoTypes.Queries;

/// <summary>
/// Query to retrieve an echo type by ID.
/// </summary>
/// <param name="Id">The unique identifier of the echo type.</param>
public record GetEchoTypeByIdQuery(string Id) : IQuery<EchoTypeDefinition?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"MasterData:EchoTypes:{Id}";

    /// <summary>
    /// Gets the cache duration in seconds.
    /// </summary>
    public int CacheDurationSeconds => 3600; // 1 hour - master data rarely changes
}
