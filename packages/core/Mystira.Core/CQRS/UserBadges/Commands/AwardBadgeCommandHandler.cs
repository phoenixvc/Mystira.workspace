using Microsoft.Extensions.Logging;
using Mystira.Core.Helpers;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;

namespace Mystira.Core.CQRS.UserBadges.Commands;

/// <summary>
/// Wolverine handler for AwardBadgeCommand.
/// Awards a badge to a user profile - this is a write operation that modifies state.
/// Checks for duplicate badges and updates the user profile's EarnedBadges list.
/// </summary>
public static class AwardBadgeCommandHandler
{
    /// <summary>
    /// Handles the AwardBadgeCommand by creating a user badge in the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<UserBadge> Handle(
        AwardBadgeCommand command,
        IUserBadgeRepository repository,
        IBadgeConfigurationRepository badgeConfigRepository,
        IUserProfileRepository profileRepository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var request = command.Request;

        if (string.IsNullOrEmpty(request.UserProfileId))
        {
            throw new ValidationException("userProfileId", "UserProfileId is required");
        }

        if (string.IsNullOrEmpty(request.BadgeConfigurationId))
        {
            throw new ValidationException("badgeConfigurationId", "BadgeConfigurationId is required");
        }

        // Check for duplicate badge - return existing if already earned
        var existingBadge = await repository.GetByUserProfileIdAndBadgeConfigIdAsync(
            request.UserProfileId, request.BadgeConfigurationId, ct);
        if (existingBadge != null)
        {
            logger.LogWarning("User {UserProfileId} already has badge {BadgeConfigurationId}",
                LogAnonymizer.HashId(request.UserProfileId), request.BadgeConfigurationId);
            return existingBadge;
        }

        var badgeConfig = await badgeConfigRepository.GetByIdAsync(request.BadgeConfigurationId, ct);
        if (badgeConfig == null)
        {
            throw new Mystira.Shared.Exceptions.NotFoundException("BadgeConfiguration", request.BadgeConfigurationId);
        }

        var badge = new UserBadge
        {
            Id = Guid.NewGuid().ToString(),
            UserProfileId = request.UserProfileId,
            BadgeConfigurationId = request.BadgeConfigurationId,
            BadgeId = badgeConfig.Id,
            BadgeName = badgeConfig.Name,
            BadgeMessage = badgeConfig.Message,
            Axis = badgeConfig.Axis?.Value,
            TriggerValue = request.TriggerValue,
            Threshold = badgeConfig.Threshold,
            GameSessionId = request.GameSessionId,
            ScenarioId = request.ScenarioId,
            ImageId = badgeConfig.ImageId,
            EarnedAt = DateTime.UtcNow
        };

        await repository.AddAsync(badge, ct);

        // Update user profile's EarnedBadges list
        var profile = await profileRepository.GetByIdAsync(request.UserProfileId, ct);
        if (profile != null)
        {
            profile.EarnedBadges.Add(badge);
            await profileRepository.UpdateAsync(profile, ct);
        }
        else
        {
            logger.LogWarning("User profile {ProfileId} not found; badge {BadgeId} created but profile not updated",
                LogAnonymizer.HashId(request.UserProfileId), badge.Id);
        }

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Awarded badge {BadgeId} to user profile {UserProfileId}",
            badge.Id, LogAnonymizer.HashId(request.UserProfileId));

        return badge;
    }
}
