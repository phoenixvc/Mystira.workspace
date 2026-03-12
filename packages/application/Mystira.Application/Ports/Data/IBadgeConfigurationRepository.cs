using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for BadgeConfiguration entity with domain-specific queries
/// </summary>
public interface IBadgeConfigurationRepository : IRepository<BadgeConfiguration>
{
    /// <summary>
    /// Gets all badge configurations for a specific axis.
    /// </summary>
    Task<IEnumerable<BadgeConfiguration>> GetByAxisAsync(string axis, CancellationToken ct = default);
}
