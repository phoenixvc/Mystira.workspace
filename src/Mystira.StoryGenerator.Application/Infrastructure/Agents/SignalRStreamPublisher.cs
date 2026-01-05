using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// SignalR-based stream publisher for production environments.
/// Broadcasts events to all clients subscribed to session groups.
/// </summary>
public class SignalRStreamPublisher : IAgentStreamPublisher
{
    private readonly IHubContext<SessionHub> _hubContext;
    private readonly ILogger<SignalRStreamPublisher> _logger;

    public SignalRStreamPublisher(IHubContext<SessionHub> hubContext, ILogger<SignalRStreamPublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishEventAsync(string sessionId, AgentStreamEvent evt)
    {
        try
        {
            // Broadcast to all clients in the session group
            await _hubContext.Clients.Group(sessionId).SendAsync("AgentEvent", new
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
}

/// <summary>
/// SignalR Hub for agent session events.
/// </summary>
public class SessionHub : Hub
{
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        await Clients.Caller.SendAsync("JoinedSession", sessionId);
    }

    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        await Clients.Caller.SendAsync("LeftSession", sessionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any session memberships if needed
        await base.OnDisconnectedAsync(exception);
    }
}