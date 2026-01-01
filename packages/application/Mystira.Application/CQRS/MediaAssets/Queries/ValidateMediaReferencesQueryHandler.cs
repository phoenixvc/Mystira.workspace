using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.CQRS.MediaAssets.Queries;

/// <summary>
/// Wolverine handler for validating media references exist in the system.
/// Ensures data integrity when creating content that references media assets.
/// </summary>
public static class ValidateMediaReferencesQueryHandler
{
    /// <summary>
    /// Handles the ValidateMediaReferencesQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="repository">The media asset repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The media validation result.</returns>
    public static async Task<MediaValidationResult> Handle(
        ValidateMediaReferencesQuery request,
        IMediaAssetRepository repository,
        ILogger<ValidateMediaReferencesQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Validating {Count} media references",
            request.MediaIds.Count);

        var result = new MediaValidationResult
        {
            TotalValidated = request.MediaIds.Count
        };

        if (request.MediaIds.Count == 0)
        {
            return result; // Empty list is valid
        }

        // Remove duplicates
        var uniqueMediaIds = request.MediaIds.Distinct().ToList();

        // Check each media ID
        var missingIds = new List<string>();

        foreach (var mediaId in uniqueMediaIds)
        {
            var exists = await repository.ExistsAsync(mediaId);
            if (!exists)
            {
                missingIds.Add(mediaId);
            }
        }

        result.MissingMediaIds = missingIds;
        result.ValidCount = uniqueMediaIds.Count - missingIds.Count;

        if (missingIds.Any())
        {
            logger.LogWarning(
                "Media validation failed: {MissingCount} missing media IDs: {MissingIds}",
                missingIds.Count,
                string.Join(", ", missingIds));
        }
        else
        {
            logger.LogInformation(
                "Media validation successful: all {Count} references valid",
                uniqueMediaIds.Count);
        }

        return result;
    }
}
