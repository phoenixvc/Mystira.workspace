using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.App.Domain.Exceptions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for resuming a paused game session
/// </summary>
public class ResumeGameSessionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResumeGameSessionUseCase> _logger;

    public ResumeGameSessionUseCase(
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ResumeGameSessionUseCase> logger)
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

        if (session.Status != SessionStatus.Paused)
        {
            throw new BusinessRuleException("SessionMustBePaused", $"Can only resume paused sessions. Current status: {session.Status}");
        }

        session.Status = SessionStatus.InProgress;
        session.IsPaused = false;
        session.PausedAt = null;

        await _repository.UpdateAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Resumed game session: {SessionId}", sessionId);
        return session;
    }
}

