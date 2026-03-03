using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Badges;

/// <summary>
/// Use case for retrieving badges for a user profile filtered by axis
/// </summary>
public class GetBadgesByAxisUseCase
{
    private readonly IUserBadgeRepository _repository;
    private readonly ILogger<GetBadgesByAxisUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetBadgesByAxisUseCase"/> class.</summary>
    /// <param name="repository">The user badge repository.</param>
    /// <param name="logger">The logger.</param>
    public GetBadgesByAxisUseCase(
        IUserBadgeRepository repository,
        ILogger<GetBadgesByAxisUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves badges for the specified user profile filtered by axis.</summary>
    /// <param name="userProfileId">The user profile identifier.</param>
    /// <param name="axis">The axis identifier.</param>
    /// <returns>A list of user badges for the specified axis.</returns>
    public async Task<List<UserBadge>> ExecuteAsync(string userProfileId, string axis)
    {
        if (string.IsNullOrWhiteSpace(userProfileId))
        {
            throw new ArgumentException("User profile ID cannot be null or empty", nameof(userProfileId));
        }

        if (string.IsNullOrWhiteSpace(axis))
        {
            throw new ArgumentException("Axis cannot be null or empty", nameof(axis));
        }

        var badges = await _repository.GetByUserProfileIdAndAxisAsync(userProfileId, axis);
        var badgeList = badges.ToList();

        _logger.LogInformation("Retrieved {Count} badges for user profile {UserProfileId} on axis {Axis}",
            badgeList.Count, userProfileId, axis);
        return badgeList;
    }
}

