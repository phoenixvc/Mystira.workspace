using Mystira.Domain.Models;

namespace Mystira.Authoring.Abstractions.Models.Scenario;

/// <summary>
/// Represents a character in a scenario for authoring purposes.
/// Uses Domain's CharacterMetadata for string-based metadata.
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
    /// Uses Domain CharacterMetadata (string-based).
    /// </summary>
    public CharacterMetadata Metadata { get; set; } = new();
}
