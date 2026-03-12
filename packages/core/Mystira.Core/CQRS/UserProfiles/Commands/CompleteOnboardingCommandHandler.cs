using Microsoft.Extensions.Logging;
using Mystira.Core.Helpers;
using Mystira.Core.Ports.Data;

namespace Mystira.Core.CQRS.UserProfiles.Commands;

/// <summary>
/// Wolverine handler for CompleteOnboardingCommand.
/// Marks the onboarding process as completed for a user profile.
/// </summary>
public static class CompleteOnboardingCommandHandler
{
    /// <summary>
    /// Handles the CompleteOnboardingCommand by marking onboarding as complete.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<bool> Handle(
        CompleteOnboardingCommand command,
        IUserProfileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var profile = await repository.GetByIdAsync(command.ProfileId, ct);
        if (profile == null)
        {
            logger.LogWarning("Profile not found: {ProfileId}", LogAnonymizer.HashId(command.ProfileId));
            return false;
        }

        // Mark onboarding as complete
        profile.HasCompletedOnboarding = true;
        profile.UpdatedAt = DateTime.UtcNow;

        // Update in repository
        await repository.UpdateAsync(profile, ct);

        // Persist changes
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Completed onboarding for profile {ProfileId}", LogAnonymizer.HashId(command.ProfileId));

        return true;
    }
}
