using Mystira.Application.CQRS.Common.Responses;

namespace Mystira.Application.CQRS.Discord.Commands;

/// <summary>
/// Command to send a rich embed to a Discord channel.
/// Requires Discord bot to be enabled and connected.
/// </summary>
/// <param name="ChannelId">The Discord channel ID to send the embed to.</param>
/// <param name="Title">The title of the embed.</param>
/// <param name="Description">The description text of the embed.</param>
/// <param name="ColorRed">The red component of the embed color (0-255).</param>
/// <param name="ColorGreen">The green component of the embed color (0-255).</param>
/// <param name="ColorBlue">The blue component of the embed color (0-255).</param>
/// <param name="Footer">Optional footer text for the embed.</param>
/// <param name="Fields">Optional list of fields to include in the embed.</param>
public record SendDiscordEmbedCommand(
    ulong ChannelId,
    string Title,
    string Description,
    byte ColorRed,
    byte ColorGreen,
    byte ColorBlue,
    string? Footer,
    List<DiscordEmbedField>? Fields
) : ICommand<CommandResponse>;

/// <summary>
/// Represents a field in a Discord embed.
/// </summary>
/// <param name="Name">The name/title of the field.</param>
/// <param name="Value">The value/content of the field.</param>
/// <param name="Inline">Indicates whether the field should be displayed inline with other fields.</param>
public record DiscordEmbedField(
    string Name,
    string Value,
    bool Inline
);
