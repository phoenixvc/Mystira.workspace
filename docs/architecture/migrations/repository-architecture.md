# Repository Architecture for Dual-Database Strategy

## Current Architecture (Hexagonal/Ports & Adapters)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           CURRENT STATE                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    APPLICATION LAYER                              │   │
│  │  Mystira.App.Application                                         │   │
│  │  ┌──────────────────────────────────────────────────────────┐    │   │
│  │  │  Ports/Data/                                              │    │   │
│  │  │  • IRepository<T>           (Generic base)               │    │   │
│  │  │  • IAccountRepository       (Account-specific)           │    │   │
│  │  │  • IUserProfileRepository   (Profile-specific)           │    │   │
│  │  │  • IGameSessionRepository   (Session-specific)           │    │   │
│  │  │  • ...                                                    │    │   │
│  │  └──────────────────────────────────────────────────────────┘    │   │
│  └───────────────────────────────┬──────────────────────────────────┘   │
│                                  │                                       │
│                                  │ Implements                            │
│                                  ▼                                       │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    INFRASTRUCTURE LAYER                           │   │
│  │  Mystira.App.Infrastructure.Data (Cosmos DB)                     │   │
│  │  ┌──────────────────────────────────────────────────────────┐    │   │
│  │  │  Repositories/                                            │    │   │
│  │  │  • Repository<T>            (Generic Cosmos implementation)│    │   │
│  │  │  • AccountRepository        (Cosmos implementation)       │    │   │
│  │  │  • UserProfileRepository    (Cosmos implementation)       │    │   │
│  │  │  • ...                                                    │    │   │
│  │  └──────────────────────────────────────────────────────────┘    │   │
│  │  ┌──────────────────────────────────────────────────────────┐    │   │
│  │  │  MystiraAppDbContext.cs     (Cosmos DB context)          │    │   │
│  │  └──────────────────────────────────────────────────────────┘    │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

## Proposed Architecture (Dual-Database with Hybrid Layer)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           PROPOSED STATE                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    APPLICATION LAYER                              │   │
│  │  Mystira.App.Application                                         │   │
│  │  ┌──────────────────────────────────────────────────────────┐    │   │
│  │  │  Ports/Data/                                              │    │   │
│  │  │  • IRepository<T>           (UNCHANGED)                  │    │   │
│  │  │  • IAccountRepository       (UNCHANGED)                  │    │   │
│  │  │  • IUserProfileRepository   (UNCHANGED)                  │    │   │
│  │  │  • ...                                                    │    │   │
│  │  └──────────────────────────────────────────────────────────┘    │   │
│  │  ┌──────────────────────────────────────────────────────────┐    │   │
│  │  │  Services/ (NEW - Domain Services)                        │    │   │
│  │  │  • IDataMigrationService    (Sync coordination)          │    │   │
│  │  │  • IFeatureFlagService      (Migration phases)           │    │   │
│  │  └──────────────────────────────────────────────────────────┘    │   │
│  └───────────────────────────────┬──────────────────────────────────┘   │
│                                  │                                       │
│                                  │ Implements                            │
│                                  ▼                                       │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    INFRASTRUCTURE LAYER                           │   │
│  │                                                                   │   │
│  │  ┌────────────────────────────────────────────────────────────┐  │   │
│  │  │  Mystira.App.Infrastructure.Hybrid (NEW - Coordinator)     │  │   │
│  │  │  ┌─────────────────────────────────────────────────────┐   │  │   │
│  │  │  │  DualWriteAccountRepository     : IAccountRepository │   │  │   │
│  │  │  │  DualWriteUserProfileRepository : IUserProfileRepository│  │   │
│  │  │  │  DualWritePendingSignupRepository                    │   │  │   │
│  │  │  │  DataSyncBackgroundService      : BackgroundService  │   │  │   │
│  │  │  │  MigrationPhaseManager          : IFeatureFlagService│   │  │   │
│  │  │  └─────────────────────────────────────────────────────┘   │  │   │
│  │  └────────────────────────────┬───────────────────────────────┘  │   │
│  │                               │                                   │   │
│  │              ┌────────────────┴────────────────┐                 │   │
│  │              │                                 │                 │   │
│  │              ▼                                 ▼                 │   │
│  │  ┌─────────────────────────┐   ┌─────────────────────────────┐  │   │
│  │  │ Infrastructure.Data     │   │ Infrastructure.PostgreSQL   │  │   │
│  │  │ (Cosmos DB - Existing)  │   │ (NEW)                       │  │   │
│  │  │                         │   │                             │  │   │
│  │  │ • MystiraAppDbContext   │   │ • PostgreSqlDbContext       │  │   │
│  │  │ • CosmosAccountRepo     │   │ • PgAccountRepository       │  │   │
│  │  │ • CosmosUserProfileRepo │   │ • PgUserProfileRepository   │  │   │
│  │  │ • ...                   │   │ • ...                       │  │   │
│  │  └─────────────────────────┘   └─────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

