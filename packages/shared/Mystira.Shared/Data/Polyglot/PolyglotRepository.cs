using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Shared.Data.Entities;
using Mystira.Shared.Telemetry;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Repository implementation with polyglot persistence, dual-write pattern,
/// Polly resilience, and automatic database routing.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class PolyglotRepository<TEntity> : IPolyglotRepository<TEntity> where TEntity : class
{
    private readonly DbContext _primaryContext;
    private readonly DbContext? _secondaryContext;
    private readonly DbSet<TEntity> _primaryDbSet;
    private readonly DbSet<TEntity>? _secondaryDbSet;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<PolyglotRepository<TEntity>> _logger;
    private readonly PolyglotOptions _options;
    private readonly string _cachePrefix;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly ResiliencePipeline _circuitBreakerPipeline;
    private readonly IDbContextResolver _contextResolver;

    /// <inheritdoc />
    public DatabaseTarget Target { get; }

    /// <inheritdoc />
    public PolyglotMode CurrentMode => _options.Mode;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolyglotRepository{TEntity}"/> class.
    /// </summary>
    public PolyglotRepository(
        IDbContextResolver contextResolver,
        IDistributedCache? cache,
        IOptions<PolyglotOptions> options,
        ILogger<PolyglotRepository<TEntity>> logger)
    {
        _contextResolver = contextResolver;
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

        // Get primary DbContext
        _primaryContext = contextResolver.Resolve(Target);
        _primaryDbSet = _primaryContext.Set<TEntity>();

        // Get secondary DbContext for dual-write mode
        if (_options.Mode == PolyglotMode.DualWrite)
        {
            var secondaryTarget = Target == DatabaseTarget.CosmosDb
                ? DatabaseTarget.PostgreSql
                : DatabaseTarget.CosmosDb;

            try
            {
                _secondaryContext = contextResolver.Resolve(secondaryTarget);
                _secondaryDbSet = _secondaryContext.Set<TEntity>();
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning(
                    "Secondary context not available for dual-write mode. Falling back to single-store.");
            }
        }

        // Build resilience pipelines
        _retryPipeline = BuildRetryPipeline();
        _circuitBreakerPipeline = BuildCircuitBreakerPipeline();
    }

    #region IRepository Implementation

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetByIdInternalAsync(id, useCache: true, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetByIdInternalAsync(id.ToString(), useCache: true, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdNoCacheAsync(string id, CancellationToken cancellationToken = default)
    {
        return await GetByIdInternalAsync(id, useCache: false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _primaryDbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _primaryDbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using var activity = MystiraActivitySource.StartRepositoryActivity("Add", typeof(TEntity).Name);
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetEntityId(entity);
        activity?.SetTag("mystira.entity_id", id);
        activity?.SetTag("mystira.polyglot_mode", _options.Mode.ToString());

        var stopwatch = Stopwatch.StartNew();
        PolyglotSyncLog? syncLog = null;

        try
        {
            // Add to primary
            await _primaryDbSet.AddAsync(entity, cancellationToken);
            await _primaryContext.SaveChangesAsync(cancellationToken);

            // Dual-write to secondary if enabled
            if (_options.Mode == PolyglotMode.DualWrite && _secondaryDbSet is not null)
            {
                syncLog = await CreateSyncLogAsync(id!, SyncOperation.Insert, cancellationToken);

                try
                {
                    await ExecuteWithResilienceAsync(async ct =>
                    {
                        // Detach and re-add to secondary context
                        var clone = CloneEntity(entity);
                        await _secondaryDbSet.AddAsync(clone, ct);
                        await _secondaryContext!.SaveChangesAsync(ct);
                    }, cancellationToken);

                    syncLog?.MarkSynced(stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    syncLog?.MarkFailed(ex.Message, ex.StackTrace);
                    _logger.LogWarning(ex,
                        "Failed to sync {EntityType} {Id} to secondary. Compensation: {Enabled}",
                        typeof(TEntity).Name, id, _options.EnableCompensation);

                    if (_options.EnableCompensation)
                    {
                        await CompensateAddAsync(entity, syncLog, cancellationToken);
                    }
                }
            }

            return entity;
        }
        finally
        {
            if (syncLog is not null && _options.EnableSyncLogging)
            {
                await SaveSyncLogAsync(syncLog, cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        foreach (var entity in entityList)
        {
            await AddAsync(entity, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using var activity = MystiraActivitySource.StartRepositoryActivity("Update", typeof(TEntity).Name);
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetEntityId(entity);
        activity?.SetTag("mystira.entity_id", id);

        var stopwatch = Stopwatch.StartNew();
        PolyglotSyncLog? syncLog = null;

        try
        {
            // Ensure entity is tracked
            var trackedEntity = _primaryContext.ChangeTracker.Entries<TEntity>()
                .FirstOrDefault(e => GetEntityId(e.Entity) == id);

            if (trackedEntity is null)
            {
                _primaryDbSet.Update(entity);
            }
            else
            {
                _primaryContext.Entry(trackedEntity.Entity).CurrentValues.SetValues(entity);
            }

            await _primaryContext.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            if (id is not null)
            {
                await InvalidateCacheInternalAsync(id, cancellationToken);
            }

            // Dual-write to secondary
            if (_options.Mode == PolyglotMode.DualWrite && _secondaryDbSet is not null && id is not null)
            {
                syncLog = await CreateSyncLogAsync(id, SyncOperation.Update, cancellationToken);

                try
                {
                    await ExecuteWithResilienceAsync(async ct =>
                    {
                        var clone = CloneEntity(entity);
                        _secondaryDbSet.Update(clone);
                        await _secondaryContext!.SaveChangesAsync(ct);
                    }, cancellationToken);

                    syncLog?.MarkSynced(stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    syncLog?.MarkFailed(ex.Message, ex.StackTrace);
                    _logger.LogWarning(ex, "Failed to sync update for {EntityType} {Id}", typeof(TEntity).Name, id);
                }
            }
        }
        finally
        {
            if (syncLog is not null && _options.EnableSyncLogging)
            {
                await SaveSyncLogAsync(syncLog, cancellationToken);
            }
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

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await DeleteAsync(id.ToString(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        using var activity = MystiraActivitySource.StartRepositoryActivity("Delete", typeof(TEntity).Name);
        ArgumentNullException.ThrowIfNull(entity);

        var id = GetEntityId(entity);
        activity?.SetTag("mystira.entity_id", id);

        var stopwatch = Stopwatch.StartNew();
        PolyglotSyncLog? syncLog = null;

        try
        {
            _primaryDbSet.Remove(entity);
            await _primaryContext.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            if (id is not null)
            {
                await InvalidateCacheInternalAsync(id, cancellationToken);
            }

            // Dual-write to secondary
            if (_options.Mode == PolyglotMode.DualWrite && _secondaryDbSet is not null && id is not null)
            {
                syncLog = await CreateSyncLogAsync(id, SyncOperation.Delete, cancellationToken);

                try
                {
                    await ExecuteWithResilienceAsync(async ct =>
                    {
                        var secondaryEntity = await _secondaryDbSet.FindAsync(new object[] { id }, ct);
                        if (secondaryEntity is not null)
                        {
                            _secondaryDbSet.Remove(secondaryEntity);
                            await _secondaryContext!.SaveChangesAsync(ct);
                        }
                    }, cancellationToken);

                    syncLog?.MarkSynced(stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    syncLog?.MarkFailed(ex.Message, ex.StackTrace);
                    _logger.LogWarning(ex, "Failed to sync delete for {EntityType} {Id}", typeof(TEntity).Name, id);
                }
            }
        }
        finally
        {
            if (syncLog is not null && _options.EnableSyncLogging)
            {
                await SaveSyncLogAsync(syncLog, cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _primaryDbSet.FindAsync(new object[] { id }, cancellationToken) is not null;
    }

    /// <inheritdoc />
    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _primaryDbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _primaryDbSet.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetBySpecAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> ListAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        ISpecification<TEntity> spec,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(spec).CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TEntity> StreamAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var entity in _primaryDbSet.AsNoTracking().AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TEntity> StreamAsync(
        ISpecification<TEntity> spec,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var entity in ApplySpecification(spec).AsNoTracking().AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    #endregion

    #region Polyglot-Specific Methods

    /// <inheritdoc />
    public async Task<bool> IsPrimaryHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _primaryContext.Database.CanConnectAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsSecondaryHealthyAsync(CancellationToken cancellationToken = default)
    {
        if (_secondaryContext is null) return false;

        try
        {
            await _secondaryContext.Database.CanConnectAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<TEntity?> GetFromBackendAsync(
        string id,
        BackendType backend,
        CancellationToken cancellationToken = default)
    {
        var dbSet = backend == BackendType.Primary ? _primaryDbSet : _secondaryDbSet;
        if (dbSet is null)
        {
            throw new InvalidOperationException($"Backend {backend} is not configured.");
        }

        return await dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConsistencyResult> ValidateConsistencyAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity).FullName ?? typeof(TEntity).Name;

        if (_secondaryDbSet is null)
        {
            return ConsistencyResult.Error(id, entityType, "Secondary backend not configured");
        }

        try
        {
            var primaryEntity = await _primaryDbSet.FindAsync(new object[] { id }, cancellationToken);
            var secondaryEntity = await _secondaryDbSet.FindAsync(new object[] { id }, cancellationToken);

            if (primaryEntity is null && secondaryEntity is null)
            {
                return ConsistencyResult.Consistent(id, entityType);
            }

            if (primaryEntity is null)
            {
                return ConsistencyResult.MissingInPrimary(id, entityType);
            }

            if (secondaryEntity is null)
            {
                return ConsistencyResult.MissingInSecondary(id, entityType);
            }

            // Compare entities using JSON serialization
            var primaryJson = JsonSerializer.Serialize(primaryEntity, _jsonOptions);
            var secondaryJson = JsonSerializer.Serialize(secondaryEntity, _jsonOptions);

            if (primaryJson == secondaryJson)
            {
                return ConsistencyResult.Consistent(id, entityType);
            }

            return ConsistencyResult.Inconsistent(id, entityType, ["Entity data differs between backends"]);
        }
        catch (Exception ex)
        {
            return ConsistencyResult.Error(id, entityType, ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _primaryContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task InvalidateCacheAsync(string id, CancellationToken cancellationToken = default)
    {
        await InvalidateCacheInternalAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PolyglotSyncLog>> GetSyncLogsAsync(
        string entityId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        // This would typically query from a sync log table
        // For now, return empty as sync logs are stored separately
        await Task.CompletedTask;
        return [];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PolyglotSyncLog>> GetPendingSyncsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return [];
    }

    #endregion

    #region Private Helpers

    private async Task<TEntity?> GetByIdInternalAsync(
        string id,
        bool useCache,
        CancellationToken cancellationToken)
    {
        using var activity = MystiraActivitySource.StartRepositoryActivity("GetById", typeof(TEntity).Name);
        activity?.SetTag("mystira.entity_id", id);

        if (useCache && _cache is not null)
        {
            var cached = await GetFromCacheAsync(id, cancellationToken);
            if (cached is not null)
            {
                activity?.RecordCacheResult(hit: true);
                return cached;
            }
            activity?.RecordCacheResult(hit: false);
        }

        var entity = await _primaryDbSet.FindAsync(new object[] { id }, cancellationToken);

        if (entity is not null && useCache && _cache is not null)
        {
            await SetCacheAsync(id, entity, cancellationToken);
        }

        activity?.SetTag("mystira.found", entity is not null);
        return entity;
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
            _logger.LogWarning(ex, "Failed to invalidate cache for {EntityType} with ID {Id}", typeof(TEntity).Name, id);
        }
    }

    private DatabaseTarget ResolveTarget()
    {
        var entityType = typeof(TEntity);
        var typeName = entityType.FullName ?? entityType.Name;

        if (_options.EntityRouting.TryGetValue(typeName, out var configuredTarget))
        {
            return configuredTarget;
        }

        var attribute = entityType.GetCustomAttribute<DatabaseTargetAttribute>();
        if (attribute is not null)
        {
            return attribute.Target;
        }

        return _options.DefaultTarget;
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator.Default.GetQuery(_primaryDbSet.AsQueryable(), specification);
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
        var idProperty = typeof(TEntity).GetProperty("Id");
        return idProperty?.GetValue(entity)?.ToString();
    }

    private TEntity CloneEntity(TEntity entity)
    {
        var json = JsonSerializer.Serialize(entity, _jsonOptions);
        return JsonSerializer.Deserialize<TEntity>(json, _jsonOptions)!;
    }

    private ResiliencePipeline BuildRetryPipeline()
    {
        if (!_options.EnableResilience) return ResiliencePipeline.Empty;

        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = _options.RetryCount,
                Delay = TimeSpan.FromMilliseconds(_options.RetryDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Retry attempt {Attempt} for {EntityType}",
                        args.AttemptNumber,
                        typeof(TEntity).Name);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    private ResiliencePipeline BuildCircuitBreakerPipeline()
    {
        if (!_options.EnableResilience) return ResiliencePipeline.Empty;

        return new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(10),
                MinimumThroughput = _options.CircuitBreakerFailureThreshold,
                BreakDuration = TimeSpan.FromSeconds(_options.CircuitBreakerDurationSeconds),
                OnOpened = args =>
                {
                    _logger.LogWarning(
                        "Circuit breaker opened for {EntityType}",
                        typeof(TEntity).Name);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation(
                        "Circuit breaker closed for {EntityType}",
                        typeof(TEntity).Name);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    private async Task ExecuteWithResilienceAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.SecondaryWriteTimeoutMs);

        await _circuitBreakerPipeline.ExecuteAsync(
            async ct => await _retryPipeline.ExecuteAsync(
                async innerCt => await action(innerCt),
                ct),
            cts.Token);
    }

    private Task<PolyglotSyncLog> CreateSyncLogAsync(
        string entityId,
        string operation,
        CancellationToken cancellationToken)
    {
        var syncLog = new PolyglotSyncLog
        {
            Id = Entities.EntityId.NewId(),
            SyncedEntityId = entityId,
            EntityType = typeof(TEntity).FullName ?? typeof(TEntity).Name,
            Operation = operation,
            Status = SyncStatus.Pending,
            SourceBackend = BackendType.Primary,
            TargetBackend = BackendType.Secondary
        };

        return Task.FromResult(syncLog);
    }

    private async Task SaveSyncLogAsync(PolyglotSyncLog syncLog, CancellationToken cancellationToken)
    {
        try
        {
            // Sync logs could be saved to a dedicated table or logging system
            _logger.LogDebug(
                "Sync {Operation} for {EntityType} {EntityId}: {Status}",
                syncLog.Operation,
                syncLog.EntityType,
                syncLog.SyncedEntityId,
                syncLog.Status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save sync log");
        }

        await Task.CompletedTask;
    }

    private async Task CompensateAddAsync(
        TEntity entity,
        PolyglotSyncLog? syncLog,
        CancellationToken cancellationToken)
    {
        try
        {
            _primaryDbSet.Remove(entity);
            await _primaryContext.SaveChangesAsync(cancellationToken);
            syncLog?.MarkCompensated(true);
            _logger.LogInformation("Compensation successful for {EntityType}", typeof(TEntity).Name);
        }
        catch (Exception ex)
        {
            syncLog?.MarkCompensated(false);
            _logger.LogError(ex, "Compensation failed for {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    #endregion
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

    /// <summary>
    /// Checks if a context is registered for the target.
    /// </summary>
    bool HasContext(DatabaseTarget target);
}

/// <summary>
/// Default implementation of IDbContextResolver using registered contexts.
/// </summary>
public class DbContextResolver : IDbContextResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type? _cosmosContextType;
    private readonly Type? _postgresContextType;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextResolver"/> class.
    /// </summary>
    public DbContextResolver(
        IServiceProvider serviceProvider,
        Type? cosmosContextType = null,
        Type? postgresContextType = null)
    {
        _serviceProvider = serviceProvider;
        _cosmosContextType = cosmosContextType;
        _postgresContextType = postgresContextType;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool HasContext(DatabaseTarget target)
    {
        return target switch
        {
            DatabaseTarget.CosmosDb => _cosmosContextType is not null,
            DatabaseTarget.PostgreSql => _postgresContextType is not null,
            _ => false
        };
    }
}
