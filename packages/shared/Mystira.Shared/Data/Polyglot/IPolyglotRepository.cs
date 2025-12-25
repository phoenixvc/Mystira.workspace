using Ardalis.Specification;

namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Repository interface for polyglot persistence with automatic database routing.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IPolyglotRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the target database for this entity type.
    /// </summary>
    DatabaseTarget Target { get; }

    /// <summary>
    /// Gets an entity by its string ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its Guid ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its string ID with caching disabled.
    /// </summary>
    Task<TEntity?> GetByIdNoCacheAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities (uses AsNoTracking for performance).
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single entity matching a specification (uses AsNoTracking).
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets entities matching a specification (uses AsNoTracking).
    /// </summary>
    Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching a specification.
    /// </summary>
    Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches a specification.
    /// </summary>
    Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity and invalidates its cache.
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by string ID and invalidates its cache.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by Guid ID and invalidates its cache.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity and invalidates its cache.
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cache for the specified entity ID.
    /// </summary>
    Task InvalidateCacheAsync(string id, CancellationToken cancellationToken = default);
}
