using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;

namespace Mystira.Infrastructure.Data.Polyglot;

/// <summary>
/// EF Core repository implementation using Ardalis.Specification.
/// Provides a clean implementation of ISpecRepository with full specification support.
///
/// This is the foundation for the polyglot repository pattern.
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public class EfSpecificationRepository<T> : RepositoryBase<T>, ISpecRepository<T> where T : class
{
    /// <summary>The database context.</summary>
    protected readonly DbContext _dbContext;

    /// <summary>The logger instance.</summary>
    protected readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EfSpecificationRepository{T}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public EfSpecificationRepository(DbContext dbContext, ILogger<EfSpecificationRepository<T>> logger)
        : base(dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public override async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }
}
