using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Request to evaluate consistency along dominator paths in a scenario.
/// Dominator paths are critical paths where one scene dominates (must precede) another.
/// </summary>
public class EvaluateDominatorPathConsistencyRequest
{
    /// <summary>
    /// The ID of the scenario to evaluate.
    /// </summary>
    [JsonPropertyName("scenario_id")]
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Optional specific scene IDs to analyze dominator paths for.
    /// If null, analyzes dominator paths for all scenes.
    /// </summary>
    [JsonPropertyName("target_scene_ids")]
    public List<string>? TargetSceneIds { get; set; }

    /// <summary>
    /// Whether to include entity state tracking along dominator paths.
    /// </summary>
    [JsonPropertyName("include_entity_tracking")]
    public bool IncludeEntityTracking { get; set; } = true;

    /// <summary>
    /// Whether to compute prefix summaries for dominator path prefixes.
    /// </summary>
    [JsonPropertyName("include_prefix_summaries")]
    public bool IncludePrefixSummaries { get; set; } = true;

    /// <summary>
    /// Maximum depth of dominator tree to analyze. 0 means no limit.
    /// </summary>
    [JsonPropertyName("max_depth")]
    public int MaxDepth { get; set; } = 0;
}

/// <summary>
/// Response from dominator path consistency evaluation.
/// </summary>
public class EvaluateDominatorPathConsistencyResponse
{
    /// <summary>
    /// The ID of the evaluated scenario.
    /// </summary>
    [JsonPropertyName("scenario_id")]
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the evaluation completed successfully.
    /// </summary>
    [JsonPropertyName("is_successful")]
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Overall assessment of dominator path consistency.
    /// </summary>
    [JsonPropertyName("overall_assessment")]
    public string OverallAssessment { get; set; } = "ok";

    /// <summary>
    /// Results for each dominator path analyzed.
    /// </summary>
    [JsonPropertyName("path_results")]
    public List<DominatorPathAnalysisResult> PathResults { get; set; } = new();

    /// <summary>
    /// Aggregated issues found across all dominator paths.
    /// </summary>
    [JsonPropertyName("issues")]
    public List<StoryContinuityIssue> Issues { get; set; } = new();

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
/// Analysis result for a single dominator path.
/// A dominator path represents scenes that must be visited in sequence.
/// </summary>
public class DominatorPathAnalysisResult
{
    /// <summary>
    /// The scene IDs that form this dominator path.
    /// </summary>
    [JsonPropertyName("path_scene_ids")]
    public List<string> PathSceneIds { get; set; } = new();

    /// <summary>
    /// The target scene that this path leads to (the dominated scene).
    /// </summary>
    [JsonPropertyName("target_scene_id")]
    public string TargetSceneId { get; set; } = string.Empty;

    /// <summary>
    /// The immediate dominator scene ID for the target scene.
    /// </summary>
    [JsonPropertyName("immediate_dominator_id")]
    public string? ImmediateDominatorId { get; set; }

    /// <summary>
    /// Whether this dominator path is consistent.
    /// </summary>
    [JsonPropertyName("is_consistent")]
    public bool IsConsistent { get; set; } = true;

    /// <summary>
    /// Consistency score for this path (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("consistency_score")]
    public double ConsistencyScore { get; set; } = 1.0;

    /// <summary>
    /// Entity state at the end of this dominator path (guaranteed state).
    /// </summary>
    [JsonPropertyName("guaranteed_entity_state")]
    public DominatorPathEntityState? GuaranteedEntityState { get; set; }

    /// <summary>
    /// Prefix summary for this dominator path, if computed.
    /// </summary>
    [JsonPropertyName("prefix_summary")]
    public ScenarioPathPrefixSummary? PrefixSummary { get; set; }

    /// <summary>
    /// Issues found specific to this dominator path.
    /// </summary>
    [JsonPropertyName("issues")]
    public List<StoryContinuityIssue> Issues { get; set; } = new();

    /// <summary>
    /// Detailed consistency evaluation result if available.
    /// </summary>
    [JsonPropertyName("consistency_result")]
    public ConsistencyEvaluationResult? ConsistencyResult { get; set; }
}

/// <summary>
/// Entity state information at a point along a dominator path.
/// </summary>
public class DominatorPathEntityState
{
    /// <summary>
    /// The scene ID where this state applies.
    /// </summary>
    [JsonPropertyName("scene_id")]
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// Entities guaranteed to be present (must have been introduced on all paths).
    /// </summary>
    [JsonPropertyName("guaranteed_present")]
    public List<DominatorPathEntity> GuaranteedPresent { get; set; } = new();

    /// <summary>
    /// Entities that may or may not be present (introduced on some paths).
    /// </summary>
    [JsonPropertyName("possibly_present")]
    public List<DominatorPathEntity> PossiblyPresent { get; set; } = new();

    /// <summary>
    /// Entities guaranteed to be absent (removed on all paths).
    /// </summary>
    [JsonPropertyName("guaranteed_absent")]
    public List<DominatorPathEntity> GuaranteedAbsent { get; set; } = new();
}

/// <summary>
/// Entity information tracked along a dominator path.
/// </summary>
public class DominatorPathEntity
{
    /// <summary>
    /// Canonical name of the entity.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Entity type: character, location, item, concept.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a proper noun.
    /// </summary>
    [JsonPropertyName("is_proper_noun")]
    public bool IsProperNoun { get; set; }

    /// <summary>
    /// Scene ID where the entity was first introduced.
    /// </summary>
    [JsonPropertyName("introduced_in")]
    public string? IntroducedIn { get; set; }

    /// <summary>
    /// Scene ID where the entity was removed (if applicable).
    /// </summary>
    [JsonPropertyName("removed_in")]
    public string? RemovedIn { get; set; }

    /// <summary>
    /// Current status: active, removed, unknown.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "active";
}
