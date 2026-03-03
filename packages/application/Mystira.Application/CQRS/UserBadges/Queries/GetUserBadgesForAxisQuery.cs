using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Query to get all badges for a user profile on a specific compass axis.
/// </summary>
/// <param name="UserProfileId">The unique identifier of the user profile.</param>
/// <param name="Axis">The compass axis to filter badges by.</param>
public record GetUserBadgesForAxisQuery(string UserProfileId, string Axis)
    : IQuery<List<UserBadge>>;
