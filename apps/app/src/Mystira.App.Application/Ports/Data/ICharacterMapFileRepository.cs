using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for CharacterMapFile singleton entity
/// </summary>
public interface ICharacterMapFileRepository
{
    Task<CharacterMapFile?> GetAsync(CancellationToken ct = default);
    Task<CharacterMapFile> AddOrUpdateAsync(CharacterMapFile entity, CancellationToken ct = default);
    Task DeleteAsync(CancellationToken ct = default);
}

