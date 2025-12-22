# MPR (Migration Planning & Roadmap) Analysis Review

## Overview

This document provides a critical analysis of all migration planning documentation, identifying bugs, inconsistencies, missed opportunities, and areas for improvement.

**Documents Reviewed**:

- `hybrid-data-strategy-roadmap.md`
- `adr-0014-implementation-roadmap.md`
- `adr-0015-implementation-roadmap.md`
- `repository-architecture.md`
- `remaining-issues-and-opportunities.md`
- `code-review-improvements.md`
- `master-implementation-checklist.md`

---

## 🔴 CRITICAL BUGS (In Our Documentation)

### BUG-A1: Sync Service Uses Guid.Parse on String IDs

**Location**: `repository-architecture.md:579, 589`

**Issue**: We standardized on `string` IDs but sync service still parses to Guid:

```csharp
// BUG: Will throw FormatException for ULID or non-Guid strings
var pgAccount = await pg.Accounts.FindAsync(Guid.Parse(item.EntityId));
```

**Impact**: Sync service will crash for any entity with ULID or non-Guid ID.

**Fix**:

```csharp
var pgAccount = await pg.Accounts.FindAsync(item.EntityId);
// Or if PostgreSQL uses Guid:
if (Guid.TryParse(item.EntityId, out var guidId))
    var pgAccount = await pg.Accounts.FindAsync(guidId);
```

---

### BUG-A2: UpdateAsync Return Type Mismatch

**Location**: `repository-architecture.md:317-333` vs `mystira-app-infrastructure-data-migration.md:301-305`

**Issue**: Inconsistent return types across docs:

```csharp
// DualWriteAccountRepository says:
public async Task<Account> UpdateAsync(Account entity, CancellationToken ct)

// But PgRepository says:
public virtual async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
// Returns void!
```

**Impact**: Compilation error or runtime failure.

**Fix**: Standardize on `Task UpdateAsync` (no return) as EF Core's natural pattern. Return the passed entity if needed.

---

### BUG-A3: SyncItem Still Has Mutable Properties

**Location**: `repository-architecture.md:447-450`

**Issue**: Despite documenting this as MED-1 in remaining-issues.md, the code still has:

```csharp
public record SyncItem
{
    public int RetryCount { get; set; }  // Should be init!
    public DateTimeOffset? LastAttemptAt { get; set; }
    public string? LastError { get; set; }
}
```

**Impact**: Records with `set` break immutability guarantees. Concurrent access issues possible.

---

### BUG-A4: ConcurrentBag Still Used for Failed Items

**Location**: `repository-architecture.md:465`

**Issue**: Despite documenting this as MED-3:

```csharp
private readonly ConcurrentBag<SyncItem> _failed = [];
```

**Impact**: Failed items retried in wrong order. `ConcurrentBag` doesn't guarantee FIFO.

---

### BUG-A5: Missing CancellationToken in Domain Methods

**Location**: `repository-architecture.md:352-376`

**Issue**: Domain-specific methods missing CancellationToken:

```csharp
public async Task<Account?> GetByEmailAsync(string email)  // No CT!
public async Task<Account?> GetByAuth0UserIdAsync(string auth0UserId)  // No CT!
public async Task<bool> ExistsByEmailAsync(string email)  // No CT!
```

**Impact**: Cannot cancel long-running operations. Inconsistent with other methods.

---

### BUG-A6: Wolverine Outbox Tables Missing Required Columns

**Location**: `adr-0015-implementation-roadmap.md:141-168`

**Issue**: Manual SQL for Wolverine tables is incomplete. Wolverine requires additional columns:

```sql
-- Missing: scheduled_time, saga_id, exception_type, exception_message
```

**Fix**: Use Wolverine's built-in migration:

```csharp
opts.PersistMessagesWithPostgresql(connectionString);
// This auto-creates correct schema
```

---

## 🟠 MEDIUM BUGS

### BUG-B1: No Transaction Around Primary Write + Queue

**Location**: `repository-architecture.md:290-315`

**Issue**: If primary write succeeds but queue fails, data is inconsistent:

```csharp
var result = await _cosmosRepo.AddAsync(entity, ct);  // Succeeds
await QueueSecondaryWriteAsync(entity.Id, SyncOperation.Insert, ct);  // Fails!
return result;  // Returns success but secondary never synced
```

