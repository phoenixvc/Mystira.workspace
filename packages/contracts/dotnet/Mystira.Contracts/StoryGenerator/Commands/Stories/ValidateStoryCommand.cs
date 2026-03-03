using System.Text.Json.Serialization;
using Mystira.Contracts.StoryGenerator.Common;
using Mystira.Contracts.StoryGenerator.Stories;

namespace Mystira.Contracts.StoryGenerator.Commands.Stories;

/// <summary>
/// Command to validate a story.
/// </summary>
public class ValidateStoryCommand
{
    /// <summary>
    /// The story to validate (JSON or YAML).
    /// </summary>
    [JsonPropertyName("story")]
    public string Story { get; set; } = string.Empty;

    /// <summary>
    /// Format of the story.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "json";

    /// <summary>
    /// Whether to validate schema conformance.
    /// </summary>
    [JsonPropertyName("validate_schema")]
    public bool ValidateSchema { get; set; } = true;

    /// <summary>
    /// Whether to validate graph structure.
    /// </summary>
    [JsonPropertyName("validate_structure")]
    public bool ValidateStructure { get; set; } = true;

    /// <summary>
    /// Whether to validate narrative consistency.
    /// </summary>
    [JsonPropertyName("validate_consistency")]
    public bool ValidateConsistency { get; set; } = true;

    /// <summary>
    /// Whether to validate entity continuity.
    /// </summary>
    [JsonPropertyName("validate_entities")]
    public bool ValidateEntities { get; set; } = true;

    /// <summary>
    /// Minimum score to pass validation.
    /// </summary>
    [JsonPropertyName("min_score")]
    public double MinScore { get; set; } = 0.7;

    /// <summary>
    /// Whether to include suggestions.
    /// </summary>
    [JsonPropertyName("include_suggestions")]
    public bool IncludeSuggestions { get; set; } = true;

    /// <summary>
    /// AI provider for LLM-based validation.
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// Model ID for LLM-based validation.
    /// </summary>
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }
}

/// <summary>
/// Response from story validation.
/// </summary>
public class ValidateStoryResponse
{
    /// <summary>
    /// Whether the story is valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    /// <summary>
    /// Overall validation score.
    /// </summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }

    /// <summary>
    /// The full validation result.
    /// </summary>
    [JsonPropertyName("result")]
    public StoryValidationResult? Result { get; set; }

    /// <summary>
    /// Quick summary of validation.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Count of errors.
    /// </summary>
    [JsonPropertyName("error_count")]
    public int ErrorCount { get; set; }

    /// <summary>
    /// Count of warnings.
    /// </summary>
    [JsonPropertyName("warning_count")]
    public int WarningCount { get; set; }

    /// <summary>
    /// Duration of validation in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; set; }

    /// <summary>
    /// Token usage if LLM was used.
    /// </summary>
    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}
