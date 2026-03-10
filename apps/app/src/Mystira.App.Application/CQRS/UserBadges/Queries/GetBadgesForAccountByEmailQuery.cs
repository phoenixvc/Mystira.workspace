using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Query to get all badges for all profiles belonging to an account (by email).
/// Orchestrates account lookup and badge aggregation.
/// </summary>
public record GetBadgesForAccountByEmailQuery(string Email) : IQuery<List<UserBadge>>;
