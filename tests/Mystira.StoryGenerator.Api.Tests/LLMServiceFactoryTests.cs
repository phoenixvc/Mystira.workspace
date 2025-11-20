using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Configuration;
using Xunit;

namespace Mystira.StoryGenerator.Api.Tests;

public class LLMServiceFactoryTests
{
    private readonly Mock<ILogger<LLMServiceFactory>> _loggerMock;
    private readonly Mock<IOptions<AiSettings>> _optionsMock;
    private readonly AiSettings _aiSettings;

    public LLMServiceFactoryTests()
    {
        _loggerMock = new Mock<ILogger<LLMServiceFactory>>();
        _optionsMock = new Mock<IOptions<AiSettings>>();

        _aiSettings = new AiSettings
        {
            DefaultProvider = "azure-openai",
            AzureOpenAI = new AzureOpenAISettings
            {
                Endpoint = "https://test.openai.azure.com",
                ApiKey = "test-key",
                DeploymentName = "test-deployment"
            }
        };

        _optionsMock.Setup(x => x.Value).Returns(_aiSettings);
    }

    [Fact]
    public void GetService_WithValidProvider_ReturnsService()
    {
        var mockService1 = new Mock<ILLMService>();
        mockService1.Setup(x => x.ProviderName).Returns("azure-openai");
        mockService1.Setup(x => x.IsAvailable()).Returns(true);

        var mockService2 = new Mock<ILLMService>();
        mockService2.Setup(x => x.ProviderName).Returns("google-gemini");
        mockService2.Setup(x => x.IsAvailable()).Returns(true);

        var services = new List<ILLMService> { mockService1.Object, mockService2.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        var result = factory.GetService("azure-openai");

        Assert.NotNull(result);
        Assert.Equal("azure-openai", result!.ProviderName);
    }

    [Fact]
    public void GetService_WithInvalidProvider_ReturnsNull()
    {
        var mockService = new Mock<ILLMService>();
        mockService.Setup(x => x.ProviderName).Returns("azure-openai");
        mockService.Setup(x => x.IsAvailable()).Returns(true);

        var services = new List<ILLMService> { mockService.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        var result = factory.GetService("invalid-provider");

        Assert.Null(result);
    }

    [Fact]
    public void GetService_WithUnavailableProvider_ReturnsNull()
    {
        var mockService = new Mock<ILLMService>();
        mockService.Setup(x => x.ProviderName).Returns("azure-openai");
        mockService.Setup(x => x.IsAvailable()).Returns(false);

        var services = new List<ILLMService> { mockService.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        var result = factory.GetService("azure-openai");

        Assert.Null(result);
    }

    [Fact]
    public void GetDefaultService_ReturnsConfiguredDefaultProvider()
    {
        var mockService = new Mock<ILLMService>();
        mockService.Setup(x => x.ProviderName).Returns("azure-openai");
        mockService.Setup(x => x.IsAvailable()).Returns(true);

        var services = new List<ILLMService> { mockService.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        var result = factory.GetDefaultService();

        Assert.NotNull(result);
        Assert.Equal("azure-openai", result!.ProviderName);
    }

    [Fact]
    public void GetDefaultService_WithUnavailableDefault_ReturnsFallback()
    {
        var mockService1 = new Mock<ILLMService>();
        mockService1.Setup(x => x.ProviderName).Returns("azure-openai");
        mockService1.Setup(x => x.IsAvailable()).Returns(false);

        var mockService2 = new Mock<ILLMService>();
        mockService2.Setup(x => x.ProviderName).Returns("google-gemini");
        mockService2.Setup(x => x.IsAvailable()).Returns(true);

        var services = new List<ILLMService> { mockService1.Object, mockService2.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        var result = factory.GetDefaultService();

        Assert.NotNull(result);
        Assert.Equal("google-gemini", result!.ProviderName);
    }

    [Fact]
    public void GetAvailableServices_ReturnsOnlyAvailableServices()
    {
        var mockService1 = new Mock<ILLMService>();
        mockService1.Setup(x => x.ProviderName).Returns("azure-openai");
        mockService1.Setup(x => x.IsAvailable()).Returns(true);

        var mockService2 = new Mock<ILLMService>();
        mockService2.Setup(x => x.ProviderName).Returns("google-gemini");
        mockService2.Setup(x => x.IsAvailable()).Returns(false);

        var services = new List<ILLMService> { mockService1.Object, mockService2.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        var result = factory.GetAvailableServices().ToList();

        Assert.Single(result);
        Assert.Equal("azure-openai", result[0].ProviderName);
    }
}
