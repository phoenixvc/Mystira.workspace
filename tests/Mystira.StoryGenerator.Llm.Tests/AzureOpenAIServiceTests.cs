using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Llm.Services.LLM;

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
                DeploymentName = "gpt-4",
                Deployments = new List<AzureOpenAIDeployment>
                {
                    new()
                    {
                        Name = "gpt-4",
                        DisplayName = "GPT-4",
                        MaxTokens = 4096,
                        DefaultTemperature = 0.7,
                        SupportsJsonSchema = true,
                        Capabilities = new List<string> { "chat", "json-schema", "story-generation" }
                    },
                    new()
                    {
                        Name = "gpt-35-turbo",
                        DisplayName = "GPT-3.5 Turbo",
                        MaxTokens = 4096,
                        DefaultTemperature = 0.7,
                        SupportsJsonSchema = false,
                        Capabilities = new List<string> { "chat", "story-generation" }
                    }
                }
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
    public void GetAvailableModels_WithValidSettings_ReturnsModels()
    {
        // Arrange
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetAvailableModels().ToList();

        // Assert
        Assert.Equal(2, result.Count);

        var gpt4Model = result.FirstOrDefault(m => m.Id == "gpt-4");
        Assert.NotNull(gpt4Model);
        Assert.Equal("GPT-4", gpt4Model.DisplayName);
        Assert.True(gpt4Model.SupportsJsonSchema);
        Assert.Contains("chat", gpt4Model.Capabilities);
        Assert.Contains("json-schema", gpt4Model.Capabilities);

        var gpt35Model = result.FirstOrDefault(m => m.Id == "gpt-35-turbo");
        Assert.NotNull(gpt35Model);
        Assert.Equal("GPT-3.5 Turbo", gpt35Model.DisplayName);
        Assert.False(gpt35Model.SupportsJsonSchema);
        Assert.Contains("chat", gpt35Model.Capabilities);
        Assert.DoesNotContain("json-schema", gpt35Model.Capabilities);
    }

    [Fact]
    public void GetAvailableModels_WithEmptyDeployments_FallsBackToLegacy()
    {
        // Arrange
        _aiSettings.AzureOpenAI.Deployments = new List<AzureOpenAIDeployment>();
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

    [Fact]
    public void ResolveEndpoint_WithDeploymentSpecificEndpoint_ReturnsDeploymentEndpoint()
    {
        // Arrange
        var deploymentEndpoint = "https://specific-deployment.openai.azure.com/";
        _aiSettings.AzureOpenAI.Deployments[0].Endpoint = deploymentEndpoint;
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = typeof(AzureOpenAIService)
            .GetMethod("ResolveEndpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(service, new object[] { "gpt-4" });

        // Assert
        Assert.Equal(deploymentEndpoint, result);
    }

    [Fact]
    public void ResolveEndpoint_WithoutDeploymentSpecificEndpoint_ReturnsDefaultEndpoint()
    {
        // Arrange
        _aiSettings.AzureOpenAI.Deployments[0].Endpoint = null;
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = typeof(AzureOpenAIService)
            .GetMethod("ResolveEndpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(service, new object[] { "gpt-4" });

        // Assert
        Assert.Equal(_aiSettings.AzureOpenAI.Endpoint, result);
    }

    [Fact]
    public void ResolveEndpoint_WithUnknownDeployment_ReturnsDefaultEndpoint()
    {
        // Arrange
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = typeof(AzureOpenAIService)
            .GetMethod("ResolveEndpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(service, new object[] { "unknown-deployment" });

        // Assert
        Assert.Equal(_aiSettings.AzureOpenAI.Endpoint, result);
    }

    [Fact]
    public void ResolveEndpoint_WithNullDeploymentName_ReturnsDefaultEndpoint()
    {
        // Arrange
        var service = new AzureOpenAIService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = typeof(AzureOpenAIService)
            .GetMethod("ResolveEndpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(service, new object[] { null });

        // Assert
        Assert.Equal(_aiSettings.AzureOpenAI.Endpoint, result);
    }
}
