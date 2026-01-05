using System.Text.Json;
using Azure.AI.Projects;
using Azure.Core;
using Microsoft.Extensions.Logging;

namespace Mystira.StoryGenerator.Infrastructure.Agents;

/// <summary>
/// Result of a thread creation operation.
/// </summary>
public class ThreadCreationResult
{
    public string ThreadId { get; set; } = string.Empty;
    public string? AssistantId { get; set; }
}

/// <summary>
/// Result of a run submission operation.
/// </summary>
public class RunSubmissionResult
{
    public string RunId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ToolOutput> ToolOutputs { get; set; } = new();
}

/// <summary>
/// Represents a tool output from the agent.
/// </summary>
public class ToolOutput
{
    public string ToolCallId { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
}

/// <summary>
/// Result of a run completion operation.
/// </summary>
public class RunCompletionResult
{
    public string RunId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public string? ErrorMessage { get; set; }
    public List<Message> Messages { get; set; } = new();
}

/// <summary>
/// Represents a message from the thread.
/// </summary>
public class Message
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Configuration for the Azure AI Foundry Agent Client.
/// </summary>
public class FoundryAgentClientConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}

/// <summary>
/// Azure AI Foundry Agent Client Wrapper.
/// Provides a thread-safe singleton pattern for interacting with Azure AI Foundry Agent Service.
/// </summary>
public sealed class FoundryAgentClient : IDisposable
{
    private static readonly Lazy<FoundryAgentClient> _instance = new(() => new FoundryAgentClient());

    /// <summary>
    /// Singleton instance of the Foundry Agent Client.
    /// </summary>
    public static FoundryAgentClient Instance => _instance.Value;

    private AIProjectClient? _projectClient;
    private AgentsClient? _agentsClient;
    private bool _disposed;

    /// <summary>
    /// Logger for the Foundry Agent Client.
    /// </summary>
    private readonly ILogger<FoundryAgentClient> _logger;

    /// <summary>
    /// Configuration for the client.
    /// </summary>
    private FoundryAgentClientConfig _config = new();

