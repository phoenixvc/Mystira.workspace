using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Common;

/// <summary>
/// Token usage statistics for LLM API calls.
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
