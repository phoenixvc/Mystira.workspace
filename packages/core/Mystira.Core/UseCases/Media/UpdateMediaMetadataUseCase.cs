using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Shared.Exceptions;
using Mystira.Contracts.App.Requests.Media;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.Core.UseCases.Media;

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
            throw new ValidationException("mediaId", "mediaId is required");
        }

        if (updateData == null)
        {
            throw new ValidationException("updateData", "updateData is required");
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
        mediaAsset.Version += 1;

        await _repository.UpdateAsync(mediaAsset, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Media updated successfully: {MediaId}", mediaId);

        return mediaAsset;
    }
}

