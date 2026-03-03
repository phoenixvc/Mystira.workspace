using Mystira.Contracts.App.Responses.Scenarios;

namespace Mystira.Application.CQRS.Scenarios.Queries;

/// <summary>
/// Query to retrieve scenarios with game state for a specific account.
/// Returns scenarios along with associated game session progress.
/// Not cached as game state changes frequently.
/// </summary>
/// <param name="AccountId">The unique identifier of the account to retrieve scenarios for.</param>
public record GetScenariosWithGameStateQuery(string AccountId)
    : IQuery<ScenarioGameStateResponse>;