## New Project Structure

```
Mystira.App/src/
├── Mystira.App.Application/                    # Domain layer (unchanged)
│   └── Ports/
│       └── Data/
│           ├── IRepository.cs
│           ├── IAccountRepository.cs
│           └── ...
│
├── Mystira.App.Domain/                         # Entities (unchanged)
│   └── Models/
│       ├── Account.cs
│       ├── UserProfile.cs
│       └── ...
│
├── Mystira.App.Infrastructure.Data/            # Cosmos DB (existing)
│   ├── MystiraAppDbContext.cs
│   └── Repositories/
│       ├── Cosmos/                             # Rename existing repos
│       │   ├── CosmosAccountRepository.cs
│       │   ├── CosmosUserProfileRepository.cs
│       │   └── ...
│       └── Repository.cs                       # Base class
│
├── Mystira.App.Infrastructure.PostgreSQL/      # NEW: PostgreSQL
│   ├── PostgreSqlDbContext.cs
│   ├── Migrations/                             # EF Core migrations
│   │   └── 20251222_InitialCreate.cs
│   └── Repositories/
│       ├── PgAccountRepository.cs
│       ├── PgUserProfileRepository.cs
│       ├── PgPendingSignupRepository.cs
│       └── PgRepository.cs                     # Base class
│
├── Mystira.App.Infrastructure.Hybrid/          # NEW: Coordination layer
│   ├── DualWrite/
│   │   ├── DualWriteAccountRepository.cs
│   │   ├── DualWriteUserProfileRepository.cs
│   │   └── DualWritePendingSignupRepository.cs
│   ├── Sync/
│   │   ├── DataSyncBackgroundService.cs
│   │   ├── SyncQueue.cs
│   │   └── SyncMetrics.cs
│   ├── Migration/
│   │   ├── MigrationPhaseManager.cs
│   │   ├── MigrationPhase.cs
│   │   └── MigrationValidationService.cs
│   └── DependencyInjection/
│       └── HybridDataServiceExtensions.cs
│
└── Mystira.App.Infrastructure.Redis/           # NEW: Caching (Phase 2)
    ├── RedisCacheService.cs
    ├── CachedAccountRepository.cs              # Decorator pattern
    └── RedisConnectionFactory.cs
```

## Migration Phases

### Phase Configuration

```csharp
public enum MigrationPhase
{
    /// <summary>
    /// All reads/writes go to Cosmos DB only (current state)
    /// </summary>
    CosmosOnly = 0,

    /// <summary>
    /// Writes go to both, reads from Cosmos DB
    /// PostgreSQL is being populated
    /// </summary>
    DualWriteCosmosRead = 1,

    /// <summary>
    /// Writes go to both, reads from PostgreSQL with Cosmos fallback
    /// Validation in progress
    /// </summary>
    DualWritePostgresRead = 2,

    /// <summary>
    /// All reads/writes go to PostgreSQL only
    /// Cosmos DB disabled but data preserved
    /// </summary>
    PostgresOnly = 3
}
```

### DI Registration by Phase

