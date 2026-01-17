using System.Text.Json.Serialization;

namespace Mystira.Contracts.Agents.Evaluation;

/// <summary>
/// Summary definition of an evaluation rubric.
/// Provides a reusable pattern for defining evaluation criteria for agent outputs.
/// </summary>
public class RubricSummary
{
    /// <summary>
    /// Unique identifier for this rubric.
    /// </summary>
    [JsonPropertyName("rubric_id")]
    public string RubricId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the rubric.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this rubric evaluates.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Category or domain of the rubric.
    /// </summary>
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    /// <summary>
    /// Weight of this rubric in overall scoring (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("weight")]
    public double Weight { get; set; } = 1.0;

    /// <summary>
    /// Minimum score required to pass this rubric.
    /// </summary>
    [JsonPropertyName("pass_threshold")]
    public double PassThreshold { get; set; } = 0.7;

    /// <summary>
    /// Whether this rubric is required for overall pass.
    /// </summary>
    [JsonPropertyName("required")]
    public bool Required { get; set; } = true;

    /// <summary>
    /// Scoring levels for this rubric.
    /// </summary>
    [JsonPropertyName("levels")]
    public List<RubricLevel> Levels { get; set; } = new();

    /// <summary>
    /// Examples of good/bad outputs for this rubric.
    /// </summary>
    [JsonPropertyName("examples")]
    public List<RubricExample>? Examples { get; set; }

    /// <summary>
    /// Prompt template for LLM-based evaluation.
    /// </summary>
    [JsonPropertyName("evaluation_prompt")]
    public string? EvaluationPrompt { get; set; }

    /// <summary>
    /// Whether this rubric is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Version of the rubric definition.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Custom metadata for extensibility.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Defines a scoring level within a rubric.
/// </summary>
public class RubricLevel
{
    /// <summary>
    /// Score for this level (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }

    /// <summary>
    /// Label for this level (e.g., "Excellent", "Good", "Needs Improvement").
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Description of what constitutes this level.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Criteria that must be met for this level.
    /// </summary>
    [JsonPropertyName("criteria")]
    public List<string>? Criteria { get; set; }
}

/// <summary>
/// Example output for a rubric, used for calibration.
/// </summary>
public class RubricExample
{
    /// <summary>
    /// The example output text.
    /// </summary>
    [JsonPropertyName("output")]
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Expected score for this example.
    /// </summary>
    [JsonPropertyName("expected_score")]
    public double ExpectedScore { get; set; }

    /// <summary>
    /// Explanation of why this score is expected.
    /// </summary>
    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    /// <summary>
    /// Whether this is a positive or negative example.
    /// </summary>
    [JsonPropertyName("is_positive")]
    public bool IsPositive { get; set; }
}

/// <summary>
/// Collection of rubrics for a specific evaluation context.
/// </summary>
public class RubricSet
{
    /// <summary>
    /// Unique identifier for this rubric set.
    /// </summary>
    [JsonPropertyName("set_id")]
    public string SetId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the rubric set.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the rubric set and its intended use.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The rubrics in this set.
    /// </summary>
    [JsonPropertyName("rubrics")]
    public List<RubricSummary> Rubrics { get; set; } = new();

    /// <summary>
    /// Minimum overall score required to pass.
    /// </summary>
    [JsonPropertyName("overall_pass_threshold")]
    public double OverallPassThreshold { get; set; } = 0.7;

    /// <summary>
    /// Version of this rubric set.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
