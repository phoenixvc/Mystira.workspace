namespace Mystira.Authoring.Abstractions.Models.Scenario;

/// <summary>
/// Represents a character in a scenario.
/// </summary>
public class ScenarioCharacter
{
    /// <summary>
    /// Unique identifier for the character.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the character.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Path or URL to character image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Path or URL to character audio/voice.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Additional metadata about the character.
    /// </summary>
    public ScenarioCharacterMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Metadata for a scenario character.
/// </summary>
public class ScenarioCharacterMetadata
{
    /// <summary>
    /// Roles the character plays in the story.
    /// </summary>
    public List<string> Role { get; set; } = new();

    /// <summary>
    /// Archetypes the character represents.
    /// </summary>
    public List<string> Archetype { get; set; } = new();

    /// <summary>
    /// Species or race of the character.
    /// </summary>
    public string Species { get; set; } = string.Empty;

    /// <summary>
    /// Age of the character.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Personality traits.
    /// </summary>
    public List<string> Traits { get; set; } = new();

    /// <summary>
    /// Character's backstory.
    /// </summary>
    public string Backstory { get; set; } = string.Empty;
}
