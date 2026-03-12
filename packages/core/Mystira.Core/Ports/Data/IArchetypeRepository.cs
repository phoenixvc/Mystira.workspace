using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository for managing ArchetypeDefinition entities.
/// </summary>
public interface IArchetypeRepository : IMasterDataRepository<ArchetypeDefinition>
{
    /// <summary>
    /// Gets an archetype definition by its name.
    /// </summary>
    Task<ArchetypeDefinition?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Checks if an archetype definition exists with the specified name.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
