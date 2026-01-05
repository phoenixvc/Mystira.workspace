using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Domain.Agents;

/// <summary>
/// Represents an evaluation report for a story iteration.
/// Contains scores and findings from the evaluation process.
/// </summary>
public class EvaluationReport
{
    /// <summary>
    /// The iteration number this report corresponds to.
    /// </summary>
    public int IterationNumber { get; set; }

    /// <summary>
    /// Timestamp when the evaluation was performed.
    /// </summary>
    public DateTime EvaluationTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Overall status of the evaluation.
    /// </summary>
    public EvaluationStatus OverallStatus { get; set; }

    /// <summary>
    /// Whether the safety gate checks passed.
    /// </summary>
    public bool SafetyGatePassed { get; set; }

    /// <summary>
    /// Score for alignment with developmental axes (0-1 scale).
    /// </summary>
    public float AxesAlignmentScore { get; set; }

    /// <summary>
    /// Score for adherence to developer principles (0-1 scale).
    /// </summary>
    public float DevPrinciplesScore { get; set; }

    /// <summary>
    /// Score for narrative logic and consistency (0-1 scale).
    /// </summary>
    public float NarrativeLogicScore { get; set; }

    /// <summary>
    /// Map of category to list of findings.
    /// </summary>
    public Dictionary<string, List<string>> Findings { get; set; } = new();

    /// <summary>
    /// Recommendations for improvement.
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Total token usage for this iteration.
    /// </summary>
    public int TokenUsage { get; set; }
}
