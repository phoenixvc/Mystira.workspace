using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Application.CQRS.Badges.Queries;

/// <summary>
/// Query to retrieve detailed information about a specific badge.
/// </summary>
/// <param name="BadgeId">The unique identifier of the badge.</param>
public sealed record GetBadgeDetailQuery(string BadgeId) : IQuery<BadgeResponse?>;
