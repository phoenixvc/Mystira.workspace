using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Wolverine handler for validating if an age group value exists.
/// </summary>
public static class ValidateAgeGroupQueryHandler
{
    /// <summary>
    /// Handles the ValidateAgeGroupQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The age group repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the age group value exists; otherwise, false.</returns>
    public static async Task<bool> Handle(
        ValidateAgeGroupQuery query,
        IAgeGroupRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Validating age group: {Value}", query.Value);
        var isValid = await repository.ExistsByValueAsync(query.Value);
        logger.LogInformation("Age group '{Value}' is {Status}", query.Value, isValid ? "valid" : "invalid");
        return isValid;
    }
}
