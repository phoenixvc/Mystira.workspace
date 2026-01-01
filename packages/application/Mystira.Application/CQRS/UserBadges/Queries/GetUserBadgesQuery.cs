using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Query to retrieve all badges earned by a user profile.
/// </summary>
/// <param name="UserProfileId">The unique identifier of the user profile.</param>
public record GetUserBadgesQuery(string UserProfileId) : IQuery<List<UserBadge>>;
