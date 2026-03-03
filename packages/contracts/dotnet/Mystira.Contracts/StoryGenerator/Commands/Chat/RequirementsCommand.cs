using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Commands.Chat;

/// <summary>
/// Command to request story requirements information.
/// </summary>
public class RequirementsCommand
{
    /// <summary>
    /// Category of requirements to retrieve.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Target age group for contextual requirements.
    /// </summary>
    [JsonPropertyName("age_group")]
    public string? AgeGroup { get; set; }

    /// <summary>
    /// Difficulty level for contextual requirements.
    /// </summary>
    [JsonPropertyName("difficulty")]
    public string? Difficulty { get; set; }
}

/// <summary>
/// Response containing requirements information.
/// </summary>
public class RequirementsResponse
{
    /// <summary>
    /// The requirements content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Category of requirements.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Structured requirements.
    /// </summary>
    [JsonPropertyName("requirements")]
    public List<Requirement>? Requirements { get; set; }

    /// <summary>
    /// Available requirement categories.
    /// </summary>
    [JsonPropertyName("available_categories")]
    public List<string>? AvailableCategories { get; set; }
}

/// <summary>
/// A single requirement.
/// </summary>
public class Requirement
{
    /// <summary>
    /// Requirement ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Requirement name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the requirement.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this requirement is mandatory.
    /// </summary>
    [JsonPropertyName("mandatory")]
    public bool Mandatory { get; set; }

    /// <summary>
    /// Category of the requirement.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Priority (1-5, 1 being highest).
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; } = 3;
}
