using Mystira.Shared.CQRS;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.UserBadges.Queries;

/// <summary>
/// Query to retrieve all badges earned by a user profile for a specific compass axis.
/// </summary>
/// <param name="UserProfileId">The unique identifier of the user profile.</param>
/// <param name="Axis">The compass axis to filter badges by.</param>
public record GetUserBadgesByAxisQuery(string UserProfileId, string Axis)
    : IQuery<List<UserBadge>>;

/// <summary>
/// Handler for processing GetUserBadgesByAxisQuery requests.
/// </summary>
public sealed class GetUserBadgesByAxisQueryHandler
{
    private readonly IUserBadgeRepository _userBadgeRepository;

    /// <summary>
    /// Initializes a new instance of the GetUserBadgesByAxisQueryHandler class.
    /// </summary>
    /// <param name="userBadgeRepository">The repository for accessing user badge data.</param>
    public GetUserBadgesByAxisQueryHandler(IUserBadgeRepository userBadgeRepository)
    {
        _userBadgeRepository = userBadgeRepository;
    }

    /// <summary>
    /// Handles the query to retrieve user badges filtered by axis.
    /// </summary>
    /// <param name="request">The query request containing the user profile ID and axis.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of user badges for the specified axis, ordered by earned date descending.</returns>
    public async Task<List<UserBadge>> Handle(GetUserBadgesByAxisQuery request, CancellationToken cancellationToken)
    {
        var badges = await _userBadgeRepository.GetByUserProfileIdAsync(request.UserProfileId);
        return badges
            .Where(b => string.Equals(b.Axis, request.Axis, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(b => b.EarnedAt)
            .ToList();
    }
}
