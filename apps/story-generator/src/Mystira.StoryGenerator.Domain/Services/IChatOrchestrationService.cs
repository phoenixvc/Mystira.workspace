using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Service that orchestrates chat completion by coordinating intent classification,
/// command dispatch via MediatR, and response mapping
/// </summary>
public interface IChatOrchestrationService
{
    /// <summary>
    /// Complete a chat interaction by detecting intent and dispatching to appropriate handlers
    /// </summary>
    /// <param name="context">Chat context containing messages and configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Orchestration response with result or clarification prompt</returns>
    Task<ChatOrchestrationResponse> CompleteAsync(ChatContext context, CancellationToken cancellationToken);
}