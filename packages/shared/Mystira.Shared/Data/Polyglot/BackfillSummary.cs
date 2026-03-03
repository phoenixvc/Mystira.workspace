namespace Mystira.Shared.Polyglot;

/// <summary>
/// Summary of a batch backfill operation.
/// </summary>
public sealed class BackfillSummary
{
    /// <summary>
    /// The entity type that was backfilled.
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// Total number of entities processed.
    /// </summary>
    public int TotalProcessed { get; init; }

    /// <summary>
    /// Number of entities inserted.
    /// </summary>
    public int Inserted { get; init; }

    /// <summary>
    /// Number of entities updated.
    /// </summary>
    public int Updated { get; init; }

    /// <summary>
    /// Number of entities skipped (already consistent).
    /// </summary>
    public int Skipped { get; init; }

    /// <summary>
    /// Number of entities that failed to backfill.
    /// </summary>
    public int Failed { get; init; }

    /// <summary>
    /// Total duration of the backfill operation in milliseconds.
    /// </summary>
    public long TotalDurationMs { get; init; }

    /// <summary>
    /// Average duration per entity in milliseconds.
    /// </summary>
    public double AverageDurationMs => TotalProcessed > 0 ? (double)TotalDurationMs / TotalProcessed : 0;

    /// <summary>
    /// Timestamp when the backfill started.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Timestamp when the backfill completed.
    /// </summary>
    public DateTime CompletedAt { get; init; }

    /// <summary>
    /// Source backend for the backfill.
    /// </summary>
    public BackendType SourceBackend { get; init; }

    /// <summary>
    /// Target backend for the backfill.
    /// </summary>
    public BackendType TargetBackend { get; init; }

    /// <summary>
    /// List of failed entity IDs for retry.
    /// </summary>
    public IReadOnlyList<string> FailedEntityIds { get; init; } = [];

    /// <summary>
    /// Error messages for failed entities.
    /// </summary>
    public IReadOnlyDictionary<string, string> Errors { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Indicates whether the backfill was fully successful.
    /// </summary>
    public bool IsFullySuccessful => Failed == 0;

    /// <summary>
    /// Success rate as a percentage.
    /// </summary>
    public double SuccessRate => TotalProcessed > 0
        ? (double)(Inserted + Updated + Skipped) / TotalProcessed * 100
        : 100;

    /// <summary>
    /// Creates a summary from individual backfill results.
    /// </summary>
    public static BackfillSummary FromResults<TEntity>(
        IReadOnlyList<BackfillResult> results,
        BackendType source,
        BackendType target,
        DateTime startedAt,
        long totalDurationMs)
    {
        var errors = new Dictionary<string, string>();
        var failedIds = new List<string>();

        foreach (var result in results.Where(r => !r.Success))
        {
            failedIds.Add(result.EntityId);
            if (result.ErrorMessage is not null)
            {
                errors[result.EntityId] = result.ErrorMessage;
            }
        }

        return new BackfillSummary
        {
            EntityType = typeof(TEntity).FullName ?? typeof(TEntity).Name,
            TotalProcessed = results.Count,
            Inserted = results.Count(r => r.Operation == "Insert"),
            Updated = results.Count(r => r.Operation == "Update"),
            Skipped = results.Count(r => r.Operation == "Skip"),
            Failed = results.Count(r => !r.Success),
            TotalDurationMs = totalDurationMs,
            StartedAt = startedAt,
            CompletedAt = DateTime.UtcNow,
            SourceBackend = source,
            TargetBackend = target,
            FailedEntityIds = failedIds,
            Errors = errors
        };
    }
}
