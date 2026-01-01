using Mystira.Shared.CQRS;
using Mystira.Application.Interfaces;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.BadgeConfigurations.Queries;

/// <summary>
/// Query to retrieve badge configurations for a specific compass axis.
/// </summary>
/// <param name="Axis">The compass axis to filter badge configurations by.</param>
public record GetBadgeConfigurationsByAxisQuery(string Axis)
    : IQuery<List<BadgeConfiguration>>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"BadgeConfigurations:Axis:{Axis}";
}

/// <summary>
/// Handler for processing GetBadgeConfigurationsByAxisQuery requests.
/// </summary>
public sealed class GetBadgeConfigurationsByAxisQueryHandler
{
    private readonly IRepository<BadgeConfiguration> _repository;

    /// <summary>
    /// Initializes a new instance of the GetBadgeConfigurationsByAxisQueryHandler class.
    /// </summary>
    /// <param name="repository">The repository for accessing badge configuration data.</param>
    public GetBadgeConfigurationsByAxisQueryHandler(IRepository<BadgeConfiguration> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query to retrieve badge configurations for a specific axis.
    /// </summary>
    /// <param name="request">The query request containing the axis filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of badge configurations for the specified axis ordered by name.</returns>
    public async Task<List<BadgeConfiguration>> Handle(GetBadgeConfigurationsByAxisQuery request, CancellationToken cancellationToken)
    {
        // BadgeConfiguration.Axis is CoreAxis?, but we're comparing with AxisId (string)
        // Use AxisId property instead which is the string value
        var list = await _repository.FindAsync(b => b.AxisId == request.Axis);
        return list.OrderBy(b => b.Name).ToList();
    }
}
