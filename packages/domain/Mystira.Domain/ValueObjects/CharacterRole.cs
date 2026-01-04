using System.Text.Json.Serialization;
using Mystira.Domain.Primitives;
using Mystira.Domain.Serialization;

namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents a character's narrative role in the Mystira universe.
/// </summary>
[JsonConverter(typeof(StringEnumJsonConverter<CharacterRole>))]
public sealed class CharacterRole : StringEnum<CharacterRole>
{
    private readonly string _displayName;

    /// <inheritdoc />
    public override string DisplayName => _displayName;

    private CharacterRole(string value, string displayName) : base(value)
    {
        _displayName = displayName;
    }

    /// <summary>Mentor - guides and teaches the protagonist.</summary>
    public static readonly CharacterRole Mentor = new("mentor", "Mentor");

    /// <summary>Trickster - creates mischief and challenges expectations.</summary>
    public static readonly CharacterRole Trickster = new("trickster", "Trickster");

    /// <summary>Guardian - protects important people or places.</summary>
    public static readonly CharacterRole Guardian = new("guardian", "Guardian");

    /// <summary>Ally - supports the protagonist's journey.</summary>
    public static readonly CharacterRole Ally = new("ally", "Ally");

    /// <summary>Antagonist - opposes the protagonist.</summary>
    public static readonly CharacterRole Antagonist = new("antagonist", "Antagonist");

    /// <summary>Herald - announces change or adventure.</summary>
    public static readonly CharacterRole Herald = new("herald", "Herald");

    /// <summary>Shapeshifter - has changing allegiances or nature.</summary>
    public static readonly CharacterRole Shapeshifter = new("shapeshifter", "Shapeshifter");

    /// <summary>Shadow - represents the dark side or fears.</summary>
    public static readonly CharacterRole Shadow = new("shadow", "Shadow");

    /// <summary>Threshold Guardian - tests the hero before passage.</summary>
    public static readonly CharacterRole ThresholdGuardian = new("threshold_guardian", "Threshold Guardian");

    /// <summary>Peacemaker - resolves conflicts and brings harmony.</summary>
    public static readonly CharacterRole Peacemaker = new("peacemaker", "Peacemaker");

    /// <summary>Comic Relief - provides humor and lightness.</summary>
    public static readonly CharacterRole ComicRelief = new("comic_relief", "Comic Relief");

    /// <summary>Sly - cunning and deceptive character.</summary>
    public static readonly CharacterRole Sly = new("sly", "Sly");

    /// <summary>Healer - provides care and restoration.</summary>
    public static readonly CharacterRole Healer = new("healer", "Healer");

    /// <summary>Sage - offers wisdom and knowledge.</summary>
    public static readonly CharacterRole Sage = new("sage", "Sage");

    /// <summary>Sidekick - loyal companion to the protagonist.</summary>
    public static readonly CharacterRole Sidekick = new("sidekick", "Sidekick");

    /// <summary>Ruler - holds power and authority.</summary>
    public static readonly CharacterRole Ruler = new("ruler", "Ruler");

    /// <summary>Outcast - excluded from society.</summary>
    public static readonly CharacterRole Outcast = new("outcast", "Outcast");

    /// <summary>Wanderer - travels without a fixed home.</summary>
    public static readonly CharacterRole Wanderer = new("wanderer", "Wanderer");
}
