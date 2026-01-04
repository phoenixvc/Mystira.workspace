using System.Text.Json.Serialization;
using Mystira.Domain.Primitives;
using Mystira.Domain.Serialization;

namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents a fantasy species in the Mystira universe.
/// </summary>
[JsonConverter(typeof(StringEnumJsonConverter<Species>))]
public sealed class Species : StringEnum<Species>
{
    private readonly string _displayName;

    /// <inheritdoc />
    public override string DisplayName => _displayName;

    private Species(string value, string displayName) : base(value)
    {
        _displayName = displayName;
    }

    /// <summary>Human - the most common species.</summary>
    public static readonly Species Human = new("human", "Human");

    /// <summary>Elf - graceful, long-lived forest dwellers.</summary>
    public static readonly Species Elf = new("elf", "Elf");

    /// <summary>Dwarf - sturdy, mountain-dwelling craftsmen.</summary>
    public static readonly Species Dwarf = new("dwarf", "Dwarf");

    /// <summary>Goblin - mischievous, cunning creatures.</summary>
    public static readonly Species Goblin = new("goblin", "Goblin");

    /// <summary>Orc - powerful, warrior species.</summary>
    public static readonly Species Orc = new("orc", "Orc");

    /// <summary>Halfling - small, nimble folk.</summary>
    public static readonly Species Halfling = new("halfling", "Halfling");

    /// <summary>Fairy - tiny, magical beings.</summary>
    public static readonly Species Fairy = new("fairy", "Fairy");

    /// <summary>Dragon - ancient, powerful reptilian creatures.</summary>
    public static readonly Species Dragon = new("dragon", "Dragon");

    /// <summary>Troll - large, regenerating creatures.</summary>
    public static readonly Species Troll = new("troll", "Troll");

    /// <summary>Gnome - inventive, underground dwellers.</summary>
    public static readonly Species Gnome = new("gnome", "Gnome");

    /// <summary>Mermaid - aquatic humanoids.</summary>
    public static readonly Species Mermaid = new("mermaid", "Mermaid");

    /// <summary>Centaur - half-human, half-horse beings.</summary>
    public static readonly Species Centaur = new("centaur", "Centaur");

    /// <summary>Phoenix - immortal fire birds.</summary>
    public static readonly Species Phoenix = new("phoenix", "Phoenix");

    /// <summary>Unicorn - magical horned horses.</summary>
    public static readonly Species Unicorn = new("unicorn", "Unicorn");

    /// <summary>Giant - enormous humanoid beings.</summary>
    public static readonly Species Giant = new("giant", "Giant");
}
