using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Exceptions;
using Mystira.Contracts.App.Requests.Media;
using Mystira.App.Domain.Models;
using System.Threading;

namespace Mystira.App.Application.UseCases.Media;

/// <summary>
/// Use case for updating media metadata
/// </summary>
public class UpdateMediaMetadataUseCase
{
    private readonly IMediaAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMediaMetadataUseCase> _logger;

    public UpdateMediaMetadataUseCase(
        IMediaAssetRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateMediaMetadataUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MediaAsset> ExecuteAsync(string mediaId, MediaUpdateRequest updateData, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
        {
            throw new ArgumentException("Media ID is required", nameof(mediaId));
        }

        if (updateData == null)
        {
            throw new ArgumentNullException(nameof(updateData));
        }

        var mediaAsset = await _repository.GetByMediaIdAsync(mediaId, ct);
        if (mediaAsset == null)
        {
            throw new NotFoundException("Media", mediaId);
        }

        // Update properties
        if (updateData.Description != null)
        {
            mediaAsset.Description = updateData.Description;
        }

        if (updateData.Tags != null)
        {
            mediaAsset.Tags = updateData.Tags;
        }

        if (!string.IsNullOrEmpty(updateData.MediaType))
        {
            mediaAsset.MediaType = updateData.MediaType;
        }

        mediaAsset.UpdatedAt = DateTime.UtcNow;
        mediaAsset.Version = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        await _repository.UpdateAsync(mediaAsset, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Media updated successfully: {MediaId}", mediaId);

        return mediaAsset;
    }
}

