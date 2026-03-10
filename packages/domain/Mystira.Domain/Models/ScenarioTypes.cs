using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Character metadata for scenarios - stores string IDs for persistence, exposes value objects for type safety.
/// </summary>
/// <remarks>
/// <para>
/// This class uses a "store IDs, expose value objects" pattern for EF Core compatibility:
/// </para>
/// <list type="bullet">
///   <item><description>String ID properties (<c>RoleIds</c>, <c>ArchetypeIds</c>, <c>SpeciesId</c>, <c>TraitIds</c>) are persisted to the database</description></item>
///   <item><description>Value object properties (<c>Roles</c>, <c>Archetypes</c>, <c>Species</c>, <c>Traits</c>) are computed getters for type-safe access</description></item>
/// </list>
/// <para>
/// <b>Usage Examples:</b>
/// </para>
/// <code>
/// // Option 1: Set string IDs directly (for deserialization/database)
/// var metadata = new ScenarioCharacterMetadata
/// {
///     RoleIds = new List&lt;string&gt; { "mentor", "guardian" },
///     ArchetypeIds = new List&lt;string&gt; { "the_listener" },
///     SpeciesId = "elf",
///     TraitIds = new List&lt;string&gt; { "wise", "calm" },
///     Age = 150,
///     Backstory = "An ancient elf sage..."
/// };
///
/// // Option 2: Use setter methods with value objects (for type-safe code)
/// var metadata = new ScenarioCharacterMetadata { Age = 150, Backstory = "..." };
/// metadata.SetRoles(new[] { CharacterRole.Mentor, CharacterRole.Guardian });
/// metadata.SetArchetypes(new[] { Archetype.TheListener });
/// metadata.SetSpecies(Species.Elf);
/// metadata.SetTraits(new[] { CharacterTrait.Wise, CharacterTrait.Calm });
///
/// // Read value objects (always use these for type safety)
/// CharacterRole firstRole = metadata.Roles.First();
/// Species? species = metadata.Species;
/// </code>
/// </remarks>
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
    public List<ValueObjects.Archetype> Archetypes => ArchetypeIds
        .Select(id => ValueObjects.Archetype.FromValue(id))
        .Where(a => a != null)
        .Cast<ValueObjects.Archetype>()
        .ToList();

    /// <summary>Gets the character archetype (alias for Archetypes for compatibility).</summary>
    public List<ValueObjects.Archetype> Archetype => Archetypes;

    /// <summary>Sets archetypes by converting to ArchetypeIds.</summary>
    public void SetArchetypes(IEnumerable<ValueObjects.Archetype> archetypes)
    {
        ArchetypeIds = archetypes.Select(a => a.Value).ToList();
    }

    // === Species ID (stored in database) ===

    /// <summary>Gets or sets the species ID (stored in database).</summary>
    public string SpeciesId { get; set; } = string.Empty;

    /// <summary>Gets the species (computed from SpeciesId).</summary>
    public ValueObjects.Species? Species => ValueObjects.Species.FromValue(SpeciesId);

    /// <summary>Sets species by converting to SpeciesId.</summary>
    public void SetSpecies(ValueObjects.Species species)
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
/// Music mood profiles for scene atmosphere.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum MusicProfile
{
    /// <summary>No music.</summary>
    None,
    /// <summary>Neutral ambient.</summary>
    Neutral,
    /// <summary>Warm and comfortable.</summary>
    Cozy,
    /// <summary>Light and fun.</summary>
    Playful,
    /// <summary>Awe and discovery.</summary>
    Wonder,
    /// <summary>Suspense and intrigue.</summary>
    Mystery,
    /// <summary>Tension and danger.</summary>
    Tense,
    /// <summary>High energy action.</summary>
    Action,
    /// <summary>Melancholy or loss.</summary>
    Sad,
    /// <summary>Triumph and celebration.</summary>
    Victory
}

/// <summary>
/// Controls whether music continues or changes between scenes.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum MusicContinuity
{
    /// <summary>Continue current track if compatible.</summary>
    PreferContinue,
    /// <summary>Allow track change.</summary>
    AllowChange,
    /// <summary>Force a track change.</summary>
    ForceChange,
    /// <summary>Silence the music.</summary>
    ForceSilence
}

/// <summary>
/// Transition style between music tracks.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum MusicTransitionHint
{
    /// <summary>Automatic transition.</summary>
    Auto,
    /// <summary>Keep current track.</summary>
    Keep,
    /// <summary>Short crossfade.</summary>
    CrossfadeShort,
    /// <summary>Normal crossfade.</summary>
    CrossfadeNormal,
    /// <summary>Long crossfade.</summary>
    CrossfadeLong,
    /// <summary>Hard cut.</summary>
    HardCut
}

/// <summary>
/// Music priority level.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum MusicPriority
{
    /// <summary>Background ambient music.</summary>
    Background,
    /// <summary>Important thematic music.</summary>
    Important
}

/// <summary>
/// Audio ducking behavior during narration or dialogue.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum MusicDucking
{
    /// <summary>No ducking.</summary>
    None,
    /// <summary>Duck during narration.</summary>
    Narration,
    /// <summary>Duck during dialogue.</summary>
    Dialogue
}

/// <summary>
/// Scene music settings for scenarios.
/// </summary>
public class SceneMusicSettings
{
    /// <summary>Gets or sets the music mood profile.</summary>
    public MusicProfile Profile { get; set; }

    /// <summary>Gets or sets the energy level.</summary>
    public double? Energy { get; set; }

    /// <summary>Gets or sets the continuity behavior.</summary>
    public MusicContinuity Continuity { get; set; } = MusicContinuity.PreferContinue;

    /// <summary>Gets or sets the transition hint.</summary>
    public MusicTransitionHint TransitionHint { get; set; } = MusicTransitionHint.Auto;

    /// <summary>Gets or sets the priority level.</summary>
    public MusicPriority Priority { get; set; } = MusicPriority.Background;

    /// <summary>Gets or sets the ducking behavior.</summary>
    public MusicDucking Ducking { get; set; } = MusicDucking.None;
}

/// <summary>
/// Scene sound effect definition.
/// </summary>
public class SceneSoundEffect
{
    /// <summary>Gets or sets the track reference.</summary>
    public string Track { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the effect can loop.</summary>
    public bool Loopable { get; set; }

    /// <summary>Gets or sets the energy level.</summary>
    public double Energy { get; set; }
}

/// <summary>
/// Music palette for scenarios — maps mood profiles to track lists.
/// </summary>
public class MusicPalette
{
    /// <summary>Gets or sets the default mood profile.</summary>
    public MusicProfile DefaultProfile { get; set; } = MusicProfile.Neutral;

    /// <summary>Gets or sets the tracks keyed by profile name (case-insensitive).</summary>
    public Dictionary<string, List<string>> TracksByProfile { get; set; } = new(StringComparer.OrdinalIgnoreCase);
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
