using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Communication.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Application.Ports.Messaging;
using Mystira.Infrastructure.WhatsApp.Configuration;

namespace Mystira.Infrastructure.WhatsApp.Services;

/// <summary>
/// Implementation of chat bot service using Azure Communication Services for WhatsApp.
/// Implements the Application port interfaces for clean architecture compliance.
/// FIX: Added IMessagingService for consistency with DiscordBotService.
///
/// Note: WhatsApp has specific limitations:
/// - Messages can only be sent to users who have initiated a conversation (24-hour window)
/// - Outside the 24-hour window, only template messages can be sent
/// - No concept of "channels" like Discord/Teams - messages are to phone numbers
/// </summary>
public class WhatsAppBotService : IMessagingService, IChatBotService, IBotCommandService, IDisposable
{
    private readonly ILogger<WhatsAppBotService> _logger;
    private readonly WhatsAppOptions _options;
    private NotificationMessagesClient? _client;
    private bool _disposed;
    private bool _isConnected;

    // Track active conversations (phone numbers that have messaged us recently)
    // FIX: Use ConcurrentDictionary for thread safety (Phase 2)
    private readonly ConcurrentDictionary<ulong, string> _activeConversations = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WhatsAppBotService"/> class.
    /// </summary>
    /// <param name="options">The WhatsApp configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public WhatsAppBotService(
        IOptions<WhatsAppOptions> options,
        ILogger<WhatsAppBotService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gets a value indicating whether the service is connected and ready.
    /// </summary>
    public bool IsConnected => _isConnected && _client != null;

    /// <summary>
    /// Gets a value indicating whether the service is enabled in configuration.
    /// </summary>
    public bool IsEnabled => _options.Enabled;

    /// <summary>
    /// Gets the platform identifier for this service.
    /// </summary>
    public ChatPlatform Platform => ChatPlatform.WhatsApp;

    /// <summary>
    /// Gets the count of registered command modules.
    /// </summary>
    public int RegisteredModuleCount => 0; // WhatsApp doesn't have command modules like Discord

