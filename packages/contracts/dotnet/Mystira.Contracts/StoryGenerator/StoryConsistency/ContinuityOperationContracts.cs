using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Status of an asynchronous continuity operation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContinuityOperationStatus
{
    /// <summary>Operation is queued and waiting to be processed.</summary>
    Queued,

    /// <summary>Operation is currently running.</summary>
    Running,

    /// <summary>Operation completed successfully.</summary>
    Succeeded,

    /// <summary>Operation failed.</summary>
    Failed
}

/// <summary>
/// Information about an asynchronous continuity evaluation operation.
/// Used for tracking long-running evaluations.
/// </summary>
public class ContinuityOperationInfo
{
    /// <summary>
    /// Unique identifier for this operation.
    /// </summary>
    [JsonPropertyName("operation_id")]
    public string OperationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The scenario ID being evaluated.
    /// </summary>
    [JsonPropertyName("scenario_id")]
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the operation.
    /// </summary>
    [JsonPropertyName("status")]
    public ContinuityOperationStatus Status { get; set; } = ContinuityOperationStatus.Queued;

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    [JsonPropertyName("progress_percent")]
    public int ProgressPercent { get; set; } = 0;

    /// <summary>
    /// Current step description.
    /// </summary>
    [JsonPropertyName("current_step")]
    public string? CurrentStep { get; set; }

    /// <summary>
    /// Total number of paths being evaluated.
    /// </summary>
    [JsonPropertyName("total_paths")]
    public int TotalPaths { get; set; }

    /// <summary>
    /// Number of paths evaluated so far.
    /// </summary>
    [JsonPropertyName("paths_evaluated")]
    public int PathsEvaluated { get; set; }

    /// <summary>
    /// Number of issues found so far.
    /// </summary>
    [JsonPropertyName("issues_found")]
    public int IssuesFound { get; set; }

    /// <summary>
    /// When the operation was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the operation started processing.
    /// </summary>
    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// When the operation completed (success or failure).
    /// </summary>
    [JsonPropertyName("completed_at")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// The result of the operation if completed successfully.
    /// </summary>
    [JsonPropertyName("result")]
    public EvaluateStoryContinuityResponse? Result { get; set; }
}
