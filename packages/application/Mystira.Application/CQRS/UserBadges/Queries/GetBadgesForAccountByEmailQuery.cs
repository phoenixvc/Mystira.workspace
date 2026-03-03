using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Query to get all badges for all profiles belonging to an account (by email).
/// Orchestrates account lookup and badge aggregation.
/// </summary>
/// <param name="Email">The email address of the account.</param>
public record GetBadgesForAccountByEmailQuery(string Email) : IQuery<List<UserBadge>>;
