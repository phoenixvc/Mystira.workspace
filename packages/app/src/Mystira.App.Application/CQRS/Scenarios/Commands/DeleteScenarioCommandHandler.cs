using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Exceptions;
using Mystira.Shared.Exceptions;
using NotFoundException = Mystira.Shared.Exceptions.NotFoundException;
using ValidationException = Mystira.Shared.Exceptions.ValidationException;

namespace Mystira.App.Application.CQRS.Scenarios.Commands;

/// <summary>
/// Wolverine handler for DeleteScenarioCommand.
/// Deletes a scenario - this is a write operation that modifies state.
/// </summary>
public static class DeleteScenarioCommandHandler
{
    /// <summary>
    /// Handles the DeleteScenarioCommand by deleting a scenario from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task Handle(
        DeleteScenarioCommand command,
        IScenarioRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.ScenarioId))
        {
            throw new ValidationException("scenarioId", "Scenario ID cannot be null or empty");
        }

        var scenario = await repository.GetByIdAsync(command.ScenarioId, ct);

        if (scenario == null)
        {
            logger.LogWarning("Scenario not found: {ScenarioId}", command.ScenarioId);
            throw new NotFoundException("Scenario", command.ScenarioId);
        }

        await repository.DeleteAsync(command.ScenarioId, ct);

        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting scenario: {ScenarioId}", command.ScenarioId);
            throw;
        }

        logger.LogInformation("Deleted scenario: {ScenarioId}", command.ScenarioId);
    }
}
