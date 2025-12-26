# Mystira 2026 Roadmap

**Last Updated**: December 2025
**Production Go-Live**: January 1, 2026
**Owner**: jurie@phoenixvc.tech (Technical), eben@phoenixvc.tech (Business)

---

## Overview

This is the single source of truth for all Mystira platform development. We go live January 1, 2026.

---

## 2-Week Action Plan (Dec 26 - Jan 8)

### This Repo (Mystira.workspace)

| Task | Owner | Status |
|------|-------|--------|
| ✅ Consolidate redundant planning docs | - | Done |
| ✅ Create @mystira/core-types package | - | Done |
| ✅ Update all migration guides with correct versions | - | Done |
| ✅ Define domain events schema (AccountCreated, SessionCompleted, etc.) | - | Done |
| ✅ Create rollback procedures per service | - | Done |
| ✅ Finalize OpenAPI specs (events.yaml added) | - | Done |
| ✅ Add pre-launch patterns (AsyncExtensions, EntityId, RateLimiting) | - | Done |
| ✅ Update to Mystira.Shared 0.2.0-alpha | - | Done |

### Service Repos (in priority order)

| Service | Action | Target |
|---------|--------|--------|
| Mystira.App | Add Mystira.Shared 0.2.0-alpha, implement Wolverine handlers | Week 1 |
| Mystira.Admin.Api | Add Mystira.Shared 0.2.0-alpha, enable PostgreSQL read | Week 1 |
| Mystira.StoryGenerator | Add Mystira.Shared 0.2.0-alpha, Wolverine events | Week 2 |
| Mystira.Publisher | Add Service Bus subscription, event handlers | Week 2 |
| Mystira.Chain | gRPC endpoints, event subscription | Week 2 |
| Mystira.Admin.UI | Complete Phase 3, test auth flow | Week 2 |
| Mystira.DevHub | Minimal - update if needed | As needed |

### Critical Pre-Launch (patterns now in Mystira.Shared 0.2.0)

- [x] Fix `Guid.Parse` crashes → Use `EntityId` class (string IDs)
- [x] Add `CancellationToken` → Use `AsyncExtensions.EnsureToken()`
- [x] Replace fire-and-forget → Use `AsyncExtensions.SafeExecuteAsync()`
- [x] Security review → Use `RateLimitingMiddleware`
- [ ] Create performance baselines (load testing) - remaining

---

## Current Status (December 2025)

### Infrastructure - DONE ✅

| Component | Status | Details |
|-----------|--------|---------|
| Azure Resources v2.2 | ✅ Done | Naming convention migrated |
| ADR-0017 Resource Groups | ✅ Done | 3-tier organization |
| Workload Identity | ✅ Done | Managed identity for all services |
| Microsoft Entra External ID | ✅ Done | Terraform modules (replacing B2C) |
| Azure AI Foundry | ✅ Done | gpt-4o and gpt-4o-mini integrated |
| API Domain Pattern | ✅ Done | `{env}.{service}.mystira.app` |
| Distributed CI Model | ✅ Done | Component repos handle dev CI |
| Azure Service Bus | ✅ Done | Terraform module in all environments |
| Service-to-service Auth | ✅ Done | Managed Identity via Terraform |

### Mystira.Shared Package - DONE ✅

All core infrastructure is implemented in `packages/shared/Mystira.Shared`:

| Component | Version | Status |
|-----------|---------|--------|
| Polly v8 Resilience | 8.6.5 | ✅ Done |
| Wolverine + Azure Service Bus | 5.9.2 | ✅ Done |
| Ardalis.Specification | 9.3.1 | ✅ Done |
| Redis Caching | 9.0.11 | ✅ Done |
| Polyglot Repository | - | ✅ Done |
| Distributed Locking | - | ✅ Done |
| Source Generators | - | ✅ Done |
| Entity Framework Core | 9.0.11 | ✅ Done |

### OpenAPI & Type System - DONE ✅

| Component | Status | Details |
|-----------|--------|---------|
| OpenAPI specs | ✅ Done | `packages/api-spec/openapi/` |
| CI/CD spec validation | ✅ Done | `generate-contracts.yml` |
| ErrorResponse types | ✅ Done | `@mystira/core-types` |
| @mystira/core-types (NPM) | ✅ Done | `packages/core-types/` |

---

## January 2026: Production Launch

### Week 1-2: Service Migrations

All services adopt Mystira.Shared infrastructure:

| Service | Priority | Key Changes | Status |
|---------|----------|-------------|--------|
| Mystira.App | High | Add Mystira.Shared, Wolverine handlers | 🔄 Pending |
| Mystira.Admin.Api | High | Add Mystira.Shared, PostgreSQL read | 🔄 Pending |
| Mystira.StoryGenerator | Medium | Add Mystira.Shared, Wolverine events | 🔄 Pending |
| Mystira.Publisher | Medium | Event subscription via Service Bus | 🔄 Pending |
| Mystira.Chain | Medium | gRPC endpoints, event subscription | 🔄 Pending |
| Mystira.Admin.UI | Low | API contract updates | 🔄 Pending |
| Mystira.DevHub | Low | Minimal changes | 🔄 Pending |

