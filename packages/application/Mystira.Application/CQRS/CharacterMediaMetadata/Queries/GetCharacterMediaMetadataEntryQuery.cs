using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CharacterMediaMetadata.Queries;

/// <summary>
/// Query to get a specific character media metadata entry by ID.
/// </summary>
public record GetCharacterMediaMetadataEntryQuery(string EntryId) : IQuery<CharacterMediaMetadataEntry?>;
