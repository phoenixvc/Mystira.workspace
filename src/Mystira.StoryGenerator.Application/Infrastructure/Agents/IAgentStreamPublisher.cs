using Mystira.StoryGenerator.Application.Infrastructure.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Interface for publishing agent orchestration events to subscribers.
/// </summary>
public interface IAgentStreamPublisher
{
    /// <summary>
    /// Publish an event for a specific session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="evt">The event to publish.</param>
    Task PublishEventAsync(string sessionId, AgentStreamEvent evt);
}