### Week 2-3: Cross-Service Integration

| Task | Status | Details |
|------|--------|---------|
| Publish/subscribe events | 🔄 Pending | Wolverine + Azure Service Bus |
| Cache invalidation | 🔄 Pending | Redis pub/sub |
| Domain events defined | ✅ Done | 118 events across 20 categories (see events.yaml) |

### Week 3-4: Performance & Monitoring

| Task | Status | Details |
|------|--------|---------|
| Unified monitoring dashboards | ✅ Done | Terraform: Log Analytics + App Insights |
| Cache hit/miss metrics | ✅ Done | BusinessMetrics.cs + Application Insights |
| Alerting rules | ✅ Done | High error rate, slow response, exceptions, dependencies |
| Redis/ServiceBus monitoring | ✅ Done | Terraform metric alerts |
| Availability tests | ✅ Done | Synthetic monitoring per service |
| Load testing | 🔄 Pending | Performance baselines |
| Production hardening | ✅ Done | RateLimitingMiddleware in Mystira.Shared |

---

## Post-Launch (2026 TBD)

### Polyglot Persistence (NOT Full PostgreSQL Migration)

We're going **polyglot** - keeping both Cosmos DB and PostgreSQL based on use case:

| Database | Use Case | Status |
|----------|----------|--------|
| Cosmos DB | Complex documents, scenarios, sessions | Current |
| PostgreSQL | Relational data, analytics, reporting | Available |
| Redis | Caching, distributed locks, pub/sub | Available |

The `PolyglotRepository` in Mystira.Shared routes to appropriate database based on entity annotations.

### Future Enhancements

| Task | Timeline | Details |
|------|----------|---------|
| Data warehouse (Azure Synapse) | TBD | Analytics & reporting |
| ML recommendations | TBD | User preferences |
| Multi-region expansion | TBD | Geo-replication |

---

## Technical Debt (Fix in January)

### Critical - RESOLVED in Mystira.Shared 0.2.0 ✅

| Issue | Solution | Location |
|-------|----------|----------|
| `Guid.Parse` crashes | `EntityId` class with string IDs | `Mystira.Shared.Data.EntityId` |
| Missing `CancellationToken` | `EnsureToken()` extension | `Mystira.Shared.Extensions.AsyncExtensions` |
| Fire-and-forget patterns | `SafeExecuteAsync()` extension | `Mystira.Shared.Extensions.AsyncExtensions` |
| Rate limiting | `RateLimitingMiddleware` | `Mystira.Shared.Middleware.RateLimitingMiddleware` |

### High Priority

| Issue | Status | Action |
|-------|--------|--------|
| Rollback procedures | ✅ Done | `docs/operations/runbooks/service-rollback.md` |
| Performance baselines | 🔄 Pending | Establish before launch |

---

## Architecture Decisions

| ADR | Status | Description |
|-----|--------|-------------|
| ADR-0014 | ✅ Implemented | Polyglot Persistence (Ardalis.Specification) |
| ADR-0015 | ✅ Implemented | Event-Driven Architecture (Wolverine) |
| ADR-0017 | ✅ Implemented | Resource Group Organization |
| ADR-0019 | ✅ Implemented | Dockerfile Location Standardization |
| ADR-0020 | ✅ Implemented | Package Consolidation |

---

## Migration Guides

Each service has a detailed migration guide:

| Service | Guide | Priority |
|---------|-------|----------|
| Mystira.App | [mystira-app-migration.md](./docs/migrations/mystira-app-migration.md) | High |
| Mystira.Admin.Api | [mystira-admin-migration.md](./docs/migrations/mystira-admin-migration.md) | High |
| Mystira.StoryGenerator | [mystira-storygenerator-migration.md](./docs/migrations/mystira-storygenerator-migration.md) | Medium |
| Mystira.Publisher | [mystira-publisher-migration.md](./docs/migrations/mystira-publisher-migration.md) | Medium |
| Mystira.Chain | [mystira-chain-migration.md](./docs/migrations/mystira-chain-migration.md) | Medium |
| Mystira.Admin.UI | [mystira-admin-ui-migration.md](./docs/migrations/mystira-admin-ui-migration.md) | Low |
| Mystira.DevHub | [mystira-devhub-migration.md](./docs/migrations/mystira-devhub-migration.md) | Low |

---

## Success Criteria (January 1, 2026)

- [ ] All services using Mystira.Shared
- [ ] Wolverine event handlers operational
- [ ] Cross-service events working via Azure Service Bus
- [ ] Redis caching active
- [ ] Monitoring dashboards live
- [ ] Zero critical bugs

---

## Contact

| Role | Contact | Scope |
|------|---------|-------|
| Technical Lead | jurie@phoenixvc.tech | Architecture, implementation |
| Founder/Business | eben@phoenixvc.tech | Priorities, decisions |

---

## Related Documentation

- [Migration Index](./docs/migrations/MIGRATION_INDEX.md) - Detailed migration status
- [Architecture ADRs](./docs/architecture/adr/) - Decision records
- [Mystira.Shared](./packages/shared/Mystira.Shared/) - Core infrastructure package
- [OpenAPI Specs](./packages/api-spec/openapi/) - API specifications
