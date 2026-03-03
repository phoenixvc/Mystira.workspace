using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

/// <summary>
/// Represents a mention of an entity in text.
/// </summary>
public class EntityMention
{
    /// <summary>
    /// ID of the mentioned entity.
    /// </summary>
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// The text of the mention.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Scene ID where the mention occurs.
    /// </summary>
    [JsonPropertyName("scene_id")]
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// Start position in the scene text.
    /// </summary>
    [JsonPropertyName("start_pos")]
    public int? StartPos { get; set; }

    /// <summary>
    /// End position in the scene text.
    /// </summary>
    [JsonPropertyName("end_pos")]
    public int? EndPos { get; set; }

    /// <summary>
    /// Type of mention.
    /// </summary>
    [JsonPropertyName("mention_type")]
    public MentionType MentionType { get; set; } = MentionType.Reference;

    /// <summary>
    /// Context surrounding the mention.
    /// </summary>
    [JsonPropertyName("context")]
    public string? Context { get; set; }

    /// <summary>
    /// Confidence score for this mention (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; } = 1.0;
}

/// <summary>
/// Type of entity mention.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MentionType
{
    /// <summary>
    /// First introduction of the entity.
    /// </summary>
    Introduction,

    /// <summary>
    /// Reference to a previously introduced entity.
    /// </summary>
    Reference,

    /// <summary>
    /// Pronoun reference.
    /// </summary>
    Pronoun,

    /// <summary>
    /// Descriptive reference.
    /// </summary>
    Description,

    /// <summary>
    /// Dialogue mention.
    /// </summary>
    Dialogue
}
