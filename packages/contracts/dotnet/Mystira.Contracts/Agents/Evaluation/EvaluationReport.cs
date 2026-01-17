using System.Text.Json.Serialization;

namespace Mystira.Contracts.Agents.Evaluation;

/// <summary>
/// Comprehensive evaluation report for agent outputs.
/// Provides a generic pattern for assessing agent responses against rubrics and criteria.
/// </summary>
public class EvaluationReport
{
    /// <summary>
    /// Unique identifier for this evaluation report.
    /// </summary>
    [JsonPropertyName("report_id")]
    public string ReportId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the session or interaction being evaluated.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    /// <summary>
    /// ID of the agent whose output is being evaluated.
    /// </summary>
    [JsonPropertyName("agent_id")]
    public string? AgentId { get; set; }

    /// <summary>
    /// Timestamp when evaluation was performed.
    /// </summary>
    [JsonPropertyName("evaluated_at")]
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Overall score (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("overall_score")]
    public double OverallScore { get; set; }

    /// <summary>
    /// Overall pass/fail status.
    /// </summary>
    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    /// <summary>
    /// Individual rubric evaluations.
    /// </summary>
    [JsonPropertyName("rubrics")]
    public List<RubricEvaluation> Rubrics { get; set; } = new();

    /// <summary>
    /// Summary of the evaluation.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Detailed feedback and recommendations.
    /// </summary>
    [JsonPropertyName("feedback")]
    public string? Feedback { get; set; }

    /// <summary>
    /// Strengths identified in the agent output.
    /// </summary>
    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    /// <summary>
    /// Areas for improvement identified.
    /// </summary>
    [JsonPropertyName("improvements")]
    public List<string> Improvements { get; set; } = new();

    /// <summary>
    /// The input that was evaluated.
    /// </summary>
    [JsonPropertyName("input")]
    public string? Input { get; set; }

    /// <summary>
    /// The output that was evaluated.
    /// </summary>
    [JsonPropertyName("output")]
    public string? Output { get; set; }

    /// <summary>
    /// Expected output for comparison (if available).
    /// </summary>
    [JsonPropertyName("expected_output")]
    public string? ExpectedOutput { get; set; }

    /// <summary>
    /// Evaluator information (human or automated).
    /// </summary>
    [JsonPropertyName("evaluator")]
    public EvaluatorInfo? Evaluator { get; set; }

    /// <summary>
    /// Duration of the evaluation in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; set; }

    /// <summary>
    /// Custom metadata for extensibility.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Evaluation result for a single rubric.
/// </summary>
public class RubricEvaluation
{
    /// <summary>
    /// ID of the rubric being evaluated.
    /// </summary>
    [JsonPropertyName("rubric_id")]
    public string RubricId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the rubric.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Score for this rubric (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }

    /// <summary>
    /// Weight of this rubric in overall score.
    /// </summary>
    [JsonPropertyName("weight")]
    public double Weight { get; set; } = 1.0;

    /// <summary>
    /// Whether this rubric passed its threshold.
    /// </summary>
    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    /// <summary>
    /// Threshold score required to pass.
    /// </summary>
    [JsonPropertyName("threshold")]
    public double Threshold { get; set; } = 0.7;

    /// <summary>
    /// Explanation of the score.
    /// </summary>
    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    /// <summary>
    /// Evidence supporting the evaluation.
    /// </summary>
    [JsonPropertyName("evidence")]
    public List<string>? Evidence { get; set; }
}

/// <summary>
/// Information about the evaluator (human or automated).
/// </summary>
public class EvaluatorInfo
{
    /// <summary>
    /// Type of evaluator.
    /// </summary>
    [JsonPropertyName("type")]
    public EvaluatorType Type { get; set; }

    /// <summary>
    /// Evaluator identifier (model name for LLM, user ID for human).
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Model used for LLM-based evaluation.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Version of the evaluation criteria/prompt.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}

/// <summary>
/// Type of evaluator.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EvaluatorType
{
    /// <summary>
    /// LLM-based automated evaluation.
    /// </summary>
    LlmJudge,

    /// <summary>
    /// Rule-based automated evaluation.
    /// </summary>
    Automated,

    /// <summary>
    /// Human evaluator.
    /// </summary>
    Human,

    /// <summary>
    /// Hybrid evaluation combining multiple approaches.
    /// </summary>
    Hybrid
}
