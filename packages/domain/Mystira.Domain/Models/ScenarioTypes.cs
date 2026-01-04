using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Character metadata for scenarios - DTO-style for request/response handling.
/// This is distinct from the entity-style ScenarioCharacter.
/// </summary>
public class ScenarioCharacterMetadata
{
    // === Role IDs (stored in database) ===

    /// <summary>Gets or sets the role IDs (stored in database).</summary>
    public List<string> RoleIds { get; set; } = new();

    /// <summary>Gets the character roles (computed from RoleIds).</summary>
    public List<CharacterRole> Roles => RoleIds
        .Select(id => CharacterRole.FromValue(id))
        .Where(r => r != null)
        .Cast<CharacterRole>()
        .ToList();

    /// <summary>Gets the character role (alias for Roles for compatibility).</summary>
    public List<CharacterRole> Role => Roles;

    /// <summary>Sets roles by converting to RoleIds.</summary>
    public void SetRoles(IEnumerable<CharacterRole> roles)
    {
        RoleIds = roles.Select(r => r.Value).ToList();
    }

    // === Archetype IDs (stored in database) ===

    /// <summary>Gets or sets the archetype IDs (stored in database).</summary>
    public List<string> ArchetypeIds { get; set; } = new();

    /// <summary>Gets the character archetypes (computed from ArchetypeIds).</summary>
    public List<Archetype> Archetypes => ArchetypeIds
        .Select(id => Archetype.FromValue(id))
        .Where(a => a != null)
        .Cast<Archetype>()
        .ToList();

    /// <summary>Gets the character archetype (alias for Archetypes for compatibility).</summary>
    public List<Archetype> Archetype => Archetypes;

    /// <summary>Sets archetypes by converting to ArchetypeIds.</summary>
    public void SetArchetypes(IEnumerable<Archetype> archetypes)
    {
        ArchetypeIds = archetypes.Select(a => a.Value).ToList();
    }

    // === Species ID (stored in database) ===

    /// <summary>Gets or sets the species ID (stored in database).</summary>
    public string SpeciesId { get; set; } = string.Empty;

    /// <summary>Gets the species (computed from SpeciesId).</summary>
    public Species? Species => Species.FromValue(SpeciesId);

    /// <summary>Sets species by converting to SpeciesId.</summary>
    public void SetSpecies(Species species)
    {
        SpeciesId = species.Value;
    }

    // === Age (simple value) ===

    /// <summary>Gets or sets the age.</summary>
    public int Age { get; set; }

    // === Trait IDs (stored in database) ===

    /// <summary>Gets or sets the trait IDs (stored in database).</summary>
    public List<string> TraitIds { get; set; } = new();

    /// <summary>Gets the character traits (computed from TraitIds).</summary>
    public List<CharacterTrait> Traits => TraitIds
        .Select(id => CharacterTrait.FromValue(id))
        .Where(t => t != null)
        .Cast<CharacterTrait>()
        .ToList();

    /// <summary>Sets traits by converting to TraitIds.</summary>
    public void SetTraits(IEnumerable<CharacterTrait> traits)
    {
        TraitIds = traits.Select(t => t.Value).ToList();
    }

    // === Backstory (simple value) ===

    /// <summary>Gets or sets the backstory.</summary>
    public string Backstory { get; set; } = string.Empty;
}

/// <summary>
/// Scene music settings for scenarios.
/// </summary>
public class SceneMusicSettings
{
    /// <summary>Gets or sets the mood profile.</summary>
    public string? MoodProfile { get; set; }

    /// <summary>Gets or sets the track ID.</summary>
    public string? TrackId { get; set; }

    /// <summary>Gets or sets the volume level.</summary>
    public float? Volume { get; set; }

    /// <summary>Gets or sets whether to loop.</summary>
    public bool? Loop { get; set; }

    /// <summary>Gets or sets the fade in duration in seconds.</summary>
    public float? FadeInSeconds { get; set; }

    /// <summary>Gets or sets the fade out duration in seconds.</summary>
    public float? FadeOutSeconds { get; set; }
}

/// <summary>
/// Scene sound effect definition.
/// </summary>
public class SceneSoundEffect
{
    /// <summary>Gets or sets the sound effect ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the track ID.</summary>
    public string? TrackId { get; set; }

    /// <summary>Gets or sets the volume level.</summary>
    public float? Volume { get; set; }

    /// <summary>Gets or sets whether to loop.</summary>
    public bool? Loop { get; set; }

    /// <summary>Gets or sets the trigger type.</summary>
    public string? TriggerType { get; set; }

    /// <summary>Gets or sets the delay in seconds.</summary>
    public float? DelaySeconds { get; set; }
}

/// <summary>
/// Music palette for scenarios - allowed background-music tracks grouped by mood profile.
/// </summary>
public class MusicPalette
{
    /// <summary>Gets or sets the mood to tracks mapping.</summary>
    public Dictionary<string, List<string>> MoodTracks { get; set; } = new();

    /// <summary>Gets or sets the default mood.</summary>
    public string? DefaultMood { get; set; }
}

/// <summary>
/// YAML structure for character map export/import.
/// </summary>
public class CharacterMapYaml
{
    /// <summary>Gets or sets the characters.</summary>
    public List<CharacterMapYamlEntry> Characters { get; set; } = new();
}

/// <summary>
/// YAML entry for character map.
/// </summary>
public class CharacterMapYamlEntry
{
    /// <summary>Gets or sets the character ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the character name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the character image reference.</summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>Gets or sets the character audio reference.</summary>
    public string? Audio { get; set; }

    /// <summary>Gets or sets the character metadata.</summary>
    public CharacterMetadata Metadata { get; set; } = new();
}
