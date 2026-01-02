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
    Unknown = 0,
    Character = 1,
    Location = 2,
    Item = 3,
    Concept = 4,
    Event = 5,
    Organization = 6
}

/// <summary>
/// Confidence level for entity classification.
/// </summary>
public enum Confidence
{
    Unknown = 0,
    Low = 1,
    Medium = 2,
    High = 3
}
