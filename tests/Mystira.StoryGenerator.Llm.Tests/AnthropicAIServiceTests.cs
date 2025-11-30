using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Llm.Services.LLM;

namespace Mystira.StoryGenerator.Llm.Tests;

public class AnthropicAIServiceTests
{
    private readonly Mock<ILogger<AnthropicAIService>> _loggerMock;
    private readonly Mock<IOptions<AiSettings>> _optionsMock;
    private readonly AiSettings _aiSettings;

    public AnthropicAIServiceTests()
    {
        _loggerMock = new Mock<ILogger<AnthropicAIService>>();
        _optionsMock = new Mock<IOptions<AiSettings>>();

        _aiSettings = new AiSettings
        {
            Anthropic = new AnthropicSettings
            {
                ApiKey = "test-key",
                ModelName = "claude-3-5-sonnet-20241022",
                Models = new List<AnthropicModel>
                {
                    new()
                    {
                        Name = "claude-3-5-sonnet-20241022",
                        DisplayName = "Claude 3.5 Sonnet",
                        MaxTokens = 8192,
                        DefaultTemperature = 0.7,
                        SupportsJsonMode = true,
                        Capabilities = new List<string> { "chat", "story-generation" }
                    },
                    new()
                    {
                        Name = "claude-3-opus-20250219",
                        DisplayName = "Claude 3 Opus",
                        MaxTokens = 8192,
                        DefaultTemperature = 0.7,
                        SupportsJsonMode = true,
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
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = service.IsAvailable();

        Assert.True(result);
    }

    [Fact]
    public void IsAvailable_WithMissingApiKey_ReturnsFalse()
    {
        _aiSettings.Anthropic.ApiKey = string.Empty;
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = service.IsAvailable();

        Assert.False(result);
    }

    [Fact]
    public void IsAvailable_WithMissingModelName_ReturnsFalse()
    {
        _aiSettings.Anthropic.ModelName = string.Empty;
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = service.IsAvailable();

        Assert.False(result);
    }

    [Fact]
    public void GetAvailableModels_WithValidSettings_ReturnsSelectedModel()
    {
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = service.GetAvailableModels().ToList();

        Assert.Single(result);
        var sonnetModel = result[0];
        Assert.Equal("claude-3-5-sonnet-20241022", sonnetModel.Id);
        Assert.Equal("Claude 3.5 Sonnet", sonnetModel.DisplayName);
        Assert.True(sonnetModel.SupportsJsonSchema);
        Assert.Contains("chat", sonnetModel.Capabilities);
    }

    [Fact]
    public void GetAvailableModels_WithEmptyModels_FallsBackToDefault()
    {
        _aiSettings.Anthropic.Models = new List<AnthropicModel>();
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = service.GetAvailableModels().ToList();

        Assert.Single(result);
        Assert.Equal("claude-3-5-sonnet-20241022", result[0].Id);
        Assert.Equal("Claude 3.5 Sonnet", result[0].DisplayName);
    }

    [Fact]
    public void GetAvailableModels_WithInvalidSettings_ReturnsEmpty()
    {
        _aiSettings.Anthropic.ApiKey = string.Empty;
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = service.GetAvailableModels();

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("claude-3-5-sonnet-20241022", "Claude 3.5 Sonnet")]
    [InlineData("claude-3-opus-20250219", "Claude 3 Opus")]
    [InlineData("claude-3-haiku-20240307", "Claude 3 Haiku")]
    [InlineData("custom-model", "custom-model")]
    public void GetAvailableModels_WithDifferentModels_ReturnsCorrectDisplayName(string modelName, string expectedDisplayName)
    {
        _aiSettings.Anthropic.ModelName = modelName;
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = service.GetAvailableModels().ToList();

        Assert.Single(result);
        Assert.Equal(expectedDisplayName, result[0].DisplayName);
    }

    [Fact]
    public void ProviderName_ReturnsCorrectName()
    {
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        Assert.Equal("anthropic", service.ProviderName);
    }

    [Fact]
    public void ResolveEndpoint_WithModelSpecificEndpoint_ReturnsModelEndpoint()
    {
        var modelEndpoint = "https://custom-anthropic-endpoint.com";
        _aiSettings.Anthropic.Models[0].Endpoint = modelEndpoint;
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = typeof(AnthropicAIService)
            .GetMethod("ResolveEndpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(service, new object[] { "claude-3-5-sonnet-20241022" });

        Assert.Equal(modelEndpoint, result);
    }

    [Fact]
    public void ResolveEndpoint_WithoutModelSpecificEndpoint_ReturnsEmpty()
    {
        _aiSettings.Anthropic.Models[0].Endpoint = null;
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = typeof(AnthropicAIService)
            .GetMethod("ResolveEndpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(service, new object[] { "claude-3-5-sonnet-20241022" });

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveEndpoint_WithUnknownModel_ReturnsEmpty()
    {
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = typeof(AnthropicAIService)
            .GetMethod("ResolveEndpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(service, new object[] { "unknown-model" });

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveEndpoint_WithNullModelName_ReturnsEmpty()
    {
        var service = new AnthropicAIService(_optionsMock.Object, _loggerMock.Object);

        var result = typeof(AnthropicAIService)
            .GetMethod("ResolveEndpoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(service, new object[] { null });

        Assert.Equal(string.Empty, result);
    }
}
