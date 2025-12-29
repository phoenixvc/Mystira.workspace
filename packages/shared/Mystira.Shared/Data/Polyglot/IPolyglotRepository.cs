using Ardalis.Specification;
using Mystira.Shared.Data.Repositories;

namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Repository interface for polyglot persistence with automatic database routing,
/// dual-write support, and consistency validation.
/// Extends IRepository with polyglot-specific capabilities.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IPolyglotRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the target database for this entity type.
    /// </summary>
    DatabaseTarget Target { get; }

    /// <summary>
    /// Gets the current polyglot persistence mode.
    /// </summary>
    PolyglotMode CurrentMode { get; }

    /// <summary>
    /// Gets an entity by its string ID with caching disabled.
    /// </summary>
    Task<TEntity?> GetByIdNoCacheAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity matching a specification (uses AsNoTracking).
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the primary backend is healthy and available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if primary backend is healthy.</returns>
    Task<bool> IsPrimaryHealthyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the secondary backend is healthy and available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if secondary backend is healthy.</returns>
    Task<bool> IsSecondaryHealthyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity from a specific backend.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="backend">The backend to read from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if found, null otherwise.</returns>
    Task<TEntity?> GetFromBackendAsync(
        string id,
        BackendType backend,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates consistency of an entity between primary and secondary backends.
    /// </summary>
    /// <param name="id">The entity ID to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Consistency validation result.</returns>
    Task<ConsistencyResult> ValidateConsistencyAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cache for the specified entity ID.
    /// </summary>
    Task InvalidateCacheAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sync logs for a specific entity.
    /// </summary>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="limit">Maximum number of logs to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of sync log entries.</returns>
    Task<IReadOnlyList<PolyglotSyncLog>> GetSyncLogsAsync(
        string entityId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending sync operations.
    /// </summary>
    /// <param name="limit">Maximum number of pending syncs to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of pending sync log entries.</returns>
    Task<IReadOnlyList<PolyglotSyncLog>> GetPendingSyncsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);
}
