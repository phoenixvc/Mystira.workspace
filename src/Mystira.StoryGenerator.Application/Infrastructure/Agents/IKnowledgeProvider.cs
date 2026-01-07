using Azure.AI.Projects;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Defines the interface for knowledge providers that can be used with Azure AI Foundry agents.
/// Different implementations use different knowledge sources (File Search, AI Search, etc.)
/// </summary>
public interface IKnowledgeProvider
{
    /// <summary>
    /// Gets the name of this knowledge provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Attaches the knowledge source to an agent.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tool definition for the agent.</returns>
    Task<ToolDefinition> AttachToAgentAsync(string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attaches the knowledge source to a thread.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    /// <param name="ageGroup">Optional age group for filtering knowledge sources.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tool definition for the thread.</returns>
    Task<ToolDefinition> AttachToThreadAsync(string threadId, string? ageGroup = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the tool definition for this knowledge provider.
    /// </summary>
    /// <param name="ageGroup">Optional age group for age-specific tool configuration.</param>
    /// <returns>The tool definition.</returns>
    ToolDefinition GetToolDefinition(string? ageGroup = null);

    /// <summary>
    /// Returns prompt-ready guidance on how the agent should use the configured knowledge source.
    /// </summary>
    /// <param name="ageGroup">Optional age group for age-specific guidance.</param>
    string GetContextualGuidance(string? ageGroup = null);
}
