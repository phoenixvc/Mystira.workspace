using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Result of dominator path analysis.
/// </summary>
public class DominatorPathAnalysisResult
{
    /// <summary>
    /// Whether the analysis passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    /// <summary>
    /// Overall score (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }

    /// <summary>
    /// Dominator paths found.
    /// </summary>
    [JsonPropertyName("dominator_paths")]
    public List<DominatorPath> DominatorPaths { get; set; } = new();

    /// <summary>
    /// Entity introduction violations.
    /// </summary>
    [JsonPropertyName("entity_violations")]
    public List<EntityIntroductionViolation> EntityViolations { get; set; } = new();

    /// <summary>
    /// Consistency issues along dominator paths.
    /// </summary>
    [JsonPropertyName("path_issues")]
    public List<ContinuityIssue> PathIssues { get; set; } = new();

    /// <summary>
    /// Summary of the analysis.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Duration of analysis in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; set; }
}

/// <summary>
/// Represents a dominator path in the scenario graph.
/// </summary>
public class DominatorPath
{
    /// <summary>
    /// Path identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Scene IDs in the path.
    /// </summary>
    [JsonPropertyName("scene_ids")]
    public List<string> SceneIds { get; set; } = new();

    /// <summary>
    /// Whether this is a mandatory path (all players must traverse).
    /// </summary>
    [JsonPropertyName("is_mandatory")]
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Dominator node for this path.
    /// </summary>
    [JsonPropertyName("dominator_id")]
    public string? DominatorId { get; set; }
}
