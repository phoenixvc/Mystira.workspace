using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing AxisAchievement entities.
/// </summary>
public interface IAxisAchievementRepository : IRepository<AxisAchievement>
{
    /// <summary>
    /// Gets all axis achievements for a specific age group.
    /// </summary>
    Task<IEnumerable<AxisAchievement>> GetByAgeGroupAsync(string ageGroupId, CancellationToken ct = default);

    /// <summary>
    /// Gets all axis achievements for a specific compass axis.
    /// </summary>
    Task<IEnumerable<AxisAchievement>> GetByCompassAxisAsync(string compassAxisId, CancellationToken ct = default);
}
