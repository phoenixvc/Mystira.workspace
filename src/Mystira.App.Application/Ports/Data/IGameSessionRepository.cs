using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for GameSession entity with domain-specific queries
/// </summary>
public interface IGameSessionRepository : IRepository<GameSession, string>
{
    Task<IEnumerable<GameSession>> GetByAccountIdAsync(string accountId, CancellationToken ct = default);
    Task<IEnumerable<GameSession>> GetByProfileIdAsync(string profileId, CancellationToken ct = default);
    Task<IEnumerable<GameSession>> GetInProgressSessionsAsync(string accountId, CancellationToken ct = default);
    Task<GameSession?> GetActiveSessionForScenarioAsync(string accountId, string scenarioId, CancellationToken ct = default);
    Task<IEnumerable<GameSession>> GetActiveSessionsByScenarioAndAccountAsync(string scenarioId, string accountId, CancellationToken ct = default);
    Task<int> GetActiveSessionsCountAsync(CancellationToken ct = default);
}

