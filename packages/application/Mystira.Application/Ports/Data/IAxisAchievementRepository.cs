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
    /// <param name="ageGroupId">The age group identifier.</param>
    /// <returns>A collection of axis achievements for the specified age group.</returns>
    Task<IEnumerable<AxisAchievement>> GetByAgeGroupAsync(string ageGroupId);

    /// <summary>
    /// Gets all axis achievements for a specific compass axis.
    /// </summary>
    /// <param name="compassAxisId">The compass axis identifier.</param>
    /// <returns>A collection of axis achievements for the specified compass axis.</returns>
    Task<IEnumerable<AxisAchievement>> GetByCompassAxisAsync(string compassAxisId);
}
