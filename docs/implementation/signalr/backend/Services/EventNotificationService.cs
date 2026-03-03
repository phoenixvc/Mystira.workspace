using Microsoft.AspNetCore.SignalR;
using Mystira.App.Admin.Api.Hubs;

namespace Mystira.App.Admin.Api.Services;

/// <summary>
/// Service for broadcasting events to SignalR clients.
/// Use this instead of polling endpoints for real-time updates.
/// </summary>
public interface IEventNotificationService
{
    /// <summary>
    /// Notify clients about a scenario update.
    /// </summary>
    Task NotifyScenarioUpdatedAsync(string scenarioId, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Notify clients about published content.
    /// </summary>
    Task NotifyContentPublishedAsync(string contentId, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Notify clients about user activity.
    /// </summary>
    Task NotifyUserActivityAsync(string userId, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a generic event to all connected clients.
    /// </summary>
    Task NotifyAllAsync(string eventType, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a generic event to a specific group of clients.
    /// </summary>
    Task NotifyGroupAsync(string groupName, string eventType, object data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send an event to a specific user (all their connections).
    /// </summary>
    Task NotifyUserAsync(string userId, string eventType, object data, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of event notification service using SignalR.
/// </summary>
public class EventNotificationService : IEventNotificationService
{
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly ILogger<EventNotificationService> _logger;

    public EventNotificationService(
        IHubContext<EventsHub> hubContext,
        ILogger<EventNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyScenarioUpdatedAsync(string scenarioId, object data, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Broadcasting scenario update: {ScenarioId}", scenarioId);
        
        try
        {
            await _hubContext.Clients.All.SendAsync(
                "ScenarioUpdated",
                new
                {
                    scenarioId,
                    data,
                    timestamp = DateTimeOffset.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast scenario update for {ScenarioId}", scenarioId);
            throw;
        }
    }

    public async Task NotifyContentPublishedAsync(string contentId, object data, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Broadcasting content published: {ContentId}", contentId);
        
        try
        {
            await _hubContext.Clients.All.SendAsync(
                "ContentPublished",
                new
                {
                    contentId,
                    data,
                    timestamp = DateTimeOffset.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast content published for {ContentId}", contentId);
            throw;
        }
    }

    public async Task NotifyUserActivityAsync(string userId, object data, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Broadcasting user activity: {UserId}", userId);
        
        try
        {
            await _hubContext.Clients.All.SendAsync(
                "UserActivity",
                new
                {
                    userId,
                    data,
                    timestamp = DateTimeOffset.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast user activity for {UserId}", userId);
            throw;
        }
    }

    public async Task NotifyAllAsync(string eventType, object data, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Broadcasting event to all clients: {EventType}", eventType);
        
        try
        {
            await _hubContext.Clients.All.SendAsync(
                eventType,
                new
                {
                    data,
                    timestamp = DateTimeOffset.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast event {EventType}", eventType);
            throw;
        }
    }

    public async Task NotifyGroupAsync(string groupName, string eventType, object data, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Broadcasting event to group: Group={GroupName}, EventType={EventType}",
            groupName,
            eventType);
        
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync(
                eventType,
                new
                {
                    data,
                    timestamp = DateTimeOffset.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast event {EventType} to group {GroupName}", eventType, groupName);
            throw;
        }
    }

    public async Task NotifyUserAsync(string userId, string eventType, object data, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Broadcasting event to user: UserId={UserId}, EventType={EventType}",
            userId,
            eventType);
        
        try
        {
            await _hubContext.Clients.User(userId).SendAsync(
                eventType,
                new
                {
                    data,
                    timestamp = DateTimeOffset.UtcNow
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast event {EventType} to user {UserId}", eventType, userId);
            throw;
        }
    }
}
