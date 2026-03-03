using System.Text.Json.Serialization;
using Mystira.Contracts.StoryGenerator.Entities;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Contract for Prefix Summary produced by PrefixSummaryLlmService according to its JSON schema.
/// Includes back-compat computed properties used by existing aggregators.
/// </summary>
public sealed class ScenarioPathPrefixSummary
{
    // Schema-aligned properties
    [JsonPropertyName("prefix_scene_ids")]
    public List<string> PrefixSceneIds { get; set; } = new();

    [JsonPropertyName("prefix_scene_id")]
    public string PrefixSceneId { get; set; } = string.Empty;

    [JsonPropertyName("prefix_summary")]
    public string PrefixSummary { get; set; } = string.Empty;

    [JsonPropertyName("time_span")]
    public string TimeSpan { get; set; } = string.Empty; // e.g., "none", "minutes", "hours", etc.

    [JsonPropertyName("entities")]
    public List<PrefixSummaryEntity> Entities { get; set; } = new();

    [JsonIgnore]
    public List<SceneEntity> DefinitelyPresentEntities =>
        Entities.Where(e => string.Equals(e.StatusAtEnd, "active", StringComparison.OrdinalIgnoreCase))
                .Select(MapToSceneEntity)
                .ToList();

    [JsonIgnore]
    public List<SceneEntity> MaybePresentEntities =>
        Entities.Where(e => IsMaybe(e.StatusAtEnd))
                .Select(MapToSceneEntity)
                .ToList();

    [JsonIgnore]
    public List<SceneEntity> DefinitelyAbsentEntities =>
        Entities.Where(e => string.Equals(e.StatusAtEnd, "removed", StringComparison.OrdinalIgnoreCase))
                .Select(MapToSceneEntity)
                .ToList();

    [JsonIgnore]
    public List<string> Notes { get; set; } = new();

    private static bool IsMaybe(string status) =>
        string.Equals(status, "unknown", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "background_only", StringComparison.OrdinalIgnoreCase);

    private static SceneEntity MapToSceneEntity(PrefixSummaryEntity e) => new()
    {
        Name = e.CanonicalName,
        Type = e.Type,
        IsProperNoun = e.IsProperNoun,
        Confidence = Confidence.Unknown
    };
}

/// <summary>
/// Entity record inside a prefix summary (schema-aligned).
/// All enum-like fields are strings per schema.
/// </summary>
public sealed class PrefixSummaryEntity
{
    [JsonPropertyName("canonical_name")]
    public string CanonicalName { get; set; } = string.Empty;

    /// <summary> "character" | "location" | "item" | "concept" </summary>
    [JsonPropertyName("type")]
    public SceneEntityType Type { get; set; }

    [JsonPropertyName("is_proper_noun")]
    public bool IsProperNoun { get; set; }
        = false;

    [JsonPropertyName("first_introduced_scene_id")]
    public string FirstIntroducedSceneId { get; set; } = string.Empty;

    [JsonPropertyName("introduction_evidence")]
    public string IntroductionEvidence { get; set; } = string.Empty;

    /// <summary> "high" | "medium" | "low" </summary>
    [JsonPropertyName("introduction_confidence")]
    public string IntroductionConfidence { get; set; } = string.Empty;

    /// <summary> "active" | "removed" | "unknown" | "background_only" </summary>
    [JsonPropertyName("status_at_end")]
    public string StatusAtEnd { get; set; } = string.Empty;

    /// <summary> "high" | "medium" | "low" </summary>
    [JsonPropertyName("status_confidence")]
    public string StatusConfidence { get; set; } = string.Empty;

    [JsonPropertyName("known_to_player_party")]
    public bool KnownToPlayerParty { get; set; }
        = false;

    /// <summary> "high" | "medium" | "low" </summary>
    [JsonPropertyName("knowledge_confidence")]
    public string KnowledgeConfidence { get; set; } = string.Empty;

    [JsonPropertyName("role_tags")]
    public List<string> RoleTags { get; set; } = new();

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
}
