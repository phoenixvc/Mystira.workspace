using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Messaging;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.Discord.Queries;

/// <summary>
/// Wolverine message handler for retrieving Discord bot status.
/// Checks bot service state via the platform-agnostic IChatBotService.
/// </summary>
public static class GetDiscordBotStatusQueryHandler
{
    public static Task<DiscordBotStatusResponse> Handle(
        GetDiscordBotStatusQuery request,
        IChatBotService chatBotService,
        ILogger<GetDiscordBotStatusQuery> logger,
        CancellationToken ct)
    {
        if (chatBotService == null)
            throw new ValidationException("chatBotService", "chatBotService is required");

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
