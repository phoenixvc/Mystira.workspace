using System.Text.Json.Serialization;
using Mystira.Contracts.Agents.Evaluation;
using Mystira.Contracts.Agents.Sessions;

namespace Mystira.Contracts.StoryGenerator.Sessions;

/// <summary>
/// Request to start a new story generation session.
/// </summary>
public record StartSessionRequest
{
    /// <summary>
    /// The title of the story to generate.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Target age group for the story.
    /// </summary>
    [JsonPropertyName("age_group")]
    public string AgeGroup { get; init; } = string.Empty;

    /// <summary>
    /// Target narrative axes.
    /// </summary>
    [JsonPropertyName("target_axes")]
    public List<string> TargetAxes { get; init; } = new();

    /// <summary>
    /// Knowledge retrieval mode.
    /// </summary>
    [JsonPropertyName("knowledge_mode")]
    public KnowledgeMode KnowledgeMode { get; init; }

    /// <summary>
    /// Maximum number of iterations.
    /// </summary>
    [JsonPropertyName("max_iterations")]
    public int MaxIterations { get; init; } = 3;

    /// <summary>
    /// Minimum number of scenes.
    /// </summary>
    [JsonPropertyName("min_scenes")]
    public int MinScenes { get; init; } = 3;

    /// <summary>
    /// Maximum number of scenes.
    /// </summary>
    [JsonPropertyName("max_scenes")]
    public int MaxScenes { get; init; } = 10;
}

/// <summary>
/// Response from starting a story generation session.
/// </summary>
public record StorySessionStartResponse
{
    /// <summary>
    /// The session ID.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// Whether the session started successfully.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}

/// <summary>
/// Current state of a story generation session.
/// </summary>
public record SessionStateResponse
{
    /// <summary>
    /// The session ID.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// Current stage of the session.
    /// </summary>
    [JsonPropertyName("stage")]
    public string Stage { get; init; } = string.Empty;

    /// <summary>
    /// Current iteration count.
    /// </summary>
    [JsonPropertyName("iteration_count")]
    public int IterationCount { get; init; }

    /// <summary>
    /// Current story version as JSON.
    /// </summary>
    [JsonPropertyName("current_story_version")]
    public string? CurrentStoryVersion { get; init; }

    /// <summary>
    /// Current story version as YAML.
    /// </summary>
    [JsonPropertyName("current_story_yaml")]
    public string? CurrentStoryYaml { get; init; }

    /// <summary>
    /// All story versions.
    /// </summary>
    [JsonPropertyName("story_versions")]
    public List<StoryVersionSnapshot> StoryVersions { get; init; } = new();

    /// <summary>
    /// Latest evaluation report.
    /// </summary>
    [JsonPropertyName("last_evaluation_report")]
    public EvaluationReport? LastEvaluationReport { get; init; }

    /// <summary>
    /// Rubric summary.
    /// </summary>
    [JsonPropertyName("rubric_summary")]
    public RubricSummary? RubricSummary { get; init; }

    /// <summary>
    /// Error message if any.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}

/// <summary>
/// Response from story evaluation.
/// </summary>
public record EvaluateResponse
{
    /// <summary>
    /// Whether evaluation passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public bool Passed { get; init; }

    /// <summary>
    /// The evaluation report.
    /// </summary>
    [JsonPropertyName("report")]
    public EvaluationReport? Report { get; init; }

    /// <summary>
    /// Updated session state.
    /// </summary>
    [JsonPropertyName("session")]
    public SessionStateResponse? Session { get; init; }
}

/// <summary>
/// Request to refine the story.
/// </summary>
public record StoryRefineRequest
{
    /// <summary>
    /// User feedback for refinement.
    /// </summary>
    [JsonPropertyName("feedback")]
    public string? Feedback { get; init; }

    /// <summary>
    /// Focus areas for refinement.
    /// </summary>
    [JsonPropertyName("focus_areas")]
    public List<string> FocusAreas { get; init; } = new();
}

/// <summary>
/// Response from story refinement.
/// </summary>
public record RefineResponse
{
    /// <summary>
    /// Whether refinement succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// Updated session state.
    /// </summary>
    [JsonPropertyName("session")]
    public SessionStateResponse? Session { get; init; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}
