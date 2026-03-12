using Mystira.Domain.Models;

namespace Mystira.Core.CQRS.MediaMetadata.Queries;

/// <summary>
/// Query to get the media metadata file.
/// </summary>
public record GetMediaMetadataFileQuery : IQuery<MediaMetadataFile?>;
