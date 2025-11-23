using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Context wrapper for chat completion requests containing all necessary information
/// </summary>
public class ChatContext
{
    /// <summary>
    /// All chat messages from the request
    /// </summary>
    public List<MystiraChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// Selected AI provider
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Selected model identifier
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Selected model name
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Temperature setting
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Maximum tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// System prompt
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// JSON schema format request
    /// </summary>
    public JsonSchemaResponseFormat? JsonSchemaFormat { get; set; }

    /// <summary>
    /// Whether schema validation should be strict
    /// </summary>
    public bool? IsSchemaValidationStrict { get; set; }

    /// <summary>
    /// Gets the latest user message from the conversation
    /// </summary>
    public string LatestUserMessage => Messages.LastOrDefault(m => m.MessageType == ChatMessageType.User)?.Content?.Trim() ?? string.Empty;
}