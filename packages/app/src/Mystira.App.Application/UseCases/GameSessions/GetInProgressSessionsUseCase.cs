using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
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
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));
        }

        var sessions = await _repository.GetInProgressSessionsAsync(accountId, ct);
        var sessionList = sessions.ToList();

        _logger.LogInformation("Retrieved {Count} in-progress game sessions for account {AccountId}", sessionList.Count, PiiMask.HashId(accountId));

        return sessionList;
    }
}

