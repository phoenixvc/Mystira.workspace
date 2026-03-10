using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
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
            throw new ValidationException("mediaId", "mediaId is required");
        }

        var mediaAsset = await _repository.GetByMediaIdAsync(mediaId, ct);

        if (mediaAsset == null)
        {
            _logger.LogWarning("Media asset not found: {MediaId}", mediaId);
        }

        return mediaAsset;
    }
}

