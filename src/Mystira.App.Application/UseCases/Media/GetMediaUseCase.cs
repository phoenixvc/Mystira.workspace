using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using System.Threading;

namespace Mystira.App.Application.UseCases.Media;

/// <summary>
/// Use case for retrieving a media asset by ID
/// </summary>
public class GetMediaUseCase
{
    private readonly IMediaAssetRepository _repository;
    private readonly ILogger<GetMediaUseCase> _logger;

    public GetMediaUseCase(
        IMediaAssetRepository repository,
        ILogger<GetMediaUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<MediaAsset?> ExecuteAsync(string mediaId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
        {
            throw new ArgumentException("Media ID is required", nameof(mediaId));
        }

        var mediaAsset = await _repository.GetByMediaIdAsync(mediaId, ct);

        if (mediaAsset == null)
        {
            _logger.LogWarning("Media asset not found: {MediaId}", mediaId);
        }

        return mediaAsset;
    }
}

