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
    /// File Search configuration (used when KnowledgeMode = "FileSearch").
    /// </summary>
    public FileSearchConfig? FileSearch { get; set; }

    /// <summary>
    /// AI Search configuration (used when KnowledgeMode = "AISearch").
    /// </summary>
    public AISearchConfig? AISearch { get; set; }
}

/// <summary>
/// Configuration for File Search knowledge mode.
/// </summary>
public class FileSearchConfig
{
    /// <summary>
    /// Agent-specific and age-specific vector store IDs for targeted knowledge retrieval.
    /// REQUIRED: All supported agent types and age groups must be explicitly configured.
    /// Example structure:
    /// {
    ///   "Writer": { "1-2": "vs_1_2_writer_v1", "3-5": "vs_3_5_writer_v1", ... },
    ///   "Judge": { "1-2": "vs_1_2_judge_v1", "3-5": "vs_3_5_judge_v1", ... },
    ///   "Refiner": { "1-2": "vs_1_2_refiner_v1", "3-5": "vs_3_5_refiner_v1", ... },
    ///   "RubricSummary": { "1-2": "vs_1_2_rubric_v1", "3-5": "vs_3_5_rubric_v1", ... }
    /// }
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> VectorStoresByAgentAndAge { get; set; } = new();

    /// <summary>
    /// [DEPRECATED] Age-specific vector store IDs (legacy single-agent configuration).
    /// Use VectorStoresByAgentAndAge for agent-specific vector stores.
    /// </summary>
    [Obsolete("Use VectorStoresByAgentAndAge for agent-specific vector stores")]
    public Dictionary<string, string> VectorStoresByAgeGroup { get; set; } = new();

    /// <summary>
    /// Maximum number of files to retrieve per search (optional).
    /// </summary>
    public int? MaxFiles { get; set; }

    /// <summary>
    /// Maximum tokens to use for search results (optional).
    /// </summary>
    public int? MaxTokens { get; set; }
}

/// <summary>
/// Configuration for AI Search knowledge mode.
/// </summary>
public class AISearchConfig
{
    /// <summary>
    /// Azure AI Search endpoint URL.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Azure AI Search API key.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Search index name containing story guidelines and principles.
    /// </summary>
    public string IndexName { get; set; } = "mystira-instructions";

    /// <summary>
    /// Field name containing the main content (optional override).
    /// </summary>
    public string? ContentFieldName { get; set; }

    /// <summary>
    /// Field name containing the age group metadata (optional override).
    /// </summary>
    public string? AgeGroupFieldName { get; set; } = "age_group";

    /// <summary>
    /// Number of results to retrieve per search (default: 5).
    /// </summary>
    public int TopK { get; set; } = 5;
}

/// <summary>
/// Agent types for vector store configuration.
/// </summary>
public enum AgentType
{
    /// <summary>
    /// The Writer agent that generates initial story content.
    /// </summary>
    Writer,

    /// <summary>
    /// The Judge agent that evaluates story quality.
    /// </summary>
    Judge,

    /// <summary>
    /// The Refiner agent that improves stories based on feedback.
    /// </summary>
    Refiner,

    /// <summary>
    /// The Rubric Summary agent that generates evaluation summaries.
    /// </summary>
    RubricSummary
}
