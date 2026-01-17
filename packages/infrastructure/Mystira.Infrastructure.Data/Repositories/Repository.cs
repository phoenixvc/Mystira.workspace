using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Generic repository implementation following the Repository pattern.
/// Extends Ardalis.Specification.EntityFrameworkCore.RepositoryBase and implements the Application port interface.
/// Supports both basic CRUD operations and specification-based queries.
/// </summary>
public class Repository<TEntity> : Ardalis.Specification.EntityFrameworkCore.RepositoryBase<TEntity>, IRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// The database context. Protected for use by derived classes.
    /// </summary>
    protected readonly DbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public Repository(DbContext context) : base(context)
    {
        _dbContext = context;
    }

    /// <summary>
    /// Gets the DbSet for the entity type.
    /// </summary>
    protected DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();

    // IRepository<TEntity> convenience methods not in IRepositoryBase

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            return await GetByIdAsync(guidId, cancellationToken);
        }
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, cancellationToken) != null;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetBySpecAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(specification, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async IAsyncEnumerable<TEntity> StreamAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var entity in DbSet.AsNoTracking().AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    /// <inheritdoc />
    public virtual async IAsyncEnumerable<TEntity> StreamAsync(
        ISpecification<TEntity> specification,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var entity in AsAsyncEnumerable(specification).WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }
}
