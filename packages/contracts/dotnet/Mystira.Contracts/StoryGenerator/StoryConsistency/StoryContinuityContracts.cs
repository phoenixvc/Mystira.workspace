using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Request to evaluate story continuity for a scenario.
/// </summary>
public class EvaluateStoryContinuityRequest
{
    /// <summary>
    /// The ID of the scenario to evaluate.
    /// </summary>
    [JsonPropertyName("scenario_id")]
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Optional specific paths to evaluate. If null, all paths in the scenario will be evaluated.
    /// </summary>
    [JsonPropertyName("paths")]
    public List<ScenarioPath>? Paths { get; set; }

    /// <summary>
    /// Whether to include entity continuity analysis.
    /// </summary>
    [JsonPropertyName("include_entity_analysis")]
    public bool IncludeEntityAnalysis { get; set; } = true;

    /// <summary>
    /// Whether to include semantic role labelling analysis.
    /// </summary>
    [JsonPropertyName("include_srl_analysis")]
    public bool IncludeSrlAnalysis { get; set; } = true;

    /// <summary>
    /// Maximum number of paths to evaluate (for large scenarios). 0 means no limit.
    /// </summary>
    [JsonPropertyName("max_paths")]
    public int MaxPaths { get; set; } = 0;
}

/// <summary>
/// Response from story continuity evaluation.
/// </summary>
public class EvaluateStoryContinuityResponse
{
    /// <summary>
    /// The ID of the evaluated scenario.
    /// </summary>
    [JsonPropertyName("scenario_id")]
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Overall continuity assessment: ok, has_minor_issues, has_major_issues, broken.
    /// </summary>
    [JsonPropertyName("overall_assessment")]
    public string OverallAssessment { get; set; } = "ok";

    /// <summary>
    /// Overall continuity score (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("continuity_score")]
    public double ContinuityScore { get; set; } = 1.0;

    /// <summary>
    /// Whether the evaluation completed successfully.
    /// </summary>
    [JsonPropertyName("is_successful")]
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// List of continuity issues found.
    /// </summary>
    [JsonPropertyName("issues")]
    public List<StoryContinuityIssue> Issues { get; set; } = new();

    /// <summary>
    /// Per-path evaluation results.
    /// </summary>
    [JsonPropertyName("path_results")]
    public List<PathConsistencyEvaluationResult> PathResults { get; set; } = new();

    /// <summary>
    /// Entity continuity issues found during analysis.
    /// </summary>
    [JsonPropertyName("entity_issues")]
    public List<EntityContinuityIssue> EntityIssues { get; set; } = new();

    /// <summary>
    /// When the evaluation was performed.
    /// </summary>
    [JsonPropertyName("evaluated_at")]
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Error message if the evaluation failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Represents a story continuity issue found during evaluation.
/// This is an API-level DTO that provides a unified view of continuity problems.
/// </summary>
public class StoryContinuityIssue
{
    /// <summary>
    /// Unique identifier for this issue.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The type of continuity issue.
    /// </summary>
    [JsonPropertyName("issue_type")]
    public StoryContinuityIssueType IssueType { get; set; }

    /// <summary>
    /// Severity: low, medium, high, critical.
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "medium";

    /// <summary>
    /// Category: entity_consistency, time_consistency, emotional_consistency, causal_consistency, narrative_consistency, other.
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The scene IDs where this issue manifests.
    /// </summary>
    [JsonPropertyName("scene_ids")]
    public List<string> SceneIds { get; set; } = new();

    /// <summary>
    /// The path where this issue was found (if path-specific).
    /// </summary>
    [JsonPropertyName("path")]
    public List<string>? Path { get; set; }

    /// <summary>
    /// The entity involved (if entity-related issue).
    /// </summary>
    [JsonPropertyName("entity_name")]
    public string? EntityName { get; set; }

    /// <summary>
    /// The entity type (character, location, item, concept) if applicable.
    /// </summary>
    [JsonPropertyName("entity_type")]
    public string? EntityType { get; set; }

    /// <summary>
    /// Brief summary of the issue.
    /// </summary>
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the issue.
    /// </summary>
    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Evidence from the text supporting this issue.
    /// </summary>
    [JsonPropertyName("evidence_span")]
    public string? EvidenceSpan { get; set; }

    /// <summary>
    /// Suggested fix for the issue.
    /// </summary>
    [JsonPropertyName("suggested_fix")]
    public string? SuggestedFix { get; set; }

    /// <summary>
    /// Confidence in this issue assessment: high, medium, low.
    /// </summary>
    [JsonPropertyName("confidence")]
    public string Confidence { get; set; } = "medium";
}

/// <summary>
/// Types of story continuity issues.
/// </summary>
public enum StoryContinuityIssueType
{
    /// <summary>Entity used but not guaranteed to have been introduced.</summary>
    EntityNotIntroduced,

    /// <summary>Entity reintroduced when already known.</summary>
    EntityReintroduced,

    /// <summary>Entity referenced as removed but not confirmed present.</summary>
    EntityIncorrectlyRemoved,

    /// <summary>New entity used as if already known.</summary>
    EntityUsedAsKnown,

    /// <summary>Ambiguous entity usage.</summary>
    EntityAmbiguousUsage,

    /// <summary>Time-related inconsistency.</summary>
    TimeInconsistency,

    /// <summary>Causal logic error.</summary>
    CausalInconsistency,

    /// <summary>Emotional or tonal inconsistency.</summary>
    EmotionalInconsistency,

    /// <summary>General narrative consistency issue.</summary>
    NarrativeInconsistency,

    /// <summary>Other type of consistency issue.</summary>
    Other
}

/// <summary>
/// Filter criteria for querying story continuity issues.
/// </summary>
public class StoryContinuityIssueFilter
{
    /// <summary>
    /// Filter by scenario ID.
    /// </summary>
    [JsonPropertyName("scenario_id")]
    public string? ScenarioId { get; set; }

    /// <summary>
    /// Filter by issue types.
    /// </summary>
    [JsonPropertyName("issue_types")]
    public List<StoryContinuityIssueType>? IssueTypes { get; set; }

    /// <summary>
    /// Filter by minimum severity: low, medium, high, critical.
    /// </summary>
    [JsonPropertyName("min_severity")]
    public string? MinSeverity { get; set; }

    /// <summary>
    /// Filter by categories.
    /// </summary>
    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    /// <summary>
    /// Filter by specific scene IDs (issues affecting any of these scenes).
    /// </summary>
    [JsonPropertyName("scene_ids")]
    public List<string>? SceneIds { get; set; }

    /// <summary>
    /// Filter by entity name.
    /// </summary>
    [JsonPropertyName("entity_name")]
    public string? EntityName { get; set; }

    /// <summary>
    /// Filter by entity type.
    /// </summary>
    [JsonPropertyName("entity_type")]
    public string? EntityType { get; set; }

    /// <summary>
    /// Filter by minimum confidence: low, medium, high.
    /// </summary>
    [JsonPropertyName("min_confidence")]
    public string? MinConfidence { get; set; }

    /// <summary>
    /// Maximum number of issues to return.
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Number of issues to skip (for pagination).
    /// </summary>
    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}
