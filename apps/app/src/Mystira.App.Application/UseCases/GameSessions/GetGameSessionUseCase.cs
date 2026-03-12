using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for retrieving a game session by ID
/// </summary>
public class GetGameSessionUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly ILogger<GetGameSessionUseCase> _logger;

    public GetGameSessionUseCase(
        IGameSessionRepository repository,
        ILogger<GetGameSessionUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<GameSession?> ExecuteAsync(string sessionId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ValidationException("sessionId", "sessionId is required");
        }

        var session = await _repository.GetByIdAsync(sessionId, ct);

        if (session == null)
        {
            _logger.LogWarning("Game session not found: {SessionId}", sessionId);
        }
        else
        {
            _logger.LogDebug("Retrieved game session: {SessionId}", sessionId);
        }

        return session;
    }
}

