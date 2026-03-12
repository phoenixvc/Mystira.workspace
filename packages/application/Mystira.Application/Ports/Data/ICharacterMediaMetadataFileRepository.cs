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
    Task<CharacterMediaMetadataFile?> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds or updates the character media metadata file.
    /// </summary>
    Task<CharacterMediaMetadataFile> AddOrUpdateAsync(CharacterMediaMetadataFile entity, CancellationToken ct = default);

    /// <summary>
    /// Deletes the character media metadata file.
    /// </summary>
    Task DeleteAsync(CancellationToken ct = default);
}
