using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Knowledge provider that uses Azure AI Foundry File Search with vector stores.
/// Supports age-specific vector stores for targeted knowledge retrieval.
/// </summary>
public class FileSearchKnowledgeProvider : IKnowledgeProvider
{
    private readonly FoundryAgentClient _client;
    private readonly FileSearchConfiguration _config;
    private readonly ILogger<FileSearchKnowledgeProvider> _logger;

    public string ProviderName => "FileSearch";

    public FileSearchKnowledgeProvider(
        FoundryAgentClient client,
        FileSearchConfiguration config,
        ILogger<FileSearchKnowledgeProvider> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Attaches the vector store to an agent for knowledge retrieval.
    /// </summary>
    public Task<ToolDefinition> AttachToAgentAsync(
        string agentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("File search configured for agent: {AgentId}", agentId);

        return Task.FromResult<ToolDefinition>(new FileSearchToolDefinition());
    }

    /// <summary>
    /// Attaches the age-specific vector store to a thread for knowledge retrieval.
    /// </summary>
    public Task<ToolDefinition> AttachToThreadAsync(
        string threadId,
        string? ageGroup = null,
        CancellationToken cancellationToken = default)
    {
        var vectorStoreId = GetVectorStoreIdForAgeGroup(ageGroup);

        _logger.LogInformation(
            "Attaching FileSearch to thread {ThreadId} with vector store {VectorStoreId} for age group {AgeGroup}",
            threadId, vectorStoreId, ageGroup ?? "default");

        // Note: The actual attachment happens when creating the thread with tools
        // This method returns the tool definition that will be used
        return Task.FromResult<ToolDefinition>(new FileSearchToolDefinition());
    }

    /// <summary>
    /// Gets the tool definition for file search.
    /// </summary>
    public ToolDefinition GetToolDefinition(string? ageGroup = null)
    {
        return new FileSearchToolDefinition();
    }

    /// <summary>
    /// Gets age-aware contextual guidance for the agent.
    /// </summary>
    public string GetContextualGuidance(string? ageGroup = null)
    {
        var baseGuidance = "Use the file_search tool to retrieve age-appropriate guidelines, safety rules, and writing principles.";

        if (!string.IsNullOrEmpty(ageGroup))
        {
            return $"{baseGuidance}\n\nThe vector store is pre-filtered for age group {ageGroup}. All retrieved documents are appropriate for this age range.";
        }

        return baseGuidance;
    }

    /// <summary>
    /// Gets the vector store ID for a specific age group.
    /// Throws exception if age group is not configured.
    /// </summary>
    public string GetVectorStoreIdForAgeGroup(string? ageGroup)
    {
        if (string.IsNullOrEmpty(ageGroup))
        {
            throw new ArgumentException(
                "Age group is required for FileSearch knowledge mode. " +
                "Ensure the session has a valid age group specified.",
                nameof(ageGroup));
        }

        if (_config.VectorStoresByAgeGroup == null || _config.VectorStoresByAgeGroup.Count == 0)
        {
            throw new InvalidOperationException(
                "No vector stores configured for FileSearch mode. " +
                "Set FoundryAgent:FileSearch:VectorStoresByAgeGroup in configuration with age-specific vector store IDs.");
        }

        if (_config.VectorStoresByAgeGroup.TryGetValue(ageGroup, out var vectorStoreId))
        {
            _logger.LogDebug("Using age-specific vector store {VectorStoreId} for age group {AgeGroup}",
                vectorStoreId, ageGroup);
            return vectorStoreId;
        }

        // No vector store found for this age group - this is an error
        var configuredAgeGroups = string.Join(", ", _config.VectorStoresByAgeGroup.Keys);
        throw new InvalidOperationException(
            $"No vector store configured for age group '{ageGroup}'. " +
            $"Configured age groups: [{configuredAgeGroups}]. " +
            $"Add a vector store for age group '{ageGroup}' to FoundryAgent:FileSearch:VectorStoresByAgeGroup configuration.");
    }

    /// <summary>
    /// Configuration for File Search.
    /// </summary>
    public class FileSearchConfiguration
    {
        /// <summary>
        /// Age-specific vector store IDs.
        /// REQUIRED: All supported age groups must be explicitly configured.
        /// Example: { "1-2": "vs_abc123", "6-9": "vs_def456" }
        /// </summary>
        public Dictionary<string, string> VectorStoresByAgeGroup { get; set; } = new();

        public int? MaxFiles { get; set; }
        public int? MaxTokens { get; set; }
    }
}
