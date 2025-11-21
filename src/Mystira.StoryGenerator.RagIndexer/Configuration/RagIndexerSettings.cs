using System.ComponentModel.DataAnnotations;

namespace Mystira.StoryGenerator.RagIndexer.Configuration;

public class RagIndexerSettings
{
    public const string SectionName = "RagIndexer";

    [Required]
    public AzureAISearchSettings AzureAISearch { get; set; } = new();

    [Required]
    public AzureOpenAIEmbeddingSettings AzureOpenAIEmbedding { get; set; } = new();
}

public class AzureAISearchSettings
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string IndexName { get; set; } = "mystira-instructions";

    [Required]
    public string ApiKey { get; set; } = string.Empty;
}

public class AzureOpenAIEmbeddingSettings
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string DeploymentName { get; set; } = "text-embedding-ada-002";
}