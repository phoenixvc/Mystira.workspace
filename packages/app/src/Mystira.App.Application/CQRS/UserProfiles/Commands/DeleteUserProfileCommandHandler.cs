using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.App.Application.Ports.Data;

namespace Mystira.App.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Wolverine handler for DeleteUserProfileCommand.
/// Deletes a user profile and all associated data (COPPA compliance).
/// </summary>
public static class DeleteUserProfileCommandHandler
{
    /// <summary>
    /// Handles the DeleteUserProfileCommand by deleting a user profile.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<bool> Handle(
        DeleteUserProfileCommand command,
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

        // Delete profile
        await repository.DeleteAsync(profile.Id, ct);

        // Persist changes
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Deleted user profile {ProfileId}", LogAnonymizer.HashId(command.ProfileId));

        return true;
    }
}
