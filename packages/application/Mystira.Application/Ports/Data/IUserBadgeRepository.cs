using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for UserBadge entity with domain-specific queries
/// </summary>
public interface IUserBadgeRepository : IRepository<UserBadge>
{
    /// <summary>
    /// Gets all badges for a specific user profile.
    /// </summary>
    /// <param name="userProfileId">The user profile identifier.</param>
    /// <returns>A collection of user badges for the specified profile.</returns>
    Task<IEnumerable<UserBadge>> GetByUserProfileIdAsync(string userProfileId);

    /// <summary>
    /// Gets a specific badge for a user profile and badge configuration combination.
    /// </summary>
    /// <param name="userProfileId">The user profile identifier.</param>
    /// <param name="badgeConfigurationId">The badge configuration identifier.</param>
    /// <returns>The user badge if found; otherwise, null.</returns>
    Task<UserBadge?> GetByUserProfileIdAndBadgeConfigIdAsync(string userProfileId, string badgeConfigurationId);

    /// <summary>
    /// Gets all badges earned during a specific game session.
    /// </summary>
    /// <param name="gameSessionId">The game session identifier.</param>
    /// <returns>A collection of user badges earned in the specified session.</returns>
    Task<IEnumerable<UserBadge>> GetByGameSessionIdAsync(string gameSessionId);

    /// <summary>
    /// Gets all badges earned in a specific scenario.
    /// </summary>
    /// <param name="scenarioId">The scenario identifier.</param>
    /// <returns>A collection of user badges earned in the specified scenario.</returns>
    Task<IEnumerable<UserBadge>> GetByScenarioIdAsync(string scenarioId);

    /// <summary>
    /// Gets all badges for a specific user profile and axis combination.
    /// </summary>
    /// <param name="userProfileId">The user profile identifier.</param>
    /// <param name="axis">The badge axis.</param>
    /// <returns>A collection of user badges for the specified profile and axis.</returns>
    Task<IEnumerable<UserBadge>> GetByUserProfileIdAndAxisAsync(string userProfileId, string axis);
}

