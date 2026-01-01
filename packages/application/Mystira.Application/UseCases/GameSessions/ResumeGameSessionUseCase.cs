using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.GameSessions;

/// <summary>
/// Use case for resuming a paused game session
/// </summary>
public class ResumeGameSessionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResumeGameSessionUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResumeGameSessionUseCase"/> class.
    /// </summary>
    /// <param name="repository">The game session repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public ResumeGameSessionUseCase(
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ResumeGameSessionUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Resumes a paused game session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The resumed game session.</returns>
    public async Task<GameSession> ExecuteAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
        }

        var session = await _repository.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Game session not found: {sessionId}", nameof(sessionId));
        }

        if (session.Status != SessionStatus.Paused)
        {
            throw new InvalidOperationException($"Can only resume paused sessions. Current status: {session.Status}");
        }

        session.Status = SessionStatus.InProgress;
        session.IsPaused = false;
        session.PausedAt = null;

        await _repository.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Resumed game session: {SessionId}", sessionId);
        return session;
    }
}

