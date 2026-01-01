using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CompassAxes.Queries;

/// <summary>
/// Wolverine handler for retrieving all compass axes.
/// </summary>
public static class GetAllCompassAxesQueryHandler
{
    /// <summary>
    /// Handles the GetAllCompassAxesQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The compass axis repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of all compass axis definitions.</returns>
    public static async Task<List<CompassAxisDefinition>> Handle(
        GetAllCompassAxesQuery query,
        ICompassAxisRepository repository,
        ILogger<GetAllCompassAxesQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving all compass axes");
        var axes = await repository.GetAllAsync();
        logger.LogInformation("Retrieved {Count} compass axes", axes.Count);
        return axes;
    }
}
