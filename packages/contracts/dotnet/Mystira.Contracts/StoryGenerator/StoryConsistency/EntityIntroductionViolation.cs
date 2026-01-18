using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

/// <summary>
/// Represents a violation where an entity is referenced before introduction.
/// </summary>
public class EntityIntroductionViolation
{
    /// <summary>
    /// Unique identifier for the violation.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the entity.
    /// </summary>
    [JsonPropertyName("entity_name")]
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity.
    /// </summary>
    [JsonPropertyName("entity_type")]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Scene where the entity is first referenced.
    /// </summary>
    [JsonPropertyName("referenced_in_scene")]
    public string ReferencedInScene { get; set; } = string.Empty;

    /// <summary>
    /// Scene where the entity should have been introduced.
    /// </summary>
    [JsonPropertyName("expected_introduction_scene")]
    public string? ExpectedIntroductionScene { get; set; }

    /// <summary>
    /// Scene where the entity is actually introduced (if any).
    /// </summary>
    [JsonPropertyName("actual_introduction_scene")]
    public string? ActualIntroductionScene { get; set; }

    /// <summary>
    /// The path where this violation occurs.
    /// </summary>
    [JsonPropertyName("path")]
    public List<string>? Path { get; set; }

    /// <summary>
    /// Description of the violation.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity of the violation.
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "medium";

    /// <summary>
    /// Suggested fix.
    /// </summary>
    [JsonPropertyName("suggested_fix")]
    public string? SuggestedFix { get; set; }
}
