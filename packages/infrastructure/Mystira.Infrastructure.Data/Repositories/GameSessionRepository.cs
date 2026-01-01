using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for GameSession entity with domain-specific queries
/// </summary>
public class GameSessionRepository : Repository<GameSession>, IGameSessionRepository
{
    public GameSessionRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GameSession>> GetByAccountIdAsync(string accountId)
    {
        return await _dbSet
            .Where(s => s.AccountId == accountId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameSession>> GetByProfileIdAsync(string profileId)
    {
        return await _dbSet
            .Where(s => s.ProfileId == profileId || s.PlayerNames.Contains(profileId))
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all in-progress or paused game sessions for a specific account.
    /// </summary>
    /// <param name="accountId">The account identifier to filter sessions by.</param>
    /// <returns>A collection of active game sessions for the specified account.</returns>
    /// <exception cref="ArgumentException">Thrown when accountId is null or empty.</exception>
    public async Task<IEnumerable<GameSession>> GetInProgressSessionsAsync(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID cannot be null or empty.", nameof(accountId));

        return await _dbSet
            .Where(s => s.AccountId == accountId &&
                       (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused))
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<GameSession?> GetActiveSessionForScenarioAsync(string accountId, string scenarioId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.AccountId == accountId &&
                                      s.ScenarioId == scenarioId &&
                                      (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused));
    }

    public async Task<IEnumerable<GameSession>> GetActiveSessionsByScenarioAndAccountAsync(string scenarioId, string accountId)
    {
        return await _dbSet
            .Where(s => s.ScenarioId == scenarioId &&
                       s.AccountId == accountId &&
                       (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused))
            .ToListAsync();
    }

    public async Task<int> GetActiveSessionsCountAsync()
    {
        return await _dbSet
            .CountAsync(s => s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused);
    }
}

