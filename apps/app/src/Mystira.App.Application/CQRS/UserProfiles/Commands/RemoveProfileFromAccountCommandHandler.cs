using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.Core.Ports.Data;

namespace Mystira.App.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Wolverine handler for removing a user profile from its associated account.
/// Coordinates updates to both the profile (clears AccountId) and account (removes profile from list).
/// </summary>
public static class RemoveProfileFromAccountCommandHandler
{
    /// <summary>
    /// Handles the RemoveProfileFromAccountCommand by unlinking a profile from its account.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<bool> Handle(
        RemoveProfileFromAccountCommand command,
        IUserProfileRepository profileRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        // Get the profile
        var profile = await profileRepository.GetByIdAsync(command.ProfileId, ct);
        if (profile == null)
        {
            logger.LogWarning("Cannot remove profile from account: Profile not found {ProfileId}",
                LogAnonymizer.HashId(command.ProfileId));
            return false;
        }

        // Check if profile is linked to an account
        if (string.IsNullOrEmpty(profile.AccountId))
        {
            logger.LogInformation("Profile {ProfileId} is not linked to any account", LogAnonymizer.HashId(command.ProfileId));
            return true; // Already unlinked, consider this success
        }

        var accountId = profile.AccountId;

        // Get the account
        var account = await accountRepository.GetByIdAsync(accountId, ct);
        if (account != null)
        {
            // Remove profile ID from account's profile list
            if (account.UserProfileIds.Contains(command.ProfileId))
            {
                account.UserProfileIds.Remove(command.ProfileId);
                await accountRepository.UpdateAsync(account, ct);

                logger.LogInformation("Removed profile {ProfileId} from account {AccountId}",
                    LogAnonymizer.HashId(command.ProfileId), LogAnonymizer.HashId(accountId));
            }
        }
        else
        {
            logger.LogWarning("Account {AccountId} not found, but profile still linked to it", LogAnonymizer.HashId(accountId));
        }

        // Clear profile's account ID
        profile.AccountId = null;
        profile.UpdatedAt = DateTime.UtcNow;
        await profileRepository.UpdateAsync(profile, ct);

        // Save all changes
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Successfully removed profile {ProfileId} from account {AccountId}",
            LogAnonymizer.HashId(command.ProfileId), LogAnonymizer.HashId(accountId));

        return true;
    }
}
