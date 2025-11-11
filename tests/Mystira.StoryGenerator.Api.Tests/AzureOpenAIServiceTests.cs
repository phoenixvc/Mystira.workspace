using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;

namespace Mystira.StoryGenerator.Api.Tests;

public class AzureOpenAIServiceTests
{
    private readonly Mock<ILogger<AzureOpenAIService>> _loggerMock;
    private readonly Mock<IOptions<AiSettings>> _optionsMock;

    public AzureOpenAIServiceTests()
    {
        _loggerMock = new Mock<ILogger<AzureOpenAIService>>();
        _optionsMock = new Mock<IOptions<AiSettings>>();
    }

    [Fact]
    public void ProviderName_ReturnsCorrectName()
    {
        // Arrange
        var settings = new AiSettings
        {
            AzureOpenAI = new AzureOpenAISettings
            {
                Endpoint = "https://test.openai.azure.com",
                ApiKey = "test-key",
                DeploymentName = "test-deployment"
            }
        };
        _optionsMock.Setup(x => x.Value).Returns(settings);

        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act & Assert
        Assert.Equal("azure-openai", service.ProviderName);
    }

    [Fact]
    public void IsAvailable_WithValidSettings_ReturnsTrue()
    {
        // Arrange
        var settings = new AiSettings
        {
            AzureOpenAI = new AzureOpenAISettings
            {
                Endpoint = "https://test.openai.azure.com",
                ApiKey = "test-key",
                DeploymentName = "test-deployment"
            }
        };
        _optionsMock.Setup(x => x.Value).Returns(settings);

        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act & Assert
        Assert.True(service.IsAvailable());
    }

    [Theory]
    [InlineData("", "test-key", "test-deployment")]
    [InlineData("https://test.openai.azure.com", "", "test-deployment")]
    [InlineData("https://test.openai.azure.com", "test-key", "")]
    public void IsAvailable_WithInvalidSettings_ReturnsFalse(string endpoint, string apiKey, string deploymentName)
    {
        // Arrange
        var settings = new AiSettings
        {
            AzureOpenAI = new AzureOpenAISettings
            {
                Endpoint = endpoint,
                ApiKey = apiKey,
                DeploymentName = deploymentName
            }
        };
        _optionsMock.Setup(x => x.Value).Returns(settings);

        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act & Assert
        Assert.False(service.IsAvailable());
    }

    [Fact]
    public async Task CompleteAsync_WithUnavailableService_ReturnsErrorResponse()
    {
        // Arrange
        var settings = new AiSettings
        {
            AzureOpenAI = new AzureOpenAISettings
            {
                Endpoint = "",
                ApiKey = "",
                DeploymentName = ""
            }
        };
        _optionsMock.Setup(x => x.Value).Returns(settings);

        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        var request = new ChatCompletionRequest
        {
            Provider = "azure-openai",
            Messages = new List<MystiraChatMessage>
            {
                new() { MessageType = ChatMessageType.User, Content = "Hello" }
            }
        };

        // Act
        var result = await service.CompleteAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not properly configured", result.Error);
    }
}
