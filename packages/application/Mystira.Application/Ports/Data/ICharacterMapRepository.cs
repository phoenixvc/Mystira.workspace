using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for CharacterMap entity with domain-specific queries
/// </summary>
public interface ICharacterMapRepository : IRepository<CharacterMap>
{
    /// <summary>
    /// Gets a character map by its name.
    /// </summary>
    /// <param name="name">The name of the character map.</param>
    /// <returns>The character map if found; otherwise, null.</returns>
    Task<CharacterMap?> GetByNameAsync(string name);

    /// <summary>
    /// Checks if a character map exists with the specified name.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if a character map exists with the name; otherwise, false.</returns>
    Task<bool> ExistsByNameAsync(string name);
}

