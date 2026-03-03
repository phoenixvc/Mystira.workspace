using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing FantasyThemeDefinition entities.
/// </summary>
public interface IFantasyThemeRepository
{
    /// <summary>
    /// Gets all fantasy theme definitions.
    /// </summary>
    /// <returns>A list of all fantasy theme definitions.</returns>
    Task<List<FantasyThemeDefinition>> GetAllAsync();

    /// <summary>
    /// Gets a fantasy theme definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the fantasy theme definition.</param>
    /// <returns>The fantasy theme definition if found; otherwise, null.</returns>
    Task<FantasyThemeDefinition?> GetByIdAsync(string id);

    /// <summary>
    /// Gets a fantasy theme definition by its name.
    /// </summary>
    /// <param name="name">The name of the fantasy theme definition.</param>
    /// <returns>The fantasy theme definition if found; otherwise, null.</returns>
    Task<FantasyThemeDefinition?> GetByNameAsync(string name);

    /// <summary>
    /// Checks if a fantasy theme definition exists with the specified name.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if a fantasy theme definition exists with the name; otherwise, false.</returns>
    Task<bool> ExistsByNameAsync(string name);

    /// <summary>
    /// Adds a new fantasy theme definition.
    /// </summary>
    /// <param name="fantasyTheme">The fantasy theme definition to add.</param>
    Task AddAsync(FantasyThemeDefinition fantasyTheme);

    /// <summary>
    /// Updates an existing fantasy theme definition.
    /// </summary>
    /// <param name="fantasyTheme">The fantasy theme definition to update.</param>
    Task UpdateAsync(FantasyThemeDefinition fantasyTheme);

    /// <summary>
    /// Deletes a fantasy theme definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the fantasy theme definition to delete.</param>
    Task DeleteAsync(string id);
}
