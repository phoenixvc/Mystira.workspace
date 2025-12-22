# Mystira Platform: Hybrid Data Strategy Roadmap

## Executive Summary

This roadmap outlines the implementation of ADR-0013 (Data Management and Storage Strategy) across all Mystira platform components. The strategy transitions from a Cosmos DB-only architecture to a hybrid polyglot persistence model using Cosmos DB, PostgreSQL, and Redis.

---

## Platform Components Overview

| Component | Repository | Tech Stack | Data Impact |
|-----------|------------|------------|-------------|
| **Mystira.App** | `phoenixvc/Mystira.App` | C# .NET 9 | **Primary** - User data migration |
| **Mystira.Admin.Api** | `phoenixvc/Mystira.Admin.Api` | C# ASP.NET Core | **High** - Read-only PostgreSQL access |
| **Mystira.Admin.UI** | `phoenixvc/Mystira.Admin.UI` | TypeScript, React | **Low** - API contract updates |
| **Mystira.StoryGenerator** | `phoenixvc/Mystira.StoryGenerator` | C# .NET | **Medium** - Already uses PostgreSQL |
| **Mystira.Publisher** | `phoenixvc/Mystira.Publisher` | TypeScript, Node | **Medium** - Event-driven integration |
| **Mystira.Chain** | `phoenixvc/Mystira.Chain` | Python, gRPC | **Low** - Blockchain data isolated |
| **Mystira.DevHub** | `phoenixvc/Mystira.DevHub` | TypeScript | **Low** - Developer tooling |
| **Infrastructure** | Workspace-native | Terraform, K8s | **High** - Database provisioning |

---

## Timeline Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                            IMPLEMENTATION TIMELINE                                │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│  SHORT-TERM (0-3 months)                                                         │
│  ════════════════════════                                                        │
│  Phase 1: Foundation           ████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░   │
│  • Infrastructure provisioning                                                   │
│  • Mystira.App repository refactor                                               │
│  • Dual-write implementation                                                     │
│                                                                                  │
│  MEDIUM-TERM (3-6 months)                                                        │
│  ════════════════════════                                                        │
│  Phase 2: Migration            ░░░░░░░░░░░░░░░░████████████████░░░░░░░░░░░░░░░  │
│  • Data sync & validation                                                        │
│  • Admin API PostgreSQL read                                                     │
│  • Event-driven architecture                                                     │
│                                                                                  │
│  Phase 3: Optimization         ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░████████████░░░  │
│  • Redis caching layer                                                           │
│  • Cross-service data contracts                                                  │
│  • Publisher/Chain integration                                                   │
│                                                                                  │
│  LONG-TERM (6-18 months)                                                         │
│  ═══════════════════════                                                         │
│  Phase 4: Evolution            ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░████ │
│  • PostgreSQL primary cutover                                                    │
│  • Cosmos DB decommission                                                        │
│  • Advanced analytics & ML                                                       │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

# SHORT-MEDIUM TERM (0-6 Months)

## Phase 1: Foundation (Weeks 1-6)

### 1.1 Infrastructure (Terraform)

**Location**: `infra/terraform/modules/`

| Task | Module | Priority | Effort |
|------|--------|----------|--------|
| ✅ Add PostgreSQL Flexible Server | `shared/postgresql` | P0 | Done |
| ✅ Add Redis Cache | `shared/redis` | P0 | Done |
| ✅ Configure Mystira.App connections | `mystira-app` | P0 | Done |
| ⬜ Add PostgreSQL to Story Generator module | `story-generator` | P1 | 1 day |
| ⬜ Add connection strings to Key Vault | `shared` | P1 | 0.5 day |
| ⬜ Configure network security (private endpoints) | `shared` | P2 | 2 days |
| ⬜ Add lifecycle policies for blob storage tiers | `mystira-app` | P1 | Done |

**Deliverables**:
- [ ] PostgreSQL Flexible Server (dev, staging, prod)
- [ ] Redis Cache (dev, staging, prod)
- [ ] Connection strings in Key Vault
- [ ] Private endpoint configuration
- [ ] Blob storage lifecycle rules

---

### 1.2 Mystira.App (Main Application)

**Repository**: `phoenixvc/Mystira.App`

#### 1.2.1 Project Structure Changes

