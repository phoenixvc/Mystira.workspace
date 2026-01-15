using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Knowledge provider that uses Azure AI Search for knowledge retrieval.
/// Configures tool for specified index with support for metadata filters.
/// </summary>
public class AISearchKnowledgeProvider : IKnowledgeProvider
{
    private readonly FoundryAgentClient _client;
    private readonly AISearchConfiguration _config;
    private readonly ILogger<AISearchKnowledgeProvider> _logger;
    private SearchClient? _searchClient;

    public string ProviderName => "AISearch";

    public AISearchKnowledgeProvider(
        FoundryAgentClient client,
        AISearchConfiguration config,
        ILogger<AISearchKnowledgeProvider> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Attaches the AI Search tool to an agent.
    /// </summary>
    public Task<ToolDefinition> AttachToAgentAsync(
        string agentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attaching AI Search to agent: {AgentId}", agentId);

        var toolDefinition = GetToolDefinition();
        _logger.LogInformation("Attached AI Search tool to agent: {AgentId}", agentId);

        return Task.FromResult(toolDefinition);
    }

    /// <summary>
    /// Attaches the AI Search tool to a thread.
    /// </summary>
    public Task<ToolDefinition> AttachToThreadAsync(
        string threadId,
        string? ageGroup = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attaching AI Search to thread: {ThreadId} for age group: {AgeGroup}",
            threadId, ageGroup ?? "all");

        var toolDefinition = GetToolDefinition(ageGroup);
        _logger.LogInformation("Attached AI Search tool to thread: {ThreadId}", threadId);

        return Task.FromResult(toolDefinition);
    }

    /// <summary>
    /// Gets the tool definition for Azure AI Search.
    /// </summary>
    public ToolDefinition GetToolDefinition(string? ageGroup = null)
    {
        return new AzureAISearchToolDefinition();
    }

    /// <summary>
    /// Gets age-aware contextual guidance for using AI Search.
    /// </summary>
    public string GetContextualGuidance(string? ageGroup = null)
    {
        _logger.LogInformation(
            "Generating contextual guidance from AISearch knowledge provider for age group: {AgeGroup}, Index: {IndexName}",
            ageGroup ?? "default", _config.IndexName);

        var baseGuidance = $"Use the azure_ai_search tool to retrieve relevant instructions, safety guidance, and writing principles from the '{_config.IndexName}' index.";

        if (!string.IsNullOrEmpty(ageGroup))
        {
            _logger.LogInformation(
                "AISearch contextual knowledge enabled: Index {IndexName} will be filtered for age group {AgeGroup}",
                _config.IndexName, ageGroup);
            return $"{baseGuidance}\n\n**IMPORTANT**: Always include filter 'age_group eq {ageGroup}' in your searches to retrieve age-appropriate guidelines. Only use information from documents matching this age group.";
        }

        return baseGuidance;
    }

    /// <summary>
    /// Searches the AI Search index for relevant documents.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="filters">Optional metadata filters.</param>
    /// <param name="topK">Number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    public async Task<SearchResults> SearchAsync(
        string query,
        SearchFilters? filters = null,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        EnsureSearchClientInitialized();

        _logger.LogInformation("Searching index {IndexName} for: {Query}", _config.IndexName, query);

        try
        {
            var searchOptions = new SearchOptions
            {
                Size = topK,
                IncludeTotalCount = true
            };

            // Add filters if provided
            if (filters != null)
            {
                var filterBuilder = new List<string>();

                if (!string.IsNullOrEmpty(filters.AgeGroup))
                    filterBuilder.Add($"age_group eq '{filters.AgeGroup}'");

                if (!string.IsNullOrEmpty(filters.PrincipleType))
                    filterBuilder.Add($"principle_type eq '{filters.PrincipleType}'");

                if (filters.Priority.HasValue)
                    filterBuilder.Add($"priority ge {filters.Priority.Value}");

                if (!string.IsNullOrEmpty(filters.Version))
                    filterBuilder.Add($"version eq '{filters.Version}'");

                if (filterBuilder.Count > 0)
                {
                    searchOptions.Filter = string.Join(" and ", filterBuilder);
                }
            }

            var response = await _searchClient!.SearchAsync<SearchDocument>(
                query,
                searchOptions,
                cancellationToken).ConfigureAwait(false);

            var results = new SearchResults
            {
                TotalCount = response.Value.TotalCount ?? 0,
                Results = new List<SearchResultItem>()
            };

            await foreach (var result in response.Value.GetResultsAsync().ConfigureAwait(false))
            {
                results.Results.Add(new SearchResultItem
                {
                    Score = result.Score ?? 0,
                    Document = result.Document
                });
            }

            _logger.LogInformation("Found {Count} results for query: {Query}", results.Results.Count, query);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search index {IndexName}", _config.IndexName);
            throw;
        }
    }

    private void EnsureSearchClientInitialized()
    {
        if (_searchClient == null)
        {
            _searchClient = new SearchClient(
                new Uri(_config.Endpoint),
                _config.IndexName,
                new AzureKeyCredential(_config.ApiKey));
        }
    }

    /// <summary>
    /// Configuration for AI Search knowledge provider.
    /// </summary>
    public class AISearchConfiguration
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string IndexName { get; set; } = "mystira-instructions";
        public string? ContentFieldName { get; set; }
        public string? TitleFieldName { get; set; }
        public string? AgeGroupFieldName { get; set; }
    }

    /// <summary>
    /// Filters for AI Search queries.
    /// </summary>
    public class SearchFilters
    {
        public string? AgeGroup { get; set; }
        public string? PrincipleType { get; set; }
        public int? Priority { get; set; }
        public string? Version { get; set; }
    }

    /// <summary>
    /// Search results container.
    /// </summary>
    public class SearchResults
    {
        public long TotalCount { get; set; }
        public List<SearchResultItem> Results { get; set; } = new();
    }

    /// <summary>
    /// Individual search result item.
    /// </summary>
    public class SearchResultItem
    {
        public double Score { get; set; }
        public SearchDocument Document { get; set; } = default!;
    }
}
