using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.Core.UseCases.Badges;

/// <summary>
/// Use case for retrieving badges for a user profile filtered by axis
/// </summary>
public class GetBadgesByAxisUseCase
{
    private readonly IUserBadgeRepository _repository;
    private readonly ILogger<GetBadgesByAxisUseCase> _logger;

    public GetBadgesByAxisUseCase(
        IUserBadgeRepository repository,
        ILogger<GetBadgesByAxisUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<UserBadge>> ExecuteAsync(string userProfileId, string axis, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userProfileId))
        {
            throw new ValidationException("userProfileId", "userProfileId is required");
        }

        if (string.IsNullOrWhiteSpace(axis))
        {
            throw new ValidationException("axis", "axis is required");
        }

        var badges = await _repository.GetByUserProfileIdAndAxisAsync(userProfileId, axis, ct);
        var badgeList = badges.ToList();

        _logger.LogInformation("Retrieved {Count} badges for user profile {UserProfileId} on axis {Axis}",
            badgeList.Count, PiiMask.HashId(userProfileId), axis);
        return badgeList;
    }
}

