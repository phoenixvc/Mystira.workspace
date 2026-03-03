using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.GameSessions;

/// <summary>
/// Use case for ending a game session manually
/// </summary>
public class EndGameSessionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EndGameSessionUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndGameSessionUseCase"/> class.
    /// </summary>
    /// <param name="repository">The game session repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public EndGameSessionUseCase(
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<EndGameSessionUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Ends a game session manually.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The ended game session.</returns>
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

        await _repository.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ended game session: {SessionId}", sessionId);
        return session;
    }
}

