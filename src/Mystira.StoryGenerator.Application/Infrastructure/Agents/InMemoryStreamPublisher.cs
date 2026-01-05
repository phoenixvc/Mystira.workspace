using System.Collections.Concurrent;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// In-memory implementation of stream publisher for development and testing.
/// Maintains observers and event history per session.
/// </summary>
public class InMemoryStreamPublisher : IAgentStreamPublisher
{
    private readonly ConcurrentDictionary<string, SessionObservers> _sessionObservers = new();
    private readonly ConcurrentDictionary<string, List<AgentStreamEvent>> _eventHistory = new();
    private const int MaxEventsPerSession = 100;

    /// <summary>
    /// Publish an event to all observers of the specified session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="evt">The event to publish.</param>
    public async Task PublishEventAsync(string sessionId, AgentStreamEvent evt)
    {
        // Add to event history
        _eventHistory.AddOrUpdate(sessionId, 
            new List<AgentStreamEvent> { evt },
            (key, existing) => 
            {
                existing.Add(evt);
                // Keep only the last MaxEventsPerSession events
                if (existing.Count > MaxEventsPerSession)
                {
                    existing.RemoveRange(0, existing.Count - MaxEventsPerSession);
                }
                return existing;
            });

        // Publish to observers
        if (_sessionObservers.TryGetValue(sessionId, out var observers))
        {
            var tasks = observers.Observers.Select(observer => 
                SafeInvokeObserver(observer, evt)).ToArray();
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Get the event history for a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>List of events for the session.</returns>
    public List<AgentStreamEvent> GetEventHistory(string sessionId)
    {
        return _eventHistory.TryGetValue(sessionId, out var events) 
            ? new List<AgentStreamEvent>(events) 
            : new List<AgentStreamEvent>();
    }

    /// <summary>
    /// Clear the event history for a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    public void ClearEventHistory(string sessionId)
    {
        _eventHistory.TryRemove(sessionId, out _);
    }

    /// <summary>
    /// Add an observer for a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="observer">The observer to add.</param>
    public void AddObserver(string sessionId, IAsyncObserver<AgentStreamEvent> observer)
    {
        _sessionObservers.AddOrUpdate(sessionId,
            new SessionObservers { Observers = new List<IAsyncObserver<AgentStreamEvent>> { observer } },
            (key, existing) =>
            {
                existing.Observers.Add(observer);
                return existing;
            });
    }

    /// <summary>
    /// Remove an observer from a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="observer">The observer to remove.</param>
    public void RemoveObserver(string sessionId, IAsyncObserver<AgentStreamEvent> observer)
    {
        if (_sessionObservers.TryGetValue(sessionId, out var observers))
        {
            observers.Observers.Remove(observer);
            
            // Clean up if no observers left
            if (!observers.Observers.Any())
            {
                _sessionObservers.TryRemove(sessionId, out _);
            }
        }
    }

    private static async Task SafeInvokeObserver(IAsyncObserver<AgentStreamEvent> observer, AgentStreamEvent evt)
    {
        try
        {
            await observer.OnNextAsync(evt);
        }
        catch (Exception)
        {
            // Observer exceptions should not break the publishing flow
            // In production, this would be logged
        }
    }

    private class SessionObservers
    {
        public List<IAsyncObserver<AgentStreamEvent>> Observers { get; set; } = new();
    }
}