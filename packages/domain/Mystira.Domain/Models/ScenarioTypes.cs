using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Character metadata for scenarios - DTO-style for request/response handling.
/// This is distinct from the entity-style ScenarioCharacter.
/// </summary>
public class ScenarioCharacterMetadata
{
    public List<string> Role { get; set; } = new();
    public List<Archetype> Archetype { get; set; } = new();
    public string Species { get; set; } = string.Empty;
    public int Age { get; set; }
    public List<string> Traits { get; set; } = new();
    public string Backstory { get; set; } = string.Empty;
}

/// <summary>
/// Scene music settings for scenarios.
/// </summary>
public class SceneMusicSettings
{
    public string? MoodProfile { get; set; }
    public string? TrackId { get; set; }
    public float? Volume { get; set; }
    public bool? Loop { get; set; }
    public float? FadeInSeconds { get; set; }
    public float? FadeOutSeconds { get; set; }
}

/// <summary>
/// Scene sound effect definition.
/// </summary>
public class SceneSoundEffect
{
    public string Id { get; set; } = string.Empty;
    public string? TrackId { get; set; }
    public float? Volume { get; set; }
    public bool? Loop { get; set; }
    public string? TriggerType { get; set; }
    public float? DelaySeconds { get; set; }
}

/// <summary>
/// Music palette for scenarios - allowed background-music tracks grouped by mood profile.
/// </summary>
public class MusicPalette
{
    public Dictionary<string, List<string>> MoodTracks { get; set; } = new();
    public string? DefaultMood { get; set; }
}

/// <summary>
/// YAML structure for character map export/import
/// </summary>
public class CharacterMapYaml
{
    public List<CharacterMapYamlEntry> Characters { get; set; } = new();
}

/// <summary>
/// YAML entry for character map
/// </summary>
public class CharacterMapYamlEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string? Audio { get; set; }
    public CharacterMetadata Metadata { get; set; } = new();
}
