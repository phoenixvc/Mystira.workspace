using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for GameSession entity with domain-specific queries
/// </summary>
public interface IGameSessionRepository : IRepository<GameSession>
{
    /// <summary>
    /// Gets all game sessions for a specific account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>A collection of game sessions for the specified account.</returns>
    Task<IEnumerable<GameSession>> GetByAccountIdAsync(string accountId);

    /// <summary>
    /// Gets all game sessions for a specific profile.
    /// </summary>
    /// <param name="profileId">The profile identifier.</param>
    /// <returns>A collection of game sessions for the specified profile.</returns>
    Task<IEnumerable<GameSession>> GetByProfileIdAsync(string profileId);

    /// <summary>
    /// Gets all in-progress game sessions for a specific account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>A collection of in-progress game sessions for the specified account.</returns>
    Task<IEnumerable<GameSession>> GetInProgressSessionsAsync(string accountId);

    /// <summary>
    /// Gets the active game session for a specific account and scenario combination.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="scenarioId">The scenario identifier.</param>
    /// <returns>The active game session if found; otherwise, null.</returns>
    Task<GameSession?> GetActiveSessionForScenarioAsync(string accountId, string scenarioId);

    /// <summary>
    /// Gets all active game sessions for a specific scenario and account combination.
    /// </summary>
    /// <param name="scenarioId">The scenario identifier.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>A collection of active game sessions.</returns>
    Task<IEnumerable<GameSession>> GetActiveSessionsByScenarioAndAccountAsync(string scenarioId, string accountId);

    /// <summary>
    /// Gets the total count of active game sessions.
    /// </summary>
    /// <returns>The number of active game sessions.</returns>
    Task<int> GetActiveSessionsCountAsync();
}

