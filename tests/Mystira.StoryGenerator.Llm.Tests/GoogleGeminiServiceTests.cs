using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Configuration;
using Xunit;

namespace Mystira.StoryGenerator.Llm.Tests;

public class GoogleGeminiServiceTests
{
    private readonly Mock<ILogger<GoogleGeminiService>> _loggerMock;
    private readonly Mock<IOptions<AiSettings>> _optionsMock;
    private readonly AiSettings _aiSettings;

    public GoogleGeminiServiceTests()
    {
        _loggerMock = new Mock<ILogger<GoogleGeminiService>>();
        _optionsMock = new Mock<IOptions<AiSettings>>();

        _aiSettings = new AiSettings
        {
            GoogleGemini = new GoogleGeminiSettings
            {
                ApiKey = "test-key",
                Model = "gemini-pro"
            }
        };

        _optionsMock.Setup(x => x.Value).Returns(_aiSettings);
    }

    [Fact]
    public void IsAvailable_WithValidSettings_ReturnsTrue()
    {
        // Arrange
        var service = new GoogleGeminiService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.IsAvailable();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAvailable_WithMissingApiKey_ReturnsFalse()
    {
        // Arrange
        _aiSettings.GoogleGemini.ApiKey = string.Empty;
        var service = new GoogleGeminiService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.IsAvailable();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetAvailableModels_WithValidSettings_ReturnsModel()
    {
        // Arrange
        var service = new GoogleGeminiService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetAvailableModels().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("gemini-pro", result[0].Id);
        Assert.Equal("Gemini Pro", result[0].DisplayName);
        Assert.False(result[0].SupportsJsonSchema);
        Assert.Contains("chat", result[0].Capabilities);
        Assert.Contains("story-generation", result[0].Capabilities);
        Assert.Equal(8192, result[0].MaxTokens); // Gemini typically supports higher token limits
    }

    [Fact]
    public void GetAvailableModels_WithInvalidSettings_ReturnsEmpty()
    {
        // Arrange
        _aiSettings.GoogleGemini.ApiKey = string.Empty;
        var service = new GoogleGeminiService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetAvailableModels();

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("gemini-pro", "Gemini Pro")]
    [InlineData("gemini-pro-vision", "Gemini Pro Vision")]
    [InlineData("gemini-1.5-pro", "Gemini 1.5 Pro")]
    [InlineData("gemini-1.5-flash", "Gemini 1.5 Flash")]
    [InlineData("custom-model", "custom-model")]
    public void GetAvailableModels_WithDifferentModels_ReturnsCorrectDisplayName(string modelName, string expectedDisplayName)
    {
        // Arrange
        _aiSettings.GoogleGemini.Model = modelName;
        var service = new GoogleGeminiService(_optionsMock.Object, _loggerMock.Object);

        // Act
        var result = service.GetAvailableModels().ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedDisplayName, result[0].DisplayName);
    }
}