    /// <summary>
    /// Starts the WhatsApp bot service and initializes the Azure Communication Services client.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("WhatsApp integration is disabled in configuration");
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("WhatsApp ConnectionString is not configured. Set WhatsApp:ConnectionString in configuration.");
        }

        if (string.IsNullOrWhiteSpace(_options.ChannelRegistrationId))
        {
            throw new InvalidOperationException("WhatsApp ChannelRegistrationId is not configured. Set WhatsApp:ChannelRegistrationId in configuration.");
        }

        try
        {
            _client = new NotificationMessagesClient(_options.ConnectionString);
            _isConnected = true;
            _logger.LogInformation("WhatsApp bot service started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize WhatsApp client");
            throw;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the WhatsApp bot service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsApp bot service stopped");
        _isConnected = false;
        _client = null;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a text message to the specified WhatsApp phone number.
    /// </summary>
    /// <param name="channelId">The channel identifier (phone number hash).</param>
    /// <param name="message">The message text to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendMessageAsync(ulong channelId, string message, CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _client == null)
        {
            throw new InvalidOperationException("WhatsApp bot is not connected");
        }

        var phoneNumber = GetPhoneNumberFromChannelId(channelId);
        if (string.IsNullOrEmpty(phoneNumber))
        {
            throw new InvalidOperationException($"No phone number found for channel ID {channelId}. WhatsApp requires a registered conversation.");
        }

        // Azure.Communication.Messages 1.1.0+ requires Guid for channel registration ID
        if (!Guid.TryParse(_options.ChannelRegistrationId, out var channelGuid))
        {
            throw new InvalidOperationException($"Invalid channel registration ID format: {_options.ChannelRegistrationId}");
        }

        var textContent = new TextNotificationContent(
            channelGuid,
            new[] { phoneNumber },
            message);

        // FIX: Add retry logic for transient failures (Phase 6)
        var attempt = 0;
        var maxAttempts = _options.MaxRetryAttempts > 0 ? _options.MaxRetryAttempts : 3;

        while (true)
        {
            attempt++;
            try
            {
                var result = await _client.SendAsync(textContent, cancellationToken);

                _logger.LogDebug("Sent WhatsApp message to {PhoneNumber}, MessageId: {MessageId}",
                    phoneNumber, result.Value.Receipts.FirstOrDefault()?.MessageId);
                return;
            }
            catch (RequestFailedException ex) when (IsTransientError(ex) && attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // Exponential backoff
                _logger.LogWarning(ex, "Transient error sending WhatsApp message (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}s",
                    attempt, maxAttempts, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Azure Communication Services error sending WhatsApp message: {Code}", ex.ErrorCode);
                throw new InvalidOperationException($"WhatsApp API error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending WhatsApp message");
                throw;
            }
        }
    }

    /// <summary>
    /// Determine if a RequestFailedException is transient and can be retried.
    /// </summary>
    private static bool IsTransientError(RequestFailedException ex)
    {
        // HTTP 429 (Too Many Requests), 5xx errors are typically transient
        return ex.Status == 429 || (ex.Status >= 500 && ex.Status < 600);
    }

    /// <summary>
    /// Sends an embed as formatted text to the specified WhatsApp phone number.
    /// Note: WhatsApp does not support rich embeds; content is converted to formatted text.
    /// </summary>
    /// <param name="channelId">The channel identifier (phone number hash).</param>
    /// <param name="embed">The embed data to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendEmbedAsync(ulong channelId, EmbedData embed, CancellationToken cancellationToken = default)
    {
        // WhatsApp doesn't support rich embeds like Discord
        // FIX: Add warning for embed color being ignored (Phase 4)
        if (embed.ColorRed != 0 || embed.ColorGreen != 0 || embed.ColorBlue != 0)
        {
            _logger.LogDebug("WhatsApp does not support embed colors. Color information will be ignored.");
        }

        // Convert to formatted text message
        var messageText = FormatEmbedAsText(embed);
        await SendMessageAsync(channelId, messageText, cancellationToken);
    }

    /// <summary>
    /// Sends a reply to a specific message in WhatsApp.
    /// Note: WhatsApp via ACS doesn't support threaded replies; sends as regular message.
    /// </summary>
    /// <param name="messageId">The message identifier to reply to.</param>
    /// <param name="channelId">The channel identifier (phone number hash).</param>
    /// <param name="reply">The reply text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ReplyToMessageAsync(ulong messageId, ulong channelId, string reply, CancellationToken cancellationToken = default)
    {
        // WhatsApp via ACS doesn't support threaded replies in the same way
        // Just send a regular message
        await SendMessageAsync(channelId, reply, cancellationToken);
    }

    /// <summary>
    /// Gets the current status of the WhatsApp bot service.
    /// </summary>
    /// <returns>The bot status information.</returns>
    public BotStatus GetStatus()
    {
        return new BotStatus
        {
            IsEnabled = _options.Enabled,
            IsConnected = IsConnected,
            BotName = "WhatsApp Bot",
            BotId = null,
            ServerCount = _activeConversations.Count // Number of active conversations
        };
    }

    // ─────────────────────────────────────────────────────────────────
    // Broadcast / First Responder (Limited support for WhatsApp)
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sends a message to multiple phone numbers and awaits the first response.
    /// Note: WhatsApp does not support real-time response monitoring; use webhooks instead.
    /// </summary>
    /// <param name="channelIds">The channel identifiers (phone number hashes) to broadcast to.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="timeout">The timeout duration for waiting for responses.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first responder result (always times out for WhatsApp).</returns>
    public async Task<FirstResponderResult> SendAndAwaitFirstResponseAsync(
        IEnumerable<ulong> channelIds,
        string message,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var sentMessages = new List<SentMessage>();

        foreach (var channelId in channelIds)
        {
            try
            {
                await SendMessageAsync(channelId, message, cancellationToken);
                sentMessages.Add(new SentMessage { ChannelId = channelId, MessageId = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send broadcast to WhatsApp number {ChannelId}", channelId);
            }
        }

        _logger.LogWarning("WhatsApp does not support real-time response monitoring via this method. Use webhooks instead.");

        return new FirstResponderResult
        {
            TimedOut = true,
            SentMessages = sentMessages
        };
    }

    /// <summary>
    /// Sends an embed to multiple phone numbers and awaits the first response.
    /// Note: WhatsApp does not support real-time response monitoring; use webhooks instead.
    /// </summary>
    /// <param name="channelIds">The channel identifiers (phone number hashes) to broadcast to.</param>
    /// <param name="embed">The embed data to send.</param>
    /// <param name="timeout">The timeout duration for waiting for responses.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first responder result (always times out for WhatsApp).</returns>
    public async Task<FirstResponderResult> SendEmbedAndAwaitFirstResponseAsync(
        IEnumerable<ulong> channelIds,
        EmbedData embed,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var sentMessages = new List<SentMessage>();

        foreach (var channelId in channelIds)
        {
            try
            {
                await SendEmbedAsync(channelId, embed, cancellationToken);
                sentMessages.Add(new SentMessage { ChannelId = channelId, MessageId = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send broadcast embed to WhatsApp number {ChannelId}", channelId);
            }
        }

        return new FirstResponderResult
        {
            TimedOut = true,
            SentMessages = sentMessages
        };
    }

    /// <summary>
    /// Broadcasts a message to multiple phone numbers with a custom response handler.
    /// Note: WhatsApp does not support real-time response monitoring via this method.
    /// </summary>
    /// <param name="channelIds">The channel identifiers (phone number hashes) to broadcast to.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="onResponse">Callback function for handling responses.</param>
    /// <param name="timeout">The timeout duration for waiting for responses.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task BroadcastWithResponseHandlerAsync(
        IEnumerable<ulong> channelIds,
        string message,
        Func<ResponseEvent, Task<bool>> onResponse,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        foreach (var channelId in channelIds)
        {
            try
            {
                await SendMessageAsync(channelId, message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send broadcast to WhatsApp number {ChannelId}", channelId);
            }
        }

        _logger.LogWarning("WhatsApp does not support real-time response monitoring via this method.");
    }

    // ─────────────────────────────────────────────────────────────────
    // IBotCommandService Implementation
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers commands from the specified assembly.
    /// Note: WhatsApp does not support slash commands; handle commands via message parsing.
    /// </summary>
    /// <param name="assembly">The assembly containing command modules.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RegisterCommandsAsync(Assembly assembly, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsApp does not support slash commands. Handle commands via message parsing.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers commands to a specific server.
    /// Note: WhatsApp does not support server-specific command registration.
    /// </summary>
    /// <param name="serverId">The server identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RegisterCommandsToServerAsync(ulong serverId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("WhatsApp does not support server-specific command registration");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers commands globally across all servers.
    /// Note: WhatsApp commands should be handled via message content parsing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task RegisterCommandsGloballyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("WhatsApp commands should be handled via message content parsing");
        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────
    // WhatsApp-specific methods
    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Register an active conversation from an incoming webhook message.
    /// Call this when receiving a message to enable proactive messaging.
    /// </summary>
    /// <param name="phoneNumber">The phone number that sent the message (with country code)</param>
    /// <returns>A channel ID that can be used for sending messages</returns>
    public ulong RegisterConversation(string phoneNumber)
    {
        var channelId = GetChannelIdFromPhoneNumber(phoneNumber);
        _activeConversations[channelId] = phoneNumber;
        _logger.LogDebug("Registered WhatsApp conversation: {PhoneNumber} -> {ChannelId}", phoneNumber, channelId);
        return channelId;
    }

    /// <summary>
    /// Get a channel ID from a phone number.
    /// FIX: Use SHA256 for deterministic, collision-resistant ID generation (Phase 2)
    /// </summary>
    public static ulong GetChannelIdFromPhoneNumber(string phoneNumber)
    {
        // Create a consistent hash from the phone number
        var normalized = phoneNumber.Replace("+", "").Replace(" ", "").Replace("-", "");

        // Try to parse as numeric first (most phone numbers are all digits)
        if (ulong.TryParse(normalized, out var result))
        {
            return result;
        }

        // Fall back to SHA256 hash for non-numeric formats
        // FIX: Use SHA256 instead of GetHashCode() to avoid negative values and collisions
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>
    /// Send a template message (required for messages outside 24-hour window).
    /// </summary>
    public async Task SendTemplateMessageAsync(
        string phoneNumber,
        string templateName,
        string templateLanguage = "en",
        IEnumerable<string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _client == null)
        {
            throw new InvalidOperationException("WhatsApp bot is not connected");
        }

        try
        {
            // Azure.Communication.Messages 1.1.0+ requires Guid for channel registration ID
            if (!Guid.TryParse(_options.ChannelRegistrationId, out var channelGuid))
            {
                throw new InvalidOperationException($"Invalid channel registration ID format: {_options.ChannelRegistrationId}");
            }

            // In version 1.1.0+, template API has changed
            var template = new MessageTemplate(templateName, templateLanguage == "en" ? "en_US" : templateLanguage);

            // Update: Template parameters handling for Azure SDK 1.1.0+
            if (parameters != null && parameters.Any())
            {
                foreach (var param in parameters)
                {
                    // Azure SDK 1.1.0+ uses MessageTemplateText for text parameters
                    template.Values.Add(new MessageTemplateText("body", param));
                }
            }

            var templateContent = new TemplateNotificationContent(
                channelGuid,
                new[] { phoneNumber },
                template);

            await _client.SendAsync(templateContent, cancellationToken);

            _logger.LogDebug("Sent WhatsApp template message to {PhoneNumber}", phoneNumber);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Communication Services error sending WhatsApp template: {Code}", ex.ErrorCode);
            throw new InvalidOperationException($"WhatsApp API error: {ex.Message}", ex);
        }
    }

    private string? GetPhoneNumberFromChannelId(ulong channelId)
    {
        return _activeConversations.TryGetValue(channelId, out var phoneNumber)
            ? phoneNumber
            : null;
    }

    private static string FormatEmbedAsText(EmbedData embed)
    {
        var lines = new List<string>();

        if (!string.IsNullOrEmpty(embed.Title))
        {
            lines.Add($"*{embed.Title}*");
        }

        if (!string.IsNullOrEmpty(embed.Description))
        {
            lines.Add(embed.Description);
        }

        if (embed.Fields != null)
        {
            lines.Add("");
            foreach (var field in embed.Fields)
            {
                lines.Add($"*{field.Name}*");
                lines.Add(field.Value);
                lines.Add("");
            }
        }

        if (!string.IsNullOrEmpty(embed.Footer))
        {
            lines.Add($"_{embed.Footer}_");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Disposes the WhatsApp bot service and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _isConnected = false;
        _activeConversations.Clear();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
