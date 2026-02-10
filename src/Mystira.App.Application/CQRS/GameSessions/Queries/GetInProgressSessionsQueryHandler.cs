using Microsoft.Extensions.Logging;
using Mystira.App.Application.Mappers;
using Mystira.App.Application.Ports.Data;
using Mystira.Contracts.App.Responses.GameSessions;

namespace Mystira.App.Application.CQRS.GameSessions.Queries;

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

        var sessions = await repository.GetInProgressSessionsAsync(request.AccountId);

        // Defensive: if historical data contains duplicates, only return the most recent active
        // session per (ScenarioId, ProfileId) pair.
        var ordered = sessions
            .OrderByDescending(s => s.StartTime)
            .ToList();

        // Filter out "zombie" sessions: active status but with no starting scene and no history.
        var meaningfulSessions = ordered
            .Where(s => !s.IsEffectivelyEmpty())
            .ToList();

        if (meaningfulSessions.Count != ordered.Count)
        {
            logger.LogWarning(
                "Filtered empty in-progress sessions for account {AccountIdPrefix}: {OriginalCount} -> {FilteredCount}",
                request.AccountId[..Math.Min(8, request.AccountId.Length)] + "...",
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
                "Deduplicated in-progress sessions for account {AccountIdPrefix}: {OriginalCount} -> {UniqueCount}",
                request.AccountId[..Math.Min(8, request.AccountId.Length)] + "...",
                meaningfulSessions.Count,
                uniqueSessions.Count);
        }

        uniqueSessions.ForEach(s => s.RecalculateCompassProgressFromHistory());
        var response = GameSessionMapper.ToResponseList(uniqueSessions);

        logger.LogDebug("Retrieved {Count} in-progress sessions for account {AccountIdPrefix}",
            response.Count, request.AccountId[..Math.Min(8, request.AccountId.Length)] + "...");

        return response;
    }
}
