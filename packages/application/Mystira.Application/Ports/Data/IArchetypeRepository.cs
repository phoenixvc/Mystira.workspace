using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing ArchetypeDefinition entities.
/// </summary>
public interface IArchetypeRepository
{
    /// <summary>
    /// Gets all archetype definitions.
    /// </summary>
    /// <returns>A list of all archetype definitions.</returns>
    Task<List<ArchetypeDefinition>> GetAllAsync();

    /// <summary>
    /// Gets an archetype definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the archetype definition.</param>
    /// <returns>The archetype definition if found; otherwise, null.</returns>
    Task<ArchetypeDefinition?> GetByIdAsync(string id);

    /// <summary>
    /// Gets an archetype definition by its name.
    /// </summary>
    /// <param name="name">The name of the archetype definition.</param>
    /// <returns>The archetype definition if found; otherwise, null.</returns>
    Task<ArchetypeDefinition?> GetByNameAsync(string name);

    /// <summary>
    /// Checks if an archetype definition exists with the specified name.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if an archetype definition exists with the name; otherwise, false.</returns>
    Task<bool> ExistsByNameAsync(string name);

    /// <summary>
    /// Adds a new archetype definition.
    /// </summary>
    /// <param name="archetype">The archetype definition to add.</param>
    Task AddAsync(ArchetypeDefinition archetype);

    /// <summary>
    /// Updates an existing archetype definition.
    /// </summary>
    /// <param name="archetype">The archetype definition to update.</param>
    Task UpdateAsync(ArchetypeDefinition archetype);

    /// <summary>
    /// Deletes an archetype definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the archetype definition to delete.</param>
    Task DeleteAsync(string id);
}
