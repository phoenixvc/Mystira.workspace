using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.CQRS.EchoTypes.Queries;

/// <summary>
/// Wolverine handler for validating if an echo type name exists.
/// </summary>
public static class ValidateEchoTypeQueryHandler
{
    /// <summary>
    /// Handles the ValidateEchoTypeQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The echo type repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the echo type name exists; otherwise, false.</returns>
    public static async Task<bool> Handle(
        ValidateEchoTypeQuery query,
        IEchoTypeRepository repository,
        ILogger<ValidateEchoTypeQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Validating echo type: {Name}", query.Name);
        var isValid = await repository.ExistsByNameAsync(query.Name);
        logger.LogInformation("Echo type '{Name}' is {Status}", query.Name, isValid ? "valid" : "invalid");
        return isValid;
    }
}
