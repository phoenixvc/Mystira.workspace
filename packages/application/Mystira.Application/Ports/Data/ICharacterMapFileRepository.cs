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
    Task<CharacterMapFile?> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds or updates the character map file.
    /// </summary>
    Task<CharacterMapFile> AddOrUpdateAsync(CharacterMapFile entity, CancellationToken ct = default);

    /// <summary>
    /// Deletes the character map file.
    /// </summary>
    Task DeleteAsync(CancellationToken ct = default);
}
