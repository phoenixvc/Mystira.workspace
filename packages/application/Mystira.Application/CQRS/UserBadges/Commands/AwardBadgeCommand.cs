using Mystira.Contracts.App.Requests.Badges;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserBadges.Commands;

public record AwardBadgeCommand(AwardBadgeRequest Request) : ICommand<UserBadge>;
