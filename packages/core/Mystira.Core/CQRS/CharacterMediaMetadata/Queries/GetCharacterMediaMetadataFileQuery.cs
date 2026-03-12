using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.CharacterMediaMetadata.Queries;

/// <summary>
/// Query to get the character media metadata file.
/// </summary>
public record GetCharacterMediaMetadataFileQuery : IQuery<CharacterMediaMetadataFile?>;
