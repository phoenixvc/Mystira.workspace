using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Core.CQRS.Badges.Queries;

public sealed record GetBadgeDetailQuery(string BadgeId) : IQuery<BadgeResponse?>;
