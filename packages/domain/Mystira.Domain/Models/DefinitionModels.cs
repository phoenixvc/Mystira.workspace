namespace Mystira.Domain.Models;

/// <summary>
/// Represents an age group definition stored in the database.
/// Age groups are used for content classification and user profile settings.
/// </summary>
public class AgeGroupDefinition
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the internal value/code.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the minimum age for this group.</summary>
    public int MinimumAge { get; set; }

    /// <summary>Gets or sets the maximum age for this group.</summary>
    public int MaximumAge { get; set; }

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents an archetype definition that can be assigned to characters in scenarios.
/// </summary>
public class ArchetypeDefinition
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents an echo type definition stored in the database.
/// Echo types are used in scenarios for game events and progression tracking.
/// </summary>
public class EchoTypeDefinition
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category of the echo type (e.g., "moral", "emotional", "behavioral", "social", "cognitive", "meta")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a fantasy theme definition stored in the database.
/// Fantasy themes are used to categorize scenarios and user preferences.
/// </summary>
public class FantasyThemeDefinition
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a compass axis definition in the game system for tracking character development.
/// This is a mutable model for database storage, distinct from the immutable CompassAxis ValueObject.
/// </summary>
public class CompassAxisDefinition
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }
}
