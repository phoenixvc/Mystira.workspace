using System.Text.Json.Serialization;
using Mystira.Domain.Primitives;
using Mystira.Domain.Serialization;

namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents the type of an Echo in the Mystira universe.
/// Echoes are fragments of past events that reveal hidden truths.
/// </summary>
[JsonConverter(typeof(StringEnumJsonConverter<EchoType>))]
public sealed class EchoType : StringEnum<EchoType>
{
    private readonly string _displayName;
    private readonly string _description;

    /// <inheritdoc />
    public override string DisplayName => _displayName;

    /// <summary>
    /// Gets the description of this echo type.
    /// </summary>
    public string Description => _description;

    private EchoType(string value, string displayName, string description) : base(value)
    {
        _displayName = displayName;
        _description = description;
    }

    /// <summary>
    /// A memory echo - reveals a character's past memory.
    /// </summary>
    public static readonly EchoType Memory = new(
        "memory",
        "Memory Echo",
        "Reveals a character's past memory or experience");

    /// <summary>
    /// A vision echo - shows a glimpse of possible futures.
    /// </summary>
    public static readonly EchoType Vision = new(
        "vision",
        "Vision Echo",
        "Shows a glimpse of possible futures or outcomes");

    /// <summary>
    /// A secret echo - uncovers hidden information.
    /// </summary>
    public static readonly EchoType Secret = new(
        "secret",
        "Secret Echo",
        "Uncovers hidden information or concealed truths");

    /// <summary>
    /// An emotion echo - reveals deep feelings and motivations.
    /// </summary>
    public static readonly EchoType Emotion = new(
        "emotion",
        "Emotion Echo",
        "Reveals deep feelings, desires, or motivations");

    /// <summary>
    /// A connection echo - shows relationships between characters or events.
    /// </summary>
    public static readonly EchoType Connection = new(
        "connection",
        "Connection Echo",
        "Shows hidden relationships between characters or events");

    /// <summary>
    /// A warning echo - alerts to danger or consequences.
    /// </summary>
    public static readonly EchoType Warning = new(
        "warning",
        "Warning Echo",
        "Alerts to potential danger or consequences of actions");

    /// <summary>
    /// A legacy echo - connects to historical or ancestral events.
    /// </summary>
    public static readonly EchoType Legacy = new(
        "legacy",
        "Legacy Echo",
        "Connects to historical events or ancestral influences");

    /// <summary>
    /// A revelation echo - provides major plot revelations.
    /// </summary>
    public static readonly EchoType Revelation = new(
        "revelation",
        "Revelation Echo",
        "Provides major plot revelations or turning points");
}
