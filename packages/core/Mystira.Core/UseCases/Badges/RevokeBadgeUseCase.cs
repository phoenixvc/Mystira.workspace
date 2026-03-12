using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.Core.UseCases.Badges;

/// <summary>
/// Use case for revoking a badge from a user profile (admin only)
/// </summary>
public class RevokeBadgeUseCase
{
    private readonly IUserBadgeRepository _badgeRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RevokeBadgeUseCase> _logger;

    public RevokeBadgeUseCase(
        IUserBadgeRepository badgeRepository,
        IUserProfileRepository userProfileRepository,
        IUnitOfWork unitOfWork,
        ILogger<RevokeBadgeUseCase> logger)
    {
        _badgeRepository = badgeRepository;
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(string badgeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(badgeId))
        {
            throw new ValidationException("badgeId", "badgeId is required");
        }

        var badge = await _badgeRepository.GetByIdAsync(badgeId, ct);
        if (badge == null)
        {
            _logger.LogWarning("Badge not found for revocation: {BadgeId}", badgeId);
            return false;
        }

        // Remove from user profile's earned badges list
        var userProfile = await _userProfileRepository.GetByIdAsync(badge.UserProfileId, ct);
        if (userProfile != null)
        {
            var badgeToRemove = userProfile.EarnedBadges.FirstOrDefault(b => b.Id == badgeId);
            if (badgeToRemove != null)
            {
                userProfile.EarnedBadges.Remove(badgeToRemove);
                await _userProfileRepository.UpdateAsync(userProfile, ct);
            }
        }

        // Delete the badge
        await _badgeRepository.DeleteAsync(badgeId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Revoked badge {BadgeId} from user profile {UserProfileId}",
            badgeId, PiiMask.HashId(badge.UserProfileId));
        return true;
    }
}

