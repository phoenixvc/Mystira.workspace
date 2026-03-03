using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for ending a game session manually
/// </summary>
public class EndGameSessionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EndGameSessionUseCase> _logger;

    public EndGameSessionUseCase(
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<EndGameSessionUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GameSession> ExecuteAsync(string sessionId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ValidationException("sessionId", "sessionId is required");
        }

        var session = await _repository.GetByIdAsync(sessionId, ct);
        if (session == null)
        {
            throw new NotFoundException("GameSession", sessionId);
        }

        if (session.Status == SessionStatus.Completed)
        {
            _logger.LogWarning("Game session {SessionId} is already completed", sessionId);
            return session;
        }

        session.Status = SessionStatus.Completed;
        session.EndTime = DateTime.UtcNow;
        session.ElapsedTime = session.EndTime.Value - session.StartTime;
        session.IsPaused = false;
        session.PausedAt = null;

        await _repository.UpdateAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Ended game session: {SessionId}", sessionId);
        return session;
    }
}

