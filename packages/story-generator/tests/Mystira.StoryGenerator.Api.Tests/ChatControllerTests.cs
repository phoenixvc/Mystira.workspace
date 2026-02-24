using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Api.Controllers;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Api.Tests;

public class ChatControllerTests
{
    private readonly Mock<IChatOrchestrationService> _chatOrchestrationMock;
    private readonly Mock<ILlmServiceFactory> _llmFactoryMock;
    private readonly Mock<ILogger<ChatController>> _loggerMock;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _chatOrchestrationMock = new Mock<IChatOrchestrationService>();
        _llmFactoryMock = new Mock<ILlmServiceFactory>();
        _loggerMock = new Mock<ILogger<ChatController>>();

        _controller = new ChatController(
            _chatOrchestrationMock.Object,
            _llmFactoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetModels_ReturnsAvailableModels()
    {
        // Arrange
        var providerModels = new List<ProviderModels>
        {
            new()
            {
                Provider = "azure-openai",
                Available = true,
                Models = new List<ChatModelInfo>
                {
                    new()
                    {
                        Id = "gpt-4",
                        DisplayName = "GPT-4",
                        MaxTokens = 4096,
                        DefaultTemperature = 0.7
                    },
                    new()
                    {
                        Id = "gpt-35-turbo",
                        DisplayName = "GPT-3.5 Turbo",
                        MaxTokens = 4096,
                        DefaultTemperature = 0.7
                    }
                }
            }
        };

        _llmFactoryMock.Setup(x => x.GetAvailableModels()).Returns(providerModels);

        // Act
        var result = _controller.GetModels();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ChatModelsResponse>(okResult.Value);

        Assert.Single(response.Providers);
        Assert.Equal(2, response.TotalModels);

        var azureProvider = response.Providers.FirstOrDefault(p => p.Provider == "azure-openai");
        Assert.NotNull(azureProvider);
        Assert.True(azureProvider.Available);
        Assert.Equal(2, azureProvider.Models.Count);
        Assert.Equal("gpt-4", azureProvider.Models[0].Id);
        Assert.Equal("gpt-35-turbo", azureProvider.Models[1].Id);
    }

    [Fact]
    public void GetModels_WithException_ReturnsInternalServerError()
    {
        // Arrange
        _llmFactoryMock.Setup(x => x.GetAvailableModels())
            .Throws(new Exception("Test exception"));

        // Act
        var result = _controller.GetModels();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);

        var response = Assert.IsType<ChatModelsResponse>(objectResult.Value);
        Assert.Empty(response.Providers);
        Assert.Equal(0, response.TotalModels);
    }
}
