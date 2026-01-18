using System.Text.Json.Serialization;
using Mystira.Contracts.StoryGenerator.Common;

namespace Mystira.Contracts.StoryGenerator.Commands.Stories;

/// <summary>
/// Command to automatically fix issues in story JSON.
/// </summary>
public class AutoFixStoryJsonCommand
{
    /// <summary>
    /// The story JSON to fix.
    /// </summary>
    [JsonPropertyName("story_json")]
    public string StoryJson { get; set; } = string.Empty;

    /// <summary>
    /// Validation errors to fix.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Whether to fix schema validation errors.
    /// </summary>
    [JsonPropertyName("fix_schema_errors")]
    public bool FixSchemaErrors { get; set; } = true;

    /// <summary>
    /// Whether to fix structural issues.
    /// </summary>
    [JsonPropertyName("fix_structure")]
    public bool FixStructure { get; set; } = true;

    /// <summary>
    /// Whether to fix consistency issues.
    /// </summary>
    [JsonPropertyName("fix_consistency")]
    public bool FixConsistency { get; set; } = true;

    /// <summary>
    /// Whether to use LLM for complex fixes.
    /// </summary>
    [JsonPropertyName("use_llm")]
    public bool UseLlm { get; set; } = false;

    /// <summary>
    /// AI provider to use for LLM fixes.
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// Model ID for LLM fixes.
    /// </summary>
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }
}

/// <summary>
/// Response from auto-fix operation.
/// </summary>
public class AutoFixStoryJsonResponse
{
    /// <summary>
    /// Whether fixing succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// The fixed story JSON.
    /// </summary>
    [JsonPropertyName("fixed_json")]
    public string? FixedJson { get; set; }

    /// <summary>
    /// Fixes that were applied.
    /// </summary>
    [JsonPropertyName("fixes_applied")]
    public List<AppliedFix>? FixesApplied { get; set; }

    /// <summary>
    /// Errors that could not be fixed.
    /// </summary>
    [JsonPropertyName("unfixed_errors")]
    public List<string>? UnfixedErrors { get; set; }

    /// <summary>
    /// Whether all errors were fixed.
    /// </summary>
    [JsonPropertyName("all_fixed")]
    public bool AllFixed { get; set; }

    /// <summary>
    /// Error message if operation failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Token usage if LLM was used.
    /// </summary>
    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}

/// <summary>
/// Describes a fix that was applied.
/// </summary>
public class AppliedFix
{
    /// <summary>
    /// Type of fix.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Location in the story.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Original error.
    /// </summary>
    [JsonPropertyName("original_error")]
    public string OriginalError { get; set; } = string.Empty;

    /// <summary>
    /// Description of the fix.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Previous value.
    /// </summary>
    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    /// <summary>
    /// New value.
    /// </summary>
    [JsonPropertyName("current")]
    public string? Current { get; set; }
}
