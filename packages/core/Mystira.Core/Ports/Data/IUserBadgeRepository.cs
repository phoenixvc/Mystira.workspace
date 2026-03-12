using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository interface for UserBadge entity with domain-specific queries
/// </summary>
public interface IUserBadgeRepository : IRepository<UserBadge>
{
    /// <summary>
    /// Gets all badges for a specific user profile.
    /// </summary>
    Task<IEnumerable<UserBadge>> GetByUserProfileIdAsync(string userProfileId, CancellationToken ct = default);

    /// <summary>
    /// Gets a specific badge for a user profile and badge configuration combination.
    /// </summary>
    Task<UserBadge?> GetByUserProfileIdAndBadgeConfigIdAsync(string userProfileId, string badgeConfigurationId, CancellationToken ct = default);

    /// <summary>
    /// Gets all badges earned during a specific game session.
    /// </summary>
    Task<IEnumerable<UserBadge>> GetByGameSessionIdAsync(string gameSessionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all badges earned in a specific scenario.
    /// </summary>
    Task<IEnumerable<UserBadge>> GetByScenarioIdAsync(string scenarioId, CancellationToken ct = default);

    /// <summary>
    /// Gets all badges for a specific user profile and axis combination.
    /// </summary>
    Task<IEnumerable<UserBadge>> GetByUserProfileIdAndAxisAsync(string userProfileId, string axis, CancellationToken ct = default);
}
