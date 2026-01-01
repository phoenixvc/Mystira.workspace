using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.MediaMetadata.Queries;

/// <summary>
/// Wolverine handler for retrieving the media metadata file.
/// Returns the singleton media metadata file containing all media entries.
/// </summary>
public static class GetMediaMetadataFileQueryHandler
{
    /// <summary>
    /// Handles the GetMediaMetadataFileQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="repository">The media metadata file repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The media metadata file if found; otherwise, null.</returns>
    public static async Task<MediaMetadataFile?> Handle(
        GetMediaMetadataFileQuery request,
        IMediaMetadataFileRepository repository,
        ILogger<GetMediaMetadataFileQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Getting media metadata file");

        var mediaMetadataFile = await repository.GetAsync();

        if (mediaMetadataFile == null)
        {
            logger.LogWarning("Media metadata file not found");
            return null;
        }

        logger.LogInformation("Found media metadata file with {Count} entries", mediaMetadataFile.Entries.Count);

        // Domain model matches API model structure, can return directly
        return new MediaMetadataFile
        {
            Id = mediaMetadataFile.Id,
            Entries = mediaMetadataFile.Entries.Select(e => new MediaMetadataEntry
            {
                Id = e.Id,
                Title = e.Title,
                FileName = e.FileName,
                Type = e.Type,
                Description = e.Description,
                AgeRating = e.AgeRating,
                SubjectReferenceId = e.SubjectReferenceId,
                ClassificationTags = e.ClassificationTags.Select(ct => new ClassificationTag
                {
                    Key = ct.Key,
                    Value = ct.Value
                }).ToList(),
                Modifiers = e.Modifiers.Select(m => new MetadataModifier
                {
                    Key = m.Key,
                    Value = m.Value
                }).ToList(),
                Loopable = e.Loopable
            }).ToList(),
            CreatedAt = mediaMetadataFile.CreatedAt,
            UpdatedAt = mediaMetadataFile.UpdatedAt,
            Version = mediaMetadataFile.Version
        };
    }
}
