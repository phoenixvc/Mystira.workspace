using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service that orchestrates chat completion by coordinating intent classification,
/// command dispatch, and response mapping.
/// </summary>
public interface IChatOrchestrationService
{
    /// <summary>
    /// Complete a chat interaction by detecting intent and dispatching to appropriate handlers.
    /// </summary>
    /// <param name="context">Chat context containing messages and configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Orchestration response with result or clarification prompt.</returns>
    Task<ChatOrchestrationResponse> CompleteAsync(AuthoringContext context, CancellationToken cancellationToken = default);
}
