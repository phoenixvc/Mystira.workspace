using System.Text.Json.Serialization;
using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Contracts.StoryGenerator.Commands.Chat;

/// <summary>
/// Command for handling free-form text input.
/// </summary>
public class FreeTextCommand
{
    /// <summary>
    /// The free-form text input.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Conversation context.
    /// </summary>
    [JsonPropertyName("context")]
    public List<MystiraChatMessage>? Context { get; set; }

    /// <summary>
    /// Current story snapshot for context.
    /// </summary>
    [JsonPropertyName("current_story")]
    public StorySnapshot? CurrentStory { get; set; }

    /// <summary>
    /// Session ID for conversation continuity.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }
}

/// <summary>
/// Response to a free text command.
/// </summary>
public class FreeTextResponse
{
    /// <summary>
    /// The response content.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Detected intent of the input.
    /// </summary>
    [JsonPropertyName("intent")]
    public string? Intent { get; set; }

    /// <summary>
    /// Whether the response includes a story update.
    /// </summary>
    [JsonPropertyName("has_story_update")]
    public bool HasStoryUpdate { get; set; }

    /// <summary>
    /// Updated story if applicable.
    /// </summary>
    [JsonPropertyName("story_update")]
    public StorySnapshot? StoryUpdate { get; set; }

    /// <summary>
    /// Suggested follow-up actions.
    /// </summary>
    [JsonPropertyName("suggestions")]
    public List<string>? Suggestions { get; set; }

    /// <summary>
    /// Usage statistics.
    /// </summary>
    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
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
