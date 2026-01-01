using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.GameSessions;

/// <summary>
/// Use case for retrieving in-progress game sessions for an account
/// </summary>
public class GetInProgressSessionsUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly ILogger<GetInProgressSessionsUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInProgressSessionsUseCase"/> class.
    /// </summary>
    /// <param name="repository">The game session repository.</param>
    /// <param name="logger">The logger instance.</param>
    public GetInProgressSessionsUseCase(
        IGameSessionRepository repository,
        ILogger<GetInProgressSessionsUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all in-progress game sessions for the specified account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>A list of in-progress game sessions.</returns>
    public async Task<List<GameSession>> ExecuteAsync(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));
        }

        var sessions = await _repository.GetInProgressSessionsAsync(accountId);
        var sessionList = sessions.ToList();

        _logger.LogInformation("Retrieved {Count} in-progress game sessions for account {AccountId}", sessionList.Count, accountId);

        return sessionList;
    }
}

