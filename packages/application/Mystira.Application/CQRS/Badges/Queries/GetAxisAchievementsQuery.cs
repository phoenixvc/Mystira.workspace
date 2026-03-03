using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Application.CQRS.Badges.Queries;

/// <summary>
/// Query to retrieve compass axis achievements for a specific age group.
/// </summary>
/// <param name="AgeGroupId">The unique identifier of the age group.</param>
public sealed record GetAxisAchievementsQuery(string AgeGroupId) : IQuery<List<AxisAchievementResponse>>;
