using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Interface for orchestrating the stateful story generation loop.
/// Manages coordination between writer-agent, validation, judge-agent, and refiner-agent flows.
/// </summary>
public interface IAgentOrchestrator
{
    /// <summary>
    /// Initialize a new story generation session with Azure AI Foundry thread creation.
    /// </summary>
    /// <param name="sessionId">Unique session identifier.</param>
    /// <param name="knowledgeMode">Knowledge retrieval mode (FileSearch or AISearch).</param>
    /// <param name="ageGroup">Target age group for the story.</param>
    /// <returns>The initialized StorySession.</returns>
    Task<StorySession> InitializeSessionAsync(string sessionId, string knowledgeMode, string ageGroup);

    /// <summary>
    /// Generate a story using the writer-agent in the pipeline.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="storyPrompt">User's story prompt.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success status and message.</returns>
    Task<(bool Success, string Message)> GenerateStoryAsync(string sessionId, string storyPrompt, CancellationToken ct);

    /// <summary>
    /// Evaluate the current story using deterministic gates and judge-agent.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success status and evaluation report.</returns>
    Task<(bool Success, EvaluationReport Report)> EvaluateStoryAsync(string sessionId, CancellationToken ct);

    /// <summary>
    /// Refine the story based on user focus areas and evaluation feedback.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="focus">User's refinement focus areas.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success status and message.</returns>
    Task<(bool Success, string Message)> RefineStoryAsync(string sessionId, UserRefinementFocus focus, CancellationToken ct);

    /// <summary>
    /// Generate a user-friendly rubric summary for the current story.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success status and rubric summary.</returns>
    Task<(bool Success, RubricSummary? Rubric)> GenerateRubricAsync(string sessionId, CancellationToken ct);

    /// <summary>
    /// Get the current state of a story session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>The StorySession or null if not found.</returns>
    Task<StorySession?> GetSessionAsync(string sessionId);
}