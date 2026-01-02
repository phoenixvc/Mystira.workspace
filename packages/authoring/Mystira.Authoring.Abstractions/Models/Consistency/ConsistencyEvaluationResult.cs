namespace Mystira.Authoring.Abstractions.Models.Consistency;

/// <summary>
/// Result of consistency evaluation for a scenario path.
/// </summary>
public class ConsistencyEvaluationResult
{
    /// <summary>
    /// Whether the path is consistent.
    /// </summary>
    public bool IsConsistent { get; set; }

    /// <summary>
    /// Overall consistency score (0.0 to 1.0).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Issues found during evaluation.
    /// </summary>
    public List<ConsistencyIssue> Issues { get; set; } = new();

    /// <summary>
    /// The path that was evaluated.
    /// </summary>
    public List<string> EvaluatedPath { get; set; } = new();

    /// <summary>
    /// When the evaluation was performed.
    /// </summary>
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A consistency issue found during evaluation.
/// </summary>
public class ConsistencyIssue
{
    /// <summary>
    /// Type of consistency issue.
    /// </summary>
    public ConsistencyIssueType Type { get; set; }

    /// <summary>
    /// Severity of the issue.
    /// </summary>
    public IssueSeverity Severity { get; set; }

    /// <summary>
    /// Description of the issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Scene ID where the issue was detected.
    /// </summary>
    public string? SceneId { get; set; }

    /// <summary>
    /// Entity name involved (if applicable).
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Suggested fix for the issue.
    /// </summary>
    public string? SuggestedFix { get; set; }
}

/// <summary>
/// Type of consistency issue.
/// </summary>
public enum ConsistencyIssueType
{
    Unknown = 0,
    EntityInconsistency = 1,
    TimelineConflict = 2,
    CharacterInconsistency = 3,
    LocationInconsistency = 4,
    PlotHole = 5,
    LogicalContradiction = 6
}

/// <summary>
/// Severity of an issue.
/// </summary>
public enum IssueSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}
