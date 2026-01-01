using Microsoft.Extensions.Logging;
using Mystira.Application.Ports;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Media;

/// <summary>
/// Use case for retrieving a media asset by filename (resolves through metadata)
/// </summary>
public class GetMediaByFilenameUseCase
{
    private readonly IMediaAssetRepository _repository;
    private readonly IMediaMetadataService _mediaMetadataService;
    private readonly ILogger<GetMediaByFilenameUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMediaByFilenameUseCase"/> class.
    /// </summary>
    /// <param name="repository">The media asset repository.</param>
    /// <param name="mediaMetadataService">The media metadata service.</param>
    /// <param name="logger">The logger instance.</param>
    public GetMediaByFilenameUseCase(
        IMediaAssetRepository repository,
        IMediaMetadataService mediaMetadataService,
        ILogger<GetMediaByFilenameUseCase> logger)
    {
        _repository = repository;
        _mediaMetadataService = mediaMetadataService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a media asset by its filename.
    /// </summary>
    /// <param name="fileName">The file name to search for.</param>
    /// <returns>The media asset if found; otherwise, null.</returns>
    public async Task<MediaAsset?> ExecuteAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required", nameof(fileName));
        }

        try
        {
            // Get metadata file to resolve filename to media ID
            var metadataFile = await _mediaMetadataService.GetMediaMetadataFileAsync();
            if (metadataFile == null)
            {
                _logger.LogWarning("Media metadata file not found");
                return null;
            }

            // Find metadata entry by filename
            var metadataEntry = metadataFile.Entries.FirstOrDefault(e => e.FileName == fileName);
            if (metadataEntry == null)
            {
                _logger.LogWarning("Media metadata entry not found for filename: {FileName}", fileName);
                return null;
            }

            // Get the media asset by the resolved media ID
            return await _repository.GetByMediaIdAsync(metadataEntry.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media by filename: {FileName}", fileName);
            return null;
        }
    }
}

