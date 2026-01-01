using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Badges;

/// <summary>
/// Use case for retrieving badges for a user profile
/// </summary>
public class GetUserBadgesUseCase
{
    private readonly IUserBadgeRepository _repository;
    private readonly ILogger<GetUserBadgesUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetUserBadgesUseCase"/> class.</summary>
    /// <param name="repository">The user badge repository.</param>
    /// <param name="logger">The logger.</param>
    public GetUserBadgesUseCase(
        IUserBadgeRepository repository,
        ILogger<GetUserBadgesUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves all badges for the specified user profile.</summary>
    /// <param name="userProfileId">The user profile identifier.</param>
    /// <returns>A list of user badges.</returns>
    public async Task<List<UserBadge>> ExecuteAsync(string userProfileId)
    {
        if (string.IsNullOrWhiteSpace(userProfileId))
        {
            throw new ArgumentException("User profile ID cannot be null or empty", nameof(userProfileId));
        }

        var badges = await _repository.GetByUserProfileIdAsync(userProfileId);
        var badgeList = badges.ToList();

        _logger.LogInformation("Retrieved {Count} badges for user profile {UserProfileId}", badgeList.Count, userProfileId);
        return badgeList;
    }
}

