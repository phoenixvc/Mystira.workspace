using System.ComponentModel.DataAnnotations;

namespace Mystira.StoryGenerator.Contracts.Configuration;

public class AiSettings
{
    public const string SectionName = "Ai";

    [Required]
    public string DefaultProvider { get; set; } = string.Empty;

    [Range(0, 2)]
    public double DefaultTemperature { get; set; } = 0.7;

    [Range(1, 4096)]
    public int DefaultMaxTokens { get; set; } = 1000;

    public AzureOpenAISettings AzureOpenAI { get; set; } = new();
    public IntentRouterSettings IntentRouter { get; set; } = new();
}

public class AzureOpenAISettings
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// Optional dictionary of deployments available for this provider.
    /// Maps deployment names to their actual deployment names (e.g., "gpt-4.1" -> "gpt-4.1", "gpt-5" -> "gpt-5").
    /// If not provided, falls back to DeploymentName.
    /// </summary>
    public Dictionary<string, string> Deployments { get; set; } = new();

    /// <summary>
    /// Optional embedding deployment used for semantic search queries.
    /// </summary>
    public string? EmbeddingDeploymentName { get; set; }

    /// <summary>
    /// List of available deployments for this Azure OpenAI resource
    /// </summary>
    public List<AzureOpenAIDeployment> Deployments { get; set; } = new();

    public SchemaValidationSettings SchemaValidation { get; set; } = new();
}

/// <summary>
/// Represents an Azure OpenAI deployment configuration
/// </summary>
public class AzureOpenAIDeployment
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of tokens supported by this deployment
    /// </summary>
    public int MaxTokens { get; set; } = 4096;

    /// <summary>
    /// Default temperature recommended for this deployment
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;

    /// <summary>
    /// Whether this deployment supports JSON schema response formatting
    /// </summary>
    public bool SupportsJsonSchema { get; set; } = true;

    /// <summary>
    /// Model capabilities for this deployment
    /// </summary>
    public List<string> Capabilities { get; set; } = new();
}

public class SchemaValidationSettings
{
    /// <summary>
    /// Path to the JSON schema file used for story generation/validation. Can be relative to the app base directory.
    /// </summary>
    public string? SchemaPath { get; set; }

    /// <summary>
    /// Whether schema validation/response formatting should be strict when supported by the provider.
    /// </summary>
    public bool IsSchemaValidationStrict { get; set; } = false;
}
