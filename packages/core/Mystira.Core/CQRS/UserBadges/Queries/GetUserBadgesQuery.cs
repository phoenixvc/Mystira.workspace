using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.UserBadges.Queries;

public record GetUserBadgesQuery(string UserProfileId) : IQuery<List<UserBadge>>;
