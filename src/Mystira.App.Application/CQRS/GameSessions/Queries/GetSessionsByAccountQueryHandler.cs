using Microsoft.Extensions.Logging;
using Mystira.App.Application.Mappers;
using Mystira.App.Application.Ports.Data;
using Mystira.Contracts.App.Responses.GameSessions;

namespace Mystira.App.Application.CQRS.GameSessions.Queries;

/// <summary>
/// Wolverine handler for GetSessionsByAccountQuery.
/// Retrieves all sessions for a specific account.
/// </summary>
public static class GetSessionsByAccountQueryHandler
{
    public static async Task<List<GameSessionResponse>> Handle(
        GetSessionsByAccountQuery request,
        IGameSessionRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        Guard.AgainstNullOrEmpty(request.AccountId, nameof(request.AccountId));

        var sessions = await repository.GetByAccountIdAsync(request.AccountId, ct);
        foreach (var s in sessions) { s.RecalculateCompassProgressFromHistory(); }
        var response = GameSessionMapper.ToResponseList(sessions);

        logger.LogDebug("Retrieved {Count} sessions for account", response.Count);
        return response;
    }
}
