# Remaining Issues, Incomplete Features, and Improvement Opportunities

## Overview

This document catalogs all identified issues, incomplete implementations, and improvement opportunities across the hybrid data strategy migration documentation and proposed architecture.

---

## Issue Categories

| Category                     | Count | Severity |
| ---------------------------- | ----- | -------- |
| 🔴 Critical Bugs (Must Fix)  | 3     | High     |
| 🟠 Medium Bugs (Should Fix)  | 8     | Medium   |
| 🟡 Incomplete Features       | 12    | Medium   |
| 🔵 Missing Documentation     | 7     | Low      |
| 🟢 Enhancement Opportunities | 15    | Low      |

---

# 🔴 CRITICAL BUGS (Must Fix Before Implementation)

## CRIT-1: Cosmos DB EF Core FindAsync Signature Mismatch

**Location**: Multiple migration docs using `FindAsync([id], ct)`

**Issue**: Cosmos DB EF Core provider has different `FindAsync` signature than SQL Server:

```csharp
// This works for SQL Server EF Core:
await _dbSet.FindAsync([id], ct);

// Cosmos DB requires partition key:
await _dbSet.FindAsync(id, ct);  // No array, requires WithPartitionKey() configuration
```

**Fix Required**:

```csharp
// For Cosmos - configure partition key in OnModelCreating
entity.HasPartitionKey(a => a.Id);

// Or use LINQ query
return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
```

**Affected Files**:

- `mystira-app-infrastructure-data-migration.md`
- `repository-architecture.md`

---

## CRIT-2: PostgreSQL JSONB Serialization Not Configured

**Location**: `mystira-app-infrastructure-data-migration.md` - AccountConfiguration

**Issue**: PostgreSQL `jsonb` columns require explicit JSON serializer configuration for EF Core:

```csharp
// Current (incomplete):
builder.Property(a => a.Settings)
    .HasColumnType("jsonb");

// Missing: Serialization configuration
```

**Fix Required**:

```csharp
// Add in DbContext OnConfiguring or via NpgsqlDataSource:
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseNpgsql(connectionString, o =>
    {
        o.UseJsonSerializationOptions(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    });
}

// Or use value converter per property:
builder.Property(a => a.Settings)
    .HasColumnType("jsonb")
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
        v => JsonSerializer.Deserialize<AccountSettings>(v, (JsonSerializerOptions)null!)!);
```

---

## CRIT-3: Missing Database Transaction for Dual-Write

**Location**: `repository-architecture.md` - DualWriteAccountRepository

**Issue**: Dual-write operations don't use transactions, risking partial writes:

```csharp
// Current (no transaction):
var result = await _cosmosRepo.AddAsync(entity, ct);
await QueueSecondaryWriteAsync(entity.Id, SyncOperation.Insert, ct);
```

**Fix Required**:

```csharp
// Use TransactionScope or explicit transactions:
public async Task<Account> AddAsync(Account entity, CancellationToken ct = default)
{
    using var scope = new TransactionScope(
        TransactionScopeAsyncFlowOption.Enabled);

    try
    {
        var result = await _cosmosRepo.AddAsync(entity, ct);
        await _syncQueue.EnqueueAsync(new SyncItem {...}, ct);
        scope.Complete();
        return result;
    }
    catch
    {
        // Transaction will rollback automatically
        throw;
    }
}
```

---

# 🟠 MEDIUM BUGS (Should Fix)

## MED-1: SyncItem Record Has Mutable Properties

**Location**: `repository-architecture.md` - SyncItem

**Issue**: Record uses `set` on mutable properties, breaking immutability:

```csharp
public record SyncItem
{
    public int RetryCount { get; set; }  // Should be init or with expression
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? LastError { get; set; }
}
```

**Fix**:

```csharp
public record SyncItem
{
    // Use init for initial values
    public int RetryCount { get; init; }
    public DateTimeOffset? LastAttemptAt { get; init; }
    public string? LastError { get; init; }

    // Use with expression for updates:
    // var updated = item with { RetryCount = item.RetryCount + 1 };
}
```

