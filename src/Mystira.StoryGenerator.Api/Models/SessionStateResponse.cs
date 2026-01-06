using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Api.Models;

/// <summary>
/// Response for getting the current state of a story generation session.
/// </summary>
public class SessionStateResponse
{
    /// <summary>
    /// The session identifier.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AI Foundry thread ID.
    /// </summary>
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>
    /// Current stage of the session.
    /// </summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>
    /// Current iteration count.
    /// </summary>
    public int IterationCount { get; set; }

    /// <summary>
    /// Estimated cost of the session in USD.
    /// </summary>
    public decimal CostEstimate { get; set; }

    /// <summary>
    /// Current story as JSON string.
    /// </summary>
    public string CurrentStoryJson { get; set; } = string.Empty;

    /// <summary>
    /// The latest evaluation report, if available.
    /// </summary>
    public EvaluationReport? LastEvaluationReport { get; set; }

    /// <summary>
    /// List of all story versions.
    /// </summary>
    public List<StoryVersionSnapshot> StoryVersions { get; set; } = new();
}