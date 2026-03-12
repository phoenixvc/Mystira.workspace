using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.UserBadges.Queries;

/// <summary>
/// Query to get all badges for a user profile on a specific compass axis.
/// </summary>
public record GetUserBadgesForAxisQuery(string UserProfileId, string Axis)
    : IQuery<List<UserBadge>>;