---

## MED-2: Missing Index on PostgreSQL email Column

**Location**: `mystira-app-infrastructure-data-migration.md` - AccountConfiguration

**Issue**: Email index created without case-insensitive collation:

```csharp
// Current:
builder.HasIndex(a => a.Email);

// Issue: ILike queries won't use this index efficiently
```

**Fix**:

```csharp
// Use citext extension or create expression index:
builder.HasIndex(a => a.Email)
    .HasMethod("btree")
    .HasFilter("is_deleted = false");

// In migration:
migrationBuilder.Sql(
    "CREATE INDEX ix_accounts_email_lower ON accounts (LOWER(email)) WHERE is_deleted = false;");
```

---

## MED-3: ConcurrentBag Not Suitable for Failed Items Queue

**Location**: `repository-architecture.md` - InMemorySyncQueue

**Issue**: `ConcurrentBag<T>` doesn't preserve order, failed items may be retried in wrong order:

```csharp
private readonly ConcurrentBag<SyncItem> _failed = [];
```

**Fix**:

```csharp
private readonly ConcurrentQueue<SyncItem> _failed = new();

public void MoveToFailed(SyncItem item)
{
    _failed.Enqueue(item with { LastAttemptAt = DateTimeOffset.UtcNow });
}
```

---

## MED-4: Redis Cache Key Race Condition

**Location**: `repository-architecture.md` - CachedAccountRepository

**Issue**: Classic cache stampede problem - multiple requests can hit database simultaneously:

```csharp
// Multiple concurrent requests for same key:
// 1. Request A: cache miss → starts DB query
// 2. Request B: cache miss → starts DB query (duplicate!)
// 3. Request A: writes to cache
// 4. Request B: writes to cache (duplicate write!)
```

**Fix** (use lock or probabilistic early expiration):

```csharp
// Option 1: Distributed lock
public async Task<Account?> GetByIdAsync(string id, CancellationToken ct)
{
    var cacheKey = GetCacheKey(id);
    var cached = await _cache.GetStringAsync(cacheKey, ct);
    if (cached != null) return JsonSerializer.Deserialize<Account>(cached);

    // Use distributed lock for expensive operations
    await using var @lock = await _lockProvider.AcquireLockAsync(
        $"lock:{cacheKey}", TimeSpan.FromSeconds(5), ct);

    // Check cache again after acquiring lock
    cached = await _cache.GetStringAsync(cacheKey, ct);
    if (cached != null) return JsonSerializer.Deserialize<Account>(cached);

    var account = await _inner.GetByIdAsync(id, ct);
    if (account != null)
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(account), ct);

    return account;
}
```

---

## MED-5: Missing Pagination in List Methods

**Location**: Multiple repository interfaces

**Issue**: `ListAsync()` methods return all entities without pagination:

```csharp
Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken ct = default);  // No limit!
```

**Fix**:

```csharp
// Add pagination parameters
Task<IReadOnlyList<TEntity>> ListAsync(
    int skip = 0,
    int take = 100,
    CancellationToken ct = default);

// Or use specification with pagination
public class PaginatedSpec<T> : Specification<T>
{
    public PaginatedSpec(int page, int pageSize)
    {
        Query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}
```

---

## MED-6: ILike SQL Injection Risk

**Location**: `mystira-app-admin-api-migration.md` - PostgresAccountQueryService

**Issue**: Direct string interpolation in LIKE pattern:

```csharp
.Where(a => EF.Functions.ILike(a.Email, $"%{query}%"))  // query could contain % or _
```

**Fix**:

```csharp
// Escape special characters
private static string EscapeLikePattern(string input)
    => input.Replace("%", "\\%").Replace("_", "\\_");

.Where(a => EF.Functions.ILike(a.Email, $"%{EscapeLikePattern(query)}%"))
```

---

## MED-7: DateTimeOffset.UtcNow Used at Entity Creation

**Location**: `mystira-app-domain-migration.md` - Entity base class

**Issue**: Using `DateTimeOffset.UtcNow` directly makes testing difficult:

```csharp
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
```

**Fix**:

```csharp
// Inject TimeProvider (or set in SaveChanges interceptor)
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
    {
        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = _timeProvider.GetUtcNow();
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = _timeProvider.GetUtcNow();
        }
        return base.SavingChangesAsync(...);
    }
}
```

---

## MED-8: Missing Optimistic Concurrency Control

**Location**: All entity definitions

**Issue**: No `RowVersion`/`ETag` for concurrency control:

```csharp
public class Account : Entity
{
    // Missing: concurrency token
}
```

**Fix**:

```csharp
public abstract class Entity
{
    // PostgreSQL uses xmin system column OR explicit version
    public uint RowVersion { get; set; }

    // Cosmos uses ETag
    public string? ETag { get; set; }
}

// Configuration:
builder.Property(a => a.RowVersion)
    .IsRowVersion();  // PostgreSQL: uses xmin
```

---

# 🟡 INCOMPLETE FEATURES

## INC-1: Sync Queue Redis Implementation Missing

**Location**: `repository-architecture.md`

**Description**: Only `InMemorySyncQueue` is implemented. Production requires Redis-backed queue.

**Required Implementation**:

```csharp
public class RedisSyncQueue : ISyncQueue
{
    private readonly IConnectionMultiplexer _redis;
    private const string QueueKey = "mystira:sync:queue";
    private const string FailedKey = "mystira:sync:failed";
    private const string ProcessingKey = "mystira:sync:processing";

    public async Task EnqueueAsync(SyncItem item, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(item);
        await db.ListRightPushAsync(QueueKey, json);
    }

    public async Task<SyncItem?> DequeueAsync(CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        // Use BRPOPLPUSH for reliable queue pattern
        var value = await db.ListRightPopLeftPushAsync(QueueKey, ProcessingKey);
        return value.IsNull ? null : JsonSerializer.Deserialize<SyncItem>(value!);
    }
}
```

---

## INC-2: DataSyncBackgroundService Incomplete

**Location**: `repository-architecture.md`

**Missing**:

- Graceful shutdown handling
- Dead letter queue implementation
- Metrics and alerting integration
- Batch processing for efficiency

---

## INC-3: MigrationPhaseManager Not Implemented

**Location**: Referenced but not defined

**Required**:

```csharp
public interface IMigrationPhaseManager
{
    MigrationPhase CurrentPhase { get; }
    Task<bool> CanAdvancePhaseAsync(CancellationToken ct);
    Task AdvancePhaseAsync(CancellationToken ct);
    Task RollbackPhaseAsync(CancellationToken ct);
    event EventHandler<MigrationPhase>? PhaseChanged;
}
```

---

## INC-4: Health Checks Not Fully Documented

**Location**: Various migration docs mention health checks but don't specify implementation

**Required**: Per-database health checks with degraded state support

---

## INC-5: Batch Migration Tool Not Implemented

**Location**: Referenced in roadmap

**Required**: Tool to migrate existing Cosmos data to PostgreSQL in batches

---

## INC-6: Reconciliation Report Service Missing

**Location**: Referenced in Admin API migration

**Required**: Service to compare data between Cosmos and PostgreSQL

---

## INC-7: Cache Invalidation Across Instances

**Location**: `CachedRepository` only invalidates local cache

**Required**: Pub/sub based cache invalidation for multi-instance deployments

---

## INC-8: Specification Evaluator for Cosmos DB

**Location**: Ardalis.Specification may not fully support Cosmos LINQ

**Required**: Custom `ISpecificationEvaluator` for Cosmos-specific queries

---

## INC-9: User Profile Repository Not Documented

**Location**: All docs focus on Account, but UserProfile needs same treatment

---

## INC-10: Terraform State Management

**Location**: Infrastructure docs don't specify state backend

**Required**: Azure Storage backend configuration

---

## INC-11: Kubernetes Secrets Integration

**Location**: Connection strings in appsettings, should use K8s secrets

---

## INC-12: Monitoring Dashboard

**Location**: Metrics defined but no dashboard templates

**Required**: Azure Monitor workbook or Grafana dashboard

---

# 🔵 MISSING DOCUMENTATION

| ID    | Document                | Description                                    |
| ----- | ----------------------- | ---------------------------------------------- |
| DOC-1 | Rollback Procedures     | Step-by-step rollback for each migration phase |
| DOC-2 | Runbook                 | Operational procedures for common issues       |
| DOC-3 | Performance Baseline    | Expected latency/throughput metrics            |
| DOC-4 | Security Considerations | Encryption, access control, audit logging      |
| DOC-5 | Disaster Recovery       | RTO/RPO, backup/restore procedures             |
| DOC-6 | Developer Onboarding    | How to work with the new architecture          |
| DOC-7 | API Contract Changes    | Breaking changes for consumers                 |

---

# 🟢 ENHANCEMENT OPPORTUNITIES

## Modern C# Features Not Fully Applied

| ID    | Feature                | Where to Apply     |
| ----- | ---------------------- | ------------------ |
| ENH-1 | File-scoped namespaces | All new files      |
| ENH-2 | Global usings          | New projects       |
| ENH-3 | Nullable enable        | All projects       |
| ENH-4 | Raw string literals    | SQL queries, JSON  |
| ENH-5 | Pattern matching       | Switch expressions |
| ENH-6 | Records for DTOs       | All API contracts  |

## Architectural Enhancements

| ID     | Enhancement                        | Benefit                     |
| ------ | ---------------------------------- | --------------------------- |
| ENH-7  | Source generators for repositories | Reduce boilerplate          |
| ENH-8  | Result<T, Error> pattern           | Better error handling       |
| ENH-9  | Strongly-typed IDs                 | Compile-time safety         |
| ENH-10 | Generic host for sync service      | Better lifecycle management |
| ENH-11 | OpenTelemetry integration          | Distributed tracing         |
| ENH-12 | Feature flags (Azure App Config)   | Runtime phase control       |

## Performance Enhancements

| ID     | Enhancement               | Benefit                 |
| ------ | ------------------------- | ----------------------- |
| ENH-13 | Connection pooling config | Better resource usage   |
| ENH-14 | Read replicas             | Scale reads             |
| ENH-15 | Query plan caching        | Faster repeated queries |

---

# Priority Matrix

## Must Fix Before Phase 1

- [ ] CRIT-1: Cosmos FindAsync signature
- [ ] CRIT-2: PostgreSQL JSONB serialization
- [ ] CRIT-3: Dual-write transactions
- [ ] MED-6: ILike SQL injection

## Must Fix Before Phase 2

- [ ] MED-2: PostgreSQL email index
- [ ] MED-4: Cache stampede prevention
- [ ] MED-5: Pagination support
- [ ] INC-1: Redis sync queue

## Should Fix Before Production

- [ ] MED-1: SyncItem immutability
- [ ] MED-3: ConcurrentBag ordering
- [ ] MED-7: TimeProvider injection
- [ ] MED-8: Optimistic concurrency
- [ ] All INC-\* items

---

# Action Items Checklist

## Immediate (This Sprint)

- [ ] Fix CRIT-1, CRIT-2, CRIT-3
- [ ] Update repository-architecture.md with fixes
- [ ] Update infrastructure-data-migration.md with JSONB config
- [ ] Add pagination to interfaces

## Next Sprint

- [ ] Implement RedisSyncQueue
- [ ] Complete DataSyncBackgroundService
- [ ] Add health check implementations
- [ ] Create reconciliation service

## Backlog

- [ ] Add source generators for repository boilerplate
- [ ] Implement Result<T, Error> pattern
- [ ] Create operational runbook
- [ ] Build monitoring dashboard

---

## See Also

- [Code Review Improvements](./code-review-improvements.md)
- [ADR-0014 Implementation Roadmap](../planning/adr-0014-implementation-roadmap.md)
- [Hybrid Data Strategy Roadmap](../planning/hybrid-data-strategy-roadmap.md)
