using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Commands.Chat;

/// <summary>
/// Command to request safety policy information.
/// </summary>
public class SafetyPolicyCommand
{
    /// <summary>
    /// Specific policy section to retrieve.
    /// </summary>
    [JsonPropertyName("section")]
    public string? Section { get; set; }

    /// <summary>
    /// Target age group for age-appropriate policies.
    /// </summary>
    [JsonPropertyName("age_group")]
    public string? AgeGroup { get; set; }
}

/// <summary>
/// Response containing safety policy information.
/// </summary>
public class SafetyPolicyResponse
{
    /// <summary>
    /// The policy content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Section of the policy.
    /// </summary>
    [JsonPropertyName("section")]
    public string? Section { get; set; }

    /// <summary>
    /// Structured policy rules.
    /// </summary>
    [JsonPropertyName("rules")]
    public List<SafetyRule>? Rules { get; set; }

    /// <summary>
    /// Available policy sections.
    /// </summary>
    [JsonPropertyName("available_sections")]
    public List<string>? AvailableSections { get; set; }

    /// <summary>
    /// Content guidelines by age group.
    /// </summary>
    [JsonPropertyName("age_guidelines")]
    public Dictionary<string, List<string>>? AgeGuidelines { get; set; }
}

/// <summary>
/// A single safety rule.
/// </summary>
public class SafetyRule
{
    /// <summary>
    /// Rule ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Rule name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the rule.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity level.
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Applicable age groups.
    /// </summary>
    [JsonPropertyName("applicable_ages")]
    public List<string>? ApplicableAges { get; set; }

    /// <summary>
    /// Example violations.
    /// </summary>
    [JsonPropertyName("examples")]
    public List<string>? Examples { get; set; }
}