**Even with error logging**, the entity exists in Cosmos but not in queue. This violates eventual consistency.

**Fix**: Use TransactionScope or implement compensating transaction:

```csharp
using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
try
{
    var result = await _cosmosRepo.AddAsync(entity, ct);
    await _syncQueue.EnqueueAsync(item, ct);
    scope.Complete();
    return result;
}
catch
{
    // Both operations rolled back
    throw;
}
```

---

### BUG-B2: Fire-and-Forget Still Possible

**Location**: `repository-architecture.md:311, 330, 347`

**Issue**: Even with error handling, `Task.Run` creates untracked background work:

```csharp
_ = Task.Run(async () => await WriteToCosmosWithErrorHandlingAsync(entity, SyncOperation.Insert));
```

**Impact**: If app crashes during `Task.Run`, operation is lost. No visibility into pending operations.

**Better**:

```csharp
// Queue all secondary writes, even when PostgreSQL is primary
await _syncQueue.EnqueueAsync(new SyncItem { ... }, ct);
```

---

### BUG-B3: DateTime.UtcNow in Background Service

**Location**: `repository-architecture.md:532`

**Issue**: Uses `DateTime.UtcNow` instead of injected `TimeProvider`:

```csharp
item.LastAttemptAt = DateTime.UtcNow;  // Not testable
```

---

### BUG-B4: Wolverine Handler Logger Not Typed

**Location**: `adr-0015-implementation-roadmap.md:325`

**Issue**: Handler uses untyped `ILogger`:

```csharp
public static async Task HandleAsync(
    AccountCreatedEvent @event,
    IAnalyticsService analytics,
    ILogger logger)  // Should be ILogger<AccountEventHandlers>
```

---

### BUG-B5: Missing Event Versioning

**Location**: `adr-0015-implementation-roadmap.md:196-230`

**Issue**: Events have no version field:

```csharp
public sealed record AccountCreatedEvent
{
    // Missing: public int Version { get; init; } = 1;
}
```

**Impact**: Cannot evolve events without breaking consumers.

---

## 🟡 TIMELINE & COORDINATION ISSUES

### TIMELINE-1: Overlapping Phases

**Issue**: Roadmaps have conflicting timelines:

| Document                        | Week 5-8 Task             |
| ------------------------------- | ------------------------- |
| hybrid-data-strategy-roadmap    | Data sync validation      |
| adr-0014-implementation-roadmap | Enhanced caching layer    |
| adr-0015-implementation-roadmap | MediatR handler migration |

**All three require significant developer time in the same period.**

**Fix**: Sequence them:

1. Week 1-4: Infrastructure + Ardalis.Specification
2. Week 5-8: Dual-write + Polly + Initial Wolverine
3. Week 9-12: MediatR migration + Cross-service events
4. Week 13-16: PostgreSQL primary cutover + Cleanup

---

### TIMELINE-2: Dependencies Not Explicit

**Issue**: Wolverine depends on PostgreSQL for outbox, but ADR-0015 doesn't reference ADR-0014 completion.

**Fix**: Add explicit dependencies in master checklist:

```
[ ] Phase 1.4 (Wolverine outbox) requires Phase 1.2 (PostgreSQL)
[ ] Phase 3.1 (Cross-service events) requires Phase 2.2 (Sync queue stable)
```

---

## 🔵 MISSED OPPORTUNITIES

### MISS-A1: No Unit of Work Pattern

**Current**: Each repository operation is independent.

**Better**: Multiple operations in single transaction:

```csharp
public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }
    IProfileRepository Profiles { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// Usage:
await _uow.Accounts.AddAsync(account, ct);
await _uow.Profiles.AddAsync(profile, ct);
await _uow.SaveChangesAsync(ct);  // Single transaction
```

---

### MISS-A2: No Domain Events → Integration Events Separation

**Current**: Services publish integration events directly.

**Better**: Separate internal domain events from external integration events:

```csharp
// Domain event (internal)
public record AccountCreated(Account Account) : IDomainEvent;

// Integration event (external, serializable)
public record AccountCreatedIntegrationEvent(...) : IIntegrationEvent;

// Domain event handler creates integration event
public static class AccountCreatedHandler
{
    public static AccountCreatedIntegrationEvent Handle(AccountCreated @event)
        => new() { AccountId = @event.Account.Id, ... };
}
```

