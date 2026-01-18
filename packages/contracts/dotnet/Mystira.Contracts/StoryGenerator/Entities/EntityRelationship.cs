using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

/// <summary>
/// Represents a relationship between two entities.
/// </summary>
public class EntityRelationship
{
    /// <summary>
    /// ID of the source entity.
    /// </summary>
    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the target entity.
    /// </summary>
    [JsonPropertyName("target_id")]
    public string TargetId { get; set; } = string.Empty;

    /// <summary>
    /// Type of relationship.
    /// </summary>
    [JsonPropertyName("type")]
    public RelationshipType Type { get; set; }

    /// <summary>
    /// Custom relationship label.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Description of the relationship.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the relationship is bidirectional.
    /// </summary>
    [JsonPropertyName("bidirectional")]
    public bool Bidirectional { get; set; }

    /// <summary>
    /// Strength of the relationship (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("strength")]
    public double Strength { get; set; } = 0.5;

    /// <summary>
    /// Scene where the relationship is established.
    /// </summary>
    [JsonPropertyName("established_in")]
    public string? EstablishedIn { get; set; }

    /// <summary>
    /// Whether the relationship changes during the story.
    /// </summary>
    [JsonPropertyName("dynamic")]
    public bool Dynamic { get; set; }
}

/// <summary>
/// Type of entity relationship.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RelationshipType
{
    /// <summary>
    /// Family relationship.
    /// </summary>
    Family,

    /// <summary>
    /// Friendship.
    /// </summary>
    Friend,

    /// <summary>
    /// Enemy or adversarial.
    /// </summary>
    Enemy,

    /// <summary>
    /// Romantic relationship.
    /// </summary>
    Romantic,

    /// <summary>
    /// Professional/work relationship.
    /// </summary>
    Professional,

    /// <summary>
    /// Mentor/student relationship.
    /// </summary>
    Mentorship,

    /// <summary>
    /// Ownership (for items/locations).
    /// </summary>
    Owns,

    /// <summary>
    /// Located in (for entities in locations).
    /// </summary>
    LocatedIn,

    /// <summary>
    /// Part of (membership in organization).
    /// </summary>
    MemberOf,

    /// <summary>
    /// Associated with.
    /// </summary>
    AssociatedWith,

    /// <summary>
    /// Other relationship type.
    /// </summary>
    Other
}
