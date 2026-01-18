using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Represents a character within a story scenario.
/// </summary>
public class ScenarioCharacter
{
    /// <summary>
    /// Unique identifier for the character.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the character.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the character.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Role of the character in the story (e.g., "protagonist", "antagonist", "npc").
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// Character archetype (e.g., "hero", "mentor", "trickster").
    /// </summary>
    [JsonPropertyName("archetype")]
    public string? Archetype { get; set; }

    /// <summary>
    /// Personality traits of the character.
    /// </summary>
    [JsonPropertyName("traits")]
    public List<string>? Traits { get; set; }

    /// <summary>
    /// Character's goals and motivations.
    /// </summary>
    [JsonPropertyName("goals")]
    public List<string>? Goals { get; set; }

    /// <summary>
    /// Character's background story.
    /// </summary>
    [JsonPropertyName("background")]
    public string? Background { get; set; }

    /// <summary>
    /// Whether this is a player character.
    /// </summary>
    [JsonPropertyName("is_player_character")]
    public bool IsPlayerCharacter { get; set; }

    /// <summary>
    /// Additional metadata about the character.
    /// </summary>
    [JsonPropertyName("metadata")]
    public ScenarioCharacterMetadata? Metadata { get; set; }

    /// <summary>
    /// Media references for the character (e.g., portrait, voice).
    /// </summary>
    [JsonPropertyName("media")]
    public MediaReferences? Media { get; set; }
}
