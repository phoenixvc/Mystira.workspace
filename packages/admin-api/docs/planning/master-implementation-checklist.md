# Master Implementation Checklist

## Overview

This is the master checklist for the Mystira platform modernization initiative. It provides a single source of truth for tracking progress across all ADRs, migrations, and improvements.

**Last Updated**: 2024-12-22

---

## Quick Links

| Document                                                                                             | Description                                 |
| ---------------------------------------------------------------------------------------------------- | ------------------------------------------- |
| [Hybrid Data Strategy Roadmap](./hybrid-data-strategy-roadmap.md)                                    | Cosmos DB → PostgreSQL migration phases     |
| [ADR-0014 Implementation Roadmap](./adr-0014-implementation-roadmap.md)                              | Polyglot persistence framework              |
| [ADR-0015 Implementation Roadmap](./adr-0015-implementation-roadmap.md)                              | Wolverine event-driven architecture         |
| [Remaining Issues & Opportunities](../architecture/migrations/remaining-issues-and-opportunities.md) | All bugs, incomplete features, enhancements |
| [Code Review Improvements](../architecture/migrations/code-review-improvements.md)                   | Code-level fixes from review                |
| [**MPR Analysis Review**](../architecture/migrations/mpr-analysis-review.md)                         | **Critical review of all documentation**    |

---

## Progress Summary

| Area                | Total Tasks | Completed | Progress |
| ------------------- | ----------- | --------- | -------- |
| Critical Bugs       | 3           | 0         | 0%       |
| Medium Bugs         | 8           | 0         | 0%       |
| ADR-0014 (Polyglot) | 15          | 0         | 0%       |
| ADR-0015 (Events)   | 12          | 0         | 0%       |
| Data Migration      | 20          | 0         | 0%       |
| **Total**           | **58**      | **0**     | **0%**   |

---

# 🔴 CRITICAL: Must Complete First

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#-critical-bugs-must-fix-before-implementation)

### CRIT-1: Cosmos DB FindAsync Signature

- [ ] Update `FindAsync([id], ct)` to `FirstOrDefaultAsync(e => e.Id == id, ct)`
- [ ] Configure partition key in `OnModelCreating`
- [ ] Update all Cosmos repository implementations
- [ ] Test with actual Cosmos DB

### CRIT-2: PostgreSQL JSONB Serialization

- [ ] Add `UseJsonSerializationOptions` to Npgsql configuration
- [ ] Or add value converters per JSONB property
- [ ] Update `AccountConfiguration` with proper serialization
- [ ] Test JSON round-trip (serialize → store → deserialize)

### CRIT-3: Dual-Write Transaction Safety

- [ ] Wrap dual-write in `TransactionScope`
- [ ] Implement proper rollback on failure
- [ ] Add retry logic for transient failures
- [ ] Integration test dual-write atomicity

---

# Phase 1: Foundation (Weeks 1-4)

## 1.1 Infrastructure Setup

