using System.Text.Json.Serialization;
using Mystira.Domain.Primitives;
using Mystira.Domain.Serialization;

namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents a character archetype in the Mystira universe.
/// Archetypes define the core personality and behavioral patterns of characters.
/// </summary>
[JsonConverter(typeof(StringEnumJsonConverter<Archetype>))]
public sealed class Archetype : StringEnum<Archetype>
{
    private readonly string _displayName;

    /// <inheritdoc />
    public override string DisplayName => _displayName;

    private Archetype(string value, string displayName) : base(value)
    {
        _displayName = displayName;
    }

    /// <summary>
    /// The Hero archetype - brave, selfless, and driven to help others.
    /// </summary>
    public static readonly Archetype Hero = new("hero", "The Hero");

    /// <summary>
    /// The Sage archetype - wise, knowledgeable, and seeking truth.
    /// </summary>
    public static readonly Archetype Sage = new("sage", "The Sage");

    /// <summary>
    /// The Explorer archetype - curious, adventurous, and seeking discovery.
    /// </summary>
    public static readonly Archetype Explorer = new("explorer", "The Explorer");

    /// <summary>
    /// The Rebel archetype - independent, rule-breaking, and seeking change.
    /// </summary>
    public static readonly Archetype Rebel = new("rebel", "The Rebel");

    /// <summary>
    /// The Magician archetype - transformative, visionary, and seeking power.
    /// </summary>
    public static readonly Archetype Magician = new("magician", "The Magician");

    /// <summary>
    /// The Innocent archetype - optimistic, pure, and seeking happiness.
    /// </summary>
    public static readonly Archetype Innocent = new("innocent", "The Innocent");

    /// <summary>
    /// The Caregiver archetype - nurturing, protective, and seeking to help.
    /// </summary>
    public static readonly Archetype Caregiver = new("caregiver", "The Caregiver");

    /// <summary>
    /// The Creator archetype - imaginative, artistic, and seeking expression.
    /// </summary>
    public static readonly Archetype Creator = new("creator", "The Creator");

    /// <summary>
    /// The Ruler archetype - commanding, responsible, and seeking control.
    /// </summary>
    public static readonly Archetype Ruler = new("ruler", "The Ruler");

    /// <summary>
    /// The Jester archetype - playful, humorous, and seeking joy.
    /// </summary>
    public static readonly Archetype Jester = new("jester", "The Jester");

    /// <summary>
    /// The Everyperson archetype - relatable, grounded, and seeking belonging.
    /// </summary>
    public static readonly Archetype Everyperson = new("everyperson", "The Everyperson");

    /// <summary>
    /// The Lover archetype - passionate, devoted, and seeking connection.
    /// </summary>
    public static readonly Archetype Lover = new("lover", "The Lover");
}
