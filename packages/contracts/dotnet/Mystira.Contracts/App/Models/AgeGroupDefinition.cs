namespace Mystira.Contracts.App.Models;

/// <summary>
/// Represents an age group definition for content filtering.
/// </summary>
public class AgeGroupDefinition
{
    /// <summary>
    /// Unique identifier for the age group.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the age group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minimum age for this group (inclusive).
    /// </summary>
    public int MinimumAge { get; set; }

    /// <summary>
    /// Maximum age for this group (inclusive).
    /// </summary>
    public int MaximumAge { get; set; }
}
