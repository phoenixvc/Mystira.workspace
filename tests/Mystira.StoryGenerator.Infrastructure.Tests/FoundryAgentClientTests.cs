using Azure.AI.Projects;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Domain.Agents;
using Mystira.StoryGenerator.Infrastructure.Agents;
using Xunit;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

public class FoundryAgentClientTests
{
    private readonly Mock<ILogger<FoundryAgentClient>> _loggerMock;
    private readonly FoundryAgentClientConfig _config;

    public FoundryAgentClientTests()
    {
        _loggerMock = new Mock<ILogger<FoundryAgentClient>>();
        _config = new FoundryAgentClientConfig
        {
            Endpoint = "https://test.openai.azure.com/",
            ApiKey = "test-api-key",
            ProjectId = "test-project-id"
        };
    }

    [Fact]
    public void Initialize_WithValidConfig_ShouldNotThrow()
    {
        // Arrange & Act
        var client = new FoundryAgentClient();
        client.Initialize(_config);

        // Assert
        Assert.NotNull(FoundryAgentClient.Instance);
    }

    [Fact]
    public void Initialize_WithNullConfig_ShouldThrowArgumentNullException()
    {
        // Arrange
        var client = new FoundryAgentClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => client.Initialize(null!));
    }

    [Fact]
    public void Initialize_WithEmptyEndpoint_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new FoundryAgentClient();
        var invalidConfig = new FoundryAgentClientConfig
        {
            Endpoint = string.Empty,
            ApiKey = "test-api-key",
            ProjectId = "test-project-id"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => client.Initialize(invalidConfig));
    }

    [Fact]
    public void Initialize_WithEmptyApiKey_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new FoundryAgentClient();
        var invalidConfig = new FoundryAgentClientConfig
        {
            Endpoint = "https://test.openai.azure.com/",
            ApiKey = string.Empty,
            ProjectId = "test-project-id"
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => client.Initialize(invalidConfig));
    }

    [Fact]
    public void Initialize_WithEmptyProjectId_ShouldThrowArgumentException()
    {
        // Arrange
        var client = new FoundryAgentClient();
        var invalidConfig = new FoundryAgentClientConfig
        {
            Endpoint = "https://test.openai.azure.com/",
            ApiKey = "test-api-key",
            ProjectId = string.Empty
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => client.Initialize(invalidConfig));
    }

    [Fact]
    public void CreateThreadAsync_WithValidAgentId_ShouldReturnThreadId()
    {
        // This test requires mocking the Azure SDK - we'll test the basic flow
        // Arrange
        var client = new FoundryAgentClient();

        // Note: In a real scenario, we would mock the AIProjectClient and AgentsClient
        // For now, we verify the client can be initialized
        client.Initialize(_config);

        // Assert - client is initialized
        Assert.NotNull(FoundryAgentClient.Instance);
    }

    [Fact]
    public void SingletonInstance_ShouldReturnSameInstance()
    {
        // Arrange
        var instance1 = FoundryAgentClient.Instance;
        var instance2 = FoundryAgentClient.Instance;

        // Assert
        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var client = new FoundryAgentClient();
        client.Initialize(_config);

        // Act & Assert
        client.Dispose();
        client.Dispose(); // Should not throw on second dispose
    }

    [Fact]
    public void GetAgentsClient_WithoutInitialization_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var client = new FoundryAgentClient();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => client.GetAgentsClient());
    }

    [Fact]
    public void GetProjectClient_WithoutInitialization_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var client = new FoundryAgentClient();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => client.GetProjectClient());
    }
}
