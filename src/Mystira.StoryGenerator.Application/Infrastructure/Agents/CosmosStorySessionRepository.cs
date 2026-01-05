using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Cosmos DB implementation of IStorySessionRepository.
/// Uses session_id as the partition key and indexes on thread_id and created_at.
/// </summary>
public class CosmosStorySessionRepository : IStorySessionRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger<CosmosStorySessionRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CosmosStorySessionRepository(
        CosmosClient cosmosClient,
        string databaseId,
        string containerId,
        ILogger<CosmosStorySessionRepository> logger)
    {
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var database = cosmosClient.GetDatabase(databaseId);
        _container = database.GetContainer(containerId);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        _logger.LogInformation("CosmosStorySessionRepository initialized with database: {DatabaseId}, container: {ContainerId}",
            databaseId, containerId);
    }

    public async Task<StorySession> CreateAsync(StorySession session, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating story session: {SessionId}", session.SessionId);

        try
        {
            var response = await _container.CreateItemAsync(
                session,
                new PartitionKey(session.SessionId),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Created story session: {SessionId}", session.SessionId);

            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create story session: {SessionId}", session.SessionId);
            throw;
        }
    }

    public async Task<StorySession?> GetAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting story session: {SessionId}", sessionId);

        try
        {
            var response = await _container.ReadItemAsync<StorySession>(
                sessionId,
                new PartitionKey(sessionId),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Story session not found: {SessionId}", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get story session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<StorySession> UpdateAsync(StorySession session, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating story session: {SessionId}", session.SessionId);

        try
        {
            session.UpdatedAt = DateTime.UtcNow;

            var response = await _container.ReplaceItemAsync(
                session,
                session.SessionId,
                new PartitionKey(session.SessionId),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Updated story session: {SessionId}", session.SessionId);

            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update story session: {SessionId}", session.SessionId);
            throw;
        }
    }

    public async Task<StorySession?> GetByThreadIdAsync(string threadId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting story session by thread ID: {ThreadId}", threadId);

        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.threadId = @threadId")
                .WithParameter("@threadId", threadId);

            var response = await _container.GetItemQueryIterator<StorySession>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    MaxItemCount = 1
                }).ReadNextAsync(cancellationToken).ConfigureAwait(false);

            var session = response.FirstOrDefault();

            if (session == null)
            {
                _logger.LogWarning("Story session not found for thread ID: {ThreadId}", threadId);
            }

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get story session by thread ID: {ThreadId}", threadId);
            throw;
        }
    }
}
