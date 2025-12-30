using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository interface for CharacterMapFile singleton entity
/// </summary>
public interface ICharacterMapFileRepository
{
    Task<CharacterMapFile?> GetAsync();
    Task<CharacterMapFile> AddOrUpdateAsync(CharacterMapFile entity);
    Task DeleteAsync();
}

