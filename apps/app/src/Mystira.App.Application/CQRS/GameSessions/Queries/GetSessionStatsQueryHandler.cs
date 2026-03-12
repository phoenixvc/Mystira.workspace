using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Models.GameSessions;
using Mystira.Contracts.App.Responses.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Wolverine handler for GetSessionStatsQuery.
/// Calculates and returns session statistics.
/// Uses static method convention for cleaner, more testable code.
/// </summary>
public static class GetSessionStatsQueryHandler
{
    /// <summary>
    /// Handles the GetSessionStatsQuery.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<SessionStatsResponse?> Handle(
        GetSessionStatsQuery request,
        IGameSessionRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        var session = await repository.GetByIdAsync(request.SessionId, ct);
        if (session == null)
        {
            logger.LogWarning("Session not found: {SessionId}", request.SessionId);
            return null;
        }

        if (session.ChoiceHistory != null)
            session.RecalculateCompassProgressFromHistory();

        var compassValues = session.CompassValues?.ToDictionary(
            cv => cv.Axis,
            cv => cv.CurrentValue
        ) ?? new Dictionary<string, double>();

        var playerProgress = session.PlayerCompassProgressTotals
            .Select(p => new PlayerCompassProgressDto
            {
                PlayerId = string.Empty,
                Axis = p.Key,
                Total = p.Value
            })
            .ToList();

        var recentEchoes = session.EchoHistory?
            .TakeLast(10)
            .Cast<object>()
            .ToList() ?? new List<object>();

        var stats = new SessionStatsResponse
        {
            CompassValues = compassValues,
            PlayerCompassProgressTotals = playerProgress,
            RecentEchoes = recentEchoes,
            Achievements = session.Achievements?.Cast<object>().ToList() ?? new List<object>(),
            TotalChoices = session.ChoiceHistory?.Count ?? 0,
            SessionDuration = session.GetTotalElapsedTime()
        };

        logger.LogDebug("Retrieved stats for session {SessionId}", request.SessionId);

        return stats;
    }
}
