using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Mystira.App.Admin.Api.Hubs;

/// <summary>
/// SignalR hub for real-time event broadcasting to admin UI clients.
/// Replaces polling mechanisms for better performance and user experience.
/// </summary>
[Authorize]
public class EventsHub : Hub
{
    private readonly ILogger<EventsHub> _logger;

    public EventsHub(ILogger<EventsHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                     ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                     ?? "anonymous";
        
        _logger.LogInformation(
            "Client connected to EventsHub: ConnectionId={ConnectionId}, UserId={UserId}",
            Context.ConnectionId,
            userId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value 
                     ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                     ?? "anonymous";
        
        if (exception != null)
        {
            _logger.LogWarning(
                exception,
                "Client disconnected with error: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                userId);
        }
        else
        {
            _logger.LogInformation(
                "Client disconnected: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client can join a specific group (e.g., for scenario-specific updates).
    /// </summary>
    /// <param name="groupName">The name of the group to join</param>
    public async Task JoinGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new HubException("Group name cannot be empty");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation(
            "Client joined group: ConnectionId={ConnectionId}, Group={GroupName}",
            Context.ConnectionId,
            groupName);
    }

    /// <summary>
    /// Client can leave a specific group.
    /// </summary>
    /// <param name="groupName">The name of the group to leave</param>
    public async Task LeaveGroup(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new HubException("Group name cannot be empty");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation(
            "Client left group: ConnectionId={ConnectionId}, Group={GroupName}",
            Context.ConnectionId,
            groupName);
    }

    /// <summary>
    /// Ping endpoint for connection keep-alive testing.
    /// </summary>
    /// <returns>Pong response with timestamp</returns>
    public Task<object> Ping()
    {
        return Task.FromResult<object>(new
        {
            Message = "Pong",
            Timestamp = DateTimeOffset.UtcNow,
            ConnectionId = Context.ConnectionId
        });
    }
}
