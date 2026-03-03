using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Messaging;

namespace Mystira.Application.CQRS.Discord.Queries;

/// <summary>
/// Wolverine message handler for retrieving Discord bot status.
/// Checks bot service state via the platform-agnostic IChatBotService.
/// </summary>
public static class GetDiscordBotStatusQueryHandler
{
    /// <summary>
    /// Handles the GetDiscordBotStatusQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="chatBotService">The chat bot service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The Discord bot status response.</returns>
    public static Task<DiscordBotStatusResponse> Handle(
        GetDiscordBotStatusQuery request,
        IChatBotService chatBotService,
        ILogger<GetDiscordBotStatusQuery> logger,
        CancellationToken ct)
    {
        if (chatBotService == null)
            throw new ArgumentNullException(nameof(chatBotService));

        var status = chatBotService.GetStatus();

        logger.LogDebug("Discord bot status: Enabled={Enabled}, Connected={Connected}, Username={Username}",
            status.IsEnabled, status.IsConnected, status.BotName);

        return Task.FromResult(new DiscordBotStatusResponse(
            Enabled: status.IsEnabled,
            Connected: status.IsConnected,
            BotUsername: status.BotName,
            BotId: status.BotId,
            Message: status.IsConnected ? "Discord bot is connected" : "Discord bot is not connected"
        ));
    }
}
