using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Application.CQRS.Badges.Queries;

/// <summary>
/// Wolverine handler for GetAxisAchievementsQuery.
/// Retrieves axis achievements for a specific age group.
/// </summary>
public static class GetAxisAchievementsQueryHandler
{
    /// <summary>
    /// Handles the GetAxisAchievementsQuery by retrieving axis achievements from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<List<AxisAchievementResponse>> Handle(
        GetAxisAchievementsQuery query,
        IAxisAchievementRepository axisAchievementRepository,
        ICompassAxisRepository axisRepository,
        CancellationToken ct)
    {
        var achievements = await axisAchievementRepository.GetByAgeGroupAsync(query.AgeGroupId);
        var axisDefinitions = await axisRepository.GetAllAsync();

        var axisLookup = axisDefinitions
            .SelectMany(a => new[] { (Key: a.Id, Value: a), (Key: a.Name, Value: a) })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        return achievements
            .OrderBy(a => a.CompassAxisId)
            .ThenBy(a => a.CurrentValue)
            .Select(a =>
            {
                axisLookup.TryGetValue(a.CompassAxisId, out var axis);
                var axisName = axis != null && !string.IsNullOrWhiteSpace(axis.Name)
                    ? axis.Name
                    : (axis?.Id ?? a.CompassAxisId);

                // Determine direction based on current value
                var direction = a.CurrentValue >= 0 ? "positive" : "negative";

                return new AxisAchievementResponse
                {
                    Id = a.Id,
                    AgeGroupId = string.Empty, // AxisAchievement doesn't have AgeGroupId
                    CompassAxisId = a.CompassAxisId,
                    CompassAxisName = axisName,
                    AxesDirection = direction,
                    Description = $"Highest: {a.HighestValue}, Lowest: {a.LowestValue}, Current: {a.CurrentValue}"
                };
            })
            .ToList();
    }
}
