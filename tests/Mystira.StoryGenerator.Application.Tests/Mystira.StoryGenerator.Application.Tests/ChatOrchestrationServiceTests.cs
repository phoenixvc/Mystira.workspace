using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Xunit;

namespace Mystira.StoryGenerator.Application.Tests;

public class ChatOrchestrationServiceTests
{
    private readonly Mock<ICommandIntentRouter> _mockCommandIntentRouter;
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILLMServiceFactory> _mockLlmServiceFactory;
    private readonly Mock<IInstructionBlockService> _mockInstructionBlockService;
    private readonly Mock<IIntentClassificationService> _mockIntentRouterService;
    private readonly Mock<ILogger<ChatOrchestrationService>> _mockLogger;
    private readonly ChatOrchestrationService _service;

    public ChatOrchestrationServiceTests()
    {
        _mockCommandIntentRouter = new Mock<ICommandIntentRouter>();
        _mockMediator = new Mock<IMediator>();
        _mockLlmServiceFactory = new Mock<ILLMServiceFactory>();
        _mockInstructionBlockService = new Mock<IInstructionBlockService>();
        _mockIntentRouterService = new Mock<IIntentClassificationService>();
        _mockLogger = new Mock<ILogger<ChatOrchestrationService>>();

        _service = new ChatOrchestrationService(
            _mockCommandIntentRouter.Object,
            _mockMediator.Object,
            _mockLlmServiceFactory.Object,
            _mockInstructionBlockService.Object,
            _mockIntentRouterService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CompleteAsync_WithNoMessages_ReturnsClarificationResponse()
    {
        // Arrange
        var context = new ChatContext
        {
            Messages = new List<MystiraChatMessage>()
        };

        // Act
        var result = await _service.CompleteAsync(context, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.RequiresClarification);
        Assert.Contains("I can generate a new story", result.Prompt);
    }

    [Fact]
    public async Task CompleteAsync_WithUnrecognizedIntent_ReturnsClarificationResponse()
    {
        // Arrange
        var context = new ChatContext
        {
            Messages = new List<MystiraChatMessage>
            {
                new() { MessageType = ChatMessageType.User, Content = "hello world" }
            }
        };

        _mockCommandIntentRouter
            .Setup(x => x.DetectPrimaryInstructionTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _service.CompleteAsync(context, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.RequiresClarification);
        Assert.Contains("I can generate a new story", result.Prompt);
    }

    [Fact]
    public async Task CompleteAsync_WithMissingParameters_ReturnsClarificationResponse()
    {
        // Arrange
        var incompleteRequest = "Generate a story for me";

        var context = new ChatContext
        {
            Messages = new List<MystiraChatMessage>
            {
                new() { MessageType = ChatMessageType.User, Content = incompleteRequest }
            }
        };

        _mockCommandIntentRouter
            .Setup(x => x.DetectPrimaryInstructionTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("story_generate_initial");

        // Mock the LLM service for parameter checking
        var mockLlmService = new Mock<ILLMService>();
        mockLlmService.Setup(x => x.ProviderName).Returns("test-llm");
        mockLlmService.Setup(x => x.IsAvailable()).Returns(true);

        var parameterCheckResponse = new ChatCompletionResponse
        {
            Success = true,
            Content = @"[""title"", ""agegroup""]", // Missing title and agegroup
            Provider = "test-llm"
        };

        mockLlmService
            .Setup(x => x.CompleteAsync(It.Is<ChatCompletionRequest>(r =>
                r.MaxTokens == 100 && r.Temperature == 0.1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parameterCheckResponse);

        _mockLlmServiceFactory
            .Setup(x => x.GetDefaultService())
            .Returns(mockLlmService.Object);

        // Act
        var result = await _service.CompleteAsync(context, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.RequiresClarification);
        Assert.Equal("story_generate_initial", result.Intent);
        Assert.Equal("GenerateStoryCommand", result.Handler);
        Assert.Contains("title", result.Prompt);
        Assert.Contains("agegroup", result.Prompt);
    }

    [Fact]
    public async Task CompleteAsync_WithValidGenerateStoryIntent_ReturnsSuccessResponse()
    {
        // Arrange
        var storyRequest = "Generate a story called 'Test Story' for kids aged 6-9 with 6 to 12 scenes";

        var context = new ChatContext
        {
            Messages = new List<MystiraChatMessage>
            {
                new() { MessageType = ChatMessageType.User, Content = storyRequest }
            }
        };

        _mockCommandIntentRouter
            .Setup(x => x.DetectPrimaryInstructionTypeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("story_generate_initial");

        // Mock the LLM service for parameter checking
        var mockLlmService = new Mock<ILLMService>();
        mockLlmService.Setup(x => x.ProviderName).Returns("test-llm");
        mockLlmService.Setup(x => x.IsAvailable()).Returns(true);

        var parameterCheckResponse = new ChatCompletionResponse
        {
            Success = true,
            Content = "[]", // No missing parameters
            Provider = "test-llm"
        };

        mockLlmService
            .Setup(x => x.CompleteAsync(It.Is<ChatCompletionRequest>(r =>
                r.MaxTokens == 100 && r.Temperature == 0.1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parameterCheckResponse);

        _mockLlmServiceFactory
            .Setup(x => x.GetDefaultService())
            .Returns(mockLlmService.Object);

        var mockStoryResponse = new GenerateJsonStoryResponse
        {
            Success = true,
            Json = @"{""title"": ""Generated Story"", ""scenes"": []}"
        };
        _mockMediator
            .Setup(x => x.Send(It.IsAny<GenerateStoryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockStoryResponse);

        // Act
        var result = await _service.CompleteAsync(context, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.RequiresClarification);
        Assert.Equal("story_generate_initial", result.Intent);
        Assert.Equal("GenerateStoryCommand", result.Handler);
        Assert.NotNull(result.Result);
    }
}