```csharp
// DependencyInjection/HybridDataServiceExtensions.cs
public static class HybridDataServiceExtensions
{
    public static IServiceCollection AddHybridDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var phase = configuration.GetValue<MigrationPhase>("DataMigration:Phase");

        // Always register both contexts
        services.AddDbContext<MystiraAppDbContext>(/* Cosmos config */);
        services.AddDbContext<PostgreSqlDbContext>(/* PostgreSQL config */);

        // Register base repositories (internal)
        services.AddScoped<CosmosAccountRepository>();
        services.AddScoped<PgAccountRepository>();

        // Register the appropriate implementation based on phase
        switch (phase)
        {
            case MigrationPhase.CosmosOnly:
                services.AddScoped<IAccountRepository, CosmosAccountRepository>();
                services.AddScoped<IUserProfileRepository, CosmosUserProfileRepository>();
                break;

            case MigrationPhase.DualWriteCosmosRead:
            case MigrationPhase.DualWritePostgresRead:
                services.AddScoped<IAccountRepository, DualWriteAccountRepository>();
                services.AddScoped<IUserProfileRepository, DualWriteUserProfileRepository>();
                services.AddHostedService<DataSyncBackgroundService>();
                break;

            case MigrationPhase.PostgresOnly:
                services.AddScoped<IAccountRepository, PgAccountRepository>();
                services.AddScoped<IUserProfileRepository, PgUserProfileRepository>();
                break;
        }

        return services;
    }
}
```

## Dual-Write Repository Implementation

