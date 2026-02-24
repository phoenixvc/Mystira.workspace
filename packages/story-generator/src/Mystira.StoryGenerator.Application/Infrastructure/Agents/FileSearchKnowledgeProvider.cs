using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Configuration;
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
        _logger.LogInformation(
            "Generating contextual guidance from FileSearch knowledge provider for age group: {AgeGroup}",
            ageGroup ?? "default");

        var baseGuidance = "Use the file_search tool to retrieve age-appropriate guidelines, safety rules, and writing principles.";

        if (!string.IsNullOrEmpty(ageGroup))
        {
            _logger.LogInformation(
                "FileSearch contextual knowledge enabled: Vector store will be filtered for age group {AgeGroup}",
                ageGroup);
            return $"{baseGuidance}\n\nThe vector store is pre-filtered for age group {ageGroup}. All retrieved documents are appropriate for this age range.";
        }

        return baseGuidance;
    }

    /// <summary>
    /// Gets the vector store ID for a specific agent type and age group.
    /// Throws exception if the combination is not configured.
    /// </summary>
    /// <param name="agentType">The agent type (Writer, Judge, Refiner, RubricSummary).</param>
    /// <param name="ageGroup">The age group (e.g., "3-5", "6-9").</param>
    /// <returns>The vector store ID for the specified agent and age group.</returns>
    public string GetVectorStoreIdForAgeGroup(AgentType agentType, string? ageGroup)
    {
        if (string.IsNullOrEmpty(ageGroup))
        {
            throw new ArgumentException(
                "Age group is required for FileSearch knowledge mode. " +
                "Ensure the session has a valid age group specified.",
                nameof(ageGroup));
        }

        var agentTypeName = agentType.ToString();

        // Try new agent-specific configuration first
        if (_config.VectorStoresByAgentAndAge != null && _config.VectorStoresByAgentAndAge.Count > 0)
        {
            if (_config.VectorStoresByAgentAndAge.TryGetValue(agentTypeName, out var ageGroupMap))
            {
                if (ageGroupMap.TryGetValue(ageGroup, out var vectorStoreId))
                {
                    _logger.LogDebug("Using agent-specific vector store {VectorStoreId} for {AgentType} agent and age group {AgeGroup}",
                        vectorStoreId, agentTypeName, ageGroup);
                    return vectorStoreId;
                }

                // Agent type exists but age group is missing
                var configuredAgeGroups = string.Join(", ", ageGroupMap.Keys);
                throw new InvalidOperationException(
                    $"No vector store configured for {agentTypeName} agent with age group '{ageGroup}'. " +
                    $"Configured age groups for {agentTypeName}: [{configuredAgeGroups}]. " +
                    $"Add vector store for age group '{ageGroup}' to FoundryAgent:FileSearch:VectorStoresByAgentAndAge:{agentTypeName} configuration.");
            }

            // Agent type doesn't exist
            var configuredAgents = string.Join(", ", _config.VectorStoresByAgentAndAge.Keys);
            throw new InvalidOperationException(
                $"No vector stores configured for {agentTypeName} agent. " +
                $"Configured agents: [{configuredAgents}]. " +
                $"Add configuration for {agentTypeName} to FoundryAgent:FileSearch:VectorStoresByAgentAndAge configuration.");
        }

        // Fall back to legacy single-agent configuration
        #pragma warning disable CS0618 // Type or member is obsolete
        if (_config.VectorStoresByAgeGroup != null && _config.VectorStoresByAgeGroup.Count > 0)
        {
            if (_config.VectorStoresByAgeGroup.TryGetValue(ageGroup, out var vectorStoreId))
            {
                _logger.LogWarning(
                    "Using legacy VectorStoresByAgeGroup configuration. Consider migrating to VectorStoresByAgentAndAge for agent-specific vector stores.");
                _logger.LogDebug("Using legacy age-specific vector store {VectorStoreId} for age group {AgeGroup}",
                    vectorStoreId, ageGroup);
                return vectorStoreId;
            }

            var configuredAgeGroups = string.Join(", ", _config.VectorStoresByAgeGroup.Keys);
            throw new InvalidOperationException(
                $"No vector store configured for age group '{ageGroup}'. " +
                $"Configured age groups: [{configuredAgeGroups}]. " +
                $"Add a vector store for age group '{ageGroup}' to FoundryAgent:FileSearch:VectorStoresByAgeGroup configuration, " +
                $"or migrate to VectorStoresByAgentAndAge for agent-specific configuration.");
        }
        #pragma warning restore CS0618 // Type or member is obsolete

        // No configuration at all
        throw new InvalidOperationException(
            "No vector stores configured for FileSearch mode. " +
            "Set FoundryAgent:FileSearch:VectorStoresByAgentAndAge in configuration with agent-specific and age-specific vector store IDs.");
    }

    /// <summary>
    /// [DEPRECATED] Gets the vector store ID for a specific age group using legacy configuration.
    /// Use the overload that accepts AgentType for agent-specific vector stores.
    /// </summary>
    [Obsolete("Use GetVectorStoreIdForAgeGroup(AgentType, string) for agent-specific vector stores")]
    public string GetVectorStoreIdForAgeGroup(string? ageGroup)
    {
        // Default to Writer agent for backward compatibility
        return GetVectorStoreIdForAgeGroup(AgentType.Writer, ageGroup);
    }

    /// <summary>
    /// [DEPRECATED] Configuration for File Search.
    /// Use FileSearchConfig from Mystira.StoryGenerator.Contracts.Configuration instead.
    /// </summary>
    [Obsolete("Use FileSearchConfig from Mystira.StoryGenerator.Contracts.Configuration")]
    public class FileSearchConfiguration
    {
        /// <summary>
        /// Agent-specific and age-specific vector store IDs.
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> VectorStoresByAgentAndAge { get; set; } = new();

        /// <summary>
        /// [DEPRECATED] Age-specific vector store IDs (legacy configuration).
        /// </summary>
        [Obsolete("Use VectorStoresByAgentAndAge")]
        public Dictionary<string, string> VectorStoresByAgeGroup { get; set; } = new();

        public int? MaxFiles { get; set; }
        public int? MaxTokens { get; set; }
    }
}
