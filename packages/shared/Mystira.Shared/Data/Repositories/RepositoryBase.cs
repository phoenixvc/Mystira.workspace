using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mystira.Shared.Data.Repositories;

/// <summary>
/// Generic repository base implementation using Entity Framework Core.
/// Implements Ardalis.Specification's IRepositoryBase with additional convenience methods.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a foundation for repository implementations. Infrastructure layer
/// repositories should extend this class and implement <c>Mystira.Application.Ports.Data.IRepository&lt;TEntity&gt;</c>.
/// </para>
/// <para>
/// Example:
/// <code>
/// public class Repository&lt;TEntity&gt; : RepositoryBase&lt;TEntity&gt;, IRepository&lt;TEntity&gt;
///     where TEntity : class
/// {
///     public Repository(MyDbContext context) : base(context) { }
/// }
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
{
    /// <summary>
    /// The database context.
    /// </summary>
    protected readonly DbContext Context;

    /// <summary>
    /// The entity set for this repository.
    /// </summary>
    protected readonly DbSet<TEntity> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public RepositoryBase(DbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<TEntity>();
    }

    #region IRepositoryBase<T> methods (from Ardalis.Specification)

    /// <inheritdoc />
    public virtual async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        var entityList = entities.ToList();
        await DbSet.AddRangeAsync(entityList, cancellationToken);
        return entityList;
    }

    /// <inheritdoc />
    public virtual Task<int> UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Update(entity);
        return Task.FromResult(1);
    }

    /// <inheritdoc />
    public virtual Task<int> UpdateRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        var entityList = entities.ToList();
        DbSet.UpdateRange(entityList);
        return Task.FromResult(entityList.Count);
    }

    /// <inheritdoc />
    public virtual Task<int> DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
        return Task.FromResult(1);
    }

    /// <inheritdoc />
    public virtual Task<int> DeleteRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        var entityList = entities.ToList();
        DbSet.RemoveRange(entityList);
        return Task.FromResult(entityList.Count);
    }

    /// <inheritdoc />
    public virtual async Task<int> DeleteRangeAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        var entities = await ApplySpecification(specification).ToListAsync(cancellationToken);
        DbSet.RemoveRange(entities);
        return entities.Count;
    }

    /// <inheritdoc />
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync<TId>(
        TId id,
        CancellationToken cancellationToken = default) where TId : notnull
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TResult?> FirstOrDefaultAsync<TResult>(
        ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> SingleOrDefaultAsync(
        ISingleResultSpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TResult?> SingleOrDefaultAsync<TResult>(
        ISingleResultSpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().SingleOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<List<TResult>> ListAsync<TResult>(
        ISpecification<TEntity, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<TEntity> AsAsyncEnumerable(ISpecification<TEntity> specification)
    {
        return ApplySpecification(specification).AsNoTracking().AsAsyncEnumerable();
    }

    #endregion

    #region Convenience methods for IRepository<T> implementations

    /// <summary>
    /// Gets an entity by its string ID.
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Gets an entity by its Guid ID.
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Gets all entities (uses AsNoTracking for performance).
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds entities matching a predicate (uses AsNoTracking for performance).
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes an entity by string ID.
    /// </summary>
    public virtual async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
        }
    }

    /// <summary>
    /// Deletes an entity by Guid ID.
    /// </summary>
    public virtual async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
        }
    }

    /// <summary>
    /// Checks if an entity exists by string ID.
    /// </summary>
    public virtual async Task<bool> ExistsAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }

    /// <summary>
    /// Checks if any entity matches a predicate.
    /// </summary>
    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Gets a single entity matching a specification (uses AsNoTracking).
    /// Alias for FirstOrDefaultAsync.
    /// </summary>
    public virtual async Task<TEntity?> GetBySpecAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(spec, cancellationToken);
    }

    /// <summary>
    /// Streams all entities asynchronously (uses AsNoTracking).
    /// </summary>
    public virtual IAsyncEnumerable<TEntity> StreamAllAsync(
        CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking().AsAsyncEnumerable();
    }

    /// <summary>
    /// Streams entities matching a specification asynchronously (uses AsNoTracking).
    /// </summary>
    public virtual IAsyncEnumerable<TEntity> StreamAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(spec).AsNoTracking().AsAsyncEnumerable();
    }

    #endregion

    #region Protected helpers

    /// <summary>
    /// Apply an Ardalis.Specification to the query.
    /// </summary>
    protected virtual IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
    {
        return SpecificationEvaluator.Default.GetQuery(DbSet.AsQueryable(), spec);
    }

    /// <summary>
    /// Apply an Ardalis.Specification with projection to the query.
    /// </summary>
    protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> spec)
    {
        return SpecificationEvaluator.Default.GetQuery(DbSet.AsQueryable(), spec);
    }

    #endregion
}

/// <summary>
/// Repository implementation with explicit key type.
/// Use when you need type-safe key operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TKey">The key type (e.g., Guid, int, string).</typeparam>
public class RepositoryBase<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// The database context.
    /// </summary>
    protected readonly DbContext Context;

    /// <summary>
    /// The entity set for this repository.
    /// </summary>
    protected readonly DbSet<TEntity> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryBase{TEntity, TKey}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public RepositoryBase(DbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public virtual Task UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
        }
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<TEntity> StreamAllAsync(
        CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking().AsAsyncEnumerable();
    }
}

/// <summary>
/// Unit of Work implementation for coordinating repository transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("No transaction has been started. Call BeginTransactionAsync first.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _disposed = true;
        }
    }
}
