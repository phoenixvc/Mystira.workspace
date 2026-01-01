using System.Reflection;
using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Messaging;

namespace Mystira.Infrastructure.Discord.Services;

/// <summary>
/// No-op implementation of chat bot services used when a platform bot is disabled.
/// Satisfies DI for IChatBotService, IMessagingService, and IBotCommandService.
/// </summary>
public class NoOpChatBotService : IChatBotService, IMessagingService, IBotCommandService
{
    private readonly ILogger<NoOpChatBotService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoOpChatBotService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public NoOpChatBotService(ILogger<NoOpChatBotService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets the chat platform. Always returns Discord as a placeholder.
    /// </summary>
    public ChatPlatform Platform => ChatPlatform.Discord;

    /// <summary>
    /// Starts the no-op chat bot service. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("NoOpChatBotService.StartAsync called – bot integration is disabled.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the no-op chat bot service. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("NoOpChatBotService.StopAsync called – bot integration is disabled.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a message to a channel. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="message">The message text.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task SendMessageAsync(ulong channelId, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] SendMessageAsync to {ChannelId}: {Message}", channelId, message);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends an embed to a channel. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="embed">The embed data.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task SendEmbedAsync(ulong channelId, EmbedData embed, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] SendEmbedAsync to {ChannelId}: {Title}", channelId, embed.Title);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Replies to a message. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="messageId">The message ID to reply to.</param>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="reply">The reply text.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task ReplyToMessageAsync(ulong messageId, ulong channelId, string reply, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] ReplyToMessageAsync to {ChannelId}/{MessageId}: {Reply}", channelId, messageId, reply);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a value indicating whether the bot is connected. Always returns false for no-op implementation.
    /// </summary>
    public bool IsConnected => false;

    /// <summary>
    /// Gets the bot status. Always returns a status indicating the bot is disabled.
    /// </summary>
    /// <returns>Bot status information indicating the service is disabled.</returns>
    public BotStatus GetStatus()
        => new BotStatus
        {
            IsEnabled = false,
            IsConnected = false,
            BotName = "NoOp Bot",
            BotId = 0,
            ServerCount = 0
        };

    /// <summary>
    /// Sends a broadcast message and waits for first response. This is a no-operation implementation that logs and returns a timeout result.
    /// </summary>
    /// <param name="channelIds">The collection of channel IDs.</param>
    /// <param name="message">The message text.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A result indicating timeout.</returns>
    public Task<FirstResponderResult> SendAndAwaitFirstResponseAsync(IEnumerable<ulong> channelIds, string message, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] Broadcast message to {Count} channels. No responses will be awaited.", channelIds?.Count() ?? 0);
        return Task.FromResult(new FirstResponderResult { TimedOut = true });
    }

    /// <summary>
    /// Sends a broadcast embed and waits for first response. This is a no-operation implementation that logs and returns a timeout result.
    /// </summary>
    /// <param name="channelIds">The collection of channel IDs.</param>
    /// <param name="embed">The embed data.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A result indicating timeout.</returns>
    public Task<FirstResponderResult> SendEmbedAndAwaitFirstResponseAsync(IEnumerable<ulong> channelIds, EmbedData embed, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] Broadcast embed to {Count} channels. No responses will be awaited.", channelIds?.Count() ?? 0);
        return Task.FromResult(new FirstResponderResult { TimedOut = true });
    }

    /// <summary>
    /// Broadcasts a message with a response handler. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="channelIds">The collection of channel IDs.</param>
    /// <param name="message">The message text.</param>
    /// <param name="onResponse">The response handler function.</param>
    /// <param name="timeout">The timeout duration.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task BroadcastWithResponseHandlerAsync(IEnumerable<ulong> channelIds, string message, Func<ResponseEvent, Task<bool>> onResponse, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] BroadcastWithResponseHandlerAsync called. No responses will be produced.");
        return Task.CompletedTask;
    }

    // IMessagingService explicit members map to IChatBotService implementations
    Task IMessagingService.StartAsync(CancellationToken cancellationToken) => StartAsync(cancellationToken);
    Task IMessagingService.StopAsync(CancellationToken cancellationToken) => StopAsync(cancellationToken);
    Task IMessagingService.SendMessageAsync(ulong channelId, string message, CancellationToken cancellationToken) => SendMessageAsync(channelId, message, cancellationToken);
    Task IMessagingService.ReplyToMessageAsync(ulong messageId, ulong channelId, string reply, CancellationToken cancellationToken) => ReplyToMessageAsync(messageId, channelId, reply, cancellationToken);
    bool IMessagingService.IsConnected => IsConnected;

    /// <summary>
    /// Registers commands from an assembly. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="assembly">The assembly containing commands.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task RegisterCommandsAsync(Assembly assembly, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] RegisterCommandsAsync called for assembly {Assembly}", assembly.FullName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers commands to a server. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="serverId">The server ID.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task RegisterCommandsToServerAsync(ulong serverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] RegisterCommandsToServerAsync called for server {ServerId}", serverId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers commands globally. This is a no-operation implementation that logs and returns immediately.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public Task RegisterCommandsGloballyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[NoOp] RegisterCommandsGloballyAsync called.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a value indicating whether the bot command service is enabled. Always returns false for no-op implementation.
    /// </summary>
    public bool IsEnabled => false;

    /// <summary>
    /// Gets the count of registered modules. Always returns 0 for no-op implementation.
    /// </summary>
    public int RegisteredModuleCount => 0;
}