```
Mystira.App/src/
├── Mystira.App.Application/           # MODIFY
│   └── Ports/Data/
│       ├── IRepository.cs             # Add CancellationToken, IEntity constraint
│       ├── IReadRepository.cs         # NEW: CQRS read interface
│       ├── IAccountQueryService.cs    # NEW: Read-only query service
│       └── IProfileQueryService.cs    # NEW: Read-only query service
│
├── Mystira.App.Domain/                # MODIFY
│   └── Models/
│       └── Base/
│           ├── Entity.cs              # Add audit fields
│           └── IEntity.cs             # NEW: Interface for entities
│
├── Mystira.App.Infrastructure.Data/   # MODIFY (rename repos)
│   └── Repositories/
│       ├── CosmosRepository.cs        # Rename from Repository.cs
│       └── CosmosAccountRepository.cs # Rename from AccountRepository.cs
│
├── Mystira.App.Infrastructure.PostgreSQL/  # NEW PROJECT
│   ├── PostgreSqlDbContext.cs
│   ├── Configuration/
│   │   └── AccountConfiguration.cs
│   ├── Repositories/
│   │   ├── PgRepository.cs
│   │   └── PgAccountRepository.cs
│   └── Migrations/
│       └── 20251222_InitialCreate.cs
│
├── Mystira.App.Infrastructure.Hybrid/     # NEW PROJECT
│   ├── DualWrite/
│   │   ├── DualWriteAccountRepository.cs
│   │   └── DualWriteUserProfileRepository.cs
│   ├── Sync/
│   │   ├── ISyncQueue.cs
│   │   ├── InMemorySyncQueue.cs
│   │   └── DataSyncBackgroundService.cs
│   └── Migration/
│       └── MigrationPhaseManager.cs
│
└── Mystira.App.Infrastructure.Redis/      # NEW PROJECT
    ├── RedisCacheService.cs
    └── CachedAccountRepository.cs
```

#### 1.2.2 Implementation Tasks

| Task | Location | Priority | Effort | Dependencies |
|------|----------|----------|--------|--------------|
| Create `Infrastructure.PostgreSQL` project | New project | P0 | 2 days | Terraform |
| Create `Infrastructure.Hybrid` project | New project | P0 | 2 days | PostgreSQL project |
| Create `Infrastructure.Redis` project | New project | P1 | 1 day | Redis infra |
| Rename Cosmos repositories | Infrastructure.Data | P0 | 0.5 day | None |
| Add `IEntity` interface | Domain | P0 | 0.5 day | None |
| Add `IReadRepository` interface | Application | P1 | 0.5 day | IEntity |
| Implement `PgAccountRepository` | PostgreSQL project | P0 | 2 days | EF Core setup |
| Implement `DualWriteAccountRepository` | Hybrid project | P0 | 2 days | Both repos |
| Add migration phase configuration | Api/Admin.Api | P0 | 1 day | None |
| Create EF Core migrations | PostgreSQL project | P0 | 1 day | Schema design |
| Add health checks | Api/Admin.Api | P1 | 0.5 day | All infra |

**Deliverables**:
- [ ] Three new infrastructure projects
- [ ] Dual-write repository pattern
- [ ] Phase-based DI registration
- [ ] EF Core migrations for PostgreSQL
- [ ] Health check endpoints

---

### 1.3 Mystira.Admin.Api

**Repository**: `phoenixvc/Mystira.Admin.Api`

| Task | Priority | Effort | Dependencies |
|------|----------|--------|--------------|
| Add PostgreSQL connection (read-only) | P1 | 1 day | Terraform |
| Implement `IAccountQueryService` | P1 | 1 day | Mystira.App contracts |
| Add migration status dashboard endpoint | P2 | 0.5 day | Phase config |
| Update health checks | P1 | 0.5 day | Redis, PostgreSQL |
| Add content caching with Redis | P2 | 1 day | Redis infra |

**Note**: Admin API only needs **read** access to PostgreSQL user data. Content management stays in Cosmos DB.

---

### 1.4 CI/CD Updates

**Location**: `.github/workflows/`

| Task | Workflow | Priority | Effort |
|------|----------|----------|--------|
| Add PostgreSQL service container for tests | `app-ci.yml` | P1 | 0.5 day |
| Add Redis service container for tests | `app-ci.yml` | P1 | 0.5 day |
| Update migration scripts | `deployment-*.yml` | P1 | 1 day |
| Add EF Core migration step | `deployment-*.yml` | P0 | 1 day |
| Add database backup before migration | `deployment-*.yml` | P1 | 0.5 day |

