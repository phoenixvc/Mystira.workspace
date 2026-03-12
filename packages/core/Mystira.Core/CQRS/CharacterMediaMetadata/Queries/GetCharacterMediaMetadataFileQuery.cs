using Mystira.Domain.Models;

namespace Mystira.Core.CQRS.CharacterMediaMetadata.Queries;

/// <summary>
/// Query to get the character media metadata file.
/// </summary>
public record GetCharacterMediaMetadataFileQuery : IQuery<CharacterMediaMetadataFile?>;
