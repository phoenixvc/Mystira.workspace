using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository for managing Badge entities.
/// </summary>
public interface IBadgeRepository : IRepository<Badge>
{
    /// <summary>
    /// Gets all badges for a specific age group.
    /// </summary>
    Task<IEnumerable<Badge>> GetByAgeGroupAsync(string ageGroupId, CancellationToken ct = default);

    /// <summary>
    /// Gets all badges for a specific compass axis.
    /// </summary>
    Task<IEnumerable<Badge>> GetByCompassAxisAsync(string compassAxisId, CancellationToken ct = default);

    /// <summary>
    /// Gets a badge by age group, compass axis, and tier order.
    /// </summary>
    Task<Badge?> GetByAgeGroupAxisAndTierAsync(string ageGroupId, string compassAxisId, int tierOrder, CancellationToken ct = default);
}
