using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

/// <summary>
/// Represents an entity in a story.
/// </summary>
public class StoryEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Canonical name of the entity.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntityType Type { get; set; }

    /// <summary>
    /// Description of the entity.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Alternative names or aliases.
    /// </summary>
    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }

    /// <summary>
    /// Attributes of the entity.
    /// </summary>
    [JsonPropertyName("attributes")]
    public Dictionary<string, string>? Attributes { get; set; }

    /// <summary>
    /// Scene where the entity is first introduced.
    /// </summary>
    [JsonPropertyName("introduced_in")]
    public string? IntroducedIn { get; set; }

    /// <summary>
    /// Scenes where the entity appears.
    /// </summary>
    [JsonPropertyName("appears_in")]
    public List<string>? AppearsIn { get; set; }

    /// <summary>
    /// Relationships with other entities.
    /// </summary>
    [JsonPropertyName("relationships")]
    public List<EntityRelationship>? Relationships { get; set; }

    /// <summary>
    /// Importance score (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("importance")]
    public double Importance { get; set; } = 0.5;

    /// <summary>
    /// Custom metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}
