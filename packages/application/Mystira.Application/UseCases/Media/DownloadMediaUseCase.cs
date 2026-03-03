using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Application.UseCases.Media;

/// <summary>
/// Use case for downloading a media file
/// </summary>
public class DownloadMediaUseCase
{
    private readonly IMediaAssetRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DownloadMediaUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadMediaUseCase"/> class.
    /// </summary>
    /// <param name="repository">The media asset repository.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger instance.</param>
    public DownloadMediaUseCase(
        IMediaAssetRepository repository,
        IHttpClientFactory httpClientFactory,
        ILogger<DownloadMediaUseCase> logger)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Downloads a media file by its identifier.
    /// </summary>
    /// <param name="mediaId">The media identifier.</param>
    /// <returns>A tuple containing the stream, content type, and file name; null if not found.</returns>
    public async Task<(Stream stream, string contentType, string fileName)?> ExecuteAsync(string mediaId)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
        {
            throw new ArgumentException("Media ID is required", nameof(mediaId));
        }

        try
        {
            var mediaAsset = await _repository.GetByMediaIdAsync(mediaId);
            if (mediaAsset == null)
            {
                _logger.LogWarning("Media asset not found: {MediaId}", mediaId);
                return null;
            }

            // Download the file from the URL
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(mediaAsset.Url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download media from URL: {Url}", mediaAsset.Url);
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            var fileName = Path.GetFileName(new Uri(mediaAsset.Url).LocalPath);

            return (stream, mediaAsset.MimeType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading media file: {MediaId}", mediaId);
            return null;
        }
    }
}

