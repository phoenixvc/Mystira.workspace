using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.Agents.Sessions;

/// <summary>
/// Request to start a new agent session.
/// Provides a generic pattern for initializing agent interactions.
/// </summary>
public class SessionStartRequest
{
    /// <summary>
    /// ID of the agent to use for this session.
    /// </summary>
    [JsonPropertyName("agent_id")]
    [Required]
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Optional user identifier for the session.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// Optional conversation/thread ID for continuing an existing session.
    /// </summary>
    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }

    /// <summary>
    /// Initial system prompt override.
    /// </summary>
    [JsonPropertyName("system_prompt")]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Initial user message to start the session (optional).
    /// </summary>
    [JsonPropertyName("initial_message")]
    public string? InitialMessage { get; set; }

    /// <summary>
    /// Context to provide to the agent at session start.
    /// </summary>
    [JsonPropertyName("context")]
    public SessionContext? Context { get; set; }

    /// <summary>
    /// Tools to enable for this session (overrides agent defaults).
    /// </summary>
    [JsonPropertyName("enabled_tools")]
    public List<string>? EnabledTools { get; set; }

    /// <summary>
    /// Whether to enable streaming for this session.
    /// </summary>
    [JsonPropertyName("enable_streaming")]
    public bool? EnableStreaming { get; set; }

    /// <summary>
    /// Session-specific model configuration overrides.
    /// </summary>
    [JsonPropertyName("model_overrides")]
    public SessionModelOverrides? ModelOverrides { get; set; }

    /// <summary>
    /// Custom metadata to attach to the session.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Context provided to an agent at session start.
/// </summary>
public class SessionContext
{
    /// <summary>
    /// Key-value pairs of context information.
    /// </summary>
    [JsonPropertyName("variables")]
    public Dictionary<string, string>? Variables { get; set; }

    /// <summary>
    /// Files or documents to provide as context.
    /// </summary>
    [JsonPropertyName("documents")]
    public List<SessionDocument>? Documents { get; set; }

    /// <summary>
    /// Previous conversation history to include.
    /// </summary>
    [JsonPropertyName("history")]
    public List<SessionMessage>? History { get; set; }

    /// <summary>
    /// Instructions or constraints for the session.
    /// </summary>
    [JsonPropertyName("instructions")]
    public List<string>? Instructions { get; set; }
}

/// <summary>
/// Document or file provided as context to an agent.
/// </summary>
public class SessionDocument
{
    /// <summary>
    /// Identifier for the document.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Document title or name.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Document content (for text documents).
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// URL or path to the document (for file references).
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// MIME type of the document.
    /// </summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }
}

/// <summary>
/// A message in conversation history.
/// </summary>
public class SessionMessage
{
    /// <summary>
    /// Role of the message sender.
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = "user";

    /// <summary>
    /// Content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the message.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// Name of the sender (optional).
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
/// Model configuration overrides for a session.
/// </summary>
public class SessionModelOverrides
{
    /// <summary>
    /// Temperature override.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    /// <summary>
    /// Max tokens override.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Specific model/deployment to use.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }
}

/// <summary>
/// Response from starting an agent session.
/// </summary>
public class SessionStartResponse
{
    /// <summary>
    /// Unique identifier for the new session.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the agent being used.
    /// </summary>
    [JsonPropertyName("agent_id")]
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Conversation/thread ID (may be same as session_id or different).
    /// </summary>
    [JsonPropertyName("conversation_id")]
    public string? ConversationId { get; set; }

    /// <summary>
    /// Whether the session was successfully created.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    /// <summary>
    /// Timestamp when the session was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session expiration time (if applicable).
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Initial agent response (if initial_message was provided).
    /// </summary>
    [JsonPropertyName("initial_response")]
    public string? InitialResponse { get; set; }

    /// <summary>
    /// Active tools for this session.
    /// </summary>
    [JsonPropertyName("active_tools")]
    public List<string>? ActiveTools { get; set; }

    /// <summary>
    /// Error message if session creation failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}
