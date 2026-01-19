using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading.Channels;
using System.Runtime.CompilerServices;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Web.Services;

/// <summary>
/// Service for handling SSE connections using JavaScript interop.
/// Required for Blazor WASM since HttpClient streaming is not supported.
/// </summary>
public class SseJsInteropService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<SseJsInteropService> _logger;
    private readonly Dictionary<string, DotNetObjectReference<SseEventHandler>> _handlers = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public SseJsInteropService(IJSRuntime jsRuntime, ILogger<SseJsInteropService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IAsyncEnumerable<AgentStreamEvent>> ConnectAsync(string sessionId, string baseUrl, CancellationToken cancellationToken = default)
    {
        var url = $"{baseUrl}/api/story-agent/sessions/{sessionId}/stream";
        _logger.LogInformation("Connecting to SSE stream via JavaScript: {Url}", url);

        var handler = new SseEventHandler(_logger, _jsonOptions);
        var dotNetRef = DotNetObjectReference.Create(handler);
        _handlers[sessionId] = dotNetRef;

        try
        {
            await _jsRuntime.InvokeVoidAsync("sseClient.connect", cancellationToken, sessionId, url, dotNetRef);
            _logger.LogInformation("SSE connection established for session {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish SSE connection for session {SessionId}", sessionId);
            throw;
        }

        return handler.GetEventsAsync(cancellationToken);
    }

    public async Task DisconnectAsync(string sessionId)
    {
        _logger.LogInformation("Disconnecting SSE stream for session {SessionId}", sessionId);

        try
        {
            await _jsRuntime.InvokeVoidAsync("sseClient.disconnect", sessionId);
            
            if (_handlers.TryGetValue(sessionId, out var handler))
            {
                handler.Dispose();
                _handlers.Remove(sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting SSE stream for session {SessionId}", sessionId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sseClient.disconnectAll");
        }
        catch
        {
            // Ignore errors during disposal
        }

        foreach (var handler in _handlers.Values)
        {
            handler.Dispose();
        }
        _handlers.Clear();
    }
}

/// <summary>
/// Handler for SSE events received from JavaScript.
/// </summary>
public class SseEventHandler
{
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Channel<AgentStreamEvent> _channel;

    public SseEventHandler(ILogger logger, JsonSerializerOptions jsonOptions)
    {
        _logger = logger;
        _jsonOptions = jsonOptions;
        _channel = Channel.CreateUnbounded<AgentStreamEvent>();
    }

    [JSInvokable]
    public Task OnSseEvent(string eventType, string eventData)
    {
        try
        {
            _logger.LogInformation("[SSE-HANDLER] Received event: {EventType}", eventType);
            Console.WriteLine($"[SSE-HANDLER] Received event: {eventType}, Data: {eventData}");

            var evt = JsonSerializer.Deserialize<AgentStreamEvent>(eventData, _jsonOptions);
            if (evt != null)
            {
                // Parse the event type from the SSE event field
                if (Enum.TryParse<AgentStreamEvent.EventType>(eventType, out var parsedType))
                {
                    evt.Type = parsedType;
                }

                if (evt.Type == AgentStreamEvent.EventType.StreamingUpdate)
                {
                    Console.WriteLine($"[SSE-HANDLER] StreamingUpdate - Message: '{evt.Message}', Length: {evt.Message?.Length ?? 0}");
                }

                _channel.Writer.TryWrite(evt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SSE-HANDLER] Error processing SSE event");
            Console.WriteLine($"[SSE-HANDLER] Error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnSseError(string error)
    {
        _logger.LogError("[SSE-HANDLER] SSE connection error: {Error}", error);
        Console.WriteLine($"[SSE-HANDLER] Connection error: {error}");
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<AgentStreamEvent> GetEventsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var evt in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return evt;
        }
    }
}
