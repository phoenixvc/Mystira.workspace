namespace Mystira.Domain.Models;

/// <summary>
/// Represents an age group definition stored in the database.
/// Age groups are used for content classification and user profile settings.
/// </summary>
public class AgeGroupDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int MinimumAge { get; set; }
    public int MaximumAge { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents an archetype definition that can be assigned to characters in scenarios.
/// </summary>
public class ArchetypeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents an echo type definition stored in the database.
/// Echo types are used in scenarios for game events and progression tracking.
/// </summary>
public class EchoTypeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category of the echo type (e.g., "moral", "emotional", "behavioral", "social", "cognitive", "meta")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a fantasy theme definition stored in the database.
/// Fantasy themes are used to categorize scenarios and user preferences.
/// </summary>
public class FantasyThemeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a compass axis definition in the game system for tracking character development.
/// This is a mutable model for database storage, distinct from the immutable CompassAxis ValueObject.
/// </summary>
public class CompassAxisDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this record is soft-deleted (for referential integrity).
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
