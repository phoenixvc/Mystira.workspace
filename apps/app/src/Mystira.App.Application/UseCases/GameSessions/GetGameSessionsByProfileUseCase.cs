using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for retrieving game sessions by profile ID
/// </summary>
public class GetGameSessionsByProfileUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly ILogger<GetGameSessionsByProfileUseCase> _logger;

    public GetGameSessionsByProfileUseCase(
        IGameSessionRepository repository,
        ILogger<GetGameSessionsByProfileUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<GameSession>> ExecuteAsync(string profileId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ValidationException("profileId", "profileId is required");
        }

        var sessions = await _repository.GetByProfileIdAsync(profileId, ct);
        var sessionList = sessions.ToList();

        _logger.LogInformation("Retrieved {Count} game sessions for profile {ProfileId}", sessionList.Count, PiiMask.HashId(profileId));

        return sessionList;
    }
}

