using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.App.Application.Mappers;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.GameSessions;

namespace Mystira.App.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Wolverine handler for GetSessionsByProfileQuery.
/// Retrieves all sessions for a specific user profile.
/// </summary>
public static class GetSessionsByProfileQueryHandler
{
    public static async Task<List<GameSessionResponse>> Handle(
        GetSessionsByProfileQuery request,
        IGameSessionRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        Guard.AgainstNullOrEmpty(request.ProfileId, nameof(request.ProfileId));

        var sessions = await repository.GetByProfileIdAsync(request.ProfileId, ct);
        foreach (var s in sessions) { s.RecalculateCompassProgressFromHistory(); }
        var response = GameSessionMapper.ToResponseList(sessions);

        logger.LogDebug("Retrieved {Count} sessions for profile {ProfileId}", response.Count, LogAnonymizer.HashId(request.ProfileId));
        return response;
    }
}
