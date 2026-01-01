using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Requests.Media;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Media;

/// <summary>
/// Use case for updating media metadata
/// </summary>
public class UpdateMediaMetadataUseCase
{
    private readonly IMediaAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMediaMetadataUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMediaMetadataUseCase"/> class.
    /// </summary>
    /// <param name="repository">The media asset repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateMediaMetadataUseCase(
        IMediaAssetRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateMediaMetadataUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Updates metadata for a media asset.
    /// </summary>
    /// <param name="mediaId">The media identifier.</param>
    /// <param name="updateData">The update request containing new metadata.</param>
    /// <returns>The updated media asset.</returns>
    public async Task<MediaAsset> ExecuteAsync(string mediaId, MediaUpdateRequest updateData)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
        {
            throw new ArgumentException("Media ID is required", nameof(mediaId));
        }

        if (updateData == null)
        {
            throw new ArgumentNullException(nameof(updateData));
        }

        var mediaAsset = await _repository.GetByMediaIdAsync(mediaId);
        if (mediaAsset == null)
        {
            throw new KeyNotFoundException($"Media with ID '{mediaId}' not found");
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

        await _repository.UpdateAsync(mediaAsset);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Media updated successfully: {MediaId}", mediaId);

        return mediaAsset;
    }
}

