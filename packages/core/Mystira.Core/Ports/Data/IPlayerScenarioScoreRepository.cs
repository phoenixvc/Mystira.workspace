using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository interface for PlayerScenarioScore entity
/// Tracks scored scenarios to ensure each profile/scenario pair is scored only once
/// </summary>
public interface IPlayerScenarioScoreRepository : IRepository<PlayerScenarioScore>
{
    /// <summary>
    /// Get score for a specific profile/scenario pair
    /// </summary>
    Task<PlayerScenarioScore?> GetByProfileAndScenarioAsync(string profileId, string scenarioId, CancellationToken ct = default);

    /// <summary>
    /// Get all scenario scores for a profile
    /// </summary>
    Task<IEnumerable<PlayerScenarioScore>> GetByProfileIdAsync(string profileId, CancellationToken ct = default);

    /// <summary>
    /// Check if a profile has already been scored for a scenario
    /// </summary>
    Task<bool> IsScenarioScoredAsync(string profileId, string scenarioId, CancellationToken ct = default);
}
