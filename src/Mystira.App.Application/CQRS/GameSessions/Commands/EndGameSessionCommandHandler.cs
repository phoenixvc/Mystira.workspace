using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Wolverine handler for EndGameSessionCommand.
/// Marks a session as completed using the domain method.
/// </summary>
public static class EndGameSessionCommandHandler
{
    public static async Task<GameSession?> Handle(
        EndGameSessionCommand command,
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var session = await repository.GetByIdAsync(command.SessionId);
        if (session == null)
        {
            logger.LogWarning("Session not found: {SessionId}", command.SessionId);
            return null;
        }

        if (!session.Complete())
        {
            logger.LogWarning("Cannot complete session {SessionId} - invalid status for completion. Current status: {Status}",
                command.SessionId, session.Status);
            return null;
        }

        await repository.UpdateAsync(session);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Ended game session {SessionId}", session.Id);
        return session;
    }
}
