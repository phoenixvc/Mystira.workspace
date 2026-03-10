using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.UserProfiles.Queries;

/// <summary>
/// Wolverine handler for GetUserProfileQuery.
/// Retrieves a single user profile by ID.
/// </summary>
public static class GetUserProfileQueryHandler
{
    /// <summary>
    /// Handles the GetUserProfileQuery by retrieving a profile from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<UserProfile?> Handle(
        GetUserProfileQuery query,
        IUserProfileRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        var profile = await repository.GetByIdAsync(query.ProfileId, ct);

        if (profile == null)
        {
            logger.LogDebug("Profile not found: {ProfileId}", LogAnonymizer.HashId(query.ProfileId));
        }
        else
        {
            logger.LogDebug("Retrieved profile {ProfileId}", LogAnonymizer.HashId(query.ProfileId));
        }

        return profile;
    }
}
