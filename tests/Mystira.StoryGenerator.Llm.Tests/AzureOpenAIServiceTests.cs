using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Configuration;
using Xunit;

namespace Mystira.StoryGenerator.Llm.Tests;

public class AzureOpenAIServiceTests
{
    private readonly Mock<ILogger<AzureOpenAIService>> _loggerMock;
    private readonly Mock<IOptions<AiSettings>> _optionsMock;
    private readonly AiSettings _aiSettings;

    public AzureOpenAIServiceTests()
    {
        _loggerMock = new Mock<ILogger<AzureOpenAIService>>();
        _optionsMock = new Mock<IOptions<AiSettings>>();

        _aiSettings = new AiSettings
        {
            AzureOpenAI = new AzureOpenAISettings
            {
                Endpoint = "https://test.openai.azure.com",
                ApiKey = "test-key",
                DeploymentName = "gpt-4"
            }
        };

        _optionsMock.Setup(x => x.Value).Returns(_aiSettings);
    }

    [Fact]
    public void IsAvailable_WithValidSettings_ReturnsTrue()
    {
        // Arrange
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.IsAvailable();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAvailable_WithMissingApiKey_ReturnsFalse()
    {
        // Arrange
        _aiSettings.AzureOpenAI.ApiKey = string.Empty;
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.IsAvailable();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetAvailableModels_WithValidSettings_ReturnsModel()
    {
        // Arrange
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetAvailableModels().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("gpt-4", result[0].Id);
        Assert.Equal("GPT-4", result[0].DisplayName);
        Assert.True(result[0].SupportsJsonSchema);
        Assert.Contains("chat", result[0].Capabilities);
        Assert.Contains("json-schema", result[0].Capabilities);
    }

    [Fact]
    public void GetAvailableModels_WithInvalidSettings_ReturnsEmpty()
    {
        // Arrange
        _aiSettings.AzureOpenAI.ApiKey = string.Empty;
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetAvailableModels();

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("gpt-4", "GPT-4")]
    [InlineData("gpt-4-turbo", "GPT-4")]
    [InlineData("gpt-3.5-turbo", "GPT-3.5 Turbo")]
    [InlineData("gpt-35-turbo", "GPT-3.5 Turbo")]
    [InlineData("custom-model", "custom-model")]
    public void GetAvailableModels_WithDifferentDeployments_ReturnsCorrectDisplayName(string deploymentName, string expectedDisplayName)
    {
        // Arrange
        _aiSettings.AzureOpenAI.DeploymentName = deploymentName;
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetAvailableModels().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedDisplayName, result[0].DisplayName);
    }
}