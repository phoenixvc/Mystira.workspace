using System.Linq.Expressions;
using Ardalis.Specification;

namespace Mystira.Shared.Data.Repositories;

/// <summary>
/// Generic repository interface following the Repository pattern.
/// Supports both basic operations and specification-based queries using Ardalis.Specification.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    // Basic CRUD operations

    /// <summary>
    /// Gets an entity by its string ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its Guid ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities (uses AsNoTracking for performance).
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching a predicate (uses AsNoTracking for performance).
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity.
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by string ID.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by Guid ID.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists by string ID.
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches a predicate.
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of all entities.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    // Specification pattern operations using Ardalis.Specification

    /// <summary>
    /// Gets a single entity matching a specification (uses AsNoTracking).
    /// </summary>
    Task<TEntity?> GetBySpecAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists entities matching a specification (uses AsNoTracking).
    /// </summary>
    Task<IEnumerable<TEntity>> ListAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching a specification.
    /// </summary>
    Task<int> CountAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default);

    // Streaming operations for large datasets

    /// <summary>
    /// Streams all entities asynchronously (uses AsNoTracking).
    /// Use for large datasets where loading all into memory is not practical.
    /// </summary>
    IAsyncEnumerable<TEntity> StreamAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams entities matching a specification asynchronously (uses AsNoTracking).
    /// </summary>
    IAsyncEnumerable<TEntity> StreamAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface with generic key type.
/// Use this when you need explicit key type control.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The primary key type (e.g., Guid, int, string).</typeparam>
public interface IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// Gets an entity by its typed ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities (uses AsNoTracking).
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an entity.
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its typed ID.
    /// </summary>
    Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists by its typed ID.
    /// </summary>
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams all entities asynchronously for large datasets.
    /// </summary>
    IAsyncEnumerable<TEntity> StreamAllAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work pattern for coordinating repository transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
