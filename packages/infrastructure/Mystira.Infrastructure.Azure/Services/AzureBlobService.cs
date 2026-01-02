using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Storage;

namespace Mystira.Infrastructure.Azure.Services;

/// <summary>
/// Azure Blob Storage implementation of the blob service for media file management.
/// </summary>
public class AzureBlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobService> _logger;
    private readonly string _containerName = "mystira-app-media";

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobService"/> class.
    /// </summary>
    /// <param name="blobServiceClient">The Azure Blob Service client.</param>
    /// <param name="logger">The logger instance.</param>
    public AzureBlobService(BlobServiceClient blobServiceClient, ILogger<AzureBlobService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a media file to Azure Blob Storage.
    /// </summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The content type of the file.</param>
    /// <returns>The URI of the uploaded blob.</returns>
    public async Task<string> UploadMediaAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            // Generate unique blob name to avoid conflicts
            var blobName = $"{Guid.NewGuid()}-{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            var uri = blobClient.Uri.ToString();
            _logger.LogInformation("Uploaded media file: {BlobName} to {uri}", blobName, uri);
            return uri;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload media file: {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    /// Gets the URL for a media file stored in Azure Blob Storage.
    /// </summary>
    /// <param name="blobName">The name of the blob.</param>
    /// <returns>The URL of the blob.</returns>
    public Task<string> GetMediaUrlAsync(string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // For public containers, we can return the direct URL
            return Task.FromResult(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get media URL for: {BlobName}", blobName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a media file from Azure Blob Storage.
    /// </summary>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <returns>True if the blob was deleted; otherwise, false.</returns>
    public async Task<bool> DeleteMediaAsync(string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DeleteIfExistsAsync();

            if (response.Value)
            {
                _logger.LogInformation("Deleted media file: {BlobName}", blobName);
            }
            else
            {
                _logger.LogWarning("Media file not found for deletion: {BlobName}", blobName);
            }

            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete media file: {BlobName}", blobName);
            throw;
        }
    }

    /// <summary>
    /// Lists all media files in Azure Blob Storage with an optional prefix filter.
    /// </summary>
    /// <param name="prefix">The optional prefix to filter blob names.</param>
    /// <returns>A list of blob names matching the prefix.</returns>
    public async Task<List<string>> ListMediaAsync(string prefix = "")
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobNames = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                blobNames.Add(blobItem.Name);
            }

            return blobNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list media files with prefix: {Prefix}", prefix);
            throw;
        }
    }

    /// <summary>
    /// Downloads a media file from Azure Blob Storage.
    /// </summary>
    /// <param name="blobName">The name of the blob to download.</param>
    /// <returns>A stream containing the blob content, or null if the blob does not exist.</returns>
    public async Task<Stream?> DownloadMediaAsync(string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadStreamingAsync();
                return response.Value.Content;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download media file: {BlobName}", blobName);
            throw;
        }
    }
}
