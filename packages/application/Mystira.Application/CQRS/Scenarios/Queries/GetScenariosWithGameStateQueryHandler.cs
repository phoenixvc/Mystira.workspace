using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Scenarios;
using Mystira.Domain.Enums;

namespace Mystira.Application.CQRS.Scenarios.Queries;

/// <summary>
/// Wolverine handler for GetScenariosWithGameStateQuery.
/// Retrieves scenarios with associated game session state.
/// Combines scenario data with player's game session progress.
/// </summary>
public static class GetScenariosWithGameStateQueryHandler
{
    /// <summary>
    /// Handles the GetScenariosWithGameStateQuery by retrieving scenarios with game state from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<ScenarioGameStateResponse> Handle(
        GetScenariosWithGameStateQuery query,
        IScenarioRepository scenarioRepository,
        IGameSessionRepository gameSessionRepository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Retrieving scenarios with game state for account: {AccountId}",
            query.AccountId);

        // 1. Get all active scenarios using direct LINQ query
        var scenarios = await scenarioRepository.GetQueryable()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Title)
            .ToListAsync(ct);

        // 2. Get all game sessions for this account
        var gameSessions = await gameSessionRepository.GetByAccountIdAsync(query.AccountId);

        // 3. Build response with game state
        var scenariosWithState = scenarios.Select(scenario =>
        {
            var sessions = gameSessions
                .Where(gs => gs.ScenarioId == scenario.Id)
                .OrderByDescending(gs => gs.StartTime)
                .ToList();

            var lastSession = sessions.FirstOrDefault();

            // Only treat sessions that are currently active as "InProgress".
            // This avoids showing scenarios as in-progress when all sessions are Completed/Abandoned.
            var hasActiveSession = sessions.Any(gs =>
                gs.Status == SessionStatus.InProgress
                || gs.Status == SessionStatus.Paused);

            var hasCompletedSession = sessions.Any(gs => gs.Status == SessionStatus.Completed);

            var gameState = hasActiveSession
                ? Mystira.Domain.Enums.ScenarioGameState.InProgress
                : hasCompletedSession
                    ? Mystira.Domain.Enums.ScenarioGameState.Completed
                    : Mystira.Domain.Enums.ScenarioGameState.NotStarted;

            return new ScenarioWithGameState
            {
                ScenarioId = scenario.Id,
                Title = scenario.Title,
                Description = scenario.Description,
                AgeGroup = scenario.AgeGroupId,
                Difficulty = scenario.Difficulty.ToString(),
                SessionLength = scenario.SessionLength.ToString(),
                CoreAxes = scenario.CoreAxes ?? new List<string>(),
                Tags = scenario.Tags?.ToList() ?? new List<string>(),
                Archetypes = scenario.Archetypes ?? new List<string>(),
                GameState = gameState.ToString(),
                LastPlayedAt = lastSession?.StartTime,
                PlayCount = sessions.Count,
                Image = scenario.Image
            };
        }).ToList();

        var response = new ScenarioGameStateResponse
        {
            Scenarios = scenariosWithState,
            TotalCount = scenarios.Count
        };

        logger.LogInformation(
            "Retrieved {Total} scenarios for account {AccountId}",
            response.TotalCount,
            query.AccountId);

        return response;
    }
}
