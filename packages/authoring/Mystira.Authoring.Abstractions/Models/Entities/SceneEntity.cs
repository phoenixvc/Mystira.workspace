using System.Text.Json.Serialization;

namespace Mystira.Authoring.Abstractions.Models.Entities;

/// <summary>
/// Entity that was introduced in a specific scene.
/// </summary>
public class SceneEntity
{
    /// <summary>
    /// Type of entity.
    /// </summary>
    public SceneEntityType Type { get; set; }

    /// <summary>
    /// Name of the entity.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a proper noun (named entity).
    /// </summary>
    [JsonPropertyName("is_proper_noun")]
    public bool IsProperNoun { get; set; } = false;

    /// <summary>
    /// Confidence level of the entity classification.
    /// </summary>
    public Confidence Confidence { get; set; } = Confidence.Unknown;
}

/// <summary>
/// Type of scene entity.
/// </summary>
public enum SceneEntityType
{
    /// <summary>Unknown entity type.</summary>
    Unknown = 0,
    /// <summary>A character or person.</summary>
    Character = 1,
    /// <summary>A location or place.</summary>
    Location = 2,
    /// <summary>An item or object.</summary>
    Item = 3,
    /// <summary>An abstract concept.</summary>
    Concept = 4,
    /// <summary>An event or occurrence.</summary>
    Event = 5,
    /// <summary>An organization or group.</summary>
    Organization = 6
}

/// <summary>
/// Confidence level for entity classification.
/// </summary>
public enum Confidence
{
    /// <summary>Confidence level unknown.</summary>
    Unknown = 0,
    /// <summary>Low confidence.</summary>
    Low = 1,
    /// <summary>Medium confidence.</summary>
    Medium = 2,
    /// <summary>High confidence.</summary>
    High = 3
}
