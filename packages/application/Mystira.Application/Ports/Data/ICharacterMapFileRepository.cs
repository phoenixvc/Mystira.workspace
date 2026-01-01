using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for CharacterMapFile singleton entity
/// </summary>
public interface ICharacterMapFileRepository
{
    /// <summary>
    /// Gets the character map file.
    /// </summary>
    /// <returns>The character map file if found; otherwise, null.</returns>
    Task<CharacterMapFile?> GetAsync();

    /// <summary>
    /// Adds or updates the character map file.
    /// </summary>
    /// <param name="entity">The character map file to add or update.</param>
    /// <returns>The added or updated character map file.</returns>
    Task<CharacterMapFile> AddOrUpdateAsync(CharacterMapFile entity);

    /// <summary>
    /// Deletes the character map file.
    /// </summary>
    Task DeleteAsync();
}

