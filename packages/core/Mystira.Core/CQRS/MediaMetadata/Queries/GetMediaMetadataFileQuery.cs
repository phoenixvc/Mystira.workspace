using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.MediaMetadata.Queries;

/// <summary>
/// Query to get the media metadata file.
/// </summary>
public record GetMediaMetadataFileQuery : IQuery<MediaMetadataFile?>;
