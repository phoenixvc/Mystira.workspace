using System.ComponentModel.DataAnnotations;

namespace Mystira.Contracts.StoryGenerator.Configuration;

/// <summary>
/// Settings for embedding generation and semantic search.
/// </summary>
public class EmbeddingSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Embedding";

    /// <summary>
    /// Whether embeddings are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Provider for embeddings (e.g., "azure-openai", "openai").
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Deployment name for Azure OpenAI embeddings.
    /// </summary>
    public string? DeploymentName { get; set; }

    /// <summary>
    /// Model name for embeddings.
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Dimension of the embedding vectors.
    /// </summary>
    [Range(1, 4096)]
    public int Dimensions { get; set; } = 1536;

    /// <summary>
    /// Maximum number of texts to embed in a single batch.
    /// </summary>
    [Range(1, 100)]
    public int BatchSize { get; set; } = 16;

    /// <summary>
    /// Whether to cache embeddings.
    /// </summary>
    public bool CacheEnabled { get; set; } = true;

    /// <summary>
    /// Cache duration in seconds.
    /// </summary>
    [Range(60, 86400)]
    public int CacheDurationSeconds { get; set; } = 3600;

    /// <summary>
    /// Similarity threshold for search results.
    /// </summary>
    [Range(0, 1)]
    public double SimilarityThreshold { get; set; } = 0.7;

    /// <summary>
    /// Maximum number of results to return from similarity search.
    /// </summary>
    [Range(1, 100)]
    public int MaxResults { get; set; } = 10;

    /// <summary>
    /// Whether settings are properly configured.
    /// </summary>
    public bool IsConfigured =>
        Enabled &&
        !string.IsNullOrWhiteSpace(Provider) &&
        !string.IsNullOrWhiteSpace(DeploymentName);
}
