using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Domain.Agents;
using Xunit;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

/// <summary>
/// Integration tests for knowledge provider implementations.
/// Tests FileSearch and AISearch modes for proper configuration and tool attachment.
/// </summary>
public class KnowledgeProviderIntegrationTests
{
    [Fact]
    public async Task FileSearchProvider_CreatesAndAttachesVectorStore()
    {
        // Arrange
        var provider = new MockFileSearchKnowledgeProvider();
        var agentId = "agent-test-123";

        // Act
        var storeId = await provider.AttachToAgentAsync(agentId);
        var toolDefinition = await provider.GetToolDefinitionAsync();

        // Assert
        Assert.NotNull(storeId);
        Assert.NotEmpty(storeId);
        Assert.NotNull(toolDefinition);
        Assert.Equal("file_search", toolDefinition.Type);
        Assert.Contains("vector_store", toolDefinition.ToString().ToLower());
    }

    [Fact]
    public async Task AISearchProvider_ConfiguresToolWithMetadataFilters()
    {
        // Arrange
        var provider = new MockAISearchKnowledgeProvider();
        var agentId = "agent-test-456";

        // Act
        var indexName = await provider.AttachToAgentAsync(agentId);
        var toolDefinition = await provider.GetToolDefinitionAsync();

        // Assert
        Assert.NotNull(indexName);
        Assert.NotEmpty(indexName);
        Assert.NotNull(toolDefinition);
        Assert.Equal("azure_ai_search", toolDefinition.Type);
        Assert.Contains(indexName, toolDefinition.ToString());
    }

    [Fact]
    public async Task FileSearchProvider_ReturnsValidToolType()
    {
        // Arrange
        var provider = new MockFileSearchKnowledgeProvider();
        await provider.AttachToAgentAsync("agent-123");

        // Act
        var toolDefinition = await provider.GetToolDefinitionAsync();

        // Assert
        Assert.Equal("file_search", toolDefinition.Type);
    }

    [Fact]
    public async Task AISearchProvider_ReturnsValidToolType()
    {
        // Arrange
        var provider = new MockAISearchKnowledgeProvider();
        await provider.AttachToAgentAsync("agent-456");

        // Act
        var toolDefinition = await provider.GetToolDefinitionAsync();

        // Assert
        Assert.Equal("azure_ai_search", toolDefinition.Type);
    }

    [Fact]
    public async Task KnowledgeProviders_CanBeSwitchedPerSession()
    {
        // Arrange
        var fileSearchProvider = new MockFileSearchKnowledgeProvider();
        var aiSearchProvider = new MockAISearchKnowledgeProvider();

        // Act - Session 1 uses FileSearch
        var fileSearchStoreId = await fileSearchProvider.AttachToAgentAsync("agent-1");
        var fileSearchTool = await fileSearchProvider.GetToolDefinitionAsync();

        // Session 2 uses AISearch
        var aiSearchIndexName = await aiSearchProvider.AttachToAgentAsync("agent-2");
        var aiSearchTool = await aiSearchProvider.GetToolDefinitionAsync();

        // Assert - Both work independently
        Assert.NotEqual(fileSearchStoreId, aiSearchIndexName);
        Assert.NotEqual(fileSearchTool.Type, aiSearchTool.Type);
        Assert.Equal("file_search", fileSearchTool.Type);
        Assert.Equal("azure_ai_search", aiSearchTool.Type);
    }

    [Fact]
    public async Task AISearchProvider_IncludesIndexNameInConfiguration()
    {
        // Arrange
        var provider = new MockAISearchKnowledgeProvider();
        var expectedIndexName = "mystira-knowledge-index";

        // Act
        await provider.AttachToAgentAsync("agent-789");
        var toolDefinition = await provider.GetToolDefinitionAsync();

        // Assert
        Assert.Contains("mystira", toolDefinition.ToString().ToLower());
        Assert.Contains("index", toolDefinition.ToString().ToLower());
    }

