using Microsoft.EntityFrameworkCore;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for GameSession entity with domain-specific queries
/// </summary>
public class GameSessionRepository : Repository<GameSession>, IGameSessionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameSessionRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GameSessionRepository(DbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GameSession>> GetByAccountIdAsync(string accountId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.AccountId == accountId)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GameSession>> GetByProfileIdAsync(string profileId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.ProfileId == profileId || s.PlayerNames.Contains(profileId))
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GameSession>> GetInProgressSessionsAsync(string accountId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID cannot be null or empty.", nameof(accountId));

        return await DbSet
            .Where(s => s.AccountId == accountId &&
                       (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused))
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<GameSession?> GetActiveSessionForScenarioAsync(string accountId, string scenarioId, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.AccountId == accountId &&
                                      s.ScenarioId == scenarioId &&
                                      (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused), ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GameSession>> GetActiveSessionsByScenarioAndAccountAsync(string scenarioId, string accountId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.ScenarioId == scenarioId &&
                       s.AccountId == accountId &&
                       (s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<int> GetActiveSessionsCountAsync(CancellationToken ct = default)
    {
        return await DbSet
            .CountAsync(s => s.Status == SessionStatus.InProgress || s.Status == SessionStatus.Paused, ct);
    }
}
