using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.Agents.Sessions;

/// <summary>
/// Request to refine or improve an agent's output.
/// Provides a generic pattern for iterative improvement of agent responses.
/// </summary>
public class RefineRequest
{
    /// <summary>
    /// ID of the session to refine output for.
    /// </summary>
    [JsonPropertyName("session_id")]
    [Required]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The original output to refine.
    /// </summary>
    [JsonPropertyName("original_output")]
    [Required]
    public string OriginalOutput { get; set; } = string.Empty;

    /// <summary>
    /// User feedback or instructions for refinement.
    /// </summary>
    [JsonPropertyName("feedback")]
    [Required]
    public string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// Specific aspects to focus on during refinement.
    /// </summary>
    [JsonPropertyName("focus_areas")]
    public List<string>? FocusAreas { get; set; }

    /// <summary>
    /// Type of refinement to perform.
    /// </summary>
    [JsonPropertyName("refinement_type")]
    public RefinementType RefinementType { get; set; } = RefinementType.Improve;

    /// <summary>
    /// Constraints to apply during refinement.
    /// </summary>
    [JsonPropertyName("constraints")]
    public RefinementConstraints? Constraints { get; set; }

    /// <summary>
    /// Maximum number of refinement iterations.
    /// </summary>
    [JsonPropertyName("max_iterations")]
    public int MaxIterations { get; set; } = 1;

    /// <summary>
    /// Whether to include thinking/reasoning in response.
    /// </summary>
    [JsonPropertyName("include_reasoning")]
    public bool IncludeReasoning { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Type of refinement to perform.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RefinementType
{
    /// <summary>
    /// General improvement of quality.
    /// </summary>
    Improve,

    /// <summary>
    /// Fix specific issues or errors.
    /// </summary>
    Fix,

    /// <summary>
    /// Expand or add more detail.
    /// </summary>
    Expand,

    /// <summary>
    /// Condense or summarize.
    /// </summary>
    Condense,

    /// <summary>
    /// Change style or tone.
    /// </summary>
    Restyle,

    /// <summary>
    /// Restructure or reorganize.
    /// </summary>
    Restructure,

    /// <summary>
    /// Complete partial output.
    /// </summary>
    Complete
}

/// <summary>
/// Constraints to apply during refinement.
/// </summary>
public class RefinementConstraints
{
    /// <summary>
    /// Maximum length in characters.
    /// </summary>
    [JsonPropertyName("max_length")]
    public int? MaxLength { get; set; }

    /// <summary>
    /// Minimum length in characters.
    /// </summary>
    [JsonPropertyName("min_length")]
    public int? MinLength { get; set; }

    /// <summary>
    /// Required elements that must be present.
    /// </summary>
    [JsonPropertyName("required_elements")]
    public List<string>? RequiredElements { get; set; }

    /// <summary>
    /// Elements that must not be changed.
    /// </summary>
    [JsonPropertyName("preserve_elements")]
    public List<string>? PreserveElements { get; set; }

    /// <summary>
    /// Target reading level or complexity.
    /// </summary>
    [JsonPropertyName("complexity_level")]
    public string? ComplexityLevel { get; set; }

    /// <summary>
    /// Target format (e.g., "markdown", "plain", "html").
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }
}

/// <summary>
/// Response from a refinement request.
/// </summary>
public class RefineResponse
{
    /// <summary>
    /// ID of the session.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Whether refinement was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// The refined output.
    /// </summary>
    [JsonPropertyName("refined_output")]
    public string RefinedOutput { get; set; } = string.Empty;

    /// <summary>
    /// Number of refinement iterations performed.
    /// </summary>
    [JsonPropertyName("iterations")]
    public int Iterations { get; set; } = 1;

    /// <summary>
    /// Changes made during refinement.
    /// </summary>
    [JsonPropertyName("changes")]
    public List<RefinementChange>? Changes { get; set; }

    /// <summary>
    /// Reasoning behind the refinements (if requested).
    /// </summary>
    [JsonPropertyName("reasoning")]
    public string? Reasoning { get; set; }

    /// <summary>
    /// Comparison metrics between original and refined.
    /// </summary>
    [JsonPropertyName("comparison")]
    public RefinementComparison? Comparison { get; set; }

    /// <summary>
    /// Error message if refinement failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Token usage for the refinement operation.
    /// </summary>
    [JsonPropertyName("usage")]
    public Orchestration.AgentUsage? Usage { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Describes a specific change made during refinement.
/// </summary>
public class RefinementChange
{
    /// <summary>
    /// Type of change.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Description of the change.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Location or section affected.
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>
    /// Original text (if applicable).
    /// </summary>
    [JsonPropertyName("original")]
    public string? Original { get; set; }

    /// <summary>
    /// Replacement text (if applicable).
    /// </summary>
    [JsonPropertyName("replacement")]
    public string? Replacement { get; set; }
}

/// <summary>
/// Comparison metrics between original and refined output.
/// </summary>
public class RefinementComparison
{
    /// <summary>
    /// Original output length.
    /// </summary>
    [JsonPropertyName("original_length")]
    public int OriginalLength { get; set; }

    /// <summary>
    /// Refined output length.
    /// </summary>
    [JsonPropertyName("refined_length")]
    public int RefinedLength { get; set; }

    /// <summary>
    /// Percentage change in length.
    /// </summary>
    [JsonPropertyName("length_change_percent")]
    public double LengthChangePercent { get; set; }

    /// <summary>
    /// Similarity score between original and refined (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("similarity_score")]
    public double? SimilarityScore { get; set; }

    /// <summary>
    /// Quality improvement score (if evaluated).
    /// </summary>
    [JsonPropertyName("quality_improvement")]
    public double? QualityImprovement { get; set; }
}
