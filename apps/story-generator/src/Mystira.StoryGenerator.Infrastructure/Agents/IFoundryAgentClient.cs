using Azure.AI.Agents.Persistent;

namespace Mystira.StoryGenerator.Infrastructure.Agents;

/// <summary>
/// Interface for the Azure AI Foundry Agent Client.
/// Provides methods for thread management, run execution, and message retrieval.
/// </summary>
public interface IFoundryAgentClient : IDisposable
{
    /// <summary>
    /// Creates a new thread for conversation with an agent.
    /// </summary>
    Task<ThreadCreationResult> CreateThreadAsync(string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new thread with vector store resources for FileSearch.
    /// </summary>
    Task<ThreadCreationResult> CreateThreadWithVectorStoresAsync(string agentId, IEnumerable<string> vectorStoreIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an existing thread.
    /// </summary>
    Task<PersistentAgentThread?> GetThreadAsync(string threadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves messages from a thread.
    /// </summary>
    Task<List<Message>> RetrieveThreadMessagesAsync(string threadId, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits tool outputs for a run that requires action.
    /// </summary>
    Task<RunSubmissionResult> SubmitToolOutputsAsync(string threadId, string runId, List<FoundryToolOutput> toolOutputs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits for a run to complete.
    /// </summary>
    Task<RunCompletionResult> WaitForRunCompletionAsync(string threadId, string runId, TimeSpan? pollInterval = null, TimeSpan? maxWait = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and immediately starts a run on a thread with an agent.
    /// </summary>
    Task<RunSubmissionResult> CreateRunAsync(string threadId, string agentId, string instructions, BinaryData? responseFormat = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates and starts a streaming run.
    /// </summary>
    IAsyncEnumerable<StreamingUpdate> StreamRunAsync(string threadId, string agentId, string instructions, BinaryData? responseFormat = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a thread.
    /// </summary>
    Task<bool> DeleteThreadAsync(string threadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the agents client for direct access.
    /// </summary>
    PersistentAgentsClient GetAgentsClient();

    /// <summary>
    /// Gets the project client for direct access.
    /// </summary>
    Azure.AI.Projects.AIProjectClient GetProjectClient();
}
