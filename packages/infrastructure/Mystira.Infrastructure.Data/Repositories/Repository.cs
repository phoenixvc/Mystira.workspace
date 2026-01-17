using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Shared.Data.Repositories;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Generic repository implementation following the Repository pattern.
/// Extends RepositoryBase and implements the Application port interface.
/// Supports both basic CRUD operations and specification-based queries.
/// </summary>
public class Repository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public Repository(DbContext context) : base(context)
    {
    }

    // All IRepositoryBase<T> methods are inherited from RepositoryBase<TEntity>
    // All convenience methods (GetByIdAsync(string), GetByIdAsync(Guid), etc.) are inherited from RepositoryBase<TEntity>

    // The following methods are explicitly implemented to satisfy IRepository<TEntity>
    // They delegate to inherited methods from RepositoryBase<TEntity>

    /// <inheritdoc />
    Task<TEntity?> IRepository<TEntity>.GetByIdAsync(string id, CancellationToken cancellationToken)
        => base.GetByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    Task<TEntity?> IRepository<TEntity>.GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => base.GetByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    Task IRepository<TEntity>.DeleteAsync(string id, CancellationToken cancellationToken)
        => base.DeleteAsync(id, cancellationToken);

    /// <inheritdoc />
    Task IRepository<TEntity>.DeleteAsync(Guid id, CancellationToken cancellationToken)
        => base.DeleteAsync(id, cancellationToken);

    /// <inheritdoc />
    Task<bool> IRepository<TEntity>.ExistsAsync(string id, CancellationToken cancellationToken)
        => base.ExistsAsync(id, cancellationToken);

    /// <inheritdoc />
    Task<IEnumerable<TEntity>> IRepository<TEntity>.GetAllAsync(CancellationToken cancellationToken)
        => base.GetAllAsync(cancellationToken);

    /// <inheritdoc />
    Task<IEnumerable<TEntity>> IRepository<TEntity>.FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken)
        => base.FindAsync(predicate, cancellationToken);

    /// <inheritdoc />
    Task<bool> IRepository<TEntity>.AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken)
        => base.AnyAsync(predicate, cancellationToken);

    /// <inheritdoc />
    Task<TEntity?> IRepository<TEntity>.GetBySpecAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
        => base.GetBySpecAsync(specification, cancellationToken);

    /// <inheritdoc />
    IAsyncEnumerable<TEntity> IRepository<TEntity>.StreamAllAsync(CancellationToken cancellationToken)
        => base.StreamAllAsync(cancellationToken);

    /// <inheritdoc />
    IAsyncEnumerable<TEntity> IRepository<TEntity>.StreamAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
        => base.StreamAsync(specification, cancellationToken);
}
