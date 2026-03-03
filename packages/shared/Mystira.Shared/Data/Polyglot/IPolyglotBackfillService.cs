namespace Mystira.Shared.Polyglot;

/// <summary>
/// Service for backfilling entities from one backend to another.
/// Supports initial migration and ongoing synchronization.
/// </summary>
/// <typeparam name="TEntity">The entity type to backfill.</typeparam>
public interface IPolyglotBackfillService<TEntity> where TEntity : class
{
    /// <summary>
    /// Backfills all entities from source to target backend.
    /// </summary>
    /// <param name="source">The source backend to read from.</param>
    /// <param name="target">The target backend to write to.</param>
    /// <param name="batchSize">Number of entities to process per batch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the backfill operation.</returns>
    Task<BackfillSummary> BackfillAllAsync(
        BackendType source,
        BackendType target,
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Backfills a specific entity by ID.
    /// </summary>
    /// <param name="entityId">The entity ID to backfill.</param>
    /// <param name="source">The source backend to read from.</param>
    /// <param name="target">The target backend to write to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the backfill operation.</returns>
    Task<BackfillResult> BackfillEntityAsync(
        string entityId,
        BackendType source,
        BackendType target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Backfills multiple entities by their IDs.
    /// </summary>
    /// <param name="entityIds">The entity IDs to backfill.</param>
    /// <param name="source">The source backend to read from.</param>
    /// <param name="target">The target backend to write to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the backfill operation.</returns>
    Task<BackfillSummary> BackfillEntitiesAsync(
        IEnumerable<string> entityIds,
        BackendType source,
        BackendType target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates consistency for all entities and returns inconsistent ones.
    /// </summary>
    /// <param name="batchSize">Number of entities to check per batch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of consistency results for inconsistent entities.</returns>
    Task<IReadOnlyList<ConsistencyResult>> FindInconsistentEntitiesAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Backfills only inconsistent entities from source to target.
    /// </summary>
    /// <param name="source">The source backend (source of truth).</param>
    /// <param name="target">The target backend to synchronize.</param>
    /// <param name="batchSize">Number of entities to process per batch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the backfill operation.</returns>
    Task<BackfillSummary> BackfillInconsistentAsync(
        BackendType source,
        BackendType target,
        int batchSize = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries failed sync operations from the sync log.
    /// </summary>
    /// <param name="maxRetries">Maximum retry attempts per entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the retry operation.</returns>
    Task<BackfillSummary> RetryFailedSyncsAsync(
        int maxRetries = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of entities in each backend.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tuple of (primaryCount, secondaryCount).</returns>
    Task<(int Primary, int Secondary)> GetEntityCountsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams entities from the source backend for processing.
    /// </summary>
    /// <param name="source">The source backend to read from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of entities.</returns>
    IAsyncEnumerable<TEntity> StreamFromBackendAsync(
        BackendType source,
        CancellationToken cancellationToken = default);
}
