using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

/// <summary>
/// Result of entity extraction from text.
/// </summary>
public class EntityExtractionResult
{
    /// <summary>
    /// Whether extraction succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Source text that was analyzed.
    /// </summary>
    [JsonPropertyName("source_text")]
    public string? SourceText { get; set; }

    /// <summary>
    /// Scene ID if extracted from a scene.
    /// </summary>
    [JsonPropertyName("scene_id")]
    public string? SceneId { get; set; }

    /// <summary>
    /// Extracted entities.
    /// </summary>
    [JsonPropertyName("entities")]
    public List<StoryEntity> Entities { get; set; } = new();

    /// <summary>
    /// Entity mentions found.
    /// </summary>
    [JsonPropertyName("mentions")]
    public List<EntityMention> Mentions { get; set; } = new();

    /// <summary>
    /// Relationships discovered.
    /// </summary>
    [JsonPropertyName("relationships")]
    public List<EntityRelationship> Relationships { get; set; } = new();

    /// <summary>
    /// Coreference chains (groups of mentions referring to same entity).
    /// </summary>
    [JsonPropertyName("coreference_chains")]
    public List<CoreferenceChain>? CoreferenceChains { get; set; }

    /// <summary>
    /// Overall confidence score.
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// Error message if extraction failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Duration of extraction in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; set; }
}

/// <summary>
/// A chain of coreferent mentions.
/// </summary>
public class CoreferenceChain
{
    /// <summary>
    /// Entity ID these mentions refer to.
    /// </summary>
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Mention texts in this chain.
    /// </summary>
    [JsonPropertyName("mentions")]
    public List<string> Mentions { get; set; } = new();

    /// <summary>
    /// The canonical/representative mention.
    /// </summary>
    [JsonPropertyName("canonical")]
    public string Canonical { get; set; } = string.Empty;
}
