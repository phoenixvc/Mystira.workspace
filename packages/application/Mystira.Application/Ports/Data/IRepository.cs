using System.Linq.Expressions;
using Ardalis.Specification;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Extended repository interface that builds on Ardalis.Specification's IRepositoryBase
/// with additional convenience methods for common operations.
/// </summary>
/// <remarks>
/// This interface inherits from <see cref="IRepositoryBase{T}"/> which provides:
/// <list type="bullet">
///   <item><description>AddAsync, AddRangeAsync - Add entities</description></item>
///   <item><description>UpdateAsync, UpdateRangeAsync - Update entities (returns Task&lt;int&gt;)</description></item>
///   <item><description>DeleteAsync, DeleteRangeAsync - Delete entities (returns Task&lt;int&gt;)</description></item>
///   <item><description>SaveChangesAsync - Persist changes</description></item>
///   <item><description>GetByIdAsync&lt;TId&gt; - Get by typed ID</description></item>
///   <item><description>ListAsync - List all or by specification (returns Task&lt;List&lt;T&gt;&gt;)</description></item>
///   <item><description>FirstOrDefaultAsync, SingleOrDefaultAsync - Get single entity by specification</description></item>
///   <item><description>CountAsync, AnyAsync - Count and existence checks by specification</description></item>
///   <item><description>AsAsyncEnumerable - Stream entities by specification</description></item>
/// </list>
/// </remarks>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IRepository<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    // String ID convenience methods (not in IRepositoryBase)

    /// <summary>
    /// Gets an entity by its string ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity by its Guid ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by string ID.
    /// </summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by Guid ID.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists by string ID.
    /// </summary>
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    // Collection retrieval (convenience wrappers)

    /// <summary>
    /// Gets all entities (uses AsNoTracking for performance).
    /// Returns IEnumerable for compatibility; consider using ListAsync() from IRepositoryBase for List&lt;T&gt;.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching a predicate (uses AsNoTracking for performance).
    /// </summary>
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches a predicate.
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    // Specification helpers

    /// <summary>
    /// Gets a single entity matching a specification (uses AsNoTracking).
    /// Alias for FirstOrDefaultAsync from IRepositoryBase.
    /// </summary>
    Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    // Streaming/async enumeration

    /// <summary>
    /// Streams all entities asynchronously (uses AsNoTracking).
    /// Use for large datasets where loading all into memory is not practical.
    /// </summary>
    IAsyncEnumerable<TEntity> StreamAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams entities matching a specification asynchronously (uses AsNoTracking).
    /// </summary>
    IAsyncEnumerable<TEntity> StreamAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}
