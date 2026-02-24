using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Wolverine handler for PauseGameSessionCommand.
/// Pauses an active game session using the domain method.
/// </summary>
public static class PauseGameSessionCommandHandler
{
    public static async Task<GameSession?> Handle(
        PauseGameSessionCommand command,
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var session = await repository.GetByIdAsync(command.SessionId, ct);
        if (session == null)
        {
            logger.LogWarning("Session not found: {SessionId}", command.SessionId);
            return null;
        }

        if (!session.Pause())
        {
            logger.LogWarning("Cannot pause session {SessionId} - not in progress. Current status: {Status}",
                command.SessionId, session.Status);
            return null;
        }

        await repository.UpdateAsync(session, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Paused game session {SessionId}", session.Id);
        return session;
    }
}
