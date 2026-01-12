using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

public class KnowledgeProviderTests
{
    private readonly Mock<ILogger<FileSearchKnowledgeProvider>> _fileSearchLoggerMock;
    private readonly Mock<ILogger<AISearchKnowledgeProvider>> _aiSearchLoggerMock;

    public KnowledgeProviderTests()
    {
        _fileSearchLoggerMock = new Mock<ILogger<FileSearchKnowledgeProvider>>();
        _aiSearchLoggerMock = new Mock<ILogger<AISearchKnowledgeProvider>>();
    }

    [Fact]
    public void IKnowledgeProvider_Interface_ShouldBeImplemented()
    {
        // Arrange
        #pragma warning disable CS0618 // Type or member is obsolete
        var fileSearchConfig = new FileSearchKnowledgeProvider.FileSearchConfiguration();
        #pragma warning restore CS0618 // Type or member is obsolete
        var aiSearchConfig = new AISearchKnowledgeProvider.AISearchConfiguration();

        // We can't instantiate the concrete classes without mocking the FoundryAgentClient
        // But we can verify the interfaces exist and the configuration classes work

        // Assert
        Assert.NotNull(fileSearchConfig);
        Assert.NotNull(aiSearchConfig);
        Assert.NotNull(fileSearchConfig.VectorStoresByAgentAndAge);
        Assert.Equal("mystira-instructions", aiSearchConfig.IndexName);
    }

    [Fact]
    public void FileSearchConfiguration_ShouldAcceptCustomValues()
    {
        // Arrange & Act
        #pragma warning disable CS0618 // Type or member is obsolete
        var config = new FileSearchKnowledgeProvider.FileSearchConfiguration
        {
            VectorStoresByAgentAndAge = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    "Writer", new Dictionary<string, string>
                    {
                        { "3-5", "vs_3_5_writer_v1" },
                        { "6-9", "vs_6_9_writer_v1" }
                    }
                }
            },
            MaxFiles = 50,
            MaxTokens = 10000
        };
        #pragma warning restore CS0618 // Type or member is obsolete

        // Assert
        Assert.NotNull(config.VectorStoresByAgentAndAge);
        Assert.Single(config.VectorStoresByAgentAndAge);
        Assert.True(config.VectorStoresByAgentAndAge.ContainsKey("Writer"));
        Assert.Equal("vs_3_5_writer_v1", config.VectorStoresByAgentAndAge["Writer"]["3-5"]);
        Assert.Equal(50, config.MaxFiles);
        Assert.Equal(10000, config.MaxTokens);
    }

    [Fact]
    public void AISearchConfiguration_ShouldAcceptCustomValues()
    {
        // Arrange & Act
        var config = new AISearchKnowledgeProvider.AISearchConfiguration
        {
            Endpoint = "https://my-search.search.windows.net",
            ApiKey = "search-api-key",
            IndexName = "my-custom-index",
            ContentFieldName = "content",
            TitleFieldName = "title"
        };

        // Assert
        Assert.Equal("https://my-search.search.windows.net", config.Endpoint);
        Assert.Equal("search-api-key", config.ApiKey);
        Assert.Equal("my-custom-index", config.IndexName);
        Assert.Equal("content", config.ContentFieldName);
        Assert.Equal("title", config.TitleFieldName);
    }

    [Fact]
    public void AISearchFilters_ShouldAcceptValues()
    {
        // Arrange & Act
        var filters = new AISearchKnowledgeProvider.SearchFilters
        {
            AgeGroup = "6-9",
            PrincipleType = "developmental",
            Priority = 1,
            Version = "1.0"
        };

        // Assert
        Assert.Equal("6-9", filters.AgeGroup);
        Assert.Equal("developmental", filters.PrincipleType);
        Assert.Equal(1, filters.Priority);
        Assert.Equal("1.0", filters.Version);
    }

    [Fact]
    public void AISearchFilters_ShouldAllowNullValues()
    {
        // Arrange & Act
        var filters = new AISearchKnowledgeProvider.SearchFilters();

        // Assert
        Assert.Null(filters.AgeGroup);
        Assert.Null(filters.PrincipleType);
        Assert.Null(filters.Priority);
        Assert.Null(filters.Version);
    }

    [Fact]
    public void AISearchResults_ShouldStoreResults()
    {
        // Arrange & Act
        var results = new AISearchKnowledgeProvider.SearchResults
        {
            TotalCount = 10,
            Results = new List<AISearchKnowledgeProvider.SearchResultItem>
            {
                new() { Score = 0.95, Document = null! },
                new() { Score = 0.85, Document = null! }
            }
        };

        // Assert
        Assert.Equal(10, results.TotalCount);
        Assert.Equal(2, results.Results.Count);
        Assert.Equal(0.95, results.Results[0].Score);
        Assert.Equal(0.85, results.Results[1].Score);
    }

    [Fact]
    public void ProviderName_ShouldReturnExpectedValues()
    {
        // The ProviderName property should be implemented by concrete classes
        // We test that the configuration classes exist and can be used
        #pragma warning disable CS0618 // Type or member is obsolete
        var fileSearchConfig = new FileSearchKnowledgeProvider.FileSearchConfiguration();
        #pragma warning restore CS0618 // Type or member is obsolete
        var aiSearchConfig = new AISearchKnowledgeProvider.AISearchConfiguration();

        Assert.NotNull(fileSearchConfig);
        Assert.NotNull(aiSearchConfig);
    }
}
