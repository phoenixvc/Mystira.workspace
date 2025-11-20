using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;

namespace Mystira.StoryGenerator.Api.Tests;

public class AzureOpenAIServiceOptionsTests
{
    private readonly Mock<ILogger<AzureOpenAIService>> _loggerMock = new();

    [Fact]
    public void BuildOptions_WithSchema_BuildsResponseFormat()
    {
        // Arrange
        var request = new ChatCompletionRequest
        {
            Messages = new List<MystiraChatMessage>
            {
                new() { MessageType = ChatMessageType.User, Content = "Hi" }
            },
            JsonSchemaFormat = new JsonSchemaResponseFormat
            {
                FormatName = "mystira-story-setup",
                SchemaJson = "{ \"type\": \"object\" }",
                IsStrict = true
            }
        };

        // Act
        var options = AzureOpenAIService.BuildOptions(request, _loggerMock.Object);

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options!.ResponseFormat);
    }

    [Fact]
    public void BuildOptions_WithoutSchema_ReturnsNull()
    {
        // Arrange
        var request = new ChatCompletionRequest
        {
            Messages = new List<MystiraChatMessage>
            {
                new() { MessageType = ChatMessageType.User, Content = "Hi" }
            }
        };

        // Act
        var options = AzureOpenAIService.BuildOptions(request, _loggerMock.Object);

        // Assert
        Assert.Null(options);
    }
}
