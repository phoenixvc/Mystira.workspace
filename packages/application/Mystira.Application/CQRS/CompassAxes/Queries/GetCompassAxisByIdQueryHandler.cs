using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CompassAxes.Queries;

/// <summary>
/// Wolverine handler for retrieving a compass axis by ID.
/// </summary>
public static class GetCompassAxisByIdQueryHandler
{
    /// <summary>
    /// Handles the GetCompassAxisByIdQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The compass axis repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The compass axis definition if found; otherwise, null.</returns>
    public static async Task<CompassAxisDefinition?> Handle(
        GetCompassAxisByIdQuery query,
        ICompassAxisRepository repository,
        ILogger<GetCompassAxisByIdQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving compass axis with id: {Id}", query.Id);
        var axis = await repository.GetByIdAsync(query.Id);

        if (axis == null)
        {
            logger.LogWarning("Compass axis with id {Id} not found", query.Id);
        }

        return axis;
    }
}
