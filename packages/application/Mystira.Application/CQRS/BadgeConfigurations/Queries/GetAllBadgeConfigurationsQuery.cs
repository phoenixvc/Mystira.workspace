using Mystira.Shared.CQRS;
using Mystira.Application.Interfaces;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.BadgeConfigurations.Queries;

/// <summary>
/// Query to retrieve all badge configurations.
/// </summary>
public record GetAllBadgeConfigurationsQuery
    : IQuery<List<BadgeConfiguration>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => "BadgeConfigurations:All";
}

/// <summary>
/// Handler for processing GetAllBadgeConfigurationsQuery requests.
/// </summary>
public sealed class GetAllBadgeConfigurationsQueryHandler
{
    private readonly IRepository<BadgeConfiguration> _repository;

    /// <summary>
    /// Initializes a new instance of the GetAllBadgeConfigurationsQueryHandler class.
    /// </summary>
    /// <param name="repository">The repository for accessing badge configuration data.</param>
    public GetAllBadgeConfigurationsQueryHandler(IRepository<BadgeConfiguration> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query to retrieve all badge configurations.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of all badge configurations ordered by name.</returns>
    public async Task<List<BadgeConfiguration>> Handle(GetAllBadgeConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var all = await _repository.GetAllAsync();
        // Stable ordering by Name for deterministic results in tests/clients
        return all.OrderBy(b => b.Name).ToList();
    }
}
