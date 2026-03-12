using Mystira.Domain.Models;

namespace Mystira.Core.Ports.Data;

/// <summary>
/// Repository interface for CharacterMap entity with domain-specific queries
/// </summary>
public interface ICharacterMapRepository : IRepository<CharacterMap>
{
    /// <summary>
    /// Gets a character map by its name.
    /// </summary>
    Task<CharacterMap?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Checks if a character map exists with the specified name.
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