---

## Phase 2: Data Migration (Weeks 7-12)

### 2.1 Data Sync Implementation

| Task | Component | Priority | Effort |
|------|-----------|----------|--------|
| Implement sync queue (Redis-backed) | Mystira.App | P0 | 2 days |
| Create batch migration tool | Mystira.App | P0 | 3 days |
| Implement data validation service | Mystira.App | P0 | 2 days |
| Create reconciliation reports | Admin.Api | P1 | 2 days |
| Add sync metrics to App Insights | Mystira.App | P1 | 1 day |

### 2.2 Migration Phase Rollout

```
Phase 0: CosmosOnly (Current)
    ↓
Phase 1: DualWriteCosmosRead (Dev first, then Staging)
    ↓
Phase 2: DualWritePostgresRead (Staging validation)
    ↓
Phase 3: PostgresOnly (Production - Long Term)
```

| Environment | Phase 1 Start | Phase 2 Start | Notes |
|-------------|---------------|---------------|-------|
| Development | Week 7 | Week 9 | Fast iteration |
| Staging | Week 9 | Week 11 | Full validation |
| Production | Week 11 | TBD | Gradual rollout |

### 2.3 Mystira.Admin.UI Updates

**Repository**: `phoenixvc/Mystira.Admin.UI`

| Task | Priority | Effort |
|------|----------|--------|
| Add migration dashboard component | P2 | 2 days |
| Display sync queue status | P2 | 1 day |
| Add data comparison view | P3 | 2 days |
| Update account list to use new API | P1 | 1 day |

---

## Phase 3: Cross-Service Integration (Weeks 13-20)

### 3.1 Event-Driven Architecture

**Reference**: ADR-0015 (Wolverine Framework)

| Task | Component | Priority | Effort |
|------|-----------|----------|--------|
| Add Wolverine to Mystira.App | Mystira.App | P1 | 3 days |
| Define domain events contracts | Shared library | P0 | 2 days |
| Implement event publishing | Mystira.App | P1 | 2 days |
| Add event handlers in Publisher | Mystira.Publisher | P2 | 3 days |

#### Event Contracts (Shared NuGet Package)

```csharp
// Mystira.App.Contracts (New or existing)
namespace Mystira.App.Contracts.Events;

public record AccountCreatedEvent(string AccountId, string Email, DateTimeOffset CreatedAt);
public record ProfileUpdatedEvent(string ProfileId, string AccountId, string[] ChangedProperties);
public record SessionCompletedEvent(string SessionId, string ScenarioId, int Score);
public record ScenarioPurchasedEvent(string AccountId, string ScenarioId, decimal Amount);
```

### 3.2 Mystira.Publisher Integration

**Repository**: `phoenixvc/Mystira.Publisher`

| Task | Priority | Effort | Dependencies |
|------|----------|--------|--------------|
| Add event subscription (Azure Service Bus) | P2 | 2 days | Events infrastructure |
| Handle `ScenarioPurchasedEvent` | P2 | 2 days | Event contracts |
| Add blockchain publishing on purchase | P3 | 3 days | Chain integration |
| Implement retry with dead-letter queue | P2 | 1 day | Service Bus |

### 3.3 Mystira.StoryGenerator Integration

**Repository**: `phoenixvc/Mystira.StoryGenerator`

| Task | Priority | Effort | Dependencies |
|------|----------|--------|--------------|
| Share PostgreSQL connection with App | P2 | 1 day | Terraform |
| Add read access to user profiles | P3 | 1 day | Shared PostgreSQL |
| Implement user preference caching | P3 | 2 days | Redis |

**Note**: Story Generator already uses PostgreSQL. Ensure connection pooling and read replicas are configured properly.

### 3.4 Mystira.Chain Updates

**Repository**: `phoenixvc/Mystira.Chain`

| Task | Priority | Effort | Dependencies |
|------|----------|--------|--------------|
| Add gRPC endpoint for account verification | P3 | 2 days | None |
| Subscribe to `ScenarioPurchasedEvent` | P3 | 2 days | Event infrastructure |
| Implement Story Protocol registration | P3 | 3 days | Publisher integration |

---

# MEDIUM-LONG TERM (6-18 Months)

## Phase 4: PostgreSQL Primary (Months 7-12)

### 4.1 Full PostgreSQL Cutover

