using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Wolverine handler for ResumeGameSessionCommand.
/// Resumes a paused game session using the domain method.
/// </summary>
public static class ResumeGameSessionCommandHandler
{
    public static async Task<GameSession?> Handle(
        ResumeGameSessionCommand command,
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

        if (!session.Resume())
        {
            logger.LogWarning("Cannot resume session {SessionId} - not paused. Current status: {Status}",
                command.SessionId, session.Status);
            return null;
        }

        await repository.UpdateAsync(session);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Resumed game session {SessionId}", session.Id);
        return session;
    }
}
