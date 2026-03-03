namespace Mystira.Shared.Polyglot;

/// <summary>
/// Result of a single entity backfill operation.
/// </summary>
public sealed class BackfillResult
{
    /// <summary>
    /// The entity ID that was backfilled.
    /// </summary>
    public required string EntityId { get; init; }

    /// <summary>
    /// Indicates whether the backfill was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The operation performed (Insert, Update, Skip).
    /// </summary>
    public required string Operation { get; init; }

    /// <summary>
    /// Error message if the backfill failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Duration of the operation in milliseconds.
    /// </summary>
    public long DurationMs { get; init; }

    /// <summary>
    /// Creates a successful insert result.
    /// </summary>
    public static BackfillResult Inserted(string entityId, long durationMs) => new()
    {
        EntityId = entityId,
        Success = true,
        Operation = "Insert",
        DurationMs = durationMs
    };

    /// <summary>
    /// Creates a successful update result.
    /// </summary>
    public static BackfillResult Updated(string entityId, long durationMs) => new()
    {
        EntityId = entityId,
        Success = true,
        Operation = "Update",
        DurationMs = durationMs
    };

    /// <summary>
    /// Creates a skipped result (entity already exists and is consistent).
    /// </summary>
    public static BackfillResult Skipped(string entityId) => new()
    {
        EntityId = entityId,
        Success = true,
        Operation = "Skip",
        DurationMs = 0
    };

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static BackfillResult Failed(string entityId, string errorMessage, long durationMs) => new()
    {
        EntityId = entityId,
        Success = false,
        Operation = "Failed",
        ErrorMessage = errorMessage,
        DurationMs = durationMs
    };
}
