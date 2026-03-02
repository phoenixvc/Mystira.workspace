using Microsoft.EntityFrameworkCore;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for GameSession entity with domain-specific queries
/// </summary>
public class GameSessionRepository : Repository<GameSession>, IGameSessionRepository
{
    public GameSessionRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GameSession>> GetByAccountIdAsync(string accountId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(s => s.AccountId == accountId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<GameSession>> GetByProfileIdAsync(string profileId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(s => s.ProfileId == profileId || s.PlayerNames.Contains(profileId))
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<GameSession>> GetInProgressSessionsAsync(string accountId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(s => s.AccountId == accountId &&
                       (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused))
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(ct);
    }

    public async Task<GameSession?> GetActiveSessionForScenarioAsync(string accountId, string scenarioId, CancellationToken ct = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.AccountId == accountId &&
                                      s.ScenarioId == scenarioId &&
                                      (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused), ct);
    }

    public async Task<IEnumerable<GameSession>> GetActiveSessionsByScenarioAndAccountAsync(string scenarioId, string accountId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(s => s.ScenarioId == scenarioId &&
                       s.AccountId == accountId &&
                       (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused))
            .ToListAsync(ct);
    }

    public async Task<int> GetActiveSessionsCountAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .CountAsync(s => s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused, ct);
    }
}
