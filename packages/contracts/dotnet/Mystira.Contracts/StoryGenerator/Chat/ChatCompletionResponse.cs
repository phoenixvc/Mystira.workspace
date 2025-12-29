using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Chat;

/// <summary>
/// Response model for chat completion API calls
/// </summary>
public class ChatCompletionResponse
{
    /// <summary>
    /// The generated message content
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The model used for generation
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Logical model identifier resolved from configuration, when available.
    /// </summary>
    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    /// <summary>
    /// The provider used for generation
    /// </summary>
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Usage statistics for the request
    /// </summary>
    [JsonPropertyName("usage")]
    public ChatCompletionUsage? Usage { get; set; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the response was successful
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the request failed
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// The reason the model stopped generating (e.g., "stop", "length", "content_filter")
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    /// <summary>
    /// Whether the generation was incomplete (e.g., due to length limits)
    /// </summary>
    [JsonPropertyName("is_incomplete")]
    public bool IsIncomplete { get; set; }
}

/// <summary>
/// Usage statistics for a chat completion
/// </summary>
public class ChatCompletionUsage
{
    /// <summary>
    /// Number of tokens in the prompt
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of tokens in the completion
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total number of tokens used
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
