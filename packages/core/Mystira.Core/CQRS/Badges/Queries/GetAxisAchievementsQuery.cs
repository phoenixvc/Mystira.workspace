using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Core.CQRS.Badges.Queries;

public sealed record GetAxisAchievementsQuery(string AgeGroupId) : IQuery<List<AxisAchievementResponse>>;
