# Mystira 2026 Roadmap

**Last Updated**: December 2025
**Production Go-Live**: January 1, 2026
**Owner**: jurie@phoenixvc.tech (Technical), eben@phoenixvc.tech (Business)

---

## Overview

This is the single source of truth for all Mystira platform development. We go live January 1, 2026.

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
| Domain events defined | 🔄 Pending | AccountCreated, SessionCompleted, etc. |

### Week 3-4: Performance & Monitoring

| Task | Status | Details |
|------|--------|---------|
| Unified monitoring dashboards | 🔄 Pending | Azure Monitor |
| Cache hit/miss metrics | 🔄 Pending | Application Insights |
| Load testing | 🔄 Pending | Performance baselines |
| Production hardening | 🔄 Pending | Security review, rate limiting |

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

### Critical

| Issue | Description | Action |
|-------|-------------|--------|
| `Guid.Parse` crashes | String IDs cause crashes | Use string IDs consistently |
| Missing `CancellationToken` | Async methods missing ct | Add to all async signatures |
| Fire-and-forget patterns | No error handling | Add proper error handling |

### High Priority

| Issue | Description | Action |
|-------|-------------|--------|
| Rollback procedures | Missing per-phase guides | Create before launch |
| Performance baselines | No benchmarks | Establish before launch |

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
