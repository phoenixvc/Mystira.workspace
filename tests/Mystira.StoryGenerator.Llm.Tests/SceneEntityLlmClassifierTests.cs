using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Llm.Services.DominatorBasedConsistency;

namespace Mystira.StoryGenerator.Llm.Tests;

public class SceneEntityLlmClassifierTests
{
    private static AiSettings CreateSettings(
        bool enabled = true,
        string? provider = "azure-openai",
        string? modelId = "entity-router",
        string? deploymentName = null,
        double temperature = 0.1,
        int maxTokens = 200)
    {
        return new AiSettings
        {
            DefaultProvider = provider ?? string.Empty,
            EntityClassifier = new EntityClassifierSettings
            {
                Enabled = enabled,
                Provider = provider,
                ModelId = modelId,
                DeploymentName = deploymentName,
                Temperature = temperature,
                MaxTokens = maxTokens
            }
        };
    }

    private static SceneEntityLlmClassifier CreateSut(
        AiSettings settings,
        out Mock<ILlmServiceFactory> factoryMock,
        out Mock<ILLMService> llmServiceMock)
    {
        factoryMock = new Mock<ILlmServiceFactory>(MockBehavior.Strict);
        llmServiceMock = new Mock<ILLMService>(MockBehavior.Strict);

        var options = Mock.Of<IOptions<AiSettings>>(o => o.Value == settings);
        var logger = new Mock<ILogger<SceneEntityLlmClassifier>>();

        return new SceneEntityLlmClassifier(options, factoryMock.Object, logger.Object);
    }

    [Fact]
    public async Task ClassifyAsync_WithEmptyQuery_ReturnsNull()
    {
        // Arrange
        var settings = CreateSettings();
        var sut = CreateSut(settings, out _, out _);

        // Act
        var result = await sut.ClassifyAsync("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ClassifyAsync_WhenNotConfigured_ReturnsNull()
    {
        // Arrange
        var settings = CreateSettings(enabled: false);
        var sut = CreateSut(settings, out _, out _);

        // Act
        var result = await sut.ClassifyAsync("some text");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ClassifyAsync_WhenFactoryReturnsNullService_ReturnsNull()
    {
        // Arrange
        var settings = CreateSettings(deploymentName: "my-deployment");
        var sut = CreateSut(settings, out var factoryMock, out _);

        // The classifier calls GetService(provider, deploymentName) when DeploymentName is set.
        factoryMock
            .Setup(f => f.GetService(settings.EntityClassifier.Provider!, settings.EntityClassifier.DeploymentName))
            .Returns((ILLMService?)null);

        // Act
        var result = await sut.ClassifyAsync("some text");

        // Assert
        Assert.Null(result);
        factoryMock.VerifyAll();
    }

    [Fact]
    public async Task ClassifyAsync_WhenServiceReturnsEmptyContent_ReturnsNull()
    {
        // Arrange
        var settings = CreateSettings(deploymentName: "dep-1");
        var sut = CreateSut(settings, out var factoryMock, out var llmServiceMock);

        llmServiceMock.SetupGet(s => s.DeploymentNameOrModelId).Returns("dep-1");
        factoryMock.Setup(f => f.GetService(settings.EntityClassifier.Provider!, "dep-1")).Returns(llmServiceMock.Object);

        llmServiceMock
            .Setup(s => s.CompleteAsync(It.IsAny<ChatCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatCompletionResponse { Content = string.Empty });

        // Act
        var result = await sut.ClassifyAsync("text introducing Alice at the Market");

        // Assert
        Assert.Null(result);
        factoryMock.VerifyAll();
        llmServiceMock.VerifyAll();
    }

    [Fact]
    public async Task ClassifyAsync_WhenServiceReturnsValidJson_DeserializesEntities()
    {
        // Arrange
        var settings = CreateSettings(temperature: 0.2, maxTokens: 150, deploymentName: "dep-xyz");
        var sut = CreateSut(settings, out var factoryMock, out var llmServiceMock);

        llmServiceMock.SetupGet(s => s.DeploymentNameOrModelId).Returns("dep-xyz");
        factoryMock.Setup(f => f.GetService(settings.EntityClassifier.Provider!, "dep-xyz")).Returns(llmServiceMock.Object);

        var json = "{\n  \"entities\": [\n    { \"name\": \"Alice\", \"type\": \"character\" },\n    { \"name\": \"Market\", \"type\": \"location\" }\n  ]\n}";
        llmServiceMock
            .Setup(s => s.CompleteAsync(It.Is<ChatCompletionRequest>(r =>
                    r.Provider == settings.EntityClassifier.Provider &&
                    r.ModelId == settings.EntityClassifier.ModelId &&
                    r.Model == "dep-xyz" &&
                    Math.Abs(r.Temperature - settings.EntityClassifier.Temperature) < 1e-9 &&
                    r.MaxTokens == settings.EntityClassifier.MaxTokens &&
                    r.Messages.Count == 2 &&
                    r.Messages.Any(m => m.Content != null && m.MessageType == ChatMessageType.System) &&
                    r.Messages.Any(m => m.Content == "The hero Alice arrives at the Grand Market." && m.MessageType == ChatMessageType.User)
                ), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatCompletionResponse { Content = json });

        // Act
        var result = await sut.ClassifyAsync("The hero Alice arrives at the Grand Market.");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result!.IntroducedEntities);
        Assert.Equal(2, result.IntroducedEntities.Length);
        Assert.Contains(result.IntroducedEntities, e => e.Name == "Alice");
        Assert.Contains(result.IntroducedEntities, e => e.Name == "Market" || e.Name == "Grand Market");
    }

    [Fact]
    public async Task ClassifyAsync_WithDeploymentName_UsesFactoryGetServiceAndServiceDeploymentForRequest()
    {
        // Arrange
        var settings = CreateSettings(deploymentName: "router-deploy");
        var sut = CreateSut(settings, out var factoryMock, out var llmServiceMock);

        llmServiceMock.SetupGet(s => s.DeploymentNameOrModelId).Returns("router-deploy");
        factoryMock.Setup(f => f.GetService(settings.EntityClassifier.Provider!, "router-deploy")).Returns(llmServiceMock.Object);

        var json = "{\"entities\":[{\"name\":\"Alice\",\"type\":\"character\"}]}";
        llmServiceMock
            .Setup(s => s.CompleteAsync(It.Is<ChatCompletionRequest>(r => r.Model == "router-deploy"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatCompletionResponse { Content = json });

        // Act
        var result = await sut.ClassifyAsync("Alice enters.");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result!.IntroducedEntities);
        factoryMock.VerifyAll();
        llmServiceMock.VerifyAll();
    }
}
