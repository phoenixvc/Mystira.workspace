using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Contracts.Stories;

/// <summary>
/// Represents a single consistency issue found in a path during dominator-based path analysis.
/// </summary>
public class DominatorPathAnalysisIssue
{
    /// <summary>
    /// The severity level of the issue (low, medium, high, critical).
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// The category of the issue (entity_consistency, time_consistency, emotional_consistency, causal_consistency, other).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The scene IDs where this issue occurs.
    /// </summary>
    public List<string> SceneIds { get; set; } = new();

    /// <summary>
    /// A brief summary of the issue.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// The suggested fix for this issue.
    /// </summary>
    public string SuggestedFix { get; set; } = string.Empty;
}

/// <summary>
/// Result of analyzing a single compressed path for consistency.
/// </summary>
public class DominatorPathAnalysisResult
{
    /// <summary>
    /// The compressed path as a sequence of scene IDs.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The full content/narrative of this path.
    /// </summary>
    public string PathContent { get; set; } = string.Empty;

    /// <summary>
    /// The consistency evaluation result for this path.
    /// </summary>
    public DominatorPathEvaluationResult? Result { get; set; }
}

/// <summary>
/// Consistency evaluation details for a path.
/// </summary>
public class DominatorPathEvaluationResult
{
    /// <summary>
    /// Overall assessment (ok, has_minor_issues, has_major_issues, broken).
    /// </summary>
    public string OverallAssessment { get; set; } = string.Empty;

    /// <summary>
    /// List of issues found.
    /// </summary>
    public List<DominatorPathAnalysisIssue> Issues { get; set; } = new();
}

/// <summary>
/// Request to evaluate dominator-based path consistency for a scenario.
/// </summary>
public class EvaluateDominatorPathConsistencyRequest
{
    /// <summary>
    /// The scenario to evaluate.
    /// </summary>
    public Scenario Scenario { get; set; } = new();

    /// <summary>
    /// Optional current chat story snapshot. If provided, the API will prefer parsing the
    /// scenario from <c>CurrentStory.Content</c> (expected to be JSON) and ignore <see cref="Scenario"/>.
    /// </summary>
    public Mystira.StoryGenerator.Contracts.Chat.StorySnapshot? CurrentStory { get; set; }
}

/// <summary>
/// Response containing the dominator path consistency analysis results.
/// </summary>
public class EvaluateDominatorPathConsistencyResponse
{
    /// <summary>
    /// Whether the evaluation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// List of path analysis results (one per compressed path).
    /// </summary>
    public List<DominatorPathAnalysisResult> PathResults { get; set; } = new();

    /// <summary>
    /// Error message if the evaluation failed.
    /// </summary>
    public string? Error { get; set; }
}
