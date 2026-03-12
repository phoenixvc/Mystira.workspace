using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository for managing AgeGroupDefinition entities.
/// </summary>
public interface IAgeGroupRepository : IMasterDataRepository<AgeGroupDefinition>
{
    /// <summary>
    /// Gets an age group definition by its name.
    /// </summary>
    Task<AgeGroupDefinition?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets an age group definition by its value.
    /// </summary>
    Task<AgeGroupDefinition?> GetByValueAsync(string value, CancellationToken ct = default);

    /// <summary>
    /// Checks if an age group definition exists with the specified name.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Checks if an age group definition exists with the specified value.
    /// </summary>
    Task<bool> ExistsByValueAsync(string value, CancellationToken ct = default);
}
