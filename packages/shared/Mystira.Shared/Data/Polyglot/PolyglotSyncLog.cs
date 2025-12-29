using Mystira.Shared.Data.Entities;

namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Entity for tracking sync operations between primary and secondary backends.
/// Provides audit trail for dual-write operations.
/// </summary>
public class PolyglotSyncLog : Entity
{
    /// <summary>
    /// The ID of the entity being synced.
    /// </summary>
    public required string SyncedEntityId { get; set; }

    /// <summary>
    /// The fully qualified type name of the entity.
    /// </summary>
    public required string EntityType { get; set; }

    /// <summary>
    /// The operation type (Insert, Update, Delete).
    /// </summary>
    public required string Operation { get; set; }

    /// <summary>
    /// Current sync status (Pending, Synced, Failed, Compensated).
    /// </summary>
    public required string Status { get; set; }

    /// <summary>
    /// The source backend where the operation originated.
    /// </summary>
    public BackendType SourceBackend { get; set; }

    /// <summary>
    /// The target backend where the operation needs to be applied.
    /// </summary>
    public BackendType TargetBackend { get; set; }

    /// <summary>
    /// Timestamp when the sync was initiated.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the sync completed (successfully or failed).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Number of retry attempts made.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Maximum retry attempts allowed.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Error message if sync failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stack trace or additional error details.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Serialized entity data for replay.
    /// </summary>
    public string? EntityPayload { get; set; }

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Indicates if compensation was attempted after failure.
    /// </summary>
    public bool CompensationAttempted { get; set; }

    /// <summary>
    /// Indicates if compensation was successful.
    /// </summary>
    public bool CompensationSucceeded { get; set; }

    /// <summary>
    /// Duration of the sync operation in milliseconds.
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Marks the sync as completed successfully.
    /// </summary>
    public void MarkSynced(long durationMs)
    {
        Status = SyncStatus.Synced;
        CompletedAt = DateTime.UtcNow;
        DurationMs = durationMs;
    }

    /// <summary>
    /// Marks the sync as failed.
    /// </summary>
    public void MarkFailed(string errorMessage, string? errorDetails = null)
    {
        Status = SyncStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
        ErrorDetails = errorDetails;
        RetryCount++;
    }

    /// <summary>
    /// Marks compensation as attempted.
    /// </summary>
    public void MarkCompensated(bool succeeded)
    {
        CompensationAttempted = true;
        CompensationSucceeded = succeeded;
        if (succeeded)
        {
            Status = SyncStatus.Compensated;
        }
    }

    /// <summary>
    /// Checks if retry is allowed.
    /// </summary>
    public bool CanRetry => RetryCount < MaxRetries && Status == SyncStatus.Failed;

    /// <summary>
    /// Creates a sync log entry for an insert operation.
    /// </summary>
    public static PolyglotSyncLog ForInsert<TEntity>(
        string entityId,
        BackendType source,
        BackendType target,
        string? correlationId = null) => new()
    {
        Id = Entities.EntityId.NewId(),
        SyncedEntityId = entityId,
        EntityType = typeof(TEntity).FullName ?? typeof(TEntity).Name,
        Operation = SyncOperation.Insert,
        Status = SyncStatus.Pending,
        SourceBackend = source,
        TargetBackend = target,
        CorrelationId = correlationId
    };

    /// <summary>
    /// Creates a sync log entry for an update operation.
    /// </summary>
    public static PolyglotSyncLog ForUpdate<TEntity>(
        string entityId,
        BackendType source,
        BackendType target,
        string? correlationId = null) => new()
    {
        Id = Entities.EntityId.NewId(),
        SyncedEntityId = entityId,
        EntityType = typeof(TEntity).FullName ?? typeof(TEntity).Name,
        Operation = SyncOperation.Update,
        Status = SyncStatus.Pending,
        SourceBackend = source,
        TargetBackend = target,
        CorrelationId = correlationId
    };

    /// <summary>
    /// Creates a sync log entry for a delete operation.
    /// </summary>
    public static PolyglotSyncLog ForDelete<TEntity>(
        string entityId,
        BackendType source,
        BackendType target,
        string? correlationId = null) => new()
    {
        Id = Entities.EntityId.NewId(),
        SyncedEntityId = entityId,
        EntityType = typeof(TEntity).FullName ?? typeof(TEntity).Name,
        Operation = SyncOperation.Delete,
        Status = SyncStatus.Pending,
        SourceBackend = source,
        TargetBackend = target,
        CorrelationId = correlationId
    };
}
