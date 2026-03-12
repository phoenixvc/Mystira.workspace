using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Badges;

/// <summary>
/// Use case for retrieving badges for a user profile
/// </summary>
public class GetUserBadgesUseCase
{
    private readonly IUserBadgeRepository _repository;
    private readonly ILogger<GetUserBadgesUseCase> _logger;

    public GetUserBadgesUseCase(
        IUserBadgeRepository repository,
        ILogger<GetUserBadgesUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<UserBadge>> ExecuteAsync(string userProfileId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userProfileId))
        {
            throw new ValidationException("userProfileId", "userProfileId is required");
        }

        var badges = await _repository.GetByUserProfileIdAsync(userProfileId, ct);
        var badgeList = badges.ToList();

        _logger.LogInformation("Retrieved {Count} badges for user profile {UserProfileId}", badgeList.Count, PiiMask.HashId(userProfileId));
        return badgeList;
    }
}

