using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Ports.Storage;

namespace Mystira.Application.UseCases.Media;

/// <summary>
/// Use case for deleting a media asset
/// </summary>
public class DeleteMediaUseCase
{
    private readonly IMediaAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBlobService _blobStorageService;
    private readonly ILogger<DeleteMediaUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteMediaUseCase"/> class.
    /// </summary>
    /// <param name="repository">The media asset repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="blobStorageService">The blob storage service.</param>
    /// <param name="logger">The logger instance.</param>
    public DeleteMediaUseCase(
        IMediaAssetRepository repository,
        IUnitOfWork unitOfWork,
        IBlobService blobStorageService,
        ILogger<DeleteMediaUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Deletes a media asset by its identifier.
    /// </summary>
    /// <param name="mediaId">The media identifier.</param>
    /// <returns>True if the media was deleted; false if not found.</returns>
    public async Task<bool> ExecuteAsync(string mediaId)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
        {
            throw new ArgumentException("Media ID is required", nameof(mediaId));
        }

        var mediaAsset = await _repository.GetByMediaIdAsync(mediaId);
        if (mediaAsset == null)
        {
            _logger.LogWarning("Media asset not found for deletion: {MediaId}", mediaId);
            return false;
        }

        try
        {
            // Delete from blob storage
            if (!string.IsNullOrEmpty(mediaAsset.Url))
            {
                try
                {
                    var uri = new Uri(mediaAsset.Url);
                    var blobName = Path.GetFileName(uri.LocalPath);
                    await _blobStorageService.DeleteMediaAsync(blobName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete media from blob storage: {MediaId}", mediaId);
                    // Continue with database deletion even if blob deletion fails
                }
            }

            // Delete from database
            await _repository.DeleteAsync(mediaAsset.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Media deleted successfully: {MediaId}", mediaId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media: {MediaId}", mediaId);
            throw;
        }
    }
}

