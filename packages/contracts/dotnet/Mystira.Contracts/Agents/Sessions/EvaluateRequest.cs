using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Mystira.Contracts.Agents.Evaluation;

namespace Mystira.Contracts.Agents.Sessions;

/// <summary>
/// Request to evaluate an agent's output against defined criteria.
/// Provides a generic pattern for assessing agent response quality.
/// </summary>
public class EvaluateRequest
{
    /// <summary>
    /// ID of the session to evaluate.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    /// <summary>
    /// The input that was provided to the agent.
    /// </summary>
    [JsonPropertyName("input")]
    public string? Input { get; set; }

    /// <summary>
    /// The output to evaluate.
    /// </summary>
    [JsonPropertyName("output")]
    [Required]
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Expected output for comparison (optional).
    /// </summary>
    [JsonPropertyName("expected_output")]
    public string? ExpectedOutput { get; set; }

    /// <summary>
    /// ID of the rubric set to use for evaluation.
    /// </summary>
    [JsonPropertyName("rubric_set_id")]
    public string? RubricSetId { get; set; }

    /// <summary>
    /// Specific rubric IDs to evaluate against (if not using a set).
    /// </summary>
    [JsonPropertyName("rubric_ids")]
    public List<string>? RubricIds { get; set; }

    /// <summary>
    /// Inline rubrics for ad-hoc evaluation.
    /// </summary>
    [JsonPropertyName("inline_rubrics")]
    public List<RubricSummary>? InlineRubrics { get; set; }

    /// <summary>
    /// Type of evaluator to use.
    /// </summary>
    [JsonPropertyName("evaluator_type")]
    public EvaluatorType EvaluatorType { get; set; } = EvaluatorType.LlmJudge;

    /// <summary>
    /// Model to use for LLM-based evaluation.
    /// </summary>
    [JsonPropertyName("evaluator_model")]
    public string? EvaluatorModel { get; set; }

    /// <summary>
    /// Whether to include detailed feedback.
    /// </summary>
    [JsonPropertyName("include_feedback")]
    public bool IncludeFeedback { get; set; } = true;

    /// <summary>
    /// Whether to include evidence for scores.
    /// </summary>
    [JsonPropertyName("include_evidence")]
    public bool IncludeEvidence { get; set; } = true;

    /// <summary>
    /// Additional context for the evaluator.
    /// </summary>
    [JsonPropertyName("evaluation_context")]
    public Dictionary<string, string>? EvaluationContext { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response from an evaluation request.
/// </summary>
public class EvaluateResponse
{
    /// <summary>
    /// Whether evaluation was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// The full evaluation report.
    /// </summary>
    [JsonPropertyName("report")]
    public EvaluationReport? Report { get; set; }

    /// <summary>
    /// Quick summary of pass/fail status.
    /// </summary>
    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    /// <summary>
    /// Overall score (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("overall_score")]
    public double OverallScore { get; set; }

    /// <summary>
    /// Quick summary of results per rubric.
    /// </summary>
    [JsonPropertyName("rubric_scores")]
    public Dictionary<string, double>? RubricScores { get; set; }

    /// <summary>
    /// Error message if evaluation failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Duration of evaluation in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public long? DurationMs { get; set; }

    /// <summary>
    /// Token usage for the evaluation.
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
/// Request for batch evaluation of multiple outputs.
/// </summary>
public class BatchEvaluateRequest
{
    /// <summary>
    /// Individual evaluation requests.
    /// </summary>
    [JsonPropertyName("evaluations")]
    [Required]
    [MinLength(1)]
    public List<EvaluateRequest> Evaluations { get; set; } = new();

    /// <summary>
    /// Whether to run evaluations in parallel.
    /// </summary>
    [JsonPropertyName("parallel")]
    public bool Parallel { get; set; } = true;

    /// <summary>
    /// Maximum concurrent evaluations (if parallel).
    /// </summary>
    [JsonPropertyName("max_concurrency")]
    public int MaxConcurrency { get; set; } = 5;

    /// <summary>
    /// Shared rubric set ID for all evaluations.
    /// </summary>
    [JsonPropertyName("shared_rubric_set_id")]
    public string? SharedRubricSetId { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response from batch evaluation.
/// </summary>
public class BatchEvaluateResponse
{
    /// <summary>
    /// Whether all evaluations completed successfully.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// Individual evaluation results.
    /// </summary>
    [JsonPropertyName("results")]
    public List<EvaluateResponse> Results { get; set; } = new();

    /// <summary>
    /// Aggregate statistics across all evaluations.
    /// </summary>
    [JsonPropertyName("aggregate")]
    public BatchEvaluationAggregate? Aggregate { get; set; }

    /// <summary>
    /// Total duration in milliseconds.
    /// </summary>
    [JsonPropertyName("total_duration_ms")]
    public long? TotalDurationMs { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Aggregate statistics for batch evaluation.
/// </summary>
public class BatchEvaluationAggregate
{
    /// <summary>
    /// Total number of evaluations.
    /// </summary>
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Number that passed.
    /// </summary>
    [JsonPropertyName("passed_count")]
    public int PassedCount { get; set; }

    /// <summary>
    /// Number that failed.
    /// </summary>
    [JsonPropertyName("failed_count")]
    public int FailedCount { get; set; }

    /// <summary>
    /// Pass rate (0.0 to 1.0).
    /// </summary>
    [JsonPropertyName("pass_rate")]
    public double PassRate { get; set; }

    /// <summary>
    /// Average overall score.
    /// </summary>
    [JsonPropertyName("average_score")]
    public double AverageScore { get; set; }

    /// <summary>
    /// Minimum score.
    /// </summary>
    [JsonPropertyName("min_score")]
    public double MinScore { get; set; }

    /// <summary>
    /// Maximum score.
    /// </summary>
    [JsonPropertyName("max_score")]
    public double MaxScore { get; set; }

    /// <summary>
    /// Average score per rubric.
    /// </summary>
    [JsonPropertyName("rubric_averages")]
    public Dictionary<string, double>? RubricAverages { get; set; }
}
