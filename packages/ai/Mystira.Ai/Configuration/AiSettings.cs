using System.ComponentModel.DataAnnotations;

namespace Mystira.Ai.Configuration;

/// <summary>
/// Core AI/LLM settings for provider configuration.
/// </summary>
public class AiSettings
{
    /// <summary>
    /// Configuration section name for AI settings.
    /// </summary>
    public const string SectionName = "Ai";

    /// <summary>
    /// Default LLM provider to use (e.g., "azure-openai", "anthropic").
    /// </summary>
    [Required]
    public string DefaultProvider { get; set; } = string.Empty;

    /// <summary>
    /// Default temperature for LLM completions.
    /// </summary>
    [Range(0, 2)]
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Default maximum tokens for LLM completions.
    /// </summary>
    [Range(1, 25000)]
    public int DefaultMaxTokens { get; set; } = 25000;

    /// <summary>
    /// Azure OpenAI provider settings.
    /// </summary>
    public AzureOpenAISettings AzureOpenAI { get; set; } = new();

    /// <summary>
    /// Anthropic Claude provider settings.
    /// </summary>
    public AnthropicSettings Anthropic { get; set; } = new();
}

/// <summary>
/// Azure OpenAI provider configuration.
/// </summary>
public class AzureOpenAISettings
{
    /// <summary>
    /// Azure OpenAI endpoint URL.
    /// </summary>
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI API key.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Default deployment name.
    /// </summary>
    [Required]
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// Optional embedding deployment for semantic search.
    /// </summary>
    public string? EmbeddingDeploymentName { get; set; }

    /// <summary>
    /// List of available deployments for this Azure OpenAI resource.
    /// </summary>
    public List<AzureOpenAIDeployment> Deployments { get; set; } = new();

    /// <summary>
    /// JSON schema validation settings.
    /// </summary>
    public SchemaValidationSettings SchemaValidation { get; set; } = new();
}

/// <summary>
/// Azure OpenAI deployment configuration.
/// </summary>
public class AzureOpenAIDeployment
{
    /// <summary>
    /// Deployment name.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly display name.
    /// </summary>
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional endpoint URL for this specific deployment.
    /// Falls back to the default endpoint if not specified.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Maximum tokens supported by this deployment.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Default temperature for this deployment.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Whether this deployment supports JSON schema response formatting.
    /// </summary>
    public bool SupportsJsonSchema { get; set; } = true;

    /// <summary>
    /// Model capabilities (e.g., "chat", "json-schema", "embeddings").
    /// </summary>
    public List<string> Capabilities { get; set; } = new();
}

/// <summary>
/// Anthropic Claude provider configuration.
/// </summary>
public class AnthropicSettings
{
    /// <summary>
    /// Anthropic API key.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Default model name (e.g., "claude-3-5-sonnet-20241022").
    /// </summary>
    [Required]
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Optional base URL for Anthropic API (for proxies or Azure AI integration).
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// List of available Anthropic models.
    /// </summary>
    public List<AnthropicModel> Models { get; set; } = new();
}

/// <summary>
/// Anthropic model configuration.
/// </summary>
public class AnthropicModel
{
    /// <summary>
    /// Model name/identifier.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly display name.
    /// </summary>
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional endpoint URL for this model.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Maximum tokens supported by this model.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Default temperature for this model.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Whether this model supports JSON mode output.
    /// </summary>
    public bool SupportsJsonMode { get; set; } = true;

    /// <summary>
    /// Model capabilities.
    /// </summary>
    public List<string> Capabilities { get; set; } = new();
}

/// <summary>
/// JSON schema validation settings.
/// </summary>
public class SchemaValidationSettings
{
    /// <summary>
    /// Path to the JSON schema file.
    /// </summary>
    public string? SchemaPath { get; set; }

    /// <summary>
    /// Whether schema validation should be strict.
    /// </summary>
    public bool IsSchemaValidationStrict { get; set; } = false;
}
