using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Common.Responses;
using Mystira.App.Application.CQRS.Discord.Commands;
using Mystira.App.Application.CQRS.Discord.Queries;
using Mystira.App.Application.Ports.Messaging;

namespace Mystira.App.Application.Tests.CQRS.Discord;

public class DiscordHandlerTests
{
    private readonly Mock<IChatBotService> _chatBotService;

    public DiscordHandlerTests()
    {
        _chatBotService = new Mock<IChatBotService>();
    }

    #region SendDiscordMessageCommandHandler

    [Fact]
    public async Task SendMessage_WhenBotConnected_ReturnsSuccess()
    {
        _chatBotService.Setup(s => s.IsConnected).Returns(true);
        _chatBotService.Setup(s => s.SendMessageAsync(123UL, "Hello", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await SendDiscordMessageCommandHandler.Handle(
            new SendDiscordMessageCommand(123UL, "Hello"),
            _chatBotService.Object,
            Mock.Of<ILogger<SendDiscordMessageCommand>>(),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        _chatBotService.Verify(s => s.SendMessageAsync(123UL, "Hello", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessage_WhenBotDisconnected_ReturnsError()
    {
        _chatBotService.Setup(s => s.IsConnected).Returns(false);

        var result = await SendDiscordMessageCommandHandler.Handle(
            new SendDiscordMessageCommand(123UL, "Hello"),
            _chatBotService.Object,
            Mock.Of<ILogger<SendDiscordMessageCommand>>(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        _chatBotService.Verify(s => s.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendMessage_WhenServiceThrows_ReturnsError()
    {
        _chatBotService.Setup(s => s.IsConnected).Returns(true);
        _chatBotService.Setup(s => s.SendMessageAsync(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection lost"));

        var result = await SendDiscordMessageCommandHandler.Handle(
            new SendDiscordMessageCommand(123UL, "Hello"),
            _chatBotService.Object,
            Mock.Of<ILogger<SendDiscordMessageCommand>>(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Connection lost");
    }

    #endregion

    #region SendDiscordEmbedCommandHandler

    [Fact]
    public async Task SendEmbed_WhenBotConnected_ReturnsSuccess()
    {
        _chatBotService.Setup(s => s.IsConnected).Returns(true);
        _chatBotService.Setup(s => s.SendEmbedAsync(It.IsAny<ulong>(), It.IsAny<EmbedData>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await SendDiscordEmbedCommandHandler.Handle(
            new SendDiscordEmbedCommand(123UL, "Title", "Description", 255, 0, 0, "Footer",
                new List<DiscordEmbedField> { new("Field1", "Value1", true) }),
            _chatBotService.Object,
            Mock.Of<ILogger<SendDiscordEmbedCommand>>(),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        _chatBotService.Verify(s => s.SendEmbedAsync(123UL, It.IsAny<EmbedData>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmbed_WhenBotDisconnected_ReturnsError()
    {
        _chatBotService.Setup(s => s.IsConnected).Returns(false);

        var result = await SendDiscordEmbedCommandHandler.Handle(
            new SendDiscordEmbedCommand(123UL, "Title", "Description", 255, 0, 0, null, null),
            _chatBotService.Object,
            Mock.Of<ILogger<SendDiscordEmbedCommand>>(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task SendEmbed_WhenServiceThrows_ReturnsError()
    {
        _chatBotService.Setup(s => s.IsConnected).Returns(true);
        _chatBotService.Setup(s => s.SendEmbedAsync(It.IsAny<ulong>(), It.IsAny<EmbedData>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Embed failed"));

        var result = await SendDiscordEmbedCommandHandler.Handle(
            new SendDiscordEmbedCommand(123UL, "Title", "Description", 0, 0, 0, null, null),
            _chatBotService.Object,
            Mock.Of<ILogger<SendDiscordEmbedCommand>>(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
    }

    #endregion

    #region GetDiscordBotStatusQueryHandler

    [Fact]
    public async Task GetBotStatus_WhenConnected_ReturnsConnectedStatus()
    {
        var status = new BotStatus
        {
            IsEnabled = true,
            IsConnected = true,
            BotName = "MystiraBot",
            BotId = 999UL
        };
        _chatBotService.Setup(s => s.GetStatus()).Returns(status);

        var result = await GetDiscordBotStatusQueryHandler.Handle(
            new GetDiscordBotStatusQuery(),
            _chatBotService.Object,
            Mock.Of<ILogger<GetDiscordBotStatusQuery>>(),
            CancellationToken.None);

        result.Enabled.Should().BeTrue();
        result.Connected.Should().BeTrue();
        result.BotUsername.Should().Be("MystiraBot");
        result.BotId.Should().Be(999UL);
    }

    [Fact]
    public async Task GetBotStatus_WhenDisconnected_ReturnsDisconnectedStatus()
    {
        var status = new BotStatus
        {
            IsEnabled = true,
            IsConnected = false,
            BotName = null,
            BotId = null
        };
        _chatBotService.Setup(s => s.GetStatus()).Returns(status);

        var result = await GetDiscordBotStatusQueryHandler.Handle(
            new GetDiscordBotStatusQuery(),
            _chatBotService.Object,
            Mock.Of<ILogger<GetDiscordBotStatusQuery>>(),
            CancellationToken.None);

        result.Enabled.Should().BeTrue();
        result.Connected.Should().BeFalse();
        result.BotUsername.Should().BeNull();
    }

    [Fact]
    public async Task GetBotStatus_WhenDisabled_ReturnsDisabledStatus()
    {
        var status = new BotStatus
        {
            IsEnabled = false,
            IsConnected = false
        };
        _chatBotService.Setup(s => s.GetStatus()).Returns(status);

        var result = await GetDiscordBotStatusQueryHandler.Handle(
            new GetDiscordBotStatusQuery(),
            _chatBotService.Object,
            Mock.Of<ILogger<GetDiscordBotStatusQuery>>(),
            CancellationToken.None);

        result.Enabled.Should().BeFalse();
        result.Connected.Should().BeFalse();
    }

    #endregion
}
