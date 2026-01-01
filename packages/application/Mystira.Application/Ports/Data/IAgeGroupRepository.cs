using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing AgeGroupDefinition entities.
/// </summary>
public interface IAgeGroupRepository
{
    /// <summary>
    /// Gets all age group definitions.
    /// </summary>
    /// <returns>A list of all age group definitions.</returns>
    Task<List<AgeGroupDefinition>> GetAllAsync();

    /// <summary>
    /// Gets an age group definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the age group definition.</param>
    /// <returns>The age group definition if found; otherwise, null.</returns>
    Task<AgeGroupDefinition?> GetByIdAsync(string id);

    /// <summary>
    /// Gets an age group definition by its name.
    /// </summary>
    /// <param name="name">The name of the age group definition.</param>
    /// <returns>The age group definition if found; otherwise, null.</returns>
    Task<AgeGroupDefinition?> GetByNameAsync(string name);

    /// <summary>
    /// Gets an age group definition by its value.
    /// </summary>
    /// <param name="value">The value of the age group definition.</param>
    /// <returns>The age group definition if found; otherwise, null.</returns>
    Task<AgeGroupDefinition?> GetByValueAsync(string value);

    /// <summary>
    /// Checks if an age group definition exists with the specified name.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if an age group definition exists with the name; otherwise, false.</returns>
    Task<bool> ExistsByNameAsync(string name);

    /// <summary>
    /// Checks if an age group definition exists with the specified value.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if an age group definition exists with the value; otherwise, false.</returns>
    Task<bool> ExistsByValueAsync(string value);

    /// <summary>
    /// Adds a new age group definition.
    /// </summary>
    /// <param name="ageGroup">The age group definition to add.</param>
    Task AddAsync(AgeGroupDefinition ageGroup);

    /// <summary>
    /// Updates an existing age group definition.
    /// </summary>
    /// <param name="ageGroup">The age group definition to update.</param>
    Task UpdateAsync(AgeGroupDefinition ageGroup);

    /// <summary>
    /// Deletes an age group definition by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the age group definition to delete.</param>
    Task DeleteAsync(string id);
}
