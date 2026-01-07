using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using System.Runtime.CompilerServices;

namespace Mystira.StoryGenerator.Api.Infrastructure.Agents;

/// <summary>
/// SignalR-based stream publisher for production environments.
/// Broadcasts events to all clients subscribed to session groups.
/// </summary>
public class SignalRStreamPublisher : IAgentStreamPublisher
{
    private readonly IHubContext<AgentStreamHub> _hubContext;
    private readonly ILogger<SignalRStreamPublisher> _logger;

    public SignalRStreamPublisher(IHubContext<AgentStreamHub> hubContext, ILogger<SignalRStreamPublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishEventAsync(string sessionId, AgentStreamEvent evt)
    {
        try
        {
            // Broadcast to all clients in the session group
            await _hubContext.Clients.Group($"session-{sessionId}").SendAsync("AgentEvent", new
            {
                SessionId = sessionId,
                EventType = evt.Type.ToString(),
                Phase = evt.Phase,
                Payload = evt.Payload,
                Timestamp = evt.Timestamp,
                IterationNumber = evt.IterationNumber
            });

            _logger.LogDebug("Published event {EventType} for session {SessionId}", evt.Type, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event for session {SessionId}", sessionId);
            throw;
        }
    }

    public async IAsyncEnumerable<AgentStreamEvent> SubscribeAsync(
        string sessionId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // SignalR is push-based, so this method is not used for SignalR implementation
        // Clients connect via SignalR hub methods instead
        await Task.CompletedTask;
        yield break;
    }
}

/// <summary>
/// SignalR Hub for agent session events.
/// </summary>
public class AgentStreamHub : Hub
{
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        await Clients.Caller.SendAsync("JoinedSession", sessionId);
    }

    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session-{sessionId}");
        await Clients.Caller.SendAsync("LeftSession", sessionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any session memberships if needed
        await base.OnDisconnectedAsync(exception);
    }
}
