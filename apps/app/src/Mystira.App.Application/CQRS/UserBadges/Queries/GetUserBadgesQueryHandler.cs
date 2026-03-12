using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Wolverine handler for GetUserBadgesQuery.
/// Retrieves all badges for a user profile.
/// </summary>
public static class GetUserBadgesQueryHandler
{
    /// <summary>
    /// Handles the GetUserBadgesQuery by retrieving user badges from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<List<UserBadge>> Handle(
        GetUserBadgesQuery query,
        IUserBadgeRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        var badges = await repository.GetByUserProfileIdAsync(query.UserProfileId, ct);

        logger.LogDebug("Retrieved {Count} badges for user profile {UserProfileId}",
            badges.Count(), LogAnonymizer.HashId(query.UserProfileId));

        return badges.ToList();
    }
}