| Task | Component | Priority | Effort |
|------|-----------|----------|--------|
| Switch read to PostgreSQL (Phase 2→3) | Mystira.App | P1 | 1 day |
| Validate all queries work correctly | Mystira.App | P0 | 3 days |
| Disable Cosmos DB writes | Mystira.App | P1 | 1 day |
| Archive Cosmos DB data | Infrastructure | P2 | 2 days |
| Update disaster recovery plan | Documentation | P1 | 1 day |

### 4.2 Cosmos DB Decommission

| Task | Priority | Effort |
|------|----------|--------|
| Export all Cosmos data to blob storage | P1 | 2 days |
| Verify PostgreSQL data completeness | P0 | 2 days |
| Remove Cosmos DB resources (Terraform) | P2 | 1 day |
| Update documentation | P2 | 1 day |
| Remove Cosmos code paths | P3 | 3 days |

### 4.3 Advanced Caching

| Task | Component | Priority | Effort |
|------|-----------|----------|--------|
| Implement cache-aside pattern for all repos | Mystira.App | P2 | 3 days |
| Add distributed cache invalidation | Infrastructure | P2 | 2 days |
| Implement session caching | Mystira.App | P2 | 2 days |
| Add cache hit/miss metrics | Monitoring | P3 | 1 day |

---

## Phase 5: Platform Evolution (Months 12-18)

### 5.1 Analytics & Reporting

| Task | Component | Priority | Effort |
|------|-----------|----------|--------|
| Create read replicas for analytics | Terraform | P2 | 2 days |
| Implement data warehouse (Azure Synapse) | Infrastructure | P3 | 5 days |
| Add player behavior analytics | Admin.UI | P3 | 5 days |
| Create business intelligence dashboards | Admin.UI | P3 | 5 days |

### 5.2 Machine Learning Pipeline

| Task | Component | Priority | Effort |
|------|-----------|----------|--------|
| Export training data from PostgreSQL | StoryGenerator | P3 | 3 days |
| Implement user preference ML model | StoryGenerator | P3 | 10 days |
| Add personalized scenario recommendations | Mystira.App | P3 | 5 days |

### 5.3 Multi-Region Expansion

| Task | Component | Priority | Effort |
|------|-----------|----------|--------|
| Configure PostgreSQL geo-replication | Terraform | P3 | 3 days |
| Add Redis cluster for multi-region | Terraform | P3 | 2 days |
| Implement region-aware routing | Front Door | P3 | 2 days |

---

## Component Impact Matrix

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        COMPONENT IMPACT BY PHASE                                 │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│  Component          │ Phase 1 │ Phase 2 │ Phase 3 │ Phase 4 │ Phase 5           │
│  ═══════════════════╪═════════╪═════════╪═════════╪═════════╪═══════════════    │
│  Mystira.App        │ ████████│ ████████│ ████    │ ████████│ ████              │
│  Mystira.Admin.Api  │ ████    │ ████████│ ████    │ ████    │ ████              │
│  Mystira.Admin.UI   │ ░░      │ ████    │ ████    │ ░░      │ ████████          │
│  StoryGenerator     │ ░░      │ ░░      │ ████    │ ░░      │ ████████          │
│  Publisher          │ ░░      │ ░░      │ ████████│ ░░      │ ████              │
│  Chain              │ ░░      │ ░░      │ ████    │ ░░      │ ████              │
│  DevHub             │ ░░      │ ░░      │ ░░      │ ░░      │ ████              │
│  Infrastructure     │ ████████│ ████    │ ████    │ ████████│ ████████          │
│                                                                                  │
│  Legend: ████ Major changes  ░░ Minor/None                                      │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## Risk Mitigation

### Data Loss Prevention

| Risk | Mitigation | Owner |
|------|------------|-------|
| Sync failures during dual-write | Queue with retry, dead-letter monitoring | Platform Team |
| Data inconsistency | Reconciliation reports, automated alerts | Platform Team |
| Migration rollback needed | Keep Cosmos DB data, phase-based rollback | Platform Team |

### Performance Risks

| Risk | Mitigation | Owner |
|------|------------|-------|
| PostgreSQL query performance | Read replicas, query optimization, indexes | Platform Team |
| Redis cache stampede | Cache warming, probabilistic early expiration | Platform Team |
| Network latency | Private endpoints, connection pooling | Infrastructure |

### Operational Risks

