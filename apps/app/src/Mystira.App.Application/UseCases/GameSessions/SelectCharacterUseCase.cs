using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for assigning a character to a game session
/// </summary>
public class SelectCharacterUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SelectCharacterUseCase> _logger;

    public SelectCharacterUseCase(
        IGameSessionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<SelectCharacterUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GameSession> ExecuteAsync(string sessionId, string characterId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ValidationException("sessionId", "sessionId is required");
        }

        if (string.IsNullOrWhiteSpace(characterId))
        {
            throw new ValidationException("characterId", "characterId is required");
        }

        var session = await _repository.GetByIdAsync(sessionId, ct);
        if (session == null)
        {
            throw new NotFoundException("GameSession", sessionId);
        }

        session.SelectedCharacterId = characterId;
        await _repository.UpdateAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Selected character {CharacterId} for game session {SessionId}", characterId, sessionId);
        return session;
    }
}

