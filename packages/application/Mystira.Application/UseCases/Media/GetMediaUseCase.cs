using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Media;

/// <summary>
/// Use case for retrieving a media asset by ID
/// </summary>
public class GetMediaUseCase
{
    private readonly IMediaAssetRepository _repository;
    private readonly ILogger<GetMediaUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMediaUseCase"/> class.
    /// </summary>
    /// <param name="repository">The media asset repository.</param>
    /// <param name="logger">The logger instance.</param>
    public GetMediaUseCase(
        IMediaAssetRepository repository,
        ILogger<GetMediaUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a media asset by its identifier.
    /// </summary>
    /// <param name="mediaId">The media identifier.</param>
    /// <returns>The media asset if found; otherwise, null.</returns>
    public async Task<MediaAsset?> ExecuteAsync(string mediaId)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
        {
            throw new ArgumentException("Media ID is required", nameof(mediaId));
        }

        var mediaAsset = await _repository.GetByMediaIdAsync(mediaId);

        if (mediaAsset == null)
        {
            _logger.LogWarning("Media asset not found: {MediaId}", mediaId);
        }

        return mediaAsset;
    }
}

