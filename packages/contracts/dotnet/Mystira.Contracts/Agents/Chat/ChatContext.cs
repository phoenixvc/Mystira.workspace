using System.Text.Json.Serialization;

namespace Mystira.Contracts.Agents.Chat;

/// <summary>
/// Generic chat completion context for agent interactions.
/// Provides a reusable pattern for maintaining conversation state and context.
/// </summary>
public class ChatContext
{
    /// <summary>
    /// Unique identifier for this chat context.
    /// </summary>
    [JsonPropertyName("context_id")]
    public string ContextId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Associated session ID (if applicable).
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    /// <summary>
    /// User identifier.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// Current system prompt.
    /// </summary>
    [JsonPropertyName("system_prompt")]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Conversation history messages.
    /// </summary>
    [JsonPropertyName("messages")]
    public List<ChatContextMessage> Messages { get; set; } = new();

    /// <summary>
    /// Variables available in the context.
    /// </summary>
    [JsonPropertyName("variables")]
    public Dictionary<string, object>? Variables { get; set; }

    /// <summary>
    /// Active tools in this context.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<ChatContextTool>? Tools { get; set; }

    /// <summary>
    /// Memory or state persisted across turns.
    /// </summary>
    [JsonPropertyName("memory")]
    public ChatContextMemory? Memory { get; set; }

    /// <summary>
    /// Model configuration for completions.
    /// </summary>
    [JsonPropertyName("model_config")]
    public ChatContextModelConfig? ModelConfig { get; set; }

    /// <summary>
    /// Token budget and usage tracking.
    /// </summary>
    [JsonPropertyName("token_budget")]
    public TokenBudget? TokenBudget { get; set; }

    /// <summary>
    /// Timestamp when context was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when context was last updated.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Custom metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets the total message count.
    /// </summary>
    [JsonIgnore]
    public int MessageCount => Messages.Count;

    /// <summary>
    /// Adds a user message to the context.
    /// </summary>
    public void AddUserMessage(string content, string? name = null)
    {
        Messages.Add(new ChatContextMessage
        {
            Role = "user",
            Content = content,
            Name = name,
            Timestamp = DateTime.UtcNow
        });
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an assistant message to the context.
    /// </summary>
    public void AddAssistantMessage(string content, List<ChatContextToolCall>? toolCalls = null)
    {
        Messages.Add(new ChatContextMessage
        {
            Role = "assistant",
            Content = content,
            ToolCalls = toolCalls,
            Timestamp = DateTime.UtcNow
        });
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a tool result message to the context.
    /// </summary>
    public void AddToolResult(string toolCallId, string content, bool success = true)
    {
        Messages.Add(new ChatContextMessage
        {
            Role = "tool",
            Content = content,
            ToolCallId = toolCallId,
            Timestamp = DateTime.UtcNow
        });
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// A message in the chat context.
/// </summary>
public class ChatContextMessage
{
    /// <summary>
    /// Role of the message sender (system, user, assistant, tool).
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    /// <summary>
    /// Content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Name of the sender (optional).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Tool calls made by the assistant.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public List<ChatContextToolCall>? ToolCalls { get; set; }

    /// <summary>
    /// Tool call ID this message responds to (for tool role).
    /// </summary>
    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }

    /// <summary>
    /// Timestamp of the message.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Token count for this message (if calculated).
    /// </summary>
    [JsonPropertyName("token_count")]
    public int? TokenCount { get; set; }
}

/// <summary>
/// A tool call in a message.
/// </summary>
public class ChatContextToolCall
{
    /// <summary>
    /// Unique identifier for this tool call.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the tool.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// JSON-encoded arguments.
    /// </summary>
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}

/// <summary>
/// Tool definition in the chat context.
/// </summary>
public class ChatContextTool
{
    /// <summary>
    /// Type of tool (typically "function").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    /// <summary>
    /// Function definition.
    /// </summary>
    [JsonPropertyName("function")]
    public ChatContextFunction? Function { get; set; }
}

/// <summary>
/// Function definition for a tool.
/// </summary>
public class ChatContextFunction
{
    /// <summary>
    /// Name of the function.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what the function does.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// JSON schema for the function parameters.
    /// </summary>
    [JsonPropertyName("parameters")]
    public object? Parameters { get; set; }

    /// <summary>
    /// Whether the function is strict.
    /// </summary>
    [JsonPropertyName("strict")]
    public bool? Strict { get; set; }
}

/// <summary>
/// Memory or state persisted across conversation turns.
/// </summary>
public class ChatContextMemory
{
    /// <summary>
    /// Short-term memory items (recent context).
    /// </summary>
    [JsonPropertyName("short_term")]
    public Dictionary<string, object>? ShortTerm { get; set; }

    /// <summary>
    /// Long-term memory items (persisted facts).
    /// </summary>
    [JsonPropertyName("long_term")]
    public Dictionary<string, object>? LongTerm { get; set; }

    /// <summary>
    /// Summarized conversation history.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Key facts extracted from conversation.
    /// </summary>
    [JsonPropertyName("facts")]
    public List<string>? Facts { get; set; }
}

/// <summary>
/// Model configuration for chat completions.
/// </summary>
public class ChatContextModelConfig
{
    /// <summary>
    /// Model identifier or deployment name.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Temperature for response generation.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Maximum tokens to generate.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Top-p sampling parameter.
    /// </summary>
    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    /// <summary>
    /// Response format (e.g., "text", "json_object").
    /// </summary>
    [JsonPropertyName("response_format")]
    public string? ResponseFormat { get; set; }
}

/// <summary>
/// Token budget tracking for context management.
/// </summary>
public class TokenBudget
{
    /// <summary>
    /// Maximum tokens allowed for the context.
    /// </summary>
    [JsonPropertyName("max_context_tokens")]
    public int MaxContextTokens { get; set; } = 8000;

    /// <summary>
    /// Current token count in context.
    /// </summary>
    [JsonPropertyName("current_tokens")]
    public int CurrentTokens { get; set; }

    /// <summary>
    /// Tokens reserved for the response.
    /// </summary>
    [JsonPropertyName("reserved_for_response")]
    public int ReservedForResponse { get; set; } = 1000;

    /// <summary>
    /// Strategy for handling context overflow.
    /// </summary>
    [JsonPropertyName("overflow_strategy")]
    public ContextOverflowStrategy OverflowStrategy { get; set; } = ContextOverflowStrategy.TruncateOldest;

    /// <summary>
    /// Gets available tokens for new messages.
    /// </summary>
    [JsonIgnore]
    public int AvailableTokens => MaxContextTokens - CurrentTokens - ReservedForResponse;
}

/// <summary>
/// Strategy for handling context token overflow.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContextOverflowStrategy
{
    /// <summary>
    /// Remove oldest messages first.
    /// </summary>
    TruncateOldest,

    /// <summary>
    /// Summarize older messages.
    /// </summary>
    Summarize,

    /// <summary>
    /// Remove least relevant messages.
    /// </summary>
    PruneByRelevance,

    /// <summary>
    /// Fail if context exceeds limit.
    /// </summary>
    Error
}
