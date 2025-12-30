using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Application.CQRS.Badges.Queries;

public sealed record GetProfileBadgeProgressQuery(string ProfileId) : IQuery<BadgeProgressResponse?>;
