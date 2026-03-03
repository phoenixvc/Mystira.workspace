using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.UseCases.GameSessions;

/// <summary>
/// Use case for deleting a game session
/// </summary>
public class DeleteGameSessionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteGameSessionUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteGameSessionUseCase"/> class.
    /// </summary>
    /// <param name="repository">The game session repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public DeleteGameSessionUseCase(
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteGameSessionUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Deletes a game session by its identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>True if the session was deleted; false if not found.</returns>
    public async Task<bool> ExecuteAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
        }

        var session = await _repository.GetByIdAsync(sessionId);
        if (session == null)
        {
            _logger.LogWarning("Game session not found for deletion: {SessionId}", sessionId);
            return false;
        }

        await _repository.DeleteAsync(sessionId);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted game session: {SessionId}", sessionId);
        return true;
    }
}

