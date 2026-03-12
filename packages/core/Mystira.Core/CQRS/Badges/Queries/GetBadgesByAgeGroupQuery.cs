using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Core.CQRS.Badges.Queries;

public sealed record GetBadgesByAgeGroupQuery(string AgeGroupId) : IQuery<List<BadgeResponse>>;
