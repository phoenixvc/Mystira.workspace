namespace Mystira.StoryGenerator.Contracts.Stories;

/// <summary>
/// Status values for long-running continuity operations.
/// </summary>
public static class ContinuityOperationStatus
{
    public const string Queued = "queued";
    public const string Running = "running";
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";
}

/// <summary>
/// Metadata and (optional) result for an asynchronous Story Continuity evaluation operation.
/// </summary>
public class ContinuityOperationInfo
{
    public string OperationId { get; set; } = string.Empty;
    public string Status { get; set; } = ContinuityOperationStatus.Queued;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public EvaluateStoryContinuityResponse? Result { get; set; }
    public string? Error { get; set; }
}
