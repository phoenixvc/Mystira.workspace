using System.Text.Json.Serialization;
using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Contracts.StoryGenerator.Commands.Stories;

/// <summary>
/// Command to refine an existing story.
/// </summary>
public class RefineStoryCommand
{
    /// <summary>
    /// Current story to refine (JSON or YAML).
    /// </summary>
    [JsonPropertyName("current_story")]
    public string CurrentStory { get; set; } = string.Empty;

    /// <summary>
    /// Format of the current story.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "json";

    /// <summary>
    /// Refinement instructions.
    /// </summary>
    [JsonPropertyName("instructions")]
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// Specific areas to focus on.
    /// </summary>
    [JsonPropertyName("focus_areas")]
    public List<string>? FocusAreas { get; set; }

    /// <summary>
    /// Issues to address from validation.
    /// </summary>
    [JsonPropertyName("issues_to_fix")]
    public List<string>? IssuesToFix { get; set; }

    /// <summary>
    /// Previous feedback from evaluation.
    /// </summary>
    [JsonPropertyName("feedback")]
    public string? Feedback { get; set; }

    /// <summary>
    /// AI provider to use.
    /// </summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }

    /// <summary>
    /// Model ID to use.
    /// </summary>
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    /// <summary>
    /// Whether to stream the response.
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}

/// <summary>
/// Response from story refinement.
/// </summary>
public class RefineStoryResponse
{
    /// <summary>
    /// Whether refinement succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// The refined story content.
    /// </summary>
    [JsonPropertyName("story")]
    public string? Story { get; set; }

    /// <summary>
    /// Format of the story (json/yaml).
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = "json";

    /// <summary>
    /// Story snapshot for tracking.
    /// </summary>
    [JsonPropertyName("snapshot")]
    public StorySnapshot? Snapshot { get; set; }

    /// <summary>
    /// Summary of changes made.
    /// </summary>
    [JsonPropertyName("changes_summary")]
    public string? ChangesSummary { get; set; }

    /// <summary>
    /// Specific changes made.
    /// </summary>
    [JsonPropertyName("changes")]
    public List<StoryChange>? Changes { get; set; }

    /// <summary>
    /// Error message if failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Token usage.
    /// </summary>
    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}

/// <summary>
/// Describes a change made during refinement.
/// </summary>
public class StoryChange
{
    /// <summary>
    /// Type of change.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Path in the story structure.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Description of the change.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Previous value (if applicable).
    /// </summary>
    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    /// <summary>
    /// New value.
    /// </summary>
    [JsonPropertyName("current")]
    public string? Current { get; set; }
}

/// <summary>
/// Token usage statistics.
/// </summary>
public class TokenUsage
{
    /// <summary>
    /// Tokens in the prompt.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// Tokens in the completion.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total tokens.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