    private FoundryAgentClient()
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<FoundryAgentClient>();
    }

    /// <summary>
    /// Initializes the client with the specified configuration.
    /// Must be called before using any other methods.
    /// </summary>
    /// <param name="config">The client configuration.</param>
    public void Initialize(FoundryAgentClientConfig config)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FoundryAgentClient));

        _config = config ?? throw new ArgumentNullException(nameof(config));

        if (string.IsNullOrEmpty(config.Endpoint))
            throw new ArgumentException("Endpoint is required", nameof(config));

        if (string.IsNullOrEmpty(config.ApiKey))
            throw new ArgumentException("ApiKey is required", nameof(config));

        if (string.IsNullOrEmpty(config.ProjectId))
            throw new ArgumentException("ProjectId is required", nameof(config));

        var credential = new AzureKeyCredential(config.ApiKey);
        _projectClient = new AIProjectClient(new Uri(config.Endpoint), credential);
        _agentsClient = _projectClient.GetAgentsClient();

        _logger.LogInformation("FoundryAgentClient initialized with endpoint: {Endpoint}", config.Endpoint);
    }

    /// <summary>
    /// Creates a new thread for conversation with an agent.
    /// </summary>
    /// <param name="agentId">The agent ID to associate with the thread.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The thread creation result.</returns>
    public async Task<ThreadCreationResult> CreateThreadAsync(
        string agentId,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Creating thread for agent: {AgentId}", agentId);

        try
        {
            var threadResponse = await _agentsClient!.CreateThreadAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var threadId = threadResponse.Value.Id;
            _logger.LogInformation("Created thread: {ThreadId}", threadId);

            return new ThreadCreationResult
            {
                ThreadId = threadId,
                AssistantId = agentId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create thread for agent: {AgentId}", agentId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves an existing thread.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The thread details or null if not found.</returns>
    public async Task<ThreadResponse?> GetThreadAsync(
        string threadId,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Retrieving thread: {ThreadId}", threadId);

        try
        {
            var threadResponse = await _agentsClient!.GetThreadAsync(threadId, cancellationToken)
                .ConfigureAwait(false);

            return threadResponse.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Thread not found: {ThreadId}", threadId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve thread: {ThreadId}", threadId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves messages from a thread.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    /// <param name="limit">Maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of messages.</returns>
    public async Task<List<Message>> RetrieveThreadMessagesAsync(
        string threadId,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Retrieving messages for thread: {ThreadId}", threadId);

        try
        {
            var messages = new List<Message>();
            await foreach (var messagePage in _agentsClient!.GetMessagesAsync(threadId, limit: limit, cancellationToken: cancellationToken)
                .ConfigureAwait(false))
            {
                foreach (var msg in messagePage.Data)
                {
                    messages.Add(new Message
                    {
                        Id = msg.Id,
                        Role = msg.Role.ToString(),
                        Content = msg.ContentItems.FirstOrDefault()?.Text?.Value ?? string.Empty,
                        CreatedAt = msg.CreatedAt
                    });
                }
            }

            _logger.LogInformation("Retrieved {MessageCount} messages for thread {ThreadId}", threadId, messages.Count);
            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve messages for thread: {ThreadId}", threadId);
            throw;
        }
    }

    /// <summary>
    /// Submits tool outputs for a run that requires action.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    /// <param name="runId">The run ID.</param>
    /// <param name="toolOutputs">The tool outputs to submit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The run submission result.</returns>
    public async Task<RunSubmissionResult> SubmitToolOutputsAsync(
        string threadId,
        string runId,
        List<ToolOutput> toolOutputs,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Submitting tool outputs for run: {RunId}", runId);

        try
        {
            var azureToolOutputs = toolOutputs.Select(toolOutput => new ToolOutput(
                toolCallId: toolOutput.ToolCallId,
                output: toolOutput.Output)).ToList();

            var runResponse = await _agentsClient!.SubmitToolOutputsToThreadAsync(
                threadId,
                runId,
                azureToolOutputs,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return new RunSubmissionResult
            {
                RunId = runResponse.Value.Id,
                Status = runResponse.Value.Status.ToString(),
                ToolOutputs = toolOutputs
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit tool outputs for run: {RunId}", runId);
            throw;
        }
    }

    /// <summary>
    /// Waits for a run to complete.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    /// <param name="runId">The run ID.</param>
    /// <param name="pollInterval">Polling interval (default: 1 second).</param>
    /// <param name="maxWait">Maximum wait time (default: 5 minutes).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The run completion result.</returns>
    public async Task<RunCompletionResult> WaitForRunCompletionAsync(
        string threadId,
        string runId,
        TimeSpan? pollInterval = null,
        TimeSpan? maxWait = null,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        pollInterval ??= TimeSpan.FromSeconds(1);
        maxWait ??= TimeSpan.FromMinutes(5);

        _logger.LogInformation("Waiting for run completion: {RunId}", runId);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var runResponse = await _agentsClient!.GetRunAsync(threadId, runId, cancellationToken)
                    .ConfigureAwait(false);

                var run = runResponse.Value;
                var status = run.Status.ToString();

                _logger.LogDebug("Run {RunId} status: {Status}", runId, status);

                // Check if run is in a terminal state
                if (run.Status == RunStatus.Completed ||
                    run.Status == RunStatus.Failed ||
                    run.Status == RunStatus.Cancelled ||
                    run.Status == RunStatus.Expired ||
                    run.Status == RunStatus.Incomplete)
                {
                    var messages = new List<Message>();

                    // If completed, retrieve the messages
                    if (run.Status == RunStatus.Completed)
                    {
                        try
                        {
                            messages = await RetrieveThreadMessagesAsync(threadId, cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to retrieve messages for completed run: {RunId}", runId);
                        }
                    }

                    return new RunCompletionResult
                    {
                        RunId = run.Id,
                        Status = status,
                        Completed = run.Status == RunStatus.Completed,
                        ErrorMessage = run.LastError?.Message,
                        Messages = messages
                    };
                }

                // Check max wait time
                if (DateTime.UtcNow - startTime > maxWait.Value)
                {
                    _logger.LogWarning("Run {RunId} timed out after {Duration}", runId, maxWait.Value);
                    return new RunCompletionResult
                    {
                        RunId = runId,
                        Status = "Timeout",
                        Completed = false,
                        ErrorMessage = $"Run timed out after {maxWait.Value.TotalMinutes} minutes"
                    };
                }

                await Task.Delay(pollInterval.Value, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning("Run not found: {RunId}", runId);
                return new RunCompletionResult
                {
                    RunId = runId,
                    Status = "NotFound",
                    Completed = false,
                    ErrorMessage = "Run not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while waiting for run completion: {RunId}", runId);
                throw;
            }
        }

        return new RunCompletionResult
        {
            RunId = runId,
            Status = "Cancelled",
            Completed = false,
            ErrorMessage = "Operation was cancelled"
        };
    }

    /// <summary>
    /// Creates and immediately starts a run on a thread with an agent.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="instructions">The instructions for the run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The run submission result.</returns>
    public async Task<RunSubmissionResult> CreateRunAsync(
        string threadId,
        string agentId,
        string instructions,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Creating run for agent {AgentId} on thread {ThreadId}", agentId, threadId);

        try
        {
            var runResponse = await _agentsClient!.CreateRunAsync(
                threadId,
                agentId,
                instructions: instructions,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return new RunSubmissionResult
            {
                RunId = runResponse.Value.Id,
                Status = runResponse.Value.Status.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create run for agent {AgentId} on thread {ThreadId}", agentId, threadId);
            throw;
        }
    }

    /// <summary>
    /// Creates and starts a streaming run.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="instructions">The instructions for the run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of streaming events.</returns>
    public async IAsyncEnumerable<StreamingUpdate> StreamRunAsync(
        string threadId,
        string agentId,
        string instructions,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Starting streaming run for agent {AgentId} on thread {ThreadId}", agentId, threadId);

        try
        {
            var runRequest = new CreateRunRequest(agentId, instructions: instructions);
            var streamingResponse = _agentsClient!.CreateRunStreaming(threadId, runRequest, cancellationToken);

            await foreach (var update in streamingResponse.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                yield return update;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during streaming run for agent {AgentId} on thread {ThreadId}", agentId, threadId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a thread.
    /// </summary>
    /// <param name="threadId">The thread ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted successfully.</returns>
    public async Task<bool> DeleteThreadAsync(
        string threadId,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Deleting thread: {ThreadId}", threadId);

        try
        {
            await _agentsClient!.DeleteThreadAsync(threadId, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Successfully deleted thread: {ThreadId}", threadId);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Thread not found for deletion: {ThreadId}", threadId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete thread: {ThreadId}", threadId);
            throw;
        }
    }

    /// <summary>
    /// Gets the agents client for direct access.
    /// </summary>
    /// <returns>The agents client.</returns>
    public AgentsClient GetAgentsClient()
    {
        EnsureInitialized();
        return _agentsClient!;
    }

    /// <summary>
    /// Gets the project client for direct access.
    /// </summary>
    /// <returns>The project client.</returns>
    public AIProjectClient GetProjectClient()
    {
        EnsureInitialized();
        return _projectClient!;
    }

    private void EnsureInitialized()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(FoundryAgentClient));

        if (_agentsClient == null)
        {
            throw new InvalidOperationException(
                "FoundryAgentClient is not initialized. Call Initialize() before using any methods.");
        }
    }

    /// <summary>
    /// Disposes the client and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _agentsClient?.Dispose();
        _projectClient?.Dispose();
        _logger.LogInformation("FoundryAgentClient disposed");
    }
}
