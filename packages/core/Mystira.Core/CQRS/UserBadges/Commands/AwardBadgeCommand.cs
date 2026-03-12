using Mystira.Contracts.App.Requests.Badges;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.UserBadges.Commands;

public record AwardBadgeCommand(AwardBadgeRequest Request) : ICommand<UserBadge>;