| Risk | Mitigation | Owner |
|------|------------|-------|
| Team unfamiliar with PostgreSQL | Training, documentation, pair programming | Engineering Lead |
| Complex deployment process | Automated pipelines, feature flags | DevOps |
| Monitoring gaps | Unified dashboards, alerting rules | SRE |

---

## Success Metrics

### Phase 1-2 (Foundation & Migration)

- [ ] Zero data loss during sync
- [ ] < 5ms latency impact on reads
- [ ] 100% data reconciliation success
- [ ] All environments migrated to Phase 1

### Phase 3 (Integration)

- [ ] Event delivery > 99.9%
- [ ] Cross-service latency < 100ms
- [ ] Publisher blockchain integration working

### Phase 4 (PostgreSQL Primary)

- [ ] Cosmos DB fully decommissioned
- [ ] 30% reduction in database costs
- [ ] Query performance parity or better

### Phase 5 (Evolution)

- [ ] Multi-region failover < 30s
- [ ] ML recommendations improving engagement
- [ ] Analytics dashboards operational

---

## Dependencies Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           DEPENDENCY GRAPH                                       │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│  ┌──────────────┐                                                               │
│  │  Terraform   │◄─────────────────────────────────────────────────┐            │
│  │ (PostgreSQL) │                                                   │            │
│  │  (Redis)     │                                                   │            │
│  └──────┬───────┘                                                   │            │
│         │                                                           │            │
│         ▼                                                           │            │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐        │            │
│  │ Mystira.App  │────►│ Admin.Api    │────►│ Admin.UI     │        │            │
│  │ (Primary)    │     │ (Read-only)  │     │ (Dashboard)  │        │            │
│  └──────┬───────┘     └──────────────┘     └──────────────┘        │            │
│         │                                                           │            │
│         │ Events                                                    │            │
│         ▼                                                           │            │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐        │            │
│  │ Publisher    │────►│ Chain        │     │StoryGenerator│────────┘            │
│  │ (Events)     │     │ (Blockchain) │     │ (PostgreSQL) │                     │
│  └──────────────┘     └──────────────┘     └──────────────┘                     │
│                                                                                  │
│  ─────────────────────────────────────────────────────────────────              │
│  Shared: Mystira.App.Contracts (NuGet) - Event definitions                      │
│  Shared: @mystira/shared-utils (NPM) - TypeScript utilities                     │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## Quick Reference: What Changes Where

### Mystira.App (C# .NET)
- **3 new projects**: Infrastructure.PostgreSQL, Infrastructure.Hybrid, Infrastructure.Redis
- **Modified**: Domain entities (IEntity), Application ports (IReadRepository)
- **Renamed**: Cosmos repositories (prefix with "Cosmos")

### Mystira.Admin.Api (C# ASP.NET Core)
- **Add**: PostgreSQL DbContext (read-only, NoTracking)
- **Add**: IAccountQueryService, IProfileQueryService implementations
- **Add**: Migration status endpoints

### Mystira.Admin.UI (TypeScript React)
- **Add**: Migration dashboard component
- **Update**: Account list to use new API endpoints

### Mystira.Publisher (TypeScript Node)
- **Add**: Azure Service Bus event subscription
- **Add**: Event handlers for purchase events

### Mystira.StoryGenerator (C# .NET)
- **Add**: Shared PostgreSQL read access
- **Add**: Redis caching for user preferences

### Mystira.Chain (Python)
- **Add**: gRPC account verification endpoint
- **Add**: Event subscription for blockchain registration

### Infrastructure (Terraform)
- **Done**: PostgreSQL, Redis modules
- **Add**: Service Bus for events
- **Add**: Private endpoints
- **Add**: Read replicas (long-term)

---

## Related Documents

- [ADR-0013: Data Management and Storage Strategy](../architecture/adr/0013-data-management-and-storage-strategy.md)
- [ADR-0014: Polyglot Persistence Framework Selection](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [ADR-0015: Event-Driven Architecture Framework](../architecture/adr/0015-event-driven-architecture-framework.md)
- [Repository Architecture](../architecture/migrations/repository-architecture.md)
- [Mystira.App API Migration](../architecture/migrations/subprojects/mystira-app-api-migration.md)
- [Mystira.App Admin API Migration](../architecture/migrations/subprojects/mystira-app-admin-api-migration.md)
- [Code Review Improvements](../architecture/migrations/code-review-improvements.md)
