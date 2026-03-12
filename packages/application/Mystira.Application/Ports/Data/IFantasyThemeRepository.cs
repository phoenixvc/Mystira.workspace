using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing FantasyThemeDefinition entities.
/// </summary>
public interface IFantasyThemeRepository : IMasterDataRepository<FantasyThemeDefinition>
{
    /// <summary>
    /// Gets a fantasy theme definition by its name.
    /// </summary>
    Task<FantasyThemeDefinition?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Checks if a fantasy theme definition exists with the specified name.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
