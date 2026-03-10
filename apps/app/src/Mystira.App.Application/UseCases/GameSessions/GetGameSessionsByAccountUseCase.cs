using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for retrieving game sessions by account ID
/// </summary>
public class GetGameSessionsByAccountUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly ILogger<GetGameSessionsByAccountUseCase> _logger;

    public GetGameSessionsByAccountUseCase(
        IGameSessionRepository repository,
        ILogger<GetGameSessionsByAccountUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<GameSession>> ExecuteAsync(string accountId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ValidationException("accountId", "accountId is required");
        }

        var sessions = await _repository.GetByAccountIdAsync(accountId, ct);
        var sessionList = sessions.ToList();

        _logger.LogInformation("Retrieved {Count} game sessions for account {AccountId}", sessionList.Count, PiiMask.HashId(accountId));

        return sessionList;
    }
}

