using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Knowledge provider that uses Azure AI Foundry File Search with vector stores.
/// Manages the lifecycle of vector stores for agent and thread knowledge retrieval.
/// </summary>
public class FileSearchKnowledgeProvider : IKnowledgeProvider
{
    private readonly FoundryAgentClient _client;
    private readonly FileSearchConfiguration _config;
    private readonly ILogger<FileSearchKnowledgeProvider> _logger;

    /// <summary>
    /// The vector store ID attached to this provider.
    /// </summary>
    private string? _vectorStoreId;

    /// <summary>
    /// The file IDs in the vector store.
    /// </summary>
    private readonly List<string> _fileIds = new();

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

        var toolDefinition = new ToolDefinition("file_search", new Dictionary<string, object>
        {
            { "description", "Search through uploaded files for relevant context and information." }
        });

        return Task.FromResult(toolDefinition);
    }

    /// <summary>
    /// Attaches the vector store to a thread for knowledge retrieval.
    /// </summary>
    public Task<ToolDefinition> AttachToThreadAsync(
        string threadId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("File search configured for thread: {ThreadId}", threadId);

        var toolDefinition = new ToolDefinition("file_search", new Dictionary<string, object>
        {
            { "description", "Search through uploaded files for relevant context and information." }
        });

        return Task.FromResult(toolDefinition);
    }

    /// <summary>
    /// Gets the tool definition for file search.
    /// </summary>
    public ToolDefinition GetToolDefinition()
    {
        return new ToolDefinition("file_search", new Dictionary<string, object>
        {
            { "description", "Search through uploaded files for relevant context and information." }
        });
    }

    public string GetContextualGuidance()
    {
        return "Use the file_search tool to look up any relevant project documents, safety guidance, or world lore before you write or revise. Only include information you can support from the tool results.";
    }

    /// <summary>
    /// Configuration for File Search.
    /// </summary>
    public class FileSearchConfiguration
    {
        public string VectorStoreName { get; set; } = "mystira-story-knowledge";
        public int? MaxFiles { get; set; }
        public int? MaxTokens { get; set; }
    }
}
