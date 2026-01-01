using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Specifications;
using Mystira.Contracts.App.Models.GameSessions;
using Mystira.Contracts.App.Responses.GameSessions;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Wolverine handler for GetSessionsByProfileQuery.
/// Retrieves all sessions for a specific user profile.
/// Uses static method convention for cleaner, more testable code.
/// </summary>
public static class GetSessionsByProfileQueryHandler
{
    /// <summary>
    /// Handles the GetSessionsByProfileQuery.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<List<GameSessionResponse>> Handle(
        GetSessionsByProfileQuery request,
        IGameSessionRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.ProfileId))
        {
            throw new ArgumentException("ProfileId is required");
        }

        var spec = new SessionsByProfileSpec(request.ProfileId);
        var sessions = await repository.ListAsync(spec);

        var response = sessions.Select(s =>
        {
            s.RecalculateCompassProgressFromHistory();

            return new GameSessionResponse
            {
                Id = s.Id,
                ScenarioId = s.ScenarioId,
                AccountId = s.AccountId,
                ProfileId = s.ProfileId,
                PlayerNames = s.PlayerNames,
                // PlayerCompassProgressTotals is Dictionary<string, int> where key is axis, value is total
                PlayerCompassProgressTotals = s.PlayerCompassProgressTotals.Select(kvp => new PlayerCompassProgressDto
                {
                    PlayerId = string.Empty,
                    Axis = kvp.Key,
                    Total = kvp.Value
                }).ToList(),
                Status = s.Status.ToString(),
                CurrentSceneId = s.CurrentSceneId ?? string.Empty,
                ChoiceCount = s.ChoiceHistory?.Count ?? 0,
                EchoCount = s.EchoHistory?.Count ?? 0,
                AchievementCount = s.Achievements?.Count ?? 0,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                ElapsedTime = s.GetTotalElapsedTime(),
                IsPaused = s.Status == SessionStatus.Paused,
                SceneCount = s.ChoiceHistory?.Select(c => c.SceneId).Distinct().Count() ?? 0,
                TargetAgeGroup = s.TargetAgeGroupName ?? string.Empty
            };
        }).ToList();

        logger.LogDebug("Retrieved {Count} sessions for profile {ProfileId}", response.Count, request.ProfileId);

        return response;
    }
}
