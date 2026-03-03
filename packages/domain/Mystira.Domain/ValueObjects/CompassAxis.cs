namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents a moral compass axis for character development.
/// </summary>
public sealed record CompassAxis
{
    /// <summary>
    /// Gets the axis identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of this axis.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the name for the positive end of the axis.
    /// </summary>
    public string PositiveLabel { get; }

    /// <summary>
    /// Gets the name for the negative end of the axis.
    /// </summary>
    public string NegativeLabel { get; }

    private CompassAxis(string id, string name, string description, string positiveLabel, string negativeLabel)
    {
        Id = id;
        Name = name;
        Description = description;
        PositiveLabel = positiveLabel;
        NegativeLabel = negativeLabel;
    }

    /// <summary>
    /// Courage vs Fear axis.
    /// </summary>
    public static readonly CompassAxis Courage = new(
        "courage",
        "Courage",
        "The willingness to face difficulty, danger, or pain",
        "Brave",
        "Fearful"
    );

    /// <summary>
    /// Kindness vs Cruelty axis.
    /// </summary>
    public static readonly CompassAxis Kindness = new(
        "kindness",
        "Kindness",
        "The quality of being friendly, generous, and considerate",
        "Kind",
        "Cruel"
    );

    /// <summary>
    /// Honesty vs Deception axis.
    /// </summary>
    public static readonly CompassAxis Honesty = new(
        "honesty",
        "Honesty",
        "The quality of being truthful and sincere",
        "Honest",
        "Deceptive"
    );

    /// <summary>
    /// Loyalty vs Betrayal axis.
    /// </summary>
    public static readonly CompassAxis Loyalty = new(
        "loyalty",
        "Loyalty",
        "A strong feeling of allegiance or faithfulness",
        "Loyal",
        "Treacherous"
    );

    /// <summary>
    /// Wisdom vs Foolishness axis.
    /// </summary>
    public static readonly CompassAxis Wisdom = new(
        "wisdom",
        "Wisdom",
        "The quality of having experience, knowledge, and good judgment",
        "Wise",
        "Foolish"
    );

    /// <summary>
    /// All defined compass axes.
    /// </summary>
    public static readonly IReadOnlyList<CompassAxis> All = new[]
    {
        Courage,
        Kindness,
        Honesty,
        Loyalty,
        Wisdom
    };

    /// <summary>
    /// Gets a compass axis by ID.
    /// </summary>
    /// <param name="id">The axis ID.</param>
    /// <returns>The compass axis or null if not found.</returns>
    public static CompassAxis? FromId(string? id) =>
        All.FirstOrDefault(ca => ca.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public override string ToString() => Name;
}
