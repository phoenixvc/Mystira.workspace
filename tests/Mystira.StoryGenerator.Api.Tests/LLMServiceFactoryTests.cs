using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Llm.Services.LLM;

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
        mockService1.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
        mockService1.Setup(x => x.IsAvailable()).Returns(true);

        var mockService2 = new Mock<ILLMService>();
        mockService2.Setup(x => x.ProviderName).Returns("google-gemini");
        mockService2.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
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
        mockService.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
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
        mockService.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
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
        mockService.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
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
        mockService1.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
        mockService1.Setup(x => x.IsAvailable()).Returns(false);

        var mockService2 = new Mock<ILLMService>();
        mockService2.Setup(x => x.ProviderName).Returns("google-gemini");
        mockService2.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
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
        mockService1.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
        mockService1.Setup(x => x.IsAvailable()).Returns(true);

        var mockService2 = new Mock<ILLMService>();
        mockService2.Setup(x => x.ProviderName).Returns("google-gemini");
        mockService2.Setup(x => x.DeploymentNameOrModelId).Returns((string?)null);
        mockService2.Setup(x => x.IsAvailable()).Returns(false);

        var services = new List<ILLMService> { mockService1.Object, mockService2.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        var result = factory.GetAvailableServices().ToList();

        Assert.Single(result);
        Assert.Equal("azure-openai", result[0].ProviderName);
    }

    [Fact]
    public void GetAvailableModels_ReturnsModelsFromAllProviders()
    {
        var azureModel1 = new ChatModelInfo
        {
            Id = "gpt-4",
            DisplayName = "GPT-4",
            MaxTokens = 4096,
            DefaultTemperature = 0.7
        };

        var azureModel2 = new ChatModelInfo
        {
            Id = "gpt-35-turbo",
            DisplayName = "GPT-3.5 Turbo",
            MaxTokens = 4096,
            DefaultTemperature = 0.7
        };

        var mockService1 = new Mock<ILLMService>();
        mockService1.Setup(x => x.ProviderName).Returns("azure-openai");
        mockService1.Setup(x => x.IsAvailable()).Returns(true);
        mockService1.Setup(x => x.GetAvailableModels()).Returns(new List<ChatModelInfo> { azureModel1, azureModel2 });

        var services = new List<ILLMService> { mockService1.Object };
        var factory = new LLMServiceFactory(services, _optionsMock.Object, _loggerMock.Object);

        var result = factory.GetAvailableModels().ToList();

        Assert.Single(result);
        
        var azureProvider = result.FirstOrDefault(p => p.Provider == "azure-openai");
        Assert.NotNull(azureProvider);
        Assert.True(azureProvider.Available);
        Assert.Equal(2, azureProvider.Models.Count);
        Assert.Equal("gpt-4", azureProvider.Models[0].Id);
        Assert.Equal("gpt-35-turbo", azureProvider.Models[1].Id);
    }
}
