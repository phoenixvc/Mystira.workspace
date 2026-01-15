namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Represents an event emitted during the agent orchestration process.
/// </summary>
public class AgentStreamEvent
{
    /// <summary>
    /// Type of event being emitted.
    /// </summary>
    public enum EventType
    {
        PhaseStarted,
        GenerationComplete,
        ValidationFailed,
        EvaluationPassed,
        EvaluationFailed,
        RefinementComplete,
        RubricGenerated,
        MaxIterationsReached,
        Error,
        TokenUsageUpdate
    }

    /// <summary>
    /// The type of this event.
    /// </summary>
    public EventType Type { get; set; }

    /// <summary>
    /// The phase name this event relates to.
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// The event payload data.
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional iteration number this event relates to.
    /// </summary>
    public int? IterationNumber { get; set; }
}