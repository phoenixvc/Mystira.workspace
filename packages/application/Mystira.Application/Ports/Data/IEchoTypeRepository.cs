using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing EchoTypeDefinition entities.
/// </summary>
public interface IEchoTypeRepository
{
    /// <summary>
    /// Gets all echo type definitions.
    /// </summary>
    /// <returns>A list of all echo type definitions.</returns>
    Task<List<EchoTypeDefinition>> GetAllAsync();

    /// <summary>
    /// Gets an echo type definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the echo type definition.</param>
    /// <returns>The echo type definition if found; otherwise, null.</returns>
    Task<EchoTypeDefinition?> GetByIdAsync(string id);

    /// <summary>
    /// Gets an echo type definition by its name.
    /// </summary>
    /// <param name="name">The name of the echo type definition.</param>
    /// <returns>The echo type definition if found; otherwise, null.</returns>
    Task<EchoTypeDefinition?> GetByNameAsync(string name);

    /// <summary>
    /// Checks if an echo type definition exists with the specified name.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if an echo type definition exists with the name; otherwise, false.</returns>
    Task<bool> ExistsByNameAsync(string name);

    /// <summary>
    /// Adds a new echo type definition.
    /// </summary>
    /// <param name="echoType">The echo type definition to add.</param>
    Task AddAsync(EchoTypeDefinition echoType);

    /// <summary>
    /// Updates an existing echo type definition.
    /// </summary>
    /// <param name="echoType">The echo type definition to update.</param>
    Task UpdateAsync(EchoTypeDefinition echoType);

    /// <summary>
    /// Deletes an echo type definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the echo type definition to delete.</param>
    Task DeleteAsync(string id);
}
