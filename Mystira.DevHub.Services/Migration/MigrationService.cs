using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Mystira.DevHub.Services.Migration;

/// <summary>
/// Standalone migration service for Cosmos DB and Blob Storage.
/// Uses generic/dynamic migrations to avoid external domain model dependencies.
/// </summary>
public class MigrationService : IMigrationService
{
    private readonly ILogger<MigrationService> _logger;

    // Concurrency limits for parallel operations
    private const int MaxConcurrentDocumentOperations = 100;
    private const int MaxConcurrentBlobOperations = 10;

    public MigrationService(ILogger<MigrationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Migrates any Cosmos DB container using dynamic/generic approach.
    /// Works with any document schema without requiring typed models.
    /// </summary>
    public async Task<MigrationResult> MigrateContainerAsync(
        string sourceConnectionString,
        string destConnectionString,
        string sourceDatabaseName,
        string destDatabaseName,
        string containerName,
        string partitionKeyPath = "/id",
        MigrationOptions? options = null)
    {
        options ??= new MigrationOptions();
        var stopwatch = Stopwatch.StartNew();
        var result = new MigrationResult();
        var mode = options.DryRun ? "[DRY RUN] " : "";

        try
        {
            _logger.LogInformation("{Mode}Starting migration for container: {Container} from {SourceDb} to {DestDb}",
                mode, containerName, sourceDatabaseName, destDatabaseName);

            using var sourceClient = new CosmosClient(sourceConnectionString);
            using var destClient = new CosmosClient(destConnectionString, new CosmosClientOptions
            {
                AllowBulkExecution = options.UseBulkOperations && !options.DryRun
            });

            var sourceContainerExists = await CheckContainerExists(sourceClient, sourceDatabaseName, containerName);
            if (!sourceContainerExists)
            {
                _logger.LogWarning("Source container {Container} does not exist in database {Database}, skipping migration",
                    containerName, sourceDatabaseName);
                result.Success = true;
                result.Errors.Add($"Source container '{containerName}' does not exist in database '{sourceDatabaseName}' - nothing to migrate");
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            if (!options.DryRun)
            {
                await EnsureContainerExists(destClient, destDatabaseName, containerName, partitionKeyPath);
            }

            var sourceContainer = sourceClient.GetContainer(sourceDatabaseName, containerName);

            var query = sourceContainer.GetItemQueryIterator<dynamic>("SELECT * FROM c");
            var items = new List<dynamic>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                items.AddRange(response);
            }

            result.TotalItems = items.Count;
            _logger.LogInformation("{Mode}Found {Count} items to migrate in {Container}", mode, items.Count, containerName);

            if (items.Count == 0)
            {
                _logger.LogInformation("Container {Container} is empty, nothing to migrate", containerName);
                result.Success = true;
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            if (options.DryRun)
            {
                result.Success = true;
                result.SuccessCount = items.Count;
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                _logger.LogInformation("[DRY RUN] Would migrate {Count} items in {Container}", items.Count, containerName);
                return result;
            }

            var destContainer = destClient.GetContainer(destDatabaseName, containerName);

            if (options.UseBulkOperations && items.Count > 10)
            {
                await MigrateBulkDynamicAsync(destContainer, items, partitionKeyPath, result, options, containerName);
            }
            else
            {
                await MigrateSequentialDynamicAsync(destContainer, items, partitionKeyPath, result, options, containerName);
            }

            result.Success = result.FailureCount == 0;
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            _logger.LogInformation("{Container} migration completed: {SuccessCount}/{TotalItems} successful in {Duration}",
                containerName, result.SuccessCount, result.TotalItems, result.Duration);
            return result;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error during {Container} migration", containerName);
            result.Success = false;
            result.Errors.Add($"Cosmos DB error: {ex.Message}");
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during {Container} migration", containerName);
            result.Success = false;
            result.Errors.Add($"Critical error: {ex.Message}");
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            return result;
        }
    }

    /// <summary>
    /// Migrates blob storage container from source to destination.
    /// </summary>
    public async Task<MigrationResult> MigrateBlobStorageAsync(
        string sourceStorageConnectionString,
        string destStorageConnectionString,
        string containerName,
        MigrationOptions? options = null)
    {
        options ??= new MigrationOptions();
        var stopwatch = Stopwatch.StartNew();
        var result = new MigrationResult();
        var mode = options.DryRun ? "[DRY RUN] " : "";

        try
        {
            _logger.LogInformation("{Mode}Starting blob storage migration for container: {Container}", mode, containerName);

            var sourceBlobServiceClient = new BlobServiceClient(sourceStorageConnectionString);
            var destBlobServiceClient = new BlobServiceClient(destStorageConnectionString);

            var sourceContainerClient = sourceBlobServiceClient.GetBlobContainerClient(containerName);
            var destContainerClient = destBlobServiceClient.GetBlobContainerClient(containerName);

            var sourceExists = await sourceContainerClient.ExistsAsync();
            if (!sourceExists.Value)
            {
                _logger.LogWarning("Source container {Container} does not exist, skipping migration", containerName);
                result.Success = true;
                result.Errors.Add($"Source container '{containerName}' does not exist - nothing to migrate");
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            if (!options.DryRun)
            {
                await destContainerClient.CreateIfNotExistsAsync(PublicAccessType.None);
            }

            var blobs = new List<string>();
            await foreach (var blobItem in sourceContainerClient.GetBlobsAsync())
            {
                blobs.Add(blobItem.Name);
            }

            result.TotalItems = blobs.Count;
            _logger.LogInformation("{Mode}Found {Count} blobs to migrate", mode, blobs.Count);

            if (options.DryRun)
            {
                result.Success = true;
                result.SuccessCount = blobs.Count;
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            if (blobs.Count == 0)
            {
                result.Success = true;
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                return result;
            }

            // Migrate blobs with parallel processing and retry logic
            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(MaxConcurrentBlobOperations);
            int successCount = 0;
            int failureCount = 0;

            foreach (var blobName in blobs)
            {
                await semaphore.WaitAsync();

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var sourceBlobClient = sourceContainerClient.GetBlobClient(blobName);
                        var destBlobClient = destContainerClient.GetBlobClient(blobName);

                        // Check if blob exists in destination
                        var destExists = await destBlobClient.ExistsAsync();
                        if (destExists.Value)
                        {
                            _logger.LogDebug("Blob {BlobName} already exists in destination, skipping", blobName);
                            Interlocked.Increment(ref successCount);
                            return;
                        }

                        // Retry logic for blob migration
                        int retryCount = 0;
                        bool success = false;

                        while (!success && retryCount < options.MaxRetries)
                        {
                            try
                            {
                                // Download from source and upload to destination
                                var downloadResponse = await sourceBlobClient.DownloadContentAsync();
                                var blobContent = downloadResponse.Value.Content;
                                var contentType = downloadResponse.Value.Details.ContentType;
                                var metadata = downloadResponse.Value.Details.Metadata;

                                await destBlobClient.UploadAsync(blobContent, overwrite: false);

                                // Set content type and metadata if available
                                var headers = new BlobHttpHeaders();
                                if (!string.IsNullOrEmpty(contentType))
                                {
                                    headers.ContentType = contentType;
                                }
                                await destBlobClient.SetHttpHeadersAsync(headers);

                                if (metadata != null && metadata.Count > 0)
                                {
                                    await destBlobClient.SetMetadataAsync(metadata);
                                }

                                success = true;
                                Interlocked.Increment(ref successCount);
                                _logger.LogDebug("Migrated blob: {BlobName}", blobName);
                            }
                            catch (Azure.RequestFailedException ex) when (ex.Status == 429 || ex.Status == 503)
                            {
                                // Rate limiting or service unavailable - retry with exponential backoff
                                retryCount++;
                                if (retryCount < options.MaxRetries)
                                {
                                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                                    _logger.LogWarning("Rate limited for blob {BlobName}, retry {Retry}/{MaxRetries} after {Delay}s",
                                        blobName, retryCount, options.MaxRetries, delay.TotalSeconds);
                                    await Task.Delay(delay);
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failureCount);
                        lock (result.Errors)
                        {
                            result.Errors.Add($"Failed to migrate blob {blobName}: {ex.Message}");
                        }
                        _logger.LogError(ex, "Failed to migrate blob {BlobName}", blobName);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            result.SuccessCount = successCount;
            result.FailureCount = failureCount;
            result.Success = result.FailureCount == 0;
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;

            _logger.LogInformation("Blob storage migration completed: {SuccessCount}/{TotalItems} successful",
                result.SuccessCount, result.TotalItems);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during blob storage migration");
            result.Success = false;
            result.Errors.Add($"Critical error: {ex.Message}");
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            return result;
        }
    }

    #region Private Helper Methods

    private async Task MigrateBulkDynamicAsync(
        Container destContainer,
        List<dynamic> items,
        string partitionKeyPath,
        MigrationResult result,
        MigrationOptions options,
        string containerName)
    {
        var tasks = new List<Task>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var item in items)
        {
            try
            {
                string partitionKeyValue = GetPartitionKeyValue(item, partitionKeyPath);
                tasks.Add(destContainer.UpsertItemAsync(item, new PartitionKey(partitionKeyValue))
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            Interlocked.Increment(ref successCount);
                        }
                        else
                        {
                            Interlocked.Increment(ref failureCount);
                            string itemId = item?.id?.ToString() ?? "unknown";
                            lock (result.Errors)
                            {
                                result.Errors.Add($"Failed to migrate item {itemId}: {t.Exception?.InnerException?.Message}");
                            }
                            _logger.LogError(t.Exception?.InnerException, "Failed to bulk migrate item in {Container}", containerName);
                        }
                    }));

                if (tasks.Count >= options.BulkBatchSize)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failureCount);
                string itemId = item?.id?.ToString() ?? "unknown";
                lock (result.Errors)
                {
                    result.Errors.Add($"Failed to prepare item {itemId} for migration: {ex.Message}");
                }
            }
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }

        result.SuccessCount = successCount;
        result.FailureCount = failureCount;
    }

    private async Task MigrateSequentialDynamicAsync(
        Container destContainer,
        List<dynamic> items,
        string partitionKeyPath,
        MigrationResult result,
        MigrationOptions options,
        string containerName)
    {
        foreach (var item in items)
        {
            bool success = false;
            Exception? lastException = null;

            for (int retry = 0; retry <= options.MaxRetries && !success; retry++)
            {
                try
                {
                    string partitionKeyValue = GetPartitionKeyValue(item, partitionKeyPath);
                    await destContainer.UpsertItemAsync(item, new PartitionKey(partitionKeyValue));
                    result.SuccessCount++;
                    success = true;
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests && retry < options.MaxRetries)
                {
                    var retryAfter = ex.RetryAfter ?? TimeSpan.FromSeconds(Math.Pow(2, retry));
                    _logger.LogWarning("Rate limited, retrying after {Delay}ms (attempt {Retry}/{MaxRetries})",
                        retryAfter.TotalMilliseconds, retry + 1, options.MaxRetries);
                    await Task.Delay(retryAfter);
                    lastException = ex;
                }
                catch (Exception ex) when (retry < options.MaxRetries)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retry));
                    _logger.LogWarning(ex, "Error migrating item, retrying after {Delay}ms (attempt {Retry}/{MaxRetries})",
                        delay.TotalMilliseconds, retry + 1, options.MaxRetries);
                    await Task.Delay(delay);
                    lastException = ex;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            if (!success)
            {
                result.FailureCount++;
                string itemId = item?.id?.ToString() ?? "unknown";
                result.Errors.Add($"Failed to migrate item {itemId} in {containerName} after {options.MaxRetries} retries: {lastException?.Message}");
                _logger.LogError(lastException, "Failed to migrate item in {Container} after {MaxRetries} retries",
                    containerName, options.MaxRetries);
            }
        }
    }

    private async Task<bool> CheckContainerExists(CosmosClient client, string databaseName, string containerName)
    {
        try
        {
            var container = client.GetContainer(databaseName, containerName);
            await container.ReadContainerAsync();
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private string GetPartitionKeyValue(dynamic item, string partitionKeyPath)
    {
        var propertyName = partitionKeyPath.TrimStart('/');
        var parts = propertyName.Split('/');
        dynamic current = item;

        foreach (var part in parts)
        {
            if (current is System.Text.Json.JsonElement jsonElement)
            {
                if (jsonElement.TryGetProperty(part, out var prop))
                {
                    current = prop;
                }
                else
                {
                    // Fallback to id
                    if (item is System.Text.Json.JsonElement rootElement && rootElement.TryGetProperty("id", out var idProp))
                    {
                        return idProp.ToString();
                    }
                    throw new InvalidOperationException($"Partition key path '{partitionKeyPath}' not found in item and no 'id' fallback available");
                }
            }
            else
            {
                try
                {
                    if (current == null)
                    {
                        string? itemId = item?.id?.ToString();
                        if (!string.IsNullOrEmpty(itemId))
                        {
                            return itemId;
                        }
                        throw new InvalidOperationException($"Partition key path '{partitionKeyPath}' is null and no 'id' fallback available");
                    }
                    current = ((IDictionary<string, object>)current)[part];
                }
                catch (KeyNotFoundException)
                {
                    string? itemId = item?.id?.ToString();
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        return itemId;
                    }
                    throw new InvalidOperationException($"Partition key path '{partitionKeyPath}' not found in item and no 'id' fallback available");
                }
                catch (Exception ex) when (ex is not InvalidOperationException)
                {
                    _logger.LogWarning(ex, "Unexpected exception when traversing partition key path '{PartitionKeyPath}'", partitionKeyPath);
                    string? itemId = item?.id?.ToString();
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        return itemId;
                    }
                    throw new InvalidOperationException($"Unexpected exception traversing partition key path '{partitionKeyPath}' and no 'id' fallback available", ex);
                }
            }
        }

        var result = current?.ToString();
        if (string.IsNullOrEmpty(result))
        {
            throw new InvalidOperationException($"Partition key value at path '{partitionKeyPath}' is null or empty");
        }
        return result;
    }

    private async Task EnsureContainerExists(CosmosClient client, string databaseName, string containerName, string partitionKeyPath)
    {
        try
        {
            var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            _logger.LogInformation("Database {Database} ready (created: {Created})",
                databaseName, databaseResponse.StatusCode == System.Net.HttpStatusCode.Created);

            var database = client.GetDatabase(databaseName);
            var containerResponse = await database.CreateContainerIfNotExistsAsync(containerName, partitionKeyPath);
            _logger.LogInformation("Container {Container} ready (created: {Created})",
                containerName, containerResponse.StatusCode == System.Net.HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not ensure container {Container} exists in database {Database}",
                containerName, databaseName);
        }
    }

    #endregion
}
