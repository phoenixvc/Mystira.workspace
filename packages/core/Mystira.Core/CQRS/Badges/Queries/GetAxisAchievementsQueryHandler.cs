using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Responses.Badges;

namespace Mystira.Core.CQRS.Badges.Queries;

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
        var achievements = await axisAchievementRepository.GetByAgeGroupAsync(query.AgeGroupId, ct);
        var axes = await axisRepository.GetAllAsync();

        var axisLookup = new Dictionary<string, CompassAxisDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (var a in axes)
        {
            if (!string.IsNullOrWhiteSpace(a.Id))
                axisLookup.TryAdd(a.Id, a);
            if (!string.IsNullOrWhiteSpace(a.Name))
                axisLookup.TryAdd(a.Name, a);
        }

        return achievements
            .OrderBy(a => a.CompassAxisId)
            .ThenBy(a => a.AxesDirection)
            .Select(a =>
            {
                axisLookup.TryGetValue(a.CompassAxisId, out var axis);
                var axisName = axis != null && !string.IsNullOrWhiteSpace(axis.Name)
                    ? axis.Name
                    : (axis?.Id ?? a.CompassAxisId);

                return new AxisAchievementResponse
                {
                    Id = a.Id,
                    AgeGroupId = a.AgeGroupId,
                    CompassAxisId = a.CompassAxisId,
                    CompassAxisName = axisName,
                    AxesDirection = a.AxesDirection,
                    Description = a.Description ?? string.Empty
                };
            })
            .ToList();
    }
}
