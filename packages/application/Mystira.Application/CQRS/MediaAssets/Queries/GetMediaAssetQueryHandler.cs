using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.MediaAssets.Queries;

/// <summary>
/// Wolverine handler for retrieving a media asset by MediaId
/// </summary>
public static class GetMediaAssetQueryHandler
{
    /// <summary>
    /// Handles the GetMediaAssetQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="repository">The media asset repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The media asset if found; otherwise, null.</returns>
    public static async Task<MediaAsset?> Handle(
        GetMediaAssetQuery request,
        IMediaAssetRepository repository,
        ILogger<GetMediaAssetQuery> logger,
        CancellationToken ct)
    {
        // MediaId is an external identifier stored in the MediaAsset document; do not use the DB primary key.
        var media = await repository.GetByMediaIdAsync(request.MediaId);
        if (media == null)
        {
            logger.LogDebug("Media asset not found by MediaId {MediaId}", request.MediaId);
            return null;
        }

        logger.LogDebug("Retrieved media asset by MediaId {MediaId}", request.MediaId);
        return media;
    }
}
