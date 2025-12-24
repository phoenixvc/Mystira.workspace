using System.Reflection;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Shared.Telemetry;
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
    private readonly JsonSerializerOptions _jsonOptions;

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
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Determine target database
        Target = ResolveTarget();

        // Get appropriate DbContext
        _context = contextResolver.Resolve(Target);
        _dbSet = _context.Set<TEntity>();
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync<string>(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync<Guid>(id, cancellationToken);
    }

    /// <summary>
    /// Gets an entity by its ID with generic key type support.
    /// </summary>
    public async Task<TEntity?> GetByIdAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        using var activity = MystiraActivitySource.StartRepositoryActivity("GetById", typeof(TEntity).Name);
        activity?.SetTag("mystira.entity_id", id.ToString());

        var idString = id.ToString()!;

        if (_cache is not null)
        {
            var cached = await GetFromCacheAsync(idString, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("Cache hit for {EntityType} with ID {Id}", typeof(TEntity).Name, idString);
                activity?.RecordCacheResult(hit: true);
                return cached;
            }
            activity?.RecordCacheResult(hit: false);
        }

        var entity = await GetByIdNoCacheAsync(id, cancellationToken);

        if (entity is not null && _cache is not null)
        {
            await SetCacheAsync(idString, entity, cancellationToken);
        }

        activity?.SetTag("mystira.found", entity is not null);
        return entity;
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdNoCacheAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Gets an entity by Guid ID without caching.
    /// </summary>
    public async Task<TEntity?> GetByIdNoCacheAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// Gets an entity by generic key type without caching.
    /// </summary>
    public async Task<TEntity?> GetByIdNoCacheAsync<TKey>(TKey id, CancellationToken cancellationToken = default)
        where TKey : notnull
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().ToListAsync(cancellationToken);
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
        using var activity = MystiraActivitySource.StartRepositoryActivity("Add", typeof(TEntity).Name);

        ArgumentNullException.ThrowIfNull(entity);
        await _dbSet.AddAsync(entity, cancellationToken);

        var id = GetEntityId(entity);
        activity?.SetTag("mystira.entity_id", id);

        return entity;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using var activity = MystiraActivitySource.StartRepositoryActivity("Update", typeof(TEntity).Name);

        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);

        // Invalidate cache on update - await to ensure completion
        var id = GetEntityId(entity);
        activity?.SetTag("mystira.entity_id", id);

        if (id is not null && _cache is not null)
        {
            await InvalidateCacheInternalAsync(id, cancellationToken);
        }
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

    /// <summary>
    /// Deletes an entity by Guid ID.
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdNoCacheAsync(id, cancellationToken);
        if (entity is not null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using var activity = MystiraActivitySource.StartRepositoryActivity("Delete", typeof(TEntity).Name);

        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);

        // Invalidate cache on delete - await to ensure completion
        var id = GetEntityId(entity);
        activity?.SetTag("mystira.entity_id", id);

        if (id is not null && _cache is not null)
        {
            await InvalidateCacheInternalAsync(id, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task InvalidateCacheAsync(string id, CancellationToken cancellationToken = default)
    {
        await InvalidateCacheInternalAsync(id, cancellationToken);
    }

    private async Task InvalidateCacheInternalAsync(string id, CancellationToken cancellationToken)
    {
        if (_cache is null) return;

        try
        {
            var cacheKey = $"{_cachePrefix}{id}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Invalidated cache for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
        }
        catch (Exception ex)
        {
            // Log but don't throw - cache invalidation failure shouldn't break the operation
            _logger.LogWarning(ex, "Failed to invalidate cache for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
        }
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
                return JsonSerializer.Deserialize<TEntity>(cached, _jsonOptions);
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
            var json = JsonSerializer.Serialize(entity, _jsonOptions);
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
        // Try to get Id property (supports string, Guid, int, etc.)
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
