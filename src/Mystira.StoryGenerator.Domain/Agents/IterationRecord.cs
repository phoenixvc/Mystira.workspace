namespace Mystira.StoryGenerator.Domain.Agents;

/// <summary>
/// Represents a record of a single iteration through the story generation pipeline.
/// Tracks run IDs, timing, and costs for each stage.
/// </summary>
public class IterationRecord
{
    /// <summary>
    /// The session ID this iteration belongs to.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The iteration number (1-based).
    /// </summary>
    public int IterationNumber { get; set; }

    /// <summary>
    /// The Writer agent run ID.
    /// </summary>
    public string WriterRunId { get; set; } = string.Empty;

    /// <summary>
    /// The Judge agent run ID.
    /// </summary>
    public string JudgeRunId { get; set; } = string.Empty;

    /// <summary>
    /// The Refiner agent run ID. Nullable if no refinement was needed.
    /// </summary>
    public string? RefinerRunId { get; set; }

    /// <summary>
    /// The Rubric Summary agent run ID. Nullable if no rubric summary was generated.
    /// </summary>
    public string? RubricSummaryRunId { get; set; }

    /// <summary>
    /// Duration of each stage.
    /// </summary>
    public Dictionary<string, TimeSpan> StageDurations { get; set; } = new();

    /// <summary>
    /// Token usage by stage.
    /// </summary>
    public Dictionary<string, int> TokensByStage { get; set; } = new();

    /// <summary>
    /// Estimated cost for this iteration.
    /// </summary>
    public decimal EstimatedCost { get; set; }

    /// <summary>
    /// The evaluation report for this iteration.
    /// </summary>
    public EvaluationReport EvaluationReport { get; set; } = new();

    /// <summary>
    /// Optional user feedback for this iteration.
    /// </summary>
    public string? UserFeedback { get; set; }
}
