using System.Runtime.CompilerServices;
using Mystira.StoryGenerator.Api.Models;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Contracts.Models;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Web.Services;

/// <summary>
/// Service for interacting with the Agent-based story generation API.
/// </summary>
public interface IAgentSessionService
{
    /// <summary>
    /// Start a new story generation session.
    /// </summary>
    Task<SessionStartResponse> StartSessionAsync(StartSessionRequest request);

    /// <summary>
    /// Get the current state of a session.
    /// </summary>
    Task<SessionStateResponse> GetSessionAsync(string sessionId);

    /// <summary>
    /// Evaluate the current story in a session.
    /// </summary>
    Task<EvaluateResponse> EvaluateAsync(string sessionId);

    /// <summary>
    /// Refine the story based on user feedback.
    /// </summary>
    Task<RefineResponse> RefineAsync(string sessionId, RefineRequest request);

    /// <summary>
    /// Generate a rubric for the current story.
    /// </summary>
    Task<SessionStateResponse> GenerateRubricAsync(string sessionId);

    /// <summary>
    /// Complete the session.
    /// </summary>
    Task<SessionStateResponse> CompleteAsync(string sessionId);

    /// <summary>
    /// Subscribe to real-time event stream for a session.
    /// </summary>
    IAsyncEnumerable<AgentStreamEvent> SubscribeToStreamAsync(string sessionId, CancellationToken cancellationToken = default);
}
