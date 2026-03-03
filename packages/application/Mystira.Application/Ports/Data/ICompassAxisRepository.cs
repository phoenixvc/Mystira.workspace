using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing CompassAxisDefinition entities.
/// </summary>
public interface ICompassAxisRepository
{
    /// <summary>
    /// Gets all compass axis definitions.
    /// </summary>
    /// <returns>A list of all compass axis definitions.</returns>
    Task<List<CompassAxisDefinition>> GetAllAsync();

    /// <summary>
    /// Gets a compass axis definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the compass axis definition.</param>
    /// <returns>The compass axis definition if found; otherwise, null.</returns>
    Task<CompassAxisDefinition?> GetByIdAsync(string id);

    /// <summary>
    /// Gets a compass axis definition by its name.
    /// </summary>
    /// <param name="name">The name of the compass axis definition.</param>
    /// <returns>The compass axis definition if found; otherwise, null.</returns>
    Task<CompassAxisDefinition?> GetByNameAsync(string name);

    /// <summary>
    /// Checks if a compass axis definition exists with the specified name.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if a compass axis definition exists with the name; otherwise, false.</returns>
    Task<bool> ExistsByNameAsync(string name);

    /// <summary>
    /// Adds a new compass axis definition.
    /// </summary>
    /// <param name="axis">The compass axis definition to add.</param>
    Task AddAsync(CompassAxisDefinition axis);

    /// <summary>
    /// Updates an existing compass axis definition.
    /// </summary>
    /// <param name="axis">The compass axis definition to update.</param>
    Task UpdateAsync(CompassAxisDefinition axis);

    /// <summary>
    /// Deletes a compass axis definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the compass axis definition to delete.</param>
    Task DeleteAsync(string id);
}
