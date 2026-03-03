using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Chat;

/// <summary>
/// Response model for available chat models per provider
/// </summary>
public class ChatModelsResponse
{
    /// <summary>
    /// List of providers and their available models
    /// </summary>
    [JsonPropertyName("providers")]
    public List<ProviderModels> Providers { get; set; } = new();

    /// <summary>
    /// Total count of available models across all providers
    /// </summary>
    [JsonPropertyName("totalModels")]
    public int TotalModels { get; set; }
}

/// <summary>
/// Models available for a specific provider
/// </summary>
public class ProviderModels
{
    /// <summary>
    /// Name of the provider (e.g., "azure-openai", "google-gemini")
    /// </summary>
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Whether this provider is properly configured and available
    /// </summary>
    [JsonPropertyName("available")]
    public bool Available { get; set; }

    /// <summary>
    /// List of available models for this provider
    /// </summary>
    [JsonPropertyName("models")]
    public List<ChatModelInfo> Models { get; set; } = new();
}

/// <summary>
/// Information about a specific chat model
/// </summary>
public class ChatModelInfo
{
    /// <summary>
    /// Unique identifier for the model (e.g., deployment name or model ID)
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the model
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the model
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Maximum number of tokens supported by this model
    /// </summary>
    [JsonPropertyName("maxTokens")]
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Default temperature recommended for this model
    /// </summary>
    [JsonPropertyName("defaultTemperature")]
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Minimum temperature allowed for this model
    /// </summary>
    [JsonPropertyName("minTemperature")]
    public double MinTemperature { get; set; } = 0.0;

    /// <summary>
    /// Maximum temperature allowed for this model
    /// </summary>
    [JsonPropertyName("maxTemperature")]
    public double MaxTemperature { get; set; } = 2.0;

    /// <summary>
    /// Whether this model supports JSON schema response formatting
    /// </summary>
    [JsonPropertyName("supportsJsonSchema")]
    public bool SupportsJsonSchema { get; set; }

    /// <summary>
    /// Model capabilities
    /// </summary>
    [JsonPropertyName("capabilities")]
    public List<string> Capabilities { get; set; } = new();
}
