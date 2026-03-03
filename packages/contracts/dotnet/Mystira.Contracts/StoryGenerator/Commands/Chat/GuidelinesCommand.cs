using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Commands.Chat;

/// <summary>
/// Command to request story writing guidelines.
/// </summary>
public class GuidelinesCommand
{
    /// <summary>
    /// Category of guidelines to retrieve.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Target age group for contextual guidelines.
    /// </summary>
    [JsonPropertyName("age_group")]
    public string? AgeGroup { get; set; }

    /// <summary>
    /// Specific story element to get guidelines for.
    /// </summary>
    [JsonPropertyName("element")]
    public string? Element { get; set; }
}

/// <summary>
/// Response containing guidelines information.
/// </summary>
public class GuidelinesResponse
{
    /// <summary>
    /// The guidelines content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Category of guidelines.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Structured guidelines.
    /// </summary>
    [JsonPropertyName("guidelines")]
    public List<Guideline>? Guidelines { get; set; }

    /// <summary>
    /// Available guideline categories.
    /// </summary>
    [JsonPropertyName("available_categories")]
    public List<string>? AvailableCategories { get; set; }

    /// <summary>
    /// Best practices.
    /// </summary>
    [JsonPropertyName("best_practices")]
    public List<string>? BestPractices { get; set; }
}

/// <summary>
/// A single guideline.
/// </summary>
public class Guideline
{
    /// <summary>
    /// Guideline ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Guideline title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Guideline content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Category of the guideline.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Applicable story elements.
    /// </summary>
    [JsonPropertyName("applies_to")]
    public List<string>? AppliesTo { get; set; }

    /// <summary>
    /// Priority (1-5, 1 being highest).
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Examples of good practice.
    /// </summary>
    [JsonPropertyName("examples")]
    public List<string>? Examples { get; set; }

    /// <summary>
    /// Common mistakes to avoid.
    /// </summary>
    [JsonPropertyName("avoid")]
    public List<string>? Avoid { get; set; }
}
