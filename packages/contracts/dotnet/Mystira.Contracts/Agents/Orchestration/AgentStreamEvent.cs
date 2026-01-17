using System.Text.Json.Serialization;

namespace Mystira.Contracts.Agents.Orchestration;

/// <summary>
/// Represents an event emitted during agent streaming operations.
/// Provides a generic pattern for streaming agent responses, tool calls, and status updates.
/// </summary>
public class AgentStreamEvent
{
    /// <summary>
    /// Unique identifier for this event.
    /// </summary>
    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of the stream event.
    /// </summary>
    [JsonPropertyName("event_type")]
    public AgentStreamEventType EventType { get; set; }

    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session ID this event belongs to.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    /// <summary>
    /// Agent ID that generated this event.
    /// </summary>
    [JsonPropertyName("agent_id")]
    public string? AgentId { get; set; }

    /// <summary>
    /// Content delta for token events.
    /// </summary>
    [JsonPropertyName("delta")]
    public string? Delta { get; set; }

    /// <summary>
    /// Accumulated content so far.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Tool call information for tool events.
    /// </summary>
    [JsonPropertyName("tool_call")]
    public AgentToolCall? ToolCall { get; set; }

    /// <summary>
    /// Tool result for tool completion events.
    /// </summary>
    [JsonPropertyName("tool_result")]
    public AgentToolResult? ToolResult { get; set; }

    /// <summary>
    /// Error information if the event represents an error.
    /// </summary>
    [JsonPropertyName("error")]
    public AgentError? Error { get; set; }

    /// <summary>
    /// Usage statistics (typically on completion events).
    /// </summary>
    [JsonPropertyName("usage")]
    public AgentUsage? Usage { get; set; }

    /// <summary>
    /// Reason the stream ended (for done events).
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    /// <summary>
    /// Custom metadata for extensibility.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Types of agent stream events.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgentStreamEventType
{
    /// <summary>
    /// Stream has started.
    /// </summary>
    StreamStart,

    /// <summary>
    /// Content token received.
    /// </summary>
    ContentDelta,

    /// <summary>
    /// Content block completed.
    /// </summary>
    ContentComplete,

    /// <summary>
    /// Tool call initiated.
    /// </summary>
    ToolCallStart,

    /// <summary>
    /// Tool call arguments being streamed.
    /// </summary>
    ToolCallDelta,

    /// <summary>
    /// Tool call completed.
    /// </summary>
    ToolCallComplete,

    /// <summary>
    /// Tool execution result received.
    /// </summary>
    ToolResult,

    /// <summary>
    /// Thinking/reasoning content (for models that support it).
    /// </summary>
    Thinking,

    /// <summary>
    /// Status update during processing.
    /// </summary>
    Status,

    /// <summary>
    /// Error occurred during streaming.
    /// </summary>
    Error,

    /// <summary>
    /// Stream has completed.
    /// </summary>
    Done
}

/// <summary>
/// Represents a tool call made by an agent.
/// </summary>
public class AgentToolCall
{
    /// <summary>
    /// Unique identifier for this tool call.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Name of the tool being called.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// JSON arguments for the tool call.
    /// </summary>
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }

    /// <summary>
    /// Index of this tool call in the message.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }
}

/// <summary>
/// Represents the result of a tool execution.
/// </summary>
public class AgentToolResult
{
    /// <summary>
    /// ID of the tool call this result corresponds to.
    /// </summary>
    [JsonPropertyName("tool_call_id")]
    public string ToolCallId { get; set; } = string.Empty;

    /// <summary>
    /// The result content from the tool execution.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// Whether the tool execution succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the tool execution failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Represents an error during agent operations.
/// </summary>
public class AgentError
{
    /// <summary>
    /// Error code for categorization.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details.
    /// </summary>
    [JsonPropertyName("details")]
    public Dictionary<string, object>? Details { get; set; }

    /// <summary>
    /// Whether the error is retryable.
    /// </summary>
    [JsonPropertyName("retryable")]
    public bool Retryable { get; set; }
}

/// <summary>
/// Token usage statistics for an agent operation.
/// </summary>
public class AgentUsage
{
    /// <summary>
    /// Number of tokens in the prompt.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// Number of tokens in the completion.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total tokens used.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    /// <summary>
    /// Tokens used for cached prompt (if applicable).
    /// </summary>
    [JsonPropertyName("cached_tokens")]
    public int? CachedTokens { get; set; }
}
