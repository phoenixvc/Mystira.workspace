using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing Badge entities.
/// </summary>
public interface IBadgeRepository : IRepository<Badge>
{
    /// <summary>
    /// Gets all badges for a specific age group.
    /// </summary>
    /// <param name="ageGroupId">The age group identifier.</param>
    /// <returns>A collection of badges for the specified age group.</returns>
    Task<IEnumerable<Badge>> GetByAgeGroupAsync(string ageGroupId);

    /// <summary>
    /// Gets all badges for a specific compass axis.
    /// </summary>
    /// <param name="compassAxisId">The compass axis identifier.</param>
    /// <returns>A collection of badges for the specified compass axis.</returns>
    Task<IEnumerable<Badge>> GetByCompassAxisAsync(string compassAxisId);

    /// <summary>
    /// Gets a badge by age group, compass axis, and tier order.
    /// </summary>
    /// <param name="ageGroupId">The age group identifier.</param>
    /// <param name="compassAxisId">The compass axis identifier.</param>
    /// <param name="tierOrder">The tier order.</param>
    /// <returns>The badge if found; otherwise, null.</returns>
    Task<Badge?> GetByAgeGroupAxisAndTierAsync(string ageGroupId, string compassAxisId, int tierOrder);
}
