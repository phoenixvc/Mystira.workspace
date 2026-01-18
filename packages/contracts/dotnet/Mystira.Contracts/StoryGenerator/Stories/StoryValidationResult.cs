using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Result of validating a story scenario.
/// </summary>
public class StoryValidationResult
{
    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; }

    /// <summary>
    /// Overall validation score (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<ValidationIssue> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings.
    /// </summary>
    [JsonPropertyName("warnings")]
    public List<ValidationIssue> Warnings { get; set; } = new();

    /// <summary>
    /// List of validation suggestions.
    /// </summary>
    [JsonPropertyName("suggestions")]
    public List<ValidationIssue> Suggestions { get; set; } = new();

    /// <summary>
    /// Schema validation result.
    /// </summary>
    [JsonPropertyName("schema_valid")]
    public bool SchemaValid { get; set; }

    /// <summary>
    /// Schema validation errors.
    /// </summary>
    [JsonPropertyName("schema_errors")]
    public List<string>? SchemaErrors { get; set; }

    /// <summary>
    /// Structural validation result.
    /// </summary>
    [JsonPropertyName("structure_valid")]
    public bool StructureValid { get; set; }

    /// <summary>
    /// Narrative consistency validation result.
    /// </summary>
    [JsonPropertyName("consistency_valid")]
    public bool ConsistencyValid { get; set; }

    /// <summary>
    /// Summary of the validation.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Duration of validation in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; set; }
}

/// <summary>
/// Represents a single validation issue (error, warning, or suggestion).
/// </summary>
public class ValidationIssue
{
    /// <summary>
    /// Error/warning code for categorization.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of the issue.
    /// </summary>
    [JsonPropertyName("severity")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ValidationSeverity Severity { get; set; }

    /// <summary>
    /// Location in the scenario where the issue was found.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>
    /// ID of the affected element (scene, character, etc.).
    /// </summary>
    [JsonPropertyName("element_id")]
    public string? ElementId { get; set; }

    /// <summary>
    /// Suggested fix for the issue.
    /// </summary>
    [JsonPropertyName("suggested_fix")]
    public string? SuggestedFix { get; set; }
}

/// <summary>
/// Severity levels for validation issues.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValidationSeverity
{
    /// <summary>
    /// Informational suggestion for improvement.
    /// </summary>
    Info,

    /// <summary>
    /// Warning that should be addressed but doesn't block.
    /// </summary>
    Warning,

    /// <summary>
    /// Error that must be fixed.
    /// </summary>
    Error,

    /// <summary>
    /// Critical error that prevents story execution.
    /// </summary>
    Critical
}