```csharp
// Infrastructure.Hybrid/DualWrite/DualWriteAccountRepository.cs
public class DualWriteAccountRepository : IAccountRepository
{
    private readonly CosmosAccountRepository _cosmosRepo;
    private readonly PgAccountRepository _pgRepo;
    private readonly MigrationPhaseManager _phaseManager;
    private readonly ISyncQueue _syncQueue;
    private readonly ILogger<DualWriteAccountRepository> _logger;

    public DualWriteAccountRepository(
        CosmosAccountRepository cosmosRepo,
        PgAccountRepository pgRepo,
        MigrationPhaseManager phaseManager,
        ISyncQueue syncQueue,
        ILogger<DualWriteAccountRepository> logger)
    {
        _cosmosRepo = cosmosRepo;
        _pgRepo = pgRepo;
        _phaseManager = phaseManager;
        _syncQueue = syncQueue;
        _logger = logger;
    }

    public async Task<Account?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var phase = _phaseManager.CurrentPhase;

        if (phase == MigrationPhase.DualWriteCosmosRead)
        {
            // Primary: Cosmos DB
            return await _cosmosRepo.GetByIdAsync(id, ct);
        }

        // Primary: PostgreSQL with Cosmos fallback
        var account = await _pgRepo.GetByIdAsync(id, ct);
        if (account != null) return account;

        // Fallback to Cosmos and backfill
        account = await _cosmosRepo.GetByIdAsync(id, ct);
        if (account != null)
        {
            _logger.LogInformation("Backfilling account {Id} to PostgreSQL", id);
            await _syncQueue.EnqueueAsync(new SyncItem
            {
                EntityType = nameof(Account),
                EntityId = id,
                Operation = SyncOperation.Upsert
            });
        }

        return account;
    }

    public async Task<Account> AddAsync(Account entity, CancellationToken ct = default)
    {
        var phase = _phaseManager.CurrentPhase;

        // Always write to primary first
        if (phase == MigrationPhase.DualWriteCosmosRead)
        {
            // Primary: Cosmos
            var result = await _cosmosRepo.AddAsync(entity, ct);

            // Secondary: PostgreSQL (queued for reliability)
            await QueueSecondaryWriteAsync(entity.Id, SyncOperation.Insert, ct);

            return result;
        }
        else
        {
            // Primary: PostgreSQL
            var result = await _pgRepo.AddAsync(entity, ct);

            // Secondary: Cosmos (fire-and-forget with logging)
            _ = Task.Run(async () => await WriteToCosmosWithErrorHandlingAsync(entity, SyncOperation.Insert));

            return result;
        }
    }

    public async Task<Account> UpdateAsync(Account entity, CancellationToken ct = default)
    {
        var phase = _phaseManager.CurrentPhase;

        if (phase == MigrationPhase.DualWriteCosmosRead)
        {
            var result = await _cosmosRepo.UpdateAsync(entity, ct);
            await QueueSecondaryWriteAsync(entity.Id, SyncOperation.Update, ct);
            return result;
        }
        else
        {
            var result = await _pgRepo.UpdateAsync(entity, ct);
            _ = Task.Run(async () => await WriteToCosmosWithErrorHandlingAsync(entity, SyncOperation.Update));
            return result;
        }
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var phase = _phaseManager.CurrentPhase;

        if (phase == MigrationPhase.DualWriteCosmosRead)
        {
            await _cosmosRepo.DeleteAsync(id, ct);
            await QueueSecondaryWriteAsync(id, SyncOperation.Delete, ct);
        }
        else
        {
            await _pgRepo.DeleteAsync(id, ct);
            _ = Task.Run(async () => await DeleteFromCosmosWithErrorHandlingAsync(id));
        }
    }

    // Domain-specific methods
    public async Task<Account?> GetByEmailAsync(string email)
    {
        var phase = _phaseManager.CurrentPhase;
        return phase == MigrationPhase.DualWriteCosmosRead
            ? await _cosmosRepo.GetByEmailAsync(email)
            : await _pgRepo.GetByEmailAsync(email)
              ?? await _cosmosRepo.GetByEmailAsync(email);
    }

    public async Task<Account?> GetByAuth0UserIdAsync(string auth0UserId)
    {
        var phase = _phaseManager.CurrentPhase;
        return phase == MigrationPhase.DualWriteCosmosRead
            ? await _cosmosRepo.GetByAuth0UserIdAsync(auth0UserId)
            : await _pgRepo.GetByAuth0UserIdAsync(auth0UserId)
              ?? await _cosmosRepo.GetByAuth0UserIdAsync(auth0UserId);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var phase = _phaseManager.CurrentPhase;
        return phase == MigrationPhase.DualWriteCosmosRead
            ? await _cosmosRepo.ExistsByEmailAsync(email)
            : await _pgRepo.ExistsByEmailAsync(email);
    }

    // Queue-based secondary write (reliable, retryable)
    private async Task QueueSecondaryWriteAsync(string id, SyncOperation op, CancellationToken ct)
    {
        try
        {
            await _syncQueue.EnqueueAsync(new SyncItem
            {
                EntityType = nameof(Account),
                EntityId = id,
                Operation = op
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue sync for account {Id}", id);
            // Consider alerting - this is a critical failure
        }
    }

    // Fire-and-forget with proper error handling (for non-critical secondary writes)
    private async Task WriteToCosmosWithErrorHandlingAsync(Account entity, SyncOperation op)
    {
        try
        {
            if (op == SyncOperation.Insert)
                await _cosmosRepo.AddAsync(entity);
            else
                await _cosmosRepo.UpdateAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync account {Id} to Cosmos (non-blocking)", entity.Id);
            // Don't queue for retry since PostgreSQL is primary in this phase
        }
    }

    private async Task DeleteFromCosmosWithErrorHandlingAsync(string id)
    {
        try
        {
            await _cosmosRepo.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete account {Id} from Cosmos (non-blocking)", id);
        }
    }
}
```

## Sync Queue for Failed Operations

