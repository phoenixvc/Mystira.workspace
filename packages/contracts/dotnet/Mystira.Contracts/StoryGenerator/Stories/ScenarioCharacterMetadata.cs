using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Additional metadata for a scenario character.
/// </summary>
public class ScenarioCharacterMetadata
{
    /// <summary>
    /// Age or age range of the character.
    /// </summary>
    [JsonPropertyName("age")]
    public string? Age { get; set; }

    /// <summary>
    /// Gender or gender identity of the character.
    /// </summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>
    /// Species or race of the character.
    /// </summary>
    [JsonPropertyName("species")]
    public string? Species { get; set; }

    /// <summary>
    /// Occupation or role in the world.
    /// </summary>
    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }

    /// <summary>
    /// Physical appearance description.
    /// </summary>
    [JsonPropertyName("appearance")]
    public string? Appearance { get; set; }

    /// <summary>
    /// Special abilities or skills.
    /// </summary>
    [JsonPropertyName("abilities")]
    public List<string>? Abilities { get; set; }

    /// <summary>
    /// Relationships with other characters.
    /// </summary>
    [JsonPropertyName("relationships")]
    public Dictionary<string, string>? Relationships { get; set; }

    /// <summary>
    /// Starting compass values for player characters.
    /// </summary>
    [JsonPropertyName("initial_compass")]
    public Dictionary<string, int>? InitialCompass { get; set; }

    /// <summary>
    /// Custom properties for extensibility.
    /// </summary>
    [JsonPropertyName("custom")]
    public Dictionary<string, object>? Custom { get; set; }
}
