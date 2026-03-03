using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.CQRS.CompassAxes.Queries;

/// <summary>
/// Wolverine handler for validating if a compass axis name exists.
/// </summary>
public static class ValidateCompassAxisQueryHandler
{
    /// <summary>
    /// Handles the ValidateCompassAxisQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The compass axis repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the compass axis name exists; otherwise, false.</returns>
    public static async Task<bool> Handle(
        ValidateCompassAxisQuery query,
        ICompassAxisRepository repository,
        ILogger<ValidateCompassAxisQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Validating compass axis: {Name}", query.Name);
        var isValid = await repository.ExistsByNameAsync(query.Name);
        logger.LogInformation("Compass axis '{Name}' is {Status}", query.Name, isValid ? "valid" : "invalid");
        return isValid;
    }
}
