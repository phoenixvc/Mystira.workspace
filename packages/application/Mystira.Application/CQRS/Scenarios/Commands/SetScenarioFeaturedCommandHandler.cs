using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.CQRS.Scenarios.Commands;

/// <summary>
/// Wolverine handler for SetScenarioFeaturedCommand.
/// Sets or unsets a scenario's featured status - this is an admin operation.
/// </summary>
public static class SetScenarioFeaturedCommandHandler
{
    /// <summary>
    /// Handles the SetScenarioFeaturedCommand by updating the scenario's featured status.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task Handle(
        SetScenarioFeaturedCommand command,
        IScenarioRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.ScenarioId))
        {
            throw new ArgumentException("Scenario ID cannot be null or empty", nameof(command.ScenarioId));
        }

        var scenario = await repository.GetByIdAsync(command.ScenarioId);

        if (scenario == null)
        {
            logger.LogWarning("Scenario not found: {ScenarioId}", command.ScenarioId);
            throw new InvalidOperationException($"Scenario not found: {command.ScenarioId}");
        }

        var previousStatus = scenario.IsFeatured;
        scenario.IsFeatured = command.IsFeatured;

        await repository.UpdateAsync(scenario);

        try
        {
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating featured status for scenario: {ScenarioId}", command.ScenarioId);
            throw;
        }

        logger.LogInformation(
            "Updated scenario {ScenarioId} featured status: {PreviousStatus} -> {NewStatus}",
            command.ScenarioId,
            previousStatus,
            command.IsFeatured);
    }
}
