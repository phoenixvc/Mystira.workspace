using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for CharacterMediaMetadataFile singleton entity
/// </summary>
public interface ICharacterMediaMetadataFileRepository
{
    /// <summary>
    /// Gets the character media metadata file.
    /// </summary>
    /// <returns>The character media metadata file if found; otherwise, null.</returns>
    Task<CharacterMediaMetadataFile?> GetAsync();

    /// <summary>
    /// Adds or updates the character media metadata file.
    /// </summary>
    /// <param name="entity">The character media metadata file to add or update.</param>
    /// <returns>The added or updated character media metadata file.</returns>
    Task<CharacterMediaMetadataFile> AddOrUpdateAsync(CharacterMediaMetadataFile entity);

    /// <summary>
    /// Deletes the character media metadata file.
    /// </summary>
    Task DeleteAsync();
}

