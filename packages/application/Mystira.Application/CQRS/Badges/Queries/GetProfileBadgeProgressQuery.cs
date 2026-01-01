using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Application.CQRS.Badges.Queries;

/// <summary>
/// Query to retrieve badge progress information for a specific profile.
/// </summary>
/// <param name="ProfileId">The unique identifier of the user profile.</param>
public sealed record GetProfileBadgeProgressQuery(string ProfileId) : IQuery<BadgeProgressResponse?>;
