using Microsoft.Extensions.Logging;
using Mystira.App.Application.UseCases.Accounts;

namespace Mystira.App.Application.CQRS.Accounts.Commands;

/// <summary>
/// Wolverine handler for AddCompletedScenarioCommand.
/// Delegates to AddCompletedScenarioUseCase which owns the business logic for
/// marking a scenario as completed, including list initialization and deduplication.
/// </summary>
public static class AddCompletedScenarioCommandHandler
{
    /// <summary>
    /// Handles the AddCompletedScenarioCommand by delegating to the UseCase.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<bool> Handle(
        AddCompletedScenarioCommand command,
        AddCompletedScenarioUseCase addCompletedScenarioUseCase,
        ILogger logger,
        CancellationToken ct)
    {
        try
        {
            await addCompletedScenarioUseCase.ExecuteAsync(command.AccountId, command.ScenarioId, ct);
            return true;
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Failed to add completed scenario: {Message}", ex.Message);
            return false;
        }
    }
}
