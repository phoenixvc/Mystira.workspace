using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.GameSessions;

/// <summary>
/// Use case for retrieving game sessions by account ID
/// </summary>
public class GetGameSessionsByAccountUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly ILogger<GetGameSessionsByAccountUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGameSessionsByAccountUseCase"/> class.
    /// </summary>
    /// <param name="repository">The game session repository.</param>
    /// <param name="logger">The logger instance.</param>
    public GetGameSessionsByAccountUseCase(
        IGameSessionRepository repository,
        ILogger<GetGameSessionsByAccountUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all game sessions for the specified account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>A list of game sessions for the account.</returns>
    public async Task<List<GameSession>> ExecuteAsync(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));
        }

        var sessions = await _repository.GetByAccountIdAsync(accountId);
        var sessionList = sessions.ToList();

        _logger.LogInformation("Retrieved {Count} game sessions for account {AccountId}", sessionList.Count, accountId);

        return sessionList;
    }
}

