using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.App.Domain.Exceptions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for pausing an active game session
/// </summary>
public class PauseGameSessionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PauseGameSessionUseCase> _logger;

    public PauseGameSessionUseCase(
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<PauseGameSessionUseCase> logger)
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

        if (session.Status != SessionStatus.InProgress)
        {
            throw new BusinessRuleException("SessionMustBeInProgress", $"Can only pause sessions in progress. Current status: {session.Status}");
        }

        session.Status = SessionStatus.Paused;
        session.IsPaused = true;
        session.PausedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Paused game session: {SessionId}", sessionId);
        return session;
    }
}

