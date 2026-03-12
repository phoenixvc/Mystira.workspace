using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository for managing CompassAxisDefinition entities.
/// </summary>
public interface ICompassAxisRepository : IMasterDataRepository<CompassAxisDefinition>
{
    /// <summary>
    /// Gets a compass axis definition by its name.
    /// </summary>
    Task<CompassAxisDefinition?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Checks if a compass axis definition exists with the specified name.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
