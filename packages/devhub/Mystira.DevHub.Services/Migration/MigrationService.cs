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

            // Get the actual partition key paths from the destination container if it exists
            // This prevents mismatches when the destination was created with a different partition key
            List<string> actualPartitionKeyPaths = new List<string> { partitionKeyPath };
            if (!options.DryRun)
            {
                actualPartitionKeyPaths = await EnsureContainerExistsAndGetPartitionKeys(destClient, destDatabaseName, containerName, partitionKeyPath);
                _logger.LogInformation("Using partition key paths: [{PartitionKeyPaths}] for container {Container}",
                    string.Join(", ", actualPartitionKeyPaths), containerName);
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
                await MigrateBulkDynamicAsync(destContainer, items, actualPartitionKeyPaths, result, options, containerName);
            }
            else
            {
                await MigrateSequentialDynamicAsync(destContainer, items, actualPartitionKeyPaths, result, options, containerName);
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
        List<string> partitionKeyPaths,
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
                // Ensure the document has partition key fields (inject if missing)
                var docWithPartitionKey = EnsurePartitionKeyField(item, partitionKeyPaths);
                PartitionKey partitionKey = BuildPartitionKey(item, partitionKeyPaths);
                // Capture itemId before the lambda to avoid dynamic dispatch issues
                string itemId = item?.id?.ToString() ?? "unknown";
                // Cast to Task to break the dynamic dispatch chain
                Task upsertTask = (Task)destContainer.UpsertItemAsync(docWithPartitionKey, partitionKey);
                tasks.Add(upsertTask.ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            Interlocked.Increment(ref successCount);
                        }
                        else
                        {
                            Interlocked.Increment(ref failureCount);
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
        List<string> partitionKeyPaths,
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
                    // Ensure the document has partition key fields (inject if missing)
                    var docWithPartitionKey = EnsurePartitionKeyField(item, partitionKeyPaths);
                    PartitionKey partitionKey = BuildPartitionKey(item, partitionKeyPaths);
                    await destContainer.UpsertItemAsync(docWithPartitionKey, partitionKey);
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

    /// <summary>
    /// Gets the partition key value from a document, or derives it from the id pattern.
    /// For documents with id like "EntityType|guid", extracts "EntityType" if the partition key path doesn't exist.
    /// </summary>
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
                    // Partition key path not found - try to derive from id pattern
                    return DerivePartitionKeyFromId(item, partitionKeyPath);
                }
            }
            else
            {
                try
                {
                    if (current == null)
                    {
                        return DerivePartitionKeyFromId(item, partitionKeyPath);
                    }
                    current = ((IDictionary<string, object>)current)[part];
                }
                catch (KeyNotFoundException)
                {
                    return DerivePartitionKeyFromId(item, partitionKeyPath);
                }
                catch (Exception ex) when (ex is not InvalidOperationException)
                {
                    _logger.LogWarning(ex, "Unexpected exception when traversing partition key path '{PartitionKeyPath}'", partitionKeyPath);
                    return DerivePartitionKeyFromId(item, partitionKeyPath);
                }
            }
        }

        var result = current?.ToString();
        if (string.IsNullOrEmpty(result))
        {
            return DerivePartitionKeyFromId(item, partitionKeyPath);
        }
        return result!;
    }

    /// <summary>
    /// Derives a partition key value from the document's id.
    /// If id follows pattern "EntityType|guid", extracts "EntityType".
    /// Otherwise, returns the full id.
    /// </summary>
    private string DerivePartitionKeyFromId(dynamic item, string partitionKeyPath)
    {
        string? itemId = null;

        if (item is System.Text.Json.JsonElement rootElement && rootElement.TryGetProperty("id", out var idProp))
        {
            itemId = idProp.ToString();
        }
        else
        {
            itemId = item?.id?.ToString();
        }

        if (string.IsNullOrEmpty(itemId))
        {
            throw new InvalidOperationException($"Partition key path '{partitionKeyPath}' not found and no 'id' available to derive from");
        }

        // Check for discriminator pattern: "EntityType|guid"
        var pipeIndex = itemId.IndexOf('|');
        if (pipeIndex > 0)
        {
            var entityType = itemId.Substring(0, pipeIndex);
            _logger.LogDebug("Derived partition key '{PartitionKey}' from id pattern '{Id}'", entityType, itemId);
            return entityType;
        }

        // No discriminator pattern, use full id
        return itemId;
    }

    /// <summary>
    /// Ensures the document has the partition key field(s).
    /// If the partition key field doesn't exist, adds it with the derived value.
    /// Returns a dictionary representation of the document with the partition key field.
    /// </summary>
    private Dictionary<string, object?> EnsurePartitionKeyField(dynamic item, List<string> partitionKeyPaths)
    {
        Dictionary<string, object?> doc;

        if (item is System.Text.Json.JsonElement jsonElement)
        {
            // Convert JsonElement to mutable dictionary
            doc = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonElement.GetRawText())
                  ?? new Dictionary<string, object?>();
        }
        else
        {
            // Try to serialize and deserialize to get a clean dictionary
            var json = System.Text.Json.JsonSerializer.Serialize(item);
            doc = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(json)
                  ?? new Dictionary<string, object?>();
        }

        foreach (var path in partitionKeyPaths)
        {
            var propertyName = path.TrimStart('/');

            // Only handle simple (non-nested) partition key paths for injection
            if (!propertyName.Contains('/') && !doc.ContainsKey(propertyName))
            {
                // Get the value we would use for this partition key
                var value = GetPartitionKeyValue(item, path);
                doc[propertyName] = value;
                _logger.LogDebug("Injected partition key field '{Field}' with value '{Value}'", propertyName, value);
            }
        }

        return doc;
    }

    /// <summary>
    /// Ensures the container exists and returns the actual partition key paths.
    /// If the container already exists, reads its partition key configuration to avoid mismatches.
    /// Returns a list of paths to support hierarchical partition keys.
    /// </summary>
    private async Task<List<string>> EnsureContainerExistsAndGetPartitionKeys(CosmosClient client, string databaseName, string containerName, string defaultPartitionKeyPath)
    {
        try
        {
            var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(databaseName);
            _logger.LogInformation("Database {Database} ready (created: {Created})",
                databaseName, databaseResponse.StatusCode == System.Net.HttpStatusCode.Created);

            var database = client.GetDatabase(databaseName);

            // First, check if container already exists and get its partition key configuration
            try
            {
                var container = database.GetContainer(containerName);
                var containerProperties = await container.ReadContainerAsync();

                // Check for hierarchical partition keys first (PartitionKeyPaths collection)
                var partitionKeyPaths = containerProperties.Resource.PartitionKeyPaths;
                if (partitionKeyPaths != null && partitionKeyPaths.Count > 0)
                {
                    _logger.LogInformation("Container {Container} has partition key paths: [{PartitionKeys}] (count: {Count})",
                        containerName, string.Join(", ", partitionKeyPaths), partitionKeyPaths.Count);
                    return partitionKeyPaths.ToList();
                }

                // Fall back to singular PartitionKeyPath for older containers
                var existingPartitionKeyPath = containerProperties.Resource.PartitionKeyPath;
                if (!string.IsNullOrEmpty(existingPartitionKeyPath))
                {
                    _logger.LogInformation("Container {Container} already exists with single partition key: {PartitionKey}",
                        containerName, existingPartitionKeyPath);
                    return new List<string> { existingPartitionKeyPath };
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Container doesn't exist, create it with the default partition key
                _logger.LogInformation("Container {Container} does not exist, creating with partition key: {PartitionKey}",
                    containerName, defaultPartitionKeyPath);
            }

            var containerResponse = await database.CreateContainerIfNotExistsAsync(containerName, defaultPartitionKeyPath);
            _logger.LogInformation("Container {Container} ready (created: {Created})",
                containerName, containerResponse.StatusCode == System.Net.HttpStatusCode.Created);

            return new List<string> { defaultPartitionKeyPath };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not ensure container {Container} exists in database {Database}, using default partition key",
                containerName, databaseName);
            return new List<string> { defaultPartitionKeyPath };
        }
    }

    /// <summary>
    /// Builds a PartitionKey from item values based on one or more partition key paths.
    /// Supports hierarchical partition keys.
    /// </summary>
    private PartitionKey BuildPartitionKey(dynamic item, List<string> partitionKeyPaths)
    {
        if (partitionKeyPaths.Count == 1)
        {
            // Single partition key - use simple PartitionKey
            var value = GetPartitionKeyValue(item, partitionKeyPaths[0]);
            return new PartitionKey(value);
        }

        // Hierarchical partition key - use PartitionKeyBuilder
        var builder = new PartitionKeyBuilder();
        foreach (var path in partitionKeyPaths)
        {
            var value = GetPartitionKeyValue(item, path);
            builder.Add(value);
        }
        return builder.Build();
    }

    #endregion
}
