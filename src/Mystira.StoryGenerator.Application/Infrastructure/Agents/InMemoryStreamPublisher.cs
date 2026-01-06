using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// In-memory implementation of stream publisher for development and testing.
/// Maintains observers and event history per session.
/// </summary>
public class InMemoryStreamPublisher : IAgentStreamPublisher
{
    private readonly ConcurrentDictionary<string, SessionChannel> _sessionChannels = new();
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

        // Publish to subscribers via channel
        if (_sessionChannels.TryGetValue(sessionId, out var channel))
        {
            await channel.Writer.WriteAsync(evt);
        }
    }

    /// <summary>
    /// Subscribe to events for a specific session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Async enumerable of events.</returns>
    public async IAsyncEnumerable<AgentStreamEvent> SubscribeAsync(
        string sessionId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Create or get the session channel
        var channel = _sessionChannels.GetOrAdd(sessionId, _ => 
            SessionChannel.Create());

        // First, yield all existing events (replay)
        if (_eventHistory.TryGetValue(sessionId, out var existingEvents))
        {
            foreach (var evt in existingEvents)
            {
                if (ct.IsCancellationRequested)
                    yield break;
                yield return evt;
            }
        }

        // Then stream new events
        var reader = channel.Reader;
        while (!ct.IsCancellationRequested)
        {
            if (await reader.WaitToReadAsync(ct))
            {
                if (reader.TryRead(out var evt))
                {
                    yield return evt;
                }
            }
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
    /// Complete all channels for a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    public void CompleteSession(string sessionId)
    {
        if (_sessionChannels.TryGetValue(sessionId, out var channel))
        {
            channel.Writer.Complete();
        }
    }

    private class SessionChannel
    {
        public Channel<AgentStreamEvent> Channel { get; }
        public ChannelWriter<AgentStreamEvent> Writer => Channel.Writer;
        public ChannelReader<AgentStreamEvent> Reader => Channel.Reader;

        private SessionChannel(Channel<AgentStreamEvent> channel)
        {
            Channel = channel;
        }

        public static SessionChannel Create()
        {
            var channel = Channel.CreateUnbounded<AgentStreamEvent>(
                new UnboundedChannelOptions
                {
                    SingleReader = false,
                    SingleWriter = false
                });
            return new SessionChannel(channel);
        }
    }
}