```csharp
// Infrastructure.Hybrid/Sync/SyncQueue.cs
public interface ISyncQueue
{
    Task EnqueueAsync(SyncItem item, CancellationToken ct = default);
    Task<SyncItem?> DequeueAsync(CancellationToken ct = default);
    Task<int> GetQueueLengthAsync(CancellationToken ct = default);
    Task<IEnumerable<SyncItem>> GetAllAsync(CancellationToken ct = default);
    Task RetryFailedAsync(CancellationToken ct = default);
}

public record SyncItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required SyncOperation Operation { get; init; }
    public int RetryCount { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? LastError { get; set; }
}

public enum SyncOperation
{
    Insert,
    Update,
    Upsert,
    Delete
}

// Thread-safe in-memory implementation (for development/testing)
public class InMemorySyncQueue : ISyncQueue
{
    private readonly ConcurrentQueue<SyncItem> _queue = new();
    private readonly ConcurrentBag<SyncItem> _failed = [];

    public Task EnqueueAsync(SyncItem item, CancellationToken ct = default)
    {
        _queue.Enqueue(item);
        return Task.CompletedTask;
    }

    public Task<SyncItem?> DequeueAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_queue.TryDequeue(out var item) ? item : null);
    }

    public Task<int> GetQueueLengthAsync(CancellationToken ct = default)
        => Task.FromResult(_queue.Count);

    public Task<IEnumerable<SyncItem>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IEnumerable<SyncItem>>([.. _queue]);

    public Task RetryFailedAsync(CancellationToken ct = default)
    {
        while (_failed.TryTake(out var item))
        {
            item.RetryCount++;
            _queue.Enqueue(item);
        }
        return Task.CompletedTask;
    }
}

// Can be backed by:
// - In-memory queue (simple, not durable) - InMemorySyncQueue above
// - Redis queue (recommended for production)
// - Azure Service Bus (enterprise)
// - PostgreSQL table (simple, durable)
```

## Background Sync Service

```csharp
// Infrastructure.Hybrid/Sync/DataSyncBackgroundService.cs
public class DataSyncBackgroundService : BackgroundService
{
    private readonly ISyncQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataSyncBackgroundService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var item = await _queue.DequeueAsync(stoppingToken);
            if (item == null)
            {
                await Task.Delay(1000, stoppingToken); // No items, wait
                continue;
            }

            using var scope = _scopeFactory.CreateScope();
            try
            {
                await ProcessSyncItemAsync(scope, item, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process sync item {Id}", item.Id);
                item.RetryCount++;
                item.LastAttemptAt = DateTime.UtcNow;
                item.LastError = ex.Message;

                if (item.RetryCount < 5)
                {
                    await _queue.EnqueueAsync(item);
                }
                else
                {
                    _logger.LogCritical("Sync item {Id} exceeded max retries", item.Id);
                    // Send to dead letter / alert
                }
            }
        }
    }

    private async Task ProcessSyncItemAsync(
        IServiceScope scope,
        SyncItem item,
        CancellationToken ct)
    {
        var cosmosContext = scope.ServiceProvider.GetRequiredService<MystiraAppDbContext>();
        var pgContext = scope.ServiceProvider.GetRequiredService<PostgreSqlDbContext>();

        switch (item.EntityType)
        {
            case nameof(Account):
                await SyncAccountAsync(cosmosContext, pgContext, item, ct);
                break;
            case nameof(UserProfile):
                await SyncUserProfileAsync(cosmosContext, pgContext, item, ct);
                break;
            // ... other entity types
        }
    }

    private async Task SyncAccountAsync(
        MystiraAppDbContext cosmos,
        PostgreSqlDbContext pg,
        SyncItem item,
        CancellationToken ct)
    {
        // Read from Cosmos (source of truth during DualWriteCosmosRead)
        var cosmosAccount = await cosmos.Accounts.FindAsync(item.EntityId);

        if (item.Operation == SyncOperation.Delete || cosmosAccount == null)
        {
            var pgAccount = await pg.Accounts.FindAsync(Guid.Parse(item.EntityId));
            if (pgAccount != null)
            {
                pg.Accounts.Remove(pgAccount);
                await pg.SaveChangesAsync(ct);
            }
        }
        else
        {
            // Upsert to PostgreSQL
            var pgAccount = await pg.Accounts.FindAsync(Guid.Parse(item.EntityId));
            if (pgAccount == null)
            {
                pg.Accounts.Add(MapToPostgres(cosmosAccount));
            }
            else
            {
                UpdatePostgresAccount(pgAccount, cosmosAccount);
            }
            await pg.SaveChangesAsync(ct);
        }
    }
}
```

## Redis Caching Layer (Phase 2)

