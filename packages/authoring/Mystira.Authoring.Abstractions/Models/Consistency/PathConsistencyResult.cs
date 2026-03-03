namespace Mystira.Authoring.Abstractions.Models.Consistency;

/// <summary>
/// Result of consistency evaluation for a scenario path.
/// </summary>
public class PathConsistencyResult
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
    /// <summary>Unknown issue type.</summary>
    Unknown = 0,
    /// <summary>Entity attribute or state inconsistency.</summary>
    EntityInconsistency = 1,
    /// <summary>Timeline or chronological conflict.</summary>
    TimelineConflict = 2,
    /// <summary>Character behavior or trait inconsistency.</summary>
    CharacterInconsistency = 3,
    /// <summary>Location or setting inconsistency.</summary>
    LocationInconsistency = 4,
    /// <summary>Unresolved plot thread or unexplained event.</summary>
    PlotHole = 5,
    /// <summary>Logical contradiction in the narrative.</summary>
    LogicalContradiction = 6
}

/// <summary>
/// Severity of an issue.
/// </summary>
public enum IssueSeverity
{
    /// <summary>Informational note, no action required.</summary>
    Info = 0,
    /// <summary>Warning that may need attention.</summary>
    Warning = 1,
    /// <summary>Error that should be addressed.</summary>
    Error = 2,
    /// <summary>Critical issue that must be fixed.</summary>
    Critical = 3
}
