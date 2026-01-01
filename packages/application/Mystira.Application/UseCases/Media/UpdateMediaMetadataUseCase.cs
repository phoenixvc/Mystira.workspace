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
    /// Updates metadata for a media asset with optimistic concurrency control.
    /// </summary>
    /// <param name="mediaId">The unique identifier of the media asset to update.</param>
    /// <param name="updateData">The update request containing new metadata values.</param>
    /// <returns>The updated media asset.</returns>
    /// <exception cref="ArgumentException">Thrown when mediaId is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when updateData is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the media asset is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a concurrency conflict is detected.</exception>
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

        // Capture original version for optimistic concurrency check
        var originalVersion = mediaAsset.Version;

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
        mediaAsset.Version = originalVersion + 1;

        // Perform update with optimistic concurrency - the repository/EF should be
        // configured with Version as a concurrency token. If another process modified
        // the record, SaveChangesAsync will throw DbUpdateConcurrencyException.
        try
        {
            await _repository.UpdateAsync(mediaAsset);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Concurrency") ||
                                    ex.InnerException?.GetType().Name.Contains("Concurrency") == true)
        {
            _logger.LogWarning("Concurrency conflict updating media {MediaId}. Version {Version} was stale.", mediaId, originalVersion);
            throw new InvalidOperationException(
                $"Media asset '{mediaId}' was modified by another process. Please refresh and try again.", ex);
        }

        _logger.LogInformation("Media updated successfully: {MediaId} (version {Version})", mediaId, mediaAsset.Version);

        return mediaAsset;
    }
}

