namespace Mystira.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Query to get badge statistics for a user profile.
/// Returns count of badges per axis/category.
/// </summary>
/// <param name="UserProfileId">The unique identifier of the user profile.</param>
public record GetBadgeStatisticsQuery(string UserProfileId)
    : IQuery<Dictionary<string, int>>;
