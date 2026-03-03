namespace Mystira.Admin.Api.Models;

/// <summary>
/// Constants and helpers for age group handling.
/// Provides display names and standard age group values used across the Admin API.
/// </summary>
public static class AgeGroupConstants
{
    /// <summary>
    /// All supported age groups
    /// </summary>
    public static readonly string[] AllAgeGroups = { "younger-kids", "older-kids", "teens", "adults" };

    /// <summary>
    /// Gets the display name for an age group value
    /// </summary>
    public static string GetDisplayName(string ageGroupValue)
    {
        return ageGroupValue?.ToLowerInvariant() switch
        {
            "younger-kids" => "Younger Kids (5-7)",
            "older-kids" => "Older Kids (8-10)",
            "teens" => "Teens (11-14)",
            "adults" => "Adults (15+)",
            _ => ageGroupValue ?? "Unknown"
        };
    }
}