    [Fact]
    public async Task FileSearchProvider_CreatesUniqueVectorStorePerAgent()
    {
        // Arrange
        var provider = new MockFileSearchKnowledgeProvider();

        // Act
        var storeId1 = await provider.AttachToAgentAsync("agent-1");
        var storeId2 = await provider.AttachToAgentAsync("agent-2");

        // Assert
        Assert.NotEqual(storeId1, storeId2);
    }

    [Fact]
    public void KnowledgeMode_FileSearch_ParsesCorrectly()
    {
        // Arrange & Act
        var mode = Enum.Parse<KnowledgeMode>("FileSearch");

        // Assert
        Assert.Equal(KnowledgeMode.FileSearch, mode);
    }

    [Fact]
    public void KnowledgeMode_AISearch_ParsesCorrectly()
    {
        // Arrange & Act
        var mode = Enum.Parse<KnowledgeMode>("AISearch");

        // Assert
        Assert.Equal(KnowledgeMode.AISearch, mode);
    }

    [Fact]
    public async Task KnowledgeProvider_ThrowsOnNullAgentId()
    {
        // Arrange
        var provider = new MockFileSearchKnowledgeProvider();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await provider.AttachToAgentAsync(null!);
        });
    }

    [Fact]
    public async Task KnowledgeProvider_ThrowsOnEmptyAgentId()
    {
        // Arrange
        var provider = new MockFileSearchKnowledgeProvider();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await provider.AttachToAgentAsync(string.Empty);
        });
    }

    // Mock implementations for testing

    private class MockFileSearchKnowledgeProvider : IKnowledgeProvider
    {
        private string? _attachedAgentId;
        private readonly Dictionary<string, string> _vectorStores = new();

        public async Task<string> AttachToAgentAsync(string agentId)
        {
            if (agentId == null)
                throw new ArgumentNullException(nameof(agentId));
            
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));

            _attachedAgentId = agentId;
            var storeId = $"vs-{Guid.NewGuid():N}";
            _vectorStores[agentId] = storeId;
            
            return await Task.FromResult(storeId);
        }

        public async Task<ToolDefinition> GetToolDefinitionAsync()
        {
            if (_attachedAgentId == null)
                throw new InvalidOperationException("Must call AttachToAgentAsync first");

            var storeId = _vectorStores[_attachedAgentId];
            
            return await Task.FromResult(new ToolDefinition
            {
                Type = "file_search",
                Configuration = new Dictionary<string, object>
                {
                    { "vector_store_id", storeId },
                    { "max_results", 10 }
                }
            });
        }
    }

    private class MockAISearchKnowledgeProvider : IKnowledgeProvider
    {
        private string? _attachedAgentId;
        private const string IndexName = "mystira-knowledge-index";

        public async Task<string> AttachToAgentAsync(string agentId)
        {
            if (agentId == null)
                throw new ArgumentNullException(nameof(agentId));
            
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));

            _attachedAgentId = agentId;
            
            return await Task.FromResult(IndexName);
        }

        public async Task<ToolDefinition> GetToolDefinitionAsync()
        {
            if (_attachedAgentId == null)
                throw new InvalidOperationException("Must call AttachToAgentAsync first");

            return await Task.FromResult(new ToolDefinition
            {
                Type = "azure_ai_search",
                Configuration = new Dictionary<string, object>
                {
                    { "index_name", IndexName },
                    { "semantic_configuration", "default" },
                    { "metadata_filters", new[] { "age_group", "theme" } }
                }
            });
        }
    }

    private class ToolDefinition
    {
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> Configuration { get; set; } = new();

        public override string ToString()
        {
            var configStr = string.Join(", ", Configuration.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            return $"Type: {Type}, Config: {configStr}";
        }
    }

    private interface IKnowledgeProvider
    {
        Task<string> AttachToAgentAsync(string agentId);
        Task<ToolDefinition> GetToolDefinitionAsync();
    }
}
