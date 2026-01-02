using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.Ai.Abstractions;
using Mystira.Ai.Configuration;
using Mystira.Ai.Providers;
using Xunit;

namespace Mystira.Ai.Tests;

public class LLMServiceFactoryTests
{
    private readonly Mock<IOptions<AiSettings>> _optionsMock;
    private readonly Mock<ILogger<LLMServiceFactory>> _loggerMock;
    private readonly Mock<ILLMService> _azureServiceMock;
    private readonly Mock<ILLMService> _anthropicServiceMock;

    public LLMServiceFactoryTests()
    {
        _optionsMock = new Mock<IOptions<AiSettings>>();
        _loggerMock = new Mock<ILogger<LLMServiceFactory>>();
        _azureServiceMock = new Mock<ILLMService>();
        _anthropicServiceMock = new Mock<ILLMService>();

        _azureServiceMock.Setup(s => s.ProviderName).Returns("azure-openai");
        _anthropicServiceMock.Setup(s => s.ProviderName).Returns("anthropic");
    }

    [Fact]
    public void GetService_WithValidProvider_ReturnsService()
    {
        // Arrange
        _azureServiceMock.Setup(s => s.IsAvailable()).Returns(true);
        _optionsMock.Setup(o => o.Value).Returns(new AiSettings { DefaultProvider = "azure-openai" });

        var services = new List<ILLMService> { _azureServiceMock.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        // Act
        var service = factory.GetService("azure-openai");

        // Assert
        Assert.NotNull(service);
        Assert.Equal("azure-openai", service.ProviderName);
    }

    [Fact]
    public void GetService_WithInvalidProvider_ReturnsNull()
    {
        // Arrange
        _optionsMock.Setup(o => o.Value).Returns(new AiSettings { DefaultProvider = "azure-openai" });
        var factory = new LLMServiceFactory([], _optionsMock.Object, _loggerMock.Object);

        // Act
        var service = factory.GetService("nonexistent");

        // Assert
        Assert.Null(service);
    }

    [Fact]
    public void GetService_WithEmptyProviderName_ReturnsNull()
    {
        // Arrange
        _optionsMock.Setup(o => o.Value).Returns(new AiSettings());
        var factory = new LLMServiceFactory([], _optionsMock.Object, _loggerMock.Object);

        // Act
        var service = factory.GetService("");

        // Assert
        Assert.Null(service);
    }

    [Fact]
    public void GetDefaultService_WithConfiguredDefault_ReturnsDefaultService()
    {
        // Arrange
        _azureServiceMock.Setup(s => s.IsAvailable()).Returns(true);
        _optionsMock.Setup(o => o.Value).Returns(new AiSettings { DefaultProvider = "azure-openai" });

        var services = new List<ILLMService> { _azureServiceMock.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        // Act
        var service = factory.GetDefaultService();

        // Assert
        Assert.NotNull(service);
        Assert.Equal("azure-openai", service.ProviderName);
    }

    [Fact]
    public void GetDefaultService_WithNoDefaultConfigured_ReturnsNull()
    {
        // Arrange
        _optionsMock.Setup(o => o.Value).Returns(new AiSettings { DefaultProvider = "" });
        var factory = new LLMServiceFactory([], _optionsMock.Object, _loggerMock.Object);

        // Act
        var service = factory.GetDefaultService();

        // Assert
        Assert.Null(service);
    }

    [Fact]
    public void GetAvailableModels_ReturnsModelsFromAllProviders()
    {
        // Arrange
        _azureServiceMock.Setup(s => s.IsAvailable()).Returns(true);
        _anthropicServiceMock.Setup(s => s.IsAvailable()).Returns(true);
        _optionsMock.Setup(o => o.Value).Returns(new AiSettings { DefaultProvider = "azure-openai" });

        var services = new List<ILLMService> { _azureServiceMock.Object, _anthropicServiceMock.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        // Act
        var models = factory.GetAvailableModels().ToList();

        // Assert
        Assert.Equal(2, models.Count);
        Assert.Contains(models, m => m.ProviderName == "azure-openai");
        Assert.Contains(models, m => m.ProviderName == "anthropic");
    }
}
