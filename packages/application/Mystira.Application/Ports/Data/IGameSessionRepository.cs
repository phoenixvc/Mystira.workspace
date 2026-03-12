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
    Task<IEnumerable<GameSession>> GetByAccountIdAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Gets all game sessions for a specific profile.
    /// </summary>
    Task<IEnumerable<GameSession>> GetByProfileIdAsync(string profileId, CancellationToken ct = default);

    /// <summary>
    /// Gets all in-progress game sessions for a specific account.
    /// </summary>
    Task<IEnumerable<GameSession>> GetInProgressSessionsAsync(string accountId, CancellationToken ct = default);

    /// <summary>
    /// Gets the active game session for a specific account and scenario combination.
    /// </summary>
    Task<GameSession?> GetActiveSessionForScenarioAsync(string accountId, string scenarioId, CancellationToken ct = default);

    /// <summary>
    /// Gets all active game sessions for a specific scenario and account combination.
    /// </summary>
    Task<IEnumerable<GameSession>> GetActiveSessionsByScenarioAndAccountAsync(string scenarioId, string accountId, CancellationToken ct = default);

    /// <summary>
    /// Gets the total count of active game sessions.
    /// </summary>
    Task<int> GetActiveSessionsCountAsync(CancellationToken ct = default);
}
