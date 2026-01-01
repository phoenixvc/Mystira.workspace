namespace Mystira.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Query to check if a user has earned a specific badge.
/// </summary>
/// <param name="UserProfileId">The unique identifier of the user profile.</param>
/// <param name="BadgeConfigurationId">The unique identifier of the badge configuration to check.</param>
public record HasUserEarnedBadgeQuery(string UserProfileId, string BadgeConfigurationId)
    : IQuery<bool>;
