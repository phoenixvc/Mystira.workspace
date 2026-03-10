using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.Contracts.App.Requests.Badges;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Badges;

public class AwardBadgeUseCase
{
    private readonly IUserBadgeRepository _badgeRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IBadgeRepository _newBadgeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AwardBadgeUseCase> _logger;

    public AwardBadgeUseCase(
        IUserBadgeRepository badgeRepository,
        IUserProfileRepository userProfileRepository,
        IBadgeRepository newBadgeRepository,
        IUnitOfWork unitOfWork,
        ILogger<AwardBadgeUseCase> logger)
    {
        _badgeRepository = badgeRepository;
        _userProfileRepository = userProfileRepository;
        _newBadgeRepository = newBadgeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserBadge> ExecuteAsync(AwardBadgeRequest request, CancellationToken ct = default)
    {
        if (request == null)
        {
            throw new ValidationException("request", "request is required");
        }

        var existingBadge = await _badgeRepository.GetByUserProfileIdAndBadgeConfigIdAsync(
            request.UserProfileId, request.BadgeConfigurationId, ct);

        if (existingBadge != null)
        {
            _logger.LogWarning("User {UserProfileId} already has badge {BadgeId}",
                PiiMask.HashId(request.UserProfileId), request.BadgeConfigurationId);
            return existingBadge;
        }

        var badge = await _newBadgeRepository.GetByIdAsync(request.BadgeConfigurationId, ct);
        if (badge == null)
        {
            throw new NotFoundException("Badge", request.BadgeConfigurationId);
        }

        var userProfile = await _userProfileRepository.GetByIdAsync(request.UserProfileId, ct);
        if (userProfile == null)
        {
            throw new NotFoundException("UserProfile", request.UserProfileId);
        }

        var newBadge = new UserBadge
        {
            Id = Guid.NewGuid().ToString(),
            UserProfileId = request.UserProfileId,
            BadgeConfigurationId = request.BadgeConfigurationId,
            BadgeId = badge.Id,
            BadgeName = badge.Title,
            BadgeMessage = badge.Description,
            Axis = badge.CompassAxisId,
            TriggerValue = request.TriggerValue,
            Threshold = badge.RequiredScore,
            GameSessionId = request.GameSessionId,
            ScenarioId = request.ScenarioId,
            ImageId = badge.ImageId,
            EarnedAt = DateTime.UtcNow
        };

        await _badgeRepository.AddAsync(newBadge, ct);

        userProfile.EarnedBadges.Add(newBadge);
        await _userProfileRepository.UpdateAsync(userProfile, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Awarded badge {BadgeName} to user {UserProfileId}",
            badge.Title, PiiMask.HashId(request.UserProfileId));

        return newBadge;
    }
}
