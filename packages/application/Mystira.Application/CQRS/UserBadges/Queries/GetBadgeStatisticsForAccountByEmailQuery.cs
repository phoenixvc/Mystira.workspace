namespace Mystira.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Query to get badge statistics for all profiles in an account (by email).
/// Orchestrates account lookup and statistics aggregation.
/// </summary>
/// <param name="Email">The email address of the account.</param>
public record GetBadgeStatisticsForAccountByEmailQuery(string Email)
    : IQuery<Dictionary<string, int>>;
