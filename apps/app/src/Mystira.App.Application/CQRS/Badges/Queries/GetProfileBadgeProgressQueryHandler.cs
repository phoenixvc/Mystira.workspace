using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Responses.Badges;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.Badges.Queries;

/// <summary>
/// Wolverine handler for GetProfileBadgeProgressQuery.
/// Retrieves badge progress information for a user profile.
/// </summary>
public static class GetProfileBadgeProgressQueryHandler
{
    /// <summary>
    /// Handles the GetProfileBadgeProgressQuery by retrieving badge progress from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<BadgeProgressResponse?> Handle(
        GetProfileBadgeProgressQuery query,
        IBadgeRepository badgeRepository,
        ICompassAxisRepository axisRepository,
        IUserBadgeRepository userBadgeRepository,
        IUserProfileRepository profileRepository,
        CancellationToken ct)
    {
        var profile = await profileRepository.GetByIdAsync(query.ProfileId, ct);
        if (profile == null) return null;

        var ageGroupId = profile.AgeGroupId ?? AgeGroupConstants.MiddleChildhood;

        // Retrieve badges for the age group. Some Cosmos providers may attempt to use
        // ORDER BY in the query which requires a composite index. To avoid runtime
        // failures when composite indexes are not yet deployed, always perform
        // ordering in-memory here.
        var allBadges = await badgeRepository.GetByAgeGroupAsync(ageGroupId, ct);

        var badgesByAxis = allBadges
            .GroupBy(b => b.CompassAxisId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(b => b.TierOrder).ToList()
            );

        var userBadges = (await userBadgeRepository.GetByUserProfileIdAsync(query.ProfileId, ct)).ToList();

        var axes = await axisRepository.GetAllAsync();
        var axisDictionary = axes.ToDictionary(a => a.Id, a => a);

        var response = new BadgeProgressResponse
        {
            AgeGroupId = ageGroupId,
            AxisProgresses = new List<AxisProgressResponse>()
        };

        foreach (var (axisId, badges) in badgesByAxis.OrderBy(x => x.Key))
        {
            var axis = axisDictionary.TryGetValue(axisId, out var a) ? a : null;
            var axisName = axis?.Name ?? axisId;

            // Derive current score for this axis from earned badges' values (max of TriggerValue/Threshold)
            // Match legacy data where Axis may store axis name instead of ID
            var axisUserBadges = userBadges
                .Where(ub => string.Equals(ub.Axis, axisId, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(ub.Axis, axisName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(ub => Math.Max(ub.TriggerValue ?? 0f, ub.Threshold ?? 0))
                .ThenByDescending(ub => ub.EarnedAt)
                .ToList();

            var currentScore = axisUserBadges
                .Select(ub => Math.Max(ub.TriggerValue ?? 0f, ub.Threshold ?? 0))
                .DefaultIfEmpty(0f)
                .Max();

            var axisTiers = new List<BadgeTierProgressResponse>();
            foreach (var badge in badges)
            {
                var requiredScore = (float)(badge.RequiredScore ?? 0);
                var matchedBadge = axisUserBadges
                    .FirstOrDefault(ub => Math.Max(ub.TriggerValue ?? 0f, ub.Threshold ?? 0) >= requiredScore);
                var isEarned = matchedBadge != null;

                axisTiers.Add(new BadgeTierProgressResponse
                {
                    BadgeId = badge.Id,
                    Tier = badge.Tier,
                    TierOrder = badge.TierOrder,
                    Title = badge.Title,
                    Description = badge.Description,
                    RequiredScore = requiredScore,
                    ImageId = badge.ImageId,
                    IsEarned = isEarned,
                    EarnedAt = isEarned ? matchedBadge!.EarnedAt : null,
                    ProgressToThreshold = currentScore,
                    RemainingScore = Math.Max(0, requiredScore - currentScore)
                });
            }

            response.AxisProgresses.Add(new AxisProgressResponse
            {
                CompassAxisId = axisId,
                CompassAxisName = axisName,
                CurrentScore = (int)Math.Round(currentScore, MidpointRounding.AwayFromZero),
                Tiers = axisTiers
            });
        }

        return response;
    }
}
