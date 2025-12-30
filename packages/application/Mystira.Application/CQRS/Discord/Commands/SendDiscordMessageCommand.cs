using Mystira.Application.CQRS.Common.Responses;

namespace Mystira.Application.CQRS.Discord.Commands;

/// <summary>
/// Command to send a message to a Discord channel.
/// Requires Discord bot to be enabled and connected.
/// </summary>
public record SendDiscordMessageCommand(
    ulong ChannelId,
    string Message
) : ICommand<CommandResponse>;