```csharp
// Infrastructure.Redis/CachedAccountRepository.cs
public class CachedAccountRepository : IAccountRepository
{
    private readonly IAccountRepository _inner;
    private readonly IDistributedCache _cache;
    private readonly IOptions<RedisOptions> _options;
    private readonly ILogger<CachedAccountRepository> _logger;
    private readonly TimeSpan _cacheDuration;

    public CachedAccountRepository(
        IAccountRepository inner,
        IDistributedCache cache,
        IOptions<RedisOptions> options,
        ILogger<CachedAccountRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _options = options;
        _logger = logger;
        _cacheDuration = TimeSpan.FromMinutes(options.Value.DefaultExpirationMinutes);
    }

    // Cache key includes instance prefix to avoid collisions across environments
    private string GetCacheKey(string suffix) => $"{_options.Value.InstanceName}account:{suffix}";

    public async Task<Account?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var cacheKey = GetCacheKey(id);
        var cached = await _cache.GetStringAsync(cacheKey, ct);

        if (cached != null)
        {
            _logger.LogDebug("Cache hit for {Key}", cacheKey);
            return JsonSerializer.Deserialize<Account>(cached);
        }

        _logger.LogDebug("Cache miss for {Key}", cacheKey);
        var account = await _inner.GetByIdAsync(id, ct);
        if (account != null)
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(account),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration
                },
                ct);
        }

        return account;
    }

    public async Task<Account> UpdateAsync(Account entity, CancellationToken ct = default)
    {
        var result = await _inner.UpdateAsync(entity, ct);

        // Invalidate cache (use prefixed keys)
        await _cache.RemoveAsync(GetCacheKey(entity.Id), ct);
        await _cache.RemoveAsync(GetCacheKey($"email:{entity.Email}"), ct);

        return result;
    }

    // ... implement other methods with caching
}
```

## Testing Strategy

```csharp
// Test both implementations independently
[TestClass]
public class AccountRepositoryTests
{
    [TestMethod]
    public async Task CosmosRepository_CreateAndRead_Works()
    {
        using var cosmos = new TestCosmosDbContext();
        var repo = new CosmosAccountRepository(cosmos);

        var account = new Account { Email = "test@example.com" };
        await repo.AddAsync(account);

        var retrieved = await repo.GetByIdAsync(account.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(account.Email, retrieved.Email);
    }

    [TestMethod]
    public async Task PostgresRepository_CreateAndRead_Works()
    {
        using var pg = new TestPostgreSqlDbContext();
        var repo = new PgAccountRepository(pg);

        var account = new Account { Email = "test@example.com" };
        await repo.AddAsync(account);

        var retrieved = await repo.GetByIdAsync(account.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(account.Email, retrieved.Email);
    }

    [TestMethod]
    public async Task DualWriteRepository_WritesToBoth()
    {
        // Test dual-write behavior
    }
}
```

## Summary

| Component                      | Purpose                   | Location                                |
| ------------------------------ | ------------------------- | --------------------------------------- |
| **IRepository<T>**             | Interface contracts       | Application.Ports.Data                  |
| **CosmosAccountRepository**    | Cosmos DB implementation  | Infrastructure.Data.Repositories.Cosmos |
| **PgAccountRepository**        | PostgreSQL implementation | Infrastructure.PostgreSQL.Repositories  |
| **DualWriteAccountRepository** | Coordinates both          | Infrastructure.Hybrid.DualWrite         |
| **DataSyncBackgroundService**  | Handles failed syncs      | Infrastructure.Hybrid.Sync              |
| **MigrationPhaseManager**      | Controls migration phase  | Infrastructure.Hybrid.Migration         |
| **CachedAccountRepository**    | Redis caching decorator   | Infrastructure.Redis                    |

This architecture:

1. **Keeps existing interfaces unchanged** - No breaking changes to consumers
2. **Separates concerns** - Each implementation in its own project
3. **Enables gradual migration** - Phase-based rollout
4. **Supports rollback** - Can revert to Cosmos-only at any time
5. **Adds caching** - Redis layer for performance