**Benefits**: Internal events can carry rich objects, external events are stable contracts.

---

### MISS-A3: No Cosmos Change Feed for Outbox

**Issue**: Cosmos DB doesn't support transactions with external systems.

**Current approach**: Queue secondary writes immediately.

**Better for eventual consistency**:

```csharp
// Write to Cosmos with outbox document
container.CreateTransactionalBatch(partitionKey)
    .CreateItem(account)
    .CreateItem(new OutboxMessage { Event = "AccountCreated", ... })
    .ExecuteAsync();

// Change Feed processor reads outbox and publishes to Service Bus
```

---

### MISS-A4: No Health-Based Routing

**Current**: Phase determines routing statically.

**Better**: Dynamic routing based on database health:

```csharp
public async Task<Account?> GetByIdAsync(string id, CancellationToken ct)
{
    var primaryHealth = await _healthCheck.CheckPrimaryAsync(ct);

    if (primaryHealth.Status == HealthStatus.Unhealthy)
    {
        _logger.LogWarning("Primary unhealthy, falling back to secondary");
        return await _secondaryRepo.GetByIdAsync(id, ct);
    }

    return await _primaryRepo.GetByIdAsync(id, ct);
}
```

---

### MISS-A5: No Correlation ID Propagation

**Current**: No way to trace a request across dual-write, cache, and events.

**Better**:

```csharp
public class CorrelationContext
{
    public static AsyncLocal<string?> CorrelationId { get; } = new();
}

// Middleware sets it from header
// All logs, events, and queue items include it
```

---

### MISS-A6: No Graceful Degradation Mode

**Current**: Dual-write either works or fails.

**Better**: Graceful degradation with shadow mode:

```csharp
public enum MigrationPhase
{
    CosmosOnly = 0,
    DualWriteCosmosRead = 1,
    DualWritePostgresRead = 2,
    PostgresOnly = 3,
    DegradedCosmosOnly = 10,  // Fallback if PostgreSQL down
    DegradedPostgresOnly = 11  // Fallback if Cosmos down
}
```

---

### MISS-A7: No API Versioning Strategy

**Issue**: As we migrate data models, API contracts may change.

**Should add**:

```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/accounts")]
public class AccountsController
```

---

### MISS-A8: No Feature Flags for Gradual Rollout

**Current**: Phase configured via appsettings.

**Better**: Azure App Configuration with feature flags:

```csharp
services.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString)
           .UseFeatureFlags(cfg => cfg.CacheExpirationInterval = TimeSpan.FromMinutes(1));
});

// Usage
if (await _featureManager.IsEnabledAsync("DualWriteEnabled"))
```

---

### MISS-A9: Missing Monitoring Dashboard Templates

**Current**: Metrics defined but no dashboards.

**Should add**: Azure Monitor workbook or Grafana dashboard JSON in repo:

```
infra/monitoring/
├── dashboards/
│   ├── data-sync-dashboard.json
│   ├── cache-performance-dashboard.json
│   └── event-bus-dashboard.json
└── alerts/
    ├── sync-failure-alert.json
    └── cache-miss-rate-alert.json
```

---

### MISS-A10: No Capacity Planning

**Current**: No mention of required resources.

**Should document**:

```
## Resource Requirements

| Phase | Cosmos RUs | PostgreSQL Tier | Redis Size |
|-------|------------|-----------------|------------|
| 1 | 1000 | B_Gen5_1 | C0 |
| 2 | 1000 | GP_Gen5_2 | C1 |
| 3 | 500 | GP_Gen5_2 | C2 |
| 4 | 0 | GP_Gen5_4 | C2 |
```

---

## 🟢 ARCHITECTURAL CONCERNS

### ARCH-1: No Security Considerations

**Missing**:

- Data encryption at rest
- TLS configuration
- Managed Identity for database connections
- Key Vault for connection strings
- Network isolation (Private Endpoints)
- Audit logging for data access

---

### ARCH-2: No Cost Analysis

**Issue**: Running dual databases doubles costs during migration.

**Should include**:

```
## Cost Comparison

| Phase | Monthly Cost | Duration | Total |
|-------|--------------|----------|-------|
| Current (Cosmos only) | $500 | - | - |
| Phase 1-2 (Dual) | $850 | 3 months | $2,550 |
| Phase 3 (PostgreSQL only) | $300 | Ongoing | $300/mo |

Net savings after 6 months: $500/mo
```

---

### ARCH-3: No Rollback Procedures

**Issue**: What if Phase 2 validation fails?

**Should document**:

```
## Rollback Procedure: Phase 2 → Phase 1

1. Set MigrationPhase = DualWriteCosmosRead
2. Stop DataSyncBackgroundService
3. Clear Redis cache: FLUSHDB
4. Verify all reads returning from Cosmos
5. Monitor for 24 hours
6. Resume sync service if data valid
```

---

### ARCH-4: No Load Testing Plan

**Issue**: No mention of load testing before cutover.

**Should include**:

```
## Load Testing Requirements

Before Phase 3 (PostgreSQL Read):
- 1000 concurrent users
- 50 requests/second
- P95 latency < 100ms
- Zero data inconsistencies

Tools: k6, Artillery, or Azure Load Testing
```

---

### ARCH-5: No Disaster Recovery Plan

**Issue**: No mention of backup/restore or RTO/RPO.

**Should document**:

```
## Disaster Recovery

| Database | Backup Frequency | Retention | RTO | RPO |
|----------|------------------|-----------|-----|-----|
| Cosmos | Continuous | 30 days | 4h | 0 |
| PostgreSQL | Daily + PITR | 35 days | 1h | 5min |
| Redis | None (cache) | N/A | 0 | N/A |
```

---

## Summary of Issues Found

| Category               | Count  | Severity   |
| ---------------------- | ------ | ---------- |
| Critical Bugs in Docs  | 6      | 🔴 High    |
| Medium Bugs            | 5      | 🟠 Medium  |
| Timeline Issues        | 2      | 🟡 Medium  |
| Missed Opportunities   | 10     | 🔵 Low-Med |
| Architectural Concerns | 5      | 🟢 Low-Med |
| **Total**              | **28** |            |

---

## Immediate Action Items

### Must Fix Before Implementation

1. [ ] Fix BUG-A1: Remove `Guid.Parse` from sync service
2. [ ] Fix BUG-A2: Standardize `UpdateAsync` return type
3. [ ] Fix BUG-A3/A4: Update `SyncItem` record and use `ConcurrentQueue`
4. [ ] Fix BUG-A5: Add `CancellationToken` to all methods
5. [ ] Fix BUG-A6: Use Wolverine's built-in PostgreSQL persistence
6. [ ] Fix BUG-B1: Add transaction around primary write + queue
7. [ ] Fix TIMELINE-1: Create sequenced implementation plan

### Should Address Soon

8. [ ] Add MISS-A5: Correlation ID propagation
9. [ ] Add MISS-A9: Monitoring dashboard templates
10. [ ] Add ARCH-1: Security considerations document
11. [ ] Add ARCH-3: Rollback procedures for each phase
12. [ ] Add ARCH-4: Load testing plan

### Nice to Have

13. [ ] Add MISS-A1: Unit of Work pattern
14. [ ] Add MISS-A2: Domain events separation
15. [ ] Add MISS-A8: Feature flags with Azure App Config
16. [ ] Add ARCH-2: Cost analysis
17. [ ] Add ARCH-5: Disaster recovery plan

---

## Revised Phase Timeline

Based on analysis, recommended sequencing:

```
Week 1-2:   Infrastructure (Terraform, PostgreSQL, Redis, Service Bus)
Week 3-4:   Ardalis.Specification + Polly resilience
Week 5-6:   Dual-write repositories + Sync queue
Week 7-8:   Initial Wolverine setup (parallel with sync stabilization)
Week 9-10:  MediatR handler migration (queries first)
Week 11-12: MediatR handler migration (commands)
Week 13-14: Cross-service events + Load testing
Week 15-16: PostgreSQL read validation
Week 17-18: PostgreSQL primary cutover
Week 19-20: Cosmos DB decommission + Cleanup
```

---

## See Also

- [Master Implementation Checklist](../../planning/master-implementation-checklist.md)
- [Remaining Issues & Opportunities](./remaining-issues-and-opportunities.md)
- [Code Review Improvements](./code-review-improvements.md)
