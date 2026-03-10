using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for CharacterMediaMetadataFile singleton entity
/// </summary>
public interface ICharacterMediaMetadataFileRepository
{
    Task<CharacterMediaMetadataFile?> GetAsync(CancellationToken ct = default);
    Task<CharacterMediaMetadataFile> AddOrUpdateAsync(CharacterMediaMetadataFile entity, CancellationToken ct = default);
    Task DeleteAsync(CancellationToken ct = default);
}

