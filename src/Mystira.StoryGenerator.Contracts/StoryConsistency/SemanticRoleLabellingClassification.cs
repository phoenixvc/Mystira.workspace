using Newtonsoft.Json;

namespace Mystira.StoryGenerator.Contracts.StoryConsistency;

/// <summary>
/// SRL-based per-scene entity classification output.
/// Mirrors the JSON schema defined in SemanticRoleLabellingService instructions.
/// </summary>
public sealed class SemanticRoleLabellingClassification
{
    [JsonProperty("scene_id")]
    public string SceneId { get; set; } = string.Empty;

    [JsonProperty("entity_classifications")]
    public List<SrlEntityClassification> EntityClassifications { get; set; } = new();
}

public sealed class SrlEntityClassification
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// One of: "character", "location", "item", "concept".
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("present_in_scene")]
    public bool PresentInScene { get; set; }

    /// <summary>
    /// One of: "new", "reintroduced", "already_known", "not_present".
    /// </summary>
    [JsonProperty("introduction_status")]
    public string IntroductionStatus { get; set; } = string.Empty;

    /// <summary>
    /// One of: "removed", "not_removed".
    /// </summary>
    [JsonProperty("removal_status")]
    public string RemovalStatus { get; set; } = string.Empty;

    /// <summary>
    /// SRL-style roles: e.g., "agent", "patient", "experiencer", "stimulus", "location", "goal", etc.
    /// </summary>
    [JsonProperty("semantic_roles")]
    public List<string> SemanticRoles { get; set; } = new();


    /// <summary>
    /// Whether the text represents "clear_introduction", "already_known_style", "ambiguous"
    /// </summary>
    [JsonProperty("local_usage_style")]
    public string LocalUsageStyle { get; set; } = string.Empty;

    /// <summary>
    /// One of: "high", "medium", "low".
    /// </summary>
    [JsonProperty("confidence")]
    public string Confidence { get; set; } = string.Empty;

    /// <summary>
    /// Quote or phrase from the scene that supports the decision.
    /// </summary>
    [JsonProperty("evidence_span")]
    public string EvidenceSpan { get; set; } = string.Empty;
}
