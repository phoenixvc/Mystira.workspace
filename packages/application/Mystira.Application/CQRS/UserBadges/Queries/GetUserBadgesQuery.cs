using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserBadges.Queries;

public record GetUserBadgesQuery(string UserProfileId) : IQuery<List<UserBadge>>;
