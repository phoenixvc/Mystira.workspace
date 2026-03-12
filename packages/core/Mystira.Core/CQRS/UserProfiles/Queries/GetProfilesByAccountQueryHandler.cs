using Microsoft.Extensions.Logging;
using Mystira.Core.Helpers;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;

namespace Mystira.Core.CQRS.UserProfiles.Queries;

/// <summary>
/// Wolverine handler for GetProfilesByAccountQuery.
/// Retrieves all profiles associated with a specific account.
/// </summary>
public static class GetProfilesByAccountQueryHandler
{
    /// <summary>
    /// Handles the GetProfilesByAccountQuery by retrieving profiles for an account.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<List<UserProfile>> Handle(
        GetProfilesByAccountQuery request,
        IUserProfileRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.AccountId))
        {
            throw new ValidationException("accountId", "AccountId is required");
        }

        var profiles = await repository.GetByAccountIdAsync(request.AccountId, ct);

        logger.LogDebug("Retrieved {Count} profiles for account {AccountId}",
            profiles.Count(), LogAnonymizer.HashId(request.AccountId));

        return profiles.ToList();
    }
}
