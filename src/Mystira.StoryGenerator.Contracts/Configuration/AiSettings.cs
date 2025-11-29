using System.ComponentModel.DataAnnotations;

namespace Mystira.StoryGenerator.Contracts.Configuration;

public class AiSettings
{
    public const string SectionName = "Ai";

    [Required]
    public string DefaultProvider { get; set; } = string.Empty;

    [Range(0, 2)]
    public double DefaultTemperature { get; set; } = 0.7;

    [Range(1, 25000)]
    public int DefaultMaxTokens { get; set; } = 25000;

    public AzureOpenAISettings AzureOpenAI { get; set; } = new();
    public IntentRouterSettings IntentRouter { get; set; } = new();
    public EntityClassifierSettings EntityClassifier { get; set; } = new();
    public ConsistencyEvaluatorSettings ConsistencyEvaluator { get; set; } = new();
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

/// <summary>
/// Settings for the LLM-based entity classifier
/// </summary>
public class EntityClassifierSettings
{
    public const string SectionName = "EntityClassifier";

    public bool Enabled { get; set; } = true;

    public string? Provider { get; set; }
        = null; // e.g., "azure-openai"

    public string? DeploymentName { get; set; }
        = null; // e.g., specific deployment/model name

    public string? ModelId { get; set; }
        = null; // logical model id

    [Range(0, 2)]
    public double Temperature { get; set; } = 0.1;

    [Range(1, 1000)]
    public int MaxTokens { get; set; } = 200;

    public bool IsConfigured =>
        Enabled &&
        !string.IsNullOrWhiteSpace(Provider) &&
        !string.IsNullOrWhiteSpace(DeploymentName);
}

public class ConsistencyEvaluatorSettings
{
    public const string SectionName = "ConsistencyEvaluator";

    public bool Enabled { get; set; } = true;

    public string? Provider { get; set; }
        = null; // e.g., "azure-openai"

    public string? DeploymentName { get; set; }
        = null; // e.g., specific deployment/model name

    public string? ModelId { get; set; }
        = null; // logical model id

    [Range(0, 2)]
    public double Temperature { get; set; } = 0.25;

    [Range(1, 25000)]
    public int MaxTokens { get; set; } = 10000;

    public bool IsConfigured =>
        Enabled &&
        !string.IsNullOrWhiteSpace(Provider) &&
        !string.IsNullOrWhiteSpace(DeploymentName);
}
