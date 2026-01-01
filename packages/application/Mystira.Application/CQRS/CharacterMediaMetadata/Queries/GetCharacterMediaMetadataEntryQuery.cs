using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CharacterMediaMetadata.Queries;

/// <summary>
/// Query to get a specific character media metadata entry by ID.
/// </summary>
/// <param name="EntryId">The unique identifier of the character media metadata entry.</param>
public record GetCharacterMediaMetadataEntryQuery(string EntryId) : IQuery<CharacterMediaMetadataEntry?>;
