using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.CQRS.Archetypes.Queries;

/// <summary>
/// Wolverine handler for validating if an archetype name exists.
/// </summary>
public static class ValidateArchetypeQueryHandler
{
    /// <summary>
    /// Handles the ValidateArchetypeQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The archetype repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the archetype name exists; otherwise, false.</returns>
    public static async Task<bool> Handle(
        ValidateArchetypeQuery query,
        IArchetypeRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Validating archetype: {Name}", query.Name);
        var isValid = await repository.ExistsByNameAsync(query.Name);
        logger.LogInformation("Archetype '{Name}' is {Status}", query.Name, isValid ? "valid" : "invalid");
        return isValid;
    }
}