> Reference: [hybrid-data-strategy-roadmap.md](./hybrid-data-strategy-roadmap.md#phase-1-foundation-weeks-1-6)

### Terraform Modules

- [ ] PostgreSQL Flexible Server module created
- [ ] Redis Cache module created
- [ ] Azure Service Bus module created
- [ ] Key Vault secrets configured
- [ ] Network security groups updated
- [ ] Private endpoints configured (production)

### Database Setup

- [ ] PostgreSQL instance provisioned (dev/staging)
- [ ] EF Core migrations created
- [ ] Initial schema deployed
- [ ] Connection pooling configured (PgBouncer or built-in)
- [ ] Redis Cache provisioned

## 1.2 Ardalis.Specification Integration

> Reference: [adr-0014-implementation-roadmap.md](./adr-0014-implementation-roadmap.md#phase-1-ardalisspecification-integration-week-1-2)

- [ ] `Ardalis.Specification` package added
- [ ] `Ardalis.Specification.EntityFrameworkCore` added
- [ ] Base specifications created:
  - [ ] `AccountByEmailSpec`
  - [ ] `AccountByStatusSpec`
  - [ ] `ActiveAccountsSpec`
  - [ ] `PaginatedSpec<T>`
- [ ] `IReadRepository<T>` using specifications
- [ ] Existing queries migrated to specifications

## 1.3 Repository Pattern Updates

> Reference: [code-review-improvements.md](../architecture/migrations/code-review-improvements.md)

- [ ] Generic `IRepository<TEntity, TId>` interface
- [ ] `CosmosRepository<T>` base implementation
- [ ] `PgRepository<T>` base implementation
- [ ] `SaveChangesAsync` called properly (BUG-5 fix)
- [ ] String ID used consistently (BUG-1 fix)

---

# Phase 2: Dual-Write Implementation (Weeks 5-8)

> Reference: [hybrid-data-strategy-roadmap.md](./hybrid-data-strategy-roadmap.md#phase-2-migration-weeks-7-12)

## 2.1 Sync Queue Implementation

### In-Memory Queue (Development)

- [ ] `ISyncQueue` interface defined
- [ ] `InMemorySyncQueue` with `ConcurrentQueue<T>`
- [ ] Thread-safe operations verified
- [ ] Unit tests passing

### Redis Queue (Production)

- [ ] `RedisSyncQueue` implementation
- [ ] BRPOPLPUSH reliable pattern
- [ ] Processing/failed queues
- [ ] Dead letter handling
- [ ] Connection resilience

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#inc-1-sync-queue-redis-implementation-missing)

## 2.2 Background Sync Service

- [ ] `DataSyncBackgroundService` implemented
- [ ] Graceful shutdown handling
- [ ] Batch processing for efficiency
- [ ] Error handling and retry logic
- [ ] Metrics and logging

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#inc-2-datasyncbackgroundservice-incomplete)

## 2.3 Dual-Write Repository

- [ ] `DualWriteAccountRepository` created
- [ ] Phase-aware read/write logic
- [ ] Fire-and-forget fixed (BUG-3)
- [ ] Transaction support (CRIT-3)
- [ ] Cache invalidation on write

## 2.4 Cache Layer

- [ ] `CachedAccountRepository` decorator
- [ ] Redis configuration with namespaced keys (BUG-7 fix)
- [ ] Cache stampede prevention (MED-4)
- [ ] Distributed cache invalidation

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#med-4-redis-cache-key-race-condition)

---

# Phase 3: Wolverine Integration (Weeks 5-8)

> Reference: [adr-0015-implementation-roadmap.md](./adr-0015-implementation-roadmap.md)

## 3.1 Wolverine Setup

- [ ] Wolverine packages added to all projects
- [ ] Azure Service Bus provisioned
- [ ] Topics and subscriptions created
- [ ] Outbox tables migration added
- [ ] Common configuration created

## 3.2 Domain Events

- [ ] `AccountCreatedEvent` defined
- [ ] `AccountUpdatedEvent` defined
- [ ] `SessionCompletedEvent` defined
- [ ] `ScenarioPublishedEvent` defined
- [ ] Events published from services

## 3.3 Event Handlers

- [ ] Analytics event handlers
- [ ] Cache invalidation handlers
- [ ] Notification handlers (if applicable)
- [ ] Idempotency handling

---

# Phase 4: MediatR Migration (Weeks 9-12)

> Reference: [adr-0015-implementation-roadmap.md](./adr-0015-implementation-roadmap.md#phase-3-mediatr-handler-migration-week-5-8)

## 4.1 Query Handler Migration

- [ ] `GetAccountByIdHandler` → Wolverine
- [ ] `GetAccountByEmailHandler` → Wolverine
- [ ] `ListAccountsHandler` → Wolverine
- [ ] `GetProfileHandler` → Wolverine
- [ ] `GetScenarioHandler` → Wolverine

## 4.2 Command Handler Migration

- [ ] `CreateAccountHandler` → Wolverine
- [ ] `UpdateAccountHandler` → Wolverine
- [ ] `DeleteAccountHandler` → Wolverine
- [ ] `UpdateProfileHandler` → Wolverine
- [ ] `StartSessionHandler` → Wolverine
- [ ] `EndSessionHandler` → Wolverine
- [ ] `PublishScenarioHandler` → Wolverine

## 4.3 MediatR Removal

- [ ] All MediatR packages removed
- [ ] Pipeline behaviors → Wolverine middleware
- [ ] No `IRequest`/`IRequestHandler` remaining
- [ ] Controllers using `IMessageBus`

---

# Phase 5: PostgreSQL Primary (Months 4-6)

> Reference: [hybrid-data-strategy-roadmap.md](./hybrid-data-strategy-roadmap.md#phase-4-postgresql-primary-months-7-12)

## 5.1 Data Verification

- [ ] Reconciliation service implemented
- [ ] Data comparison reports generated
- [ ] Discrepancies resolved
- [ ] 99.99% data accuracy confirmed

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#inc-6-reconciliation-report-service-missing)

## 5.2 Traffic Cutover

- [ ] Feature flags for read source
- [ ] Gradual traffic shift (10% → 50% → 100%)
- [ ] Performance monitoring during shift
- [ ] Rollback tested and documented

## 5.3 Cosmos Deprecation

- [ ] Dual-write disabled
- [ ] Cosmos read-only mode
- [ ] Backup verification
- [ ] Archive strategy implemented
- [ ] Cost monitoring (RU reduction)

---

# Medium Priority: Should Fix

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#-medium-bugs-should-fix)

## Bug Fixes

- [ ] MED-1: SyncItem record immutability (`init` instead of `set`)
- [ ] MED-2: PostgreSQL email index with case-insensitive collation
- [ ] MED-3: `ConcurrentBag` → `ConcurrentQueue` for failed items
- [ ] MED-5: Pagination added to `ListAsync` methods
- [ ] MED-6: ILike SQL injection prevention
- [ ] MED-7: TimeProvider injection for testability
- [ ] MED-8: Optimistic concurrency (RowVersion/ETag)

---

# Incomplete Features

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#-incomplete-features)

- [ ] INC-1: Redis sync queue
- [ ] INC-2: Complete DataSyncBackgroundService
- [ ] INC-3: MigrationPhaseManager
- [ ] INC-4: Health checks per database
- [ ] INC-5: Batch migration tool
- [ ] INC-6: Reconciliation report service
- [ ] INC-7: Cross-instance cache invalidation (pub/sub)
- [ ] INC-8: Cosmos specification evaluator
- [ ] INC-9: UserProfile repository documentation
- [ ] INC-10: Terraform state backend
- [ ] INC-11: Kubernetes secrets integration
- [ ] INC-12: Monitoring dashboard

---

# Missing Documentation

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#-missing-documentation)

- [ ] DOC-1: Rollback procedures for each phase
- [ ] DOC-2: Operational runbook
- [ ] DOC-3: Performance baseline metrics
- [ ] DOC-4: Security considerations
- [ ] DOC-5: Disaster recovery (RTO/RPO)
- [ ] DOC-6: Developer onboarding guide
- [ ] DOC-7: API contract change documentation

---

# Enhancement Opportunities (Backlog)

> Reference: [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md#-enhancement-opportunities)

## Modern C# Features

- [ ] ENH-1: File-scoped namespaces in all new files
- [ ] ENH-2: Global usings in new projects
- [ ] ENH-3: Nullable enable all projects
- [ ] ENH-4: Raw string literals for SQL/JSON
- [ ] ENH-5: Pattern matching with switch expressions
- [ ] ENH-6: Records for all DTOs

## Architectural Enhancements

- [ ] ENH-7: Source generators for repositories
- [ ] ENH-8: Result<T, Error> pattern
- [ ] ENH-9: Strongly-typed IDs
- [ ] ENH-10: Generic host for sync service
- [ ] ENH-11: OpenTelemetry integration
- [ ] ENH-12: Feature flags via Azure App Config

## Performance

- [ ] ENH-13: Connection pooling optimization
- [ ] ENH-14: Read replicas
- [ ] ENH-15: Query plan caching

---

# Component Impact Summary

| Component              | Phase 1 | Phase 2 | Phase 3 | Phase 4 | Phase 5 |
| ---------------------- | ------- | ------- | ------- | ------- | ------- |
| Infrastructure         | ✅      | ✅      | ✅      | ⚪      | ⚪      |
| Mystira.App            | ✅      | ✅      | ✅      | ✅      | ✅      |
| Mystira.Admin.Api      | ⚪      | ✅      | ✅      | ✅      | ✅      |
| Mystira.Admin.UI       | ⚪      | ⚪      | ✅      | ✅      | ⚪      |
| Mystira.StoryGenerator | ⚪      | ⚪      | ✅      | ✅      | ✅      |
| Mystira.Publisher      | ⚪      | ⚪      | ⚪      | ✅      | ✅      |
| Mystira.Chain          | ⚪      | ⚪      | ⚪      | ✅      | ✅      |
| Mystira.DevHub         | ⚪      | ⚪      | ⚪      | ⚪      | ✅      |

**Legend**: ✅ = Primary focus, ⚪ = No changes

---

# Weekly Review Template

## Sprint: Week X

### Completed This Week

- [ ] Task 1
- [ ] Task 2

### In Progress

- [ ] Task 3 (70% complete)

### Blockers

- Blocker 1: Description

### Next Week Priorities

1. Priority 1
2. Priority 2

### Metrics

- Tasks completed: X
- Critical bugs remaining: Y
- Test coverage: Z%

---

# Sign-Off Checklist

## Phase Completion Sign-Off

### Phase 1 Complete

- [ ] All Phase 1 tasks marked complete
- [ ] Integration tests passing
- [ ] No critical bugs remaining
- [ ] Stakeholder review completed
- [ ] **Signed off by**: **\*\***\_**\*\*** **Date**: **\*\***\_**\*\***

### Phase 2 Complete

- [ ] Dual-write operational
- [ ] Data sync verified
- [ ] Performance baseline met
- [ ] **Signed off by**: **\*\***\_**\*\*** **Date**: **\*\***\_**\*\***

### Phase 3 Complete

- [ ] Wolverine handling all events
- [ ] MediatR fully removed
- [ ] Cross-service events working
- [ ] **Signed off by**: **\*\***\_**\*\*** **Date**: **\*\***\_**\*\***

### Phase 4 Complete

- [ ] PostgreSQL serving 100% reads
- [ ] Cosmos in read-only mode
- [ ] Zero data discrepancies
- [ ] **Signed off by**: **\*\***\_**\*\*** **Date**: **\*\***\_**\*\***

### Phase 5 Complete (Final)

- [ ] All enhancement opportunities reviewed
- [ ] Documentation complete
- [ ] Team trained
- [ ] Monitoring in place
- [ ] **Signed off by**: **\*\***\_**\*\*** **Date**: **\*\***\_**\*\***

---

## References

- [ADR-0013: Data Management and Storage Strategy](../architecture/adr/0013-data-management-and-storage-strategy.md)
- [ADR-0014: Polyglot Persistence Framework Selection](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [ADR-0015: Event-Driven Architecture Framework Selection](../architecture/adr/0015-event-driven-architecture-framework.md)
- [Repository Architecture](../architecture/migrations/repository-architecture.md)
