using System.Reflection;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Repository implementation with automatic database routing and caching.
/// Routes entities to Cosmos DB or PostgreSQL based on configuration/attributes.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class PolyglotRepository<TEntity> : IPolyglotRepository<TEntity> where TEntity : class
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<PolyglotRepository<TEntity>> _logger;
    private readonly PolyglotOptions _options;
    private readonly string _cachePrefix;

    /// <inheritdoc />
    public DatabaseTarget Target { get; }

    public PolyglotRepository(
        IDbContextResolver contextResolver,
        IDistributedCache? cache,
        IOptions<PolyglotOptions> options,
        ILogger<PolyglotRepository<TEntity>> logger)
    {
        _options = options.Value;
        _logger = logger;
        _cache = _options.EnableCaching ? cache : null;
        _cachePrefix = $"polyglot:{typeof(TEntity).Name}:";

        // Determine target database
        Target = ResolveTarget();

        // Get appropriate DbContext
        _context = contextResolver.Resolve(Target);
        _dbSet = _context.Set<TEntity>();
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_cache is not null)
        {
            var cached = await GetFromCacheAsync(id, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("Cache hit for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
                return cached;
            }
        }

        var entity = await GetByIdNoCacheAsync(id, cancellationToken);

        if (entity is not null && _cache is not null)
        {
            await SetCacheAsync(id, entity, cancellationToken);
        }

        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdNoCacheAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <inheritdoc />
    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);

        // Invalidate cache on update
        var id = GetEntityId(entity);
        if (id is not null && _cache is not null)
        {
            _ = InvalidateCacheAsync(id, cancellationToken);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdNoCacheAsync(id, cancellationToken);
        if (entity is not null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);

        // Invalidate cache on delete
        var id = GetEntityId(entity);
        if (id is not null && _cache is not null)
        {
            _ = InvalidateCacheAsync(id, cancellationToken);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task InvalidateCacheAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_cache is null) return;

        var cacheKey = $"{_cachePrefix}{id}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogDebug("Invalidated cache for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
    }

    private DatabaseTarget ResolveTarget()
    {
        var entityType = typeof(TEntity);
        var typeName = entityType.FullName ?? entityType.Name;

        // Check configuration override first
        if (_options.EntityRouting.TryGetValue(typeName, out var configuredTarget))
        {
            _logger.LogDebug("Using configured target {Target} for {EntityType}", configuredTarget, typeName);
            return configuredTarget;
        }

        // Check for attribute
        var attribute = entityType.GetCustomAttribute<DatabaseTargetAttribute>();
        if (attribute is not null)
        {
            _logger.LogDebug("Using attribute target {Target} for {EntityType}", attribute.Target, typeName);
            return attribute.Target;
        }

        // Use default
        _logger.LogDebug("Using default target {Target} for {EntityType}", _options.DefaultTarget, typeName);
        return _options.DefaultTarget;
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator.Default.GetQuery(_dbSet.AsQueryable(), specification);
    }

    private async Task<TEntity?> GetFromCacheAsync(string id, CancellationToken cancellationToken)
    {
        if (_cache is null) return null;

        try
        {
            var cacheKey = $"{_cachePrefix}{id}";
            var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (cached is not null)
            {
                return JsonSerializer.Deserialize<TEntity>(cached);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get {EntityType} from cache", typeof(TEntity).Name);
        }

        return null;
    }

    private async Task SetCacheAsync(string id, TEntity entity, CancellationToken cancellationToken)
    {
        if (_cache is null) return;

        try
        {
            var cacheKey = $"{_cachePrefix}{id}";
            var json = JsonSerializer.Serialize(entity);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.CacheExpirationSeconds)
            };

            await _cache.SetStringAsync(cacheKey, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache {EntityType}", typeof(TEntity).Name);
        }
    }

    private static string? GetEntityId(TEntity entity)
    {
        // Try to get Id property
        var idProperty = typeof(TEntity).GetProperty("Id");
        return idProperty?.GetValue(entity)?.ToString();
    }
}

/// <summary>
/// Resolves DbContext instances based on database target.
/// </summary>
public interface IDbContextResolver
{
    /// <summary>
    /// Gets the DbContext for the specified target database.
    /// </summary>
    DbContext Resolve(DatabaseTarget target);
}

/// <summary>
/// Default implementation of IDbContextResolver using registered contexts.
/// </summary>
public class DbContextResolver : IDbContextResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type? _cosmosContextType;
    private readonly Type? _postgresContextType;

    public DbContextResolver(
        IServiceProvider serviceProvider,
        Type? cosmosContextType = null,
        Type? postgresContextType = null)
    {
        _serviceProvider = serviceProvider;
        _cosmosContextType = cosmosContextType;
        _postgresContextType = postgresContextType;
    }

    public DbContext Resolve(DatabaseTarget target)
    {
        var contextType = target switch
        {
            DatabaseTarget.CosmosDb => _cosmosContextType,
            DatabaseTarget.PostgreSql => _postgresContextType,
            _ => throw new ArgumentOutOfRangeException(nameof(target))
        };

        if (contextType is null)
        {
            throw new InvalidOperationException(
                $"No DbContext registered for {target}. Register a context using AddPolyglotPersistence.");
        }

        return (DbContext)_serviceProvider.GetService(contextType)!
            ?? throw new InvalidOperationException($"Could not resolve DbContext of type {contextType.Name}");
    }
}
