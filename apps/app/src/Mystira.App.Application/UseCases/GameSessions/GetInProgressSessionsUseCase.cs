using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.GameSessions;

/// <summary>
/// Use case for retrieving in-progress game sessions for an account
/// </summary>
public class GetInProgressSessionsUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly ILogger<GetInProgressSessionsUseCase> _logger;

    public GetInProgressSessionsUseCase(
        IGameSessionRepository repository,
        ILogger<GetInProgressSessionsUseCase> logger)
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

        var sessions = await _repository.GetInProgressSessionsAsync(accountId, ct);
        var sessionList = sessions.ToList();

        _logger.LogInformation("Retrieved {Count} in-progress game sessions for account {AccountId}", sessionList.Count, PiiMask.HashId(accountId));

        return sessionList;
    }
}

