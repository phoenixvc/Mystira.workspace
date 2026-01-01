using Ardalis.Specification;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Generic repository interface following the Repository pattern
/// Supports both basic operations and specification-based queries
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    // Basic CRUD operations
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all entities from the repository.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync();

    /// <summary>
    /// Finds entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate expression to filter entities.</param>
    /// <returns>A collection of entities matching the predicate.</returns>
    Task<IEnumerable<TEntity>> FindAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity.</returns>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(string id);

    /// <summary>
    /// Checks whether an entity exists with the specified identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <returns>True if the entity exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(string id);

    // Specification pattern operations
    /// <summary>
    /// Gets a single entity that matches the specified specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    Task<TEntity?> GetBySpecAsync(ISpecification<TEntity> spec);

    /// <summary>
    /// Gets all entities that match the specified specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>A collection of entities matching the specification.</returns>
    Task<IEnumerable<TEntity>> ListAsync(ISpecification<TEntity> spec);

    /// <summary>
    /// Counts the number of entities that match the specified specification.
    /// </summary>
    /// <param name="spec">The specification to apply.</param>
    /// <returns>The count of entities matching the specification.</returns>
    Task<int> CountAsync(ISpecification<TEntity> spec);
}

