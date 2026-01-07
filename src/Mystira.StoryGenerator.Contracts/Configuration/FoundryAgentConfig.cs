using System.ComponentModel.DataAnnotations;

namespace Mystira.StoryGenerator.Contracts.Configuration;

/// <summary>
/// Configuration for Azure AI Foundry Agent resources and behavior.
/// </summary>
public class FoundryAgentConfig
{
    public const string SectionName = "FoundryAgent";

    /// <summary>
    /// The Writer agent ID for generating story content.
    /// </summary>
    [Required]
    public string WriterAgentId { get; set; } = string.Empty;

    /// <summary>
    /// The Judge agent ID for evaluating story quality.
    /// </summary>
    [Required]
    public string JudgeAgentId { get; set; } = string.Empty;

    /// <summary>
    /// The Refiner agent ID for refining stories based on feedback.
    /// </summary>
    public string RefinerAgentId { get; set; } = string.Empty;

    /// <summary>
    /// The Rubric Summary agent ID for generating evaluation summaries.
    /// </summary>
    public string RubricSummaryAgentId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AI Foundry endpoint.
    /// </summary>
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure AI Foundry API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Azure AI Foundry Project ID.
    /// </summary>
    [Required]
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of iterations for story generation.
    /// </summary>
    public int MaxIterations { get; set; } = 5;

    /// <summary>
    /// Timeout for agent runs.
    /// </summary>
    public TimeSpan RunTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Knowledge mode to use (FileSearch or AISearch).
    /// </summary>
    public string KnowledgeMode { get; set; } = "AISearch";

    /// <summary>
    /// AI Search index name for knowledge retrieval.
    /// </summary>
    public string? SearchIndexName { get; set; }

    /// <summary>
    /// Vector store name for File Search mode (legacy - single store).
    /// </summary>
    public string? VectorStoreName { get; set; }

    /// <summary>
    /// Vector store IDs mapped by age group for File Search mode.
    /// Example: { "1-2": "vs_abc123", "3-5": "vs_def456", "6-9": "vs_ghi789" }
    /// </summary>
    public Dictionary<string, string>? VectorStoresByAgeGroup { get; set; }
}
