using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// SRL-based per-scene entity classification output.
/// Mirrors the JSON schema defined in SemanticRoleLabellingService instructions.
/// </summary>
public sealed class SemanticRoleLabellingClassification
{
    [JsonPropertyName("scene_id")]
    public string SceneId { get; set; } = string.Empty;

    [JsonPropertyName("entity_classifications")]
    public List<SrlEntityClassification> EntityClassifications { get; set; } = new();
}

public sealed class SrlEntityClassification
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// One of: "character", "location", "item", "concept".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("present_in_scene")]
    public bool PresentInScene { get; set; }

    /// <summary>
    /// One of: "new", "reintroduced", "already_known", "not_present".
    /// </summary>
    [JsonPropertyName("introduction_status")]
    public string IntroductionStatus { get; set; } = string.Empty;

    /// <summary>
    /// One of: "removed", "not_removed".
    /// </summary>
    [JsonPropertyName("removal_status")]
    public string RemovalStatus { get; set; } = string.Empty;

    /// <summary>
    /// SRL-style roles: e.g., "agent", "patient", "experiencer", "stimulus", "location", "goal", etc.
    /// </summary>
    [JsonPropertyName("semantic_roles")]
    public List<string> SemanticRoles { get; set; } = new();

    /// <summary>
    /// Whether the text represents "clear_introduction", "already_known_style", "ambiguous"
    /// </summary>
    [JsonPropertyName("local_usage_style")]
    public string LocalUsageStyle { get; set; } = string.Empty;

    /// <summary>
    /// True if this entity is used as a specific named entity (proper noun) in this scene text.
    /// </summary>
    [JsonPropertyName("is_proper_noun")]
    public bool IsProperNoun { get; set; }

    /// <summary>
    /// One of: "high", "medium", "low".
    /// </summary>
    [JsonPropertyName("confidence")]
    public string Confidence { get; set; } = string.Empty;

    /// <summary>
    /// Quote or phrase from the scene that supports the decision.
    /// </summary>
    [JsonPropertyName("evidence_span")]
    public string EvidenceSpan { get; set; } = string.Empty;
}
