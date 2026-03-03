using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.Agents.Configuration;

/// <summary>
/// Generic Azure AI Foundry configuration for agents.
/// Provides a reusable pattern for configuring AI agents across the platform.
/// </summary>
public class FoundryAgentConfig
{
    /// <summary>
    /// Configuration section name for binding from appsettings.
    /// </summary>
    public const string SectionName = "FoundryAgent";

    /// <summary>
    /// Unique identifier for this agent configuration.
    /// </summary>
    [JsonPropertyName("agent_id")]
    [Required]
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the agent.
    /// </summary>
    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the agent's purpose and capabilities.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Azure AI Foundry project endpoint URL.
    /// </summary>
    [JsonPropertyName("endpoint")]
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// API key or managed identity credential for authentication.
    /// </summary>
    [JsonPropertyName("api_key")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// Whether to use managed identity for authentication instead of API key.
    /// </summary>
    [JsonPropertyName("use_managed_identity")]
    public bool UseManagedIdentity { get; set; }

    /// <summary>
    /// The AI model deployment to use for this agent.
    /// </summary>
    [JsonPropertyName("deployment")]
    [Required]
    public string Deployment { get; set; } = string.Empty;

    /// <summary>
    /// Model configuration settings.
    /// </summary>
    [JsonPropertyName("model_config")]
    public AgentModelConfig ModelConfig { get; set; } = new();

    /// <summary>
    /// Tool configurations for this agent.
    /// </summary>
    [JsonPropertyName("tools")]
    public List<AgentToolConfig> Tools { get; set; } = new();

    /// <summary>
    /// System prompt template for the agent.
    /// </summary>
    [JsonPropertyName("system_prompt")]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Maximum conversation turns before summarization or truncation.
    /// </summary>
    [JsonPropertyName("max_conversation_turns")]
    [Range(1, 1000)]
    public int MaxConversationTurns { get; set; } = 50;

    /// <summary>
    /// Whether streaming is enabled for this agent.
    /// </summary>
    [JsonPropertyName("enable_streaming")]
    public bool EnableStreaming { get; set; } = true;

    /// <summary>
    /// Timeout in seconds for agent operations.
    /// </summary>
    [JsonPropertyName("timeout_seconds")]
    [Range(1, 600)]
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Retry configuration for transient failures.
    /// </summary>
    [JsonPropertyName("retry")]
    public AgentRetryConfig Retry { get; set; } = new();

    /// <summary>
    /// Whether this agent configuration is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Custom metadata for extensibility.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Model configuration settings for an agent.
/// </summary>
public class AgentModelConfig
{
    /// <summary>
    /// Controls randomness in responses (0.0 to 2.0).
    /// </summary>
    [JsonPropertyName("temperature")]
    [Range(0.0, 2.0)]
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Maximum tokens to generate.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    [Range(1, 128000)]
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Top-p (nucleus) sampling parameter.
    /// </summary>
    [JsonPropertyName("top_p")]
    [Range(0.0, 1.0)]
    public double TopP { get; set; } = 1.0;

    /// <summary>
    /// Frequency penalty (-2.0 to 2.0).
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    [Range(-2.0, 2.0)]
    public double FrequencyPenalty { get; set; } = 0.0;

    /// <summary>
    /// Presence penalty (-2.0 to 2.0).
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    [Range(-2.0, 2.0)]
    public double PresencePenalty { get; set; } = 0.0;

    /// <summary>
    /// Sequences that will stop generation.
    /// </summary>
    [JsonPropertyName("stop_sequences")]
    public List<string>? StopSequences { get; set; }
}

/// <summary>
/// Configuration for an agent tool.
/// </summary>
public class AgentToolConfig
{
    /// <summary>
    /// Unique identifier for the tool.
    /// </summary>
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of tool (e.g., "function", "code_interpreter", "file_search").
    /// </summary>
    [JsonPropertyName("type")]
    [Required]
    public string Type { get; set; } = "function";

    /// <summary>
    /// Tool name for function calling.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Description of what the tool does.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// JSON schema for the tool's parameters.
    /// </summary>
    [JsonPropertyName("parameters_schema")]
    public string? ParametersSchema { get; set; }

    /// <summary>
    /// Whether this tool is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Retry configuration for agent operations.
/// </summary>
public class AgentRetryConfig
{
    /// <summary>
    /// Maximum number of retry attempts.
    /// </summary>
    [JsonPropertyName("max_retries")]
    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial delay in milliseconds before first retry.
    /// </summary>
    [JsonPropertyName("initial_delay_ms")]
    [Range(100, 60000)]
    public int InitialDelayMs { get; set; } = 1000;

    /// <summary>
    /// Multiplier for exponential backoff.
    /// </summary>
    [JsonPropertyName("backoff_multiplier")]
    [Range(1.0, 5.0)]
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Maximum delay in milliseconds between retries.
    /// </summary>
    [JsonPropertyName("max_delay_ms")]
    [Range(1000, 300000)]
    public int MaxDelayMs { get; set; } = 30000;
}
