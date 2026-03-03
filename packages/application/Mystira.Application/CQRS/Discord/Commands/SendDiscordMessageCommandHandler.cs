using Microsoft.Extensions.Logging;
using Mystira.Application.CQRS.Common.Responses;
using Mystira.Application.Ports.Messaging;

namespace Mystira.Application.CQRS.Discord.Commands;

/// <summary>
/// Wolverine message handler for sending messages to Discord channels.
/// Validates bot connectivity and sends message via the platform-agnostic IChatBotService.
/// </summary>
public static class SendDiscordMessageCommandHandler
{
    /// <summary>
    /// Handles the SendDiscordMessageCommand.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="chatBotService">The chat bot service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A command response indicating success or failure.</returns>
    public static async Task<CommandResponse> Handle(
        SendDiscordMessageCommand command,
        IChatBotService chatBotService,
        ILogger<SendDiscordMessageCommand> logger,
        CancellationToken ct)
    {
        if (chatBotService == null)
            throw new ArgumentNullException(nameof(chatBotService));

        if (!chatBotService.IsConnected)
        {
            logger.LogWarning("Attempted to send message but chat bot is not connected");
            return new CommandResponse(false, "Chat bot is not connected");
        }

        try
        {
            await chatBotService.SendMessageAsync(command.ChannelId, command.Message, ct);
            logger.LogInformation("Successfully sent Discord message to channel {ChannelId}", command.ChannelId);
            return new CommandResponse(true, "Message sent successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending Discord message to channel {ChannelId}", command.ChannelId);
            return new CommandResponse(false, $"Error sending message: {ex.Message}");
        }
    }
}
