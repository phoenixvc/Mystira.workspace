namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents an age group for content categorization.
/// </summary>
public sealed record AgeGroup
{
    /// <summary>
    /// Gets the age group identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the minimum age for this group.
    /// </summary>
    public int MinAge { get; }

    /// <summary>
    /// Gets the maximum age for this group.
    /// </summary>
    public int MaxAge { get; }

    private AgeGroup(string id, string name, int minAge, int maxAge)
    {
        Id = id;
        Name = name;
        MinAge = minAge;
        MaxAge = maxAge;
    }

    /// <summary>
    /// Age group for children 4-6 years old.
    /// </summary>
    public static readonly AgeGroup EarlyChildhood = new("early_childhood", "Early Childhood", 4, 6);

    /// <summary>
    /// Age group for children 7-9 years old.
    /// </summary>
    public static readonly AgeGroup MiddleChildhood = new("middle_childhood", "Middle Childhood", 7, 9);

    /// <summary>
    /// Age group for children 10-12 years old.
    /// </summary>
    public static readonly AgeGroup Preteen = new("preteen", "Preteen", 10, 12);

    /// <summary>
    /// Age group for teens 13-17 years old.
    /// </summary>
    public static readonly AgeGroup Teen = new("teen", "Teen", 13, 17);

    /// <summary>
    /// Age group for adults 18+ years old.
    /// </summary>
    public static readonly AgeGroup Adult = new("adult", "Adult", 18, 99);

    /// <summary>
    /// All defined age groups.
    /// </summary>
    public static readonly IReadOnlyList<AgeGroup> All = new[]
    {
        EarlyChildhood,
        MiddleChildhood,
        Preteen,
        Teen,
        Adult
    };

    /// <summary>
    /// Gets the value/ID (alias for Id for DTO compatibility).
    /// </summary>
    public string Value => Id;

    /// <summary>
    /// Gets an age group by ID.
    /// </summary>
    /// <param name="id">The age group ID.</param>
    /// <returns>The age group or null if not found.</returns>
    public static AgeGroup? FromId(string? id) =>
        All.FirstOrDefault(ag => ag.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets an age group by value/ID (alias for FromId).
    /// </summary>
    /// <param name="value">The age group value/ID.</param>
    /// <returns>The age group or null if not found.</returns>
    public static AgeGroup? FromValue(string? value) => FromId(value);

    /// <summary>
    /// Parses an age group from a string value.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>The age group or null if not found.</returns>
    public static AgeGroup? Parse(string? value) => FromId(value);

    /// <summary>
    /// Gets the appropriate age group for a given age.
    /// </summary>
    /// <param name="age">The age in years.</param>
    /// <returns>The appropriate age group.</returns>
    public static AgeGroup ForAge(int age) =>
        All.FirstOrDefault(ag => age >= ag.MinAge && age <= ag.MaxAge) ?? Adult;

    /// <summary>
    /// Checks if a given age is within this age group.
    /// </summary>
    /// <param name="age">The age to check.</param>
    /// <returns>True if the age is within this group.</returns>
    public bool Contains(int age) => age >= MinAge && age <= MaxAge;

    /// <inheritdoc />
    public override string ToString() => Name;
}

/// <summary>
/// Constants for age group identifiers.
/// </summary>
public static class AgeGroupConstants
{
    /// <summary>
    /// Early childhood age group ID.
    /// </summary>
    public const string EarlyChildhood = "early_childhood";

    /// <summary>
    /// Middle childhood age group ID.
    /// </summary>
    public const string MiddleChildhood = "middle_childhood";

    /// <summary>
    /// Preteen age group ID.
    /// </summary>
    public const string Preteen = "preteen";

    /// <summary>
    /// Teen age group ID.
    /// </summary>
    public const string Teen = "teen";

    /// <summary>
    /// Adult age group ID.
    /// </summary>
    public const string Adult = "adult";

    /// <summary>
    /// Gets the display name for an age group ID.
    /// </summary>
    /// <param name="ageGroupId">The age group identifier.</param>
    /// <returns>The display name for the age group, or the ID if not found.</returns>
    public static string GetDisplayName(string? ageGroupId)
    {
        return ageGroupId switch
        {
            EarlyChildhood => "Early Childhood (4-6)",
            MiddleChildhood => "Middle Childhood (7-9)",
            Preteen => "Preteen (10-12)",
            Teen => "Teen (13-17)",
            Adult => "Adult (18+)",
            null => "Unknown",
            _ => ageGroupId
        };
    }

    /// <summary>
    /// Gets the short display name for an age group ID.
    /// </summary>
    /// <param name="ageGroupId">The age group identifier.</param>
    /// <returns>The short display name for the age group, or the ID if not found.</returns>
    public static string GetShortDisplayName(string? ageGroupId)
    {
        return ageGroupId switch
        {
            EarlyChildhood => "Early Childhood",
            MiddleChildhood => "Middle Childhood",
            Preteen => "Preteen",
            Teen => "Teen",
            Adult => "Adult",
            null => "Unknown",
            _ => ageGroupId
        };
    }

    /// <summary>
    /// Gets all age group IDs.
    /// </summary>
    /// <returns>A collection of all age group identifiers.</returns>
    public static IReadOnlyList<string> GetAll() => new[]
    {
        EarlyChildhood,
        MiddleChildhood,
        Preteen,
        Teen,
        Adult
    };
}
