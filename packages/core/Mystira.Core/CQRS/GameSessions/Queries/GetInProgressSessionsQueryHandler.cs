using Microsoft.Extensions.Logging;
using Mystira.Core.Helpers;
using Mystira.Core.Mappers;
using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Responses.GameSessions;

namespace Mystira.Core.CQRS.GameSessions.Queries;

/// <summary>
/// Wolverine handler for GetInProgressSessionsQuery.
/// Retrieves sessions that are currently in progress or paused.
/// Includes zombie session filtering and deduplication.
/// </summary>
public static class GetInProgressSessionsQueryHandler
{
    public static async Task<List<GameSessionResponse>> Handle(
        GetInProgressSessionsQuery request,
        IGameSessionRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        Guard.AgainstNullOrEmpty(request.AccountId, nameof(request.AccountId));

        var sessions = await repository.GetInProgressSessionsAsync(request.AccountId, ct);

        // Defensive: if historical data contains duplicates, only return the most recent active
        // session per (ScenarioId, ProfileId) pair.
        var ordered = sessions
            .OrderByDescending(s => s.StartTime)
            .ToList();

        // Filter out "zombie" sessions: active status but with no starting scene and no history.
        var meaningfulSessions = ordered
            .Where(s => !string.IsNullOrWhiteSpace(s.CurrentSceneId) || s.ChoiceHistory.Count > 0)
            .ToList();

        if (meaningfulSessions.Count != ordered.Count)
        {
            logger.LogWarning(
                "Filtered empty in-progress sessions for account {AccountId}: {OriginalCount} -> {FilteredCount}",
                LogAnonymizer.HashId(request.AccountId),
                ordered.Count,
                meaningfulSessions.Count);
        }

        var uniqueSessions = meaningfulSessions
            .GroupBy(s => $"{s.ScenarioId}::{s.ProfileId}".ToLowerInvariant())
            .Select(g => g.First())
            .ToList();

        if (uniqueSessions.Count != meaningfulSessions.Count)
        {
            logger.LogWarning(
                "Deduplicated in-progress sessions for account {AccountId}: {OriginalCount} -> {UniqueCount}",
                LogAnonymizer.HashId(request.AccountId),
                meaningfulSessions.Count,
                uniqueSessions.Count);
        }

        uniqueSessions.ForEach(s => s.RecalculateCompassProgressFromHistory());
        var response = GameSessionMapper.ToResponseList(uniqueSessions);

        logger.LogDebug("Retrieved {Count} in-progress sessions for account {AccountId}",
            response.Count, LogAnonymizer.HashId(request.AccountId));

        return response;
    }
}
