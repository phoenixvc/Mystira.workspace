using Mystira.Contracts.App.Requests.Badges;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserBadges.Commands;

/// <summary>
/// Command to award a badge to a user profile.
/// </summary>
/// <param name="Request">The request containing the badge award details.</param>
public record AwardBadgeCommand(AwardBadgeRequest Request) : ICommand<UserBadge>;
