using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Domain.Agents;

/// <summary>
/// Represents a story generation session with Azure AI Foundry Agent.
/// Tracks the full lifecycle of story creation including versions, evaluations, and user feedback.
/// </summary>
public class StorySession
{
    /// <summary>
    /// Unique identifier for this story session.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The Azure AI Foundry thread ID. Nullable until the thread is created.
    /// </summary>
    public string? ThreadId { get; set; }

    /// <summary>
    /// The knowledge retrieval mode for this session.
    /// </summary>
    public KnowledgeMode KnowledgeMode { get; set; }

    /// <summary>
    /// Target age group for this session.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// Target narrative axes for this session.
    /// </summary>
    public List<string> TargetAxes { get; set; } = new();

    /// <summary>
    /// The current stage of the story generation process.
    /// </summary>
    public StorySessionStage Stage { get; set; }

    /// <summary>
    /// The current iteration count.
    /// </summary>
    public int IterationCount { get; set; }

    /// <summary>
    /// The current story version as JSON string.
    /// </summary>
    public string CurrentStoryVersion { get; set; } = string.Empty;

    /// <summary>
    /// The current story version as YAML string.
    /// </summary>
    public string CurrentStoryYaml { get; set; } = string.Empty;

    /// <summary>
    /// Immutable history of all story versions.
    /// </summary>
    public List<StoryVersionSnapshot> StoryVersions { get; set; } = new();

    /// <summary>
    /// The latest evaluation report. Nullable if no evaluation has been performed.
    /// </summary>
    public EvaluationReport? LastEvaluationReport { get; set; }

    /// <summary>
    /// User's refinement focus areas.
    /// </summary>
    public UserRefinementFocus? UserFocus { get; set; }

    /// <summary>
    /// Timestamp when the session was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the session was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Estimated cost of the session in USD.
    /// </summary>
    public decimal CostEstimate { get; set; }
}
