namespace Mystira.DevHub.Services.Migration;

/// <summary>
/// Options for migration operations
/// </summary>
public class MigrationOptions
{
    public bool DryRun { get; set; } = false;
    public int MaxRetries { get; set; } = 3;
    public bool UseBulkOperations { get; set; } = true;
    public int BulkBatchSize { get; set; } = 100;
}

/// <summary>
/// Interface for Cosmos DB and Blob Storage migration operations.
/// Uses dynamic/generic approach to avoid external domain dependencies.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Generic container migration - migrates any container using dynamic JSON documents.
    /// Works with any document schema without requiring typed models.
    /// </summary>
    Task<MigrationResult> MigrateContainerAsync(
        string sourceConnectionString,
        string destConnectionString,
        string sourceDatabaseName,
        string destDatabaseName,
        string containerName,
        string partitionKeyPath = "/id",
        MigrationOptions? options = null);

    /// <summary>
    /// Copies blob files from source storage to destination storage
    /// </summary>
    Task<MigrationResult> MigrateBlobStorageAsync(
        string sourceStorageConnectionString,
        string destStorageConnectionString,
        string containerName,
        MigrationOptions? options = null);
}

public class MigrationResult
{
    public bool Success { get; set; }
    public int TotalItems { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public TimeSpan Duration { get; set; }

    public override string ToString()
    {
        return $"Migration Result: {SuccessCount}/{TotalItems} successful, {FailureCount} failed, Duration: {Duration.TotalSeconds:F2}s";
    }
}
