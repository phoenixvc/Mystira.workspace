using System.Text.Json.Serialization;
using Mystira.Domain.Primitives;
using Mystira.Domain.Serialization;

namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents a core moral/ethical axis in the Mystira compass system.
/// Each axis represents a spectrum between two opposing values.
/// </summary>
[JsonConverter(typeof(StringEnumJsonConverter<CoreAxis>))]
public sealed class CoreAxis : StringEnum<CoreAxis>
{
    private readonly string _displayName;
    private readonly string _positiveLabel;
    private readonly string _negativeLabel;
    private readonly string _description;

    /// <inheritdoc />
    public override string DisplayName => _displayName;

    /// <summary>
    /// Gets the label for the positive end of the axis.
    /// </summary>
    public string PositiveLabel => _positiveLabel;

    /// <summary>
    /// Gets the label for the negative end of the axis.
    /// </summary>
    public string NegativeLabel => _negativeLabel;

    /// <summary>
    /// Gets the description of this axis.
    /// </summary>
    public string Description => _description;

    private CoreAxis(string value, string displayName, string positiveLabel, string negativeLabel, string description)
        : base(value)
    {
        _displayName = displayName;
        _positiveLabel = positiveLabel;
        _negativeLabel = negativeLabel;
        _description = description;
    }

    /// <summary>
    /// Courage vs Fear - How one faces challenges and adversity.
    /// </summary>
    public static readonly CoreAxis Courage = new(
        "courage",
        "Courage",
        "Brave",
        "Fearful",
        "How one faces challenges, dangers, and adversity");

    /// <summary>
    /// Kindness vs Cruelty - How one treats others.
    /// </summary>
    public static readonly CoreAxis Kindness = new(
        "kindness",
        "Kindness",
        "Kind",
        "Cruel",
        "How one treats others, especially those who cannot help themselves");

    /// <summary>
    /// Honesty vs Deception - How truthful one is.
    /// </summary>
    public static readonly CoreAxis Honesty = new(
        "honesty",
        "Honesty",
        "Honest",
        "Deceptive",
        "How truthful and transparent one is in their dealings");

    /// <summary>
    /// Loyalty vs Betrayal - How faithful one is to commitments.
    /// </summary>
    public static readonly CoreAxis Loyalty = new(
        "loyalty",
        "Loyalty",
        "Loyal",
        "Disloyal",
        "How faithful one is to their friends, family, and causes");

    /// <summary>
    /// Justice vs Injustice - How fairly one treats others.
    /// </summary>
    public static readonly CoreAxis Justice = new(
        "justice",
        "Justice",
        "Just",
        "Unjust",
        "How fairly one treats others and upholds what is right");

    /// <summary>
    /// Wisdom vs Foolishness - How thoughtfully one makes decisions.
    /// </summary>
    public static readonly CoreAxis Wisdom = new(
        "wisdom",
        "Wisdom",
        "Wise",
        "Foolish",
        "How thoughtfully and carefully one makes decisions");

    /// <summary>
    /// Compassion vs Indifference - How much one cares about others' suffering.
    /// </summary>
    public static readonly CoreAxis Compassion = new(
        "compassion",
        "Compassion",
        "Compassionate",
        "Indifferent",
        "How much one cares about and responds to others' suffering");

    /// <summary>
    /// Humility vs Pride - How one views themselves relative to others.
    /// </summary>
    public static readonly CoreAxis Humility = new(
        "humility",
        "Humility",
        "Humble",
        "Proud",
        "How one views themselves in relation to others");
}
