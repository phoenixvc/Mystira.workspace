namespace Mystira.Application.CQRS.Accounts.Commands;

/// <summary>
/// Command to add a completed scenario to an account.
/// Tracks which scenarios the account has finished.
/// </summary>
/// <param name="AccountId">The unique identifier of the account.</param>
/// <param name="ScenarioId">The unique identifier of the completed scenario.</param>
public record AddCompletedScenarioCommand(
    string AccountId,
    string ScenarioId
) : ICommand<bool>;
