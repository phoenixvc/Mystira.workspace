# Mystira 2026 Roadmap

**Last Updated**: December 2025
**Status**: Active
**Owner**: jurie@phoenixvc.tech (Technical), eben@phoenixvc.tech (Business)

---

## Overview

This is the single source of truth for all Mystira platform development priorities in 2026. It consolidates all planning documents into one clear path forward.

---

## Current Status (December 2025)

### Completed

- **Infrastructure Foundation**: Azure resources migrated to v2.2 naming convention
- **ADR-0017**: 3-tier resource group organization implemented
- **Workload Identity**: All services use Azure managed identity
- **Microsoft Entra External ID**: Terraform modules ready (replacing B2C)
- **Azure AI Foundry**: Integrated with gpt-4o and gpt-4o-mini models
- **API Domain Standardization**: `{env}.{service}.mystira.app` pattern
- **Distributed CI Model**: Component repos handle dev CI workflows
- **Mystira.Shared Package**: Published to NuGet (v0.2.0+)
- **@mystira/core-types**: Published to NPM

### In Progress

- **Unified Type System (Migration 001)**: OpenAPI specs + code generation
- **Admin API/UI Extraction**: Phase 3 (70% complete)
- **Service Migrations**: Adopting Mystira.Shared across all services

---

## Q1 2026: Core Infrastructure

### Priority 1: Complete Unified Type System

| Task | Status | Owner |
|------|--------|-------|
| Migrate existing ErrorResponse types | Pending | TBD |
| Update consuming services to use core-types | Pending | TBD |
| CI/CD for OpenAPI spec validation | Done | - |
| Publish Mystira.Core NuGet package | Done | - |

### Priority 2: Polyglot Persistence (ADR-0014)

Implement hybrid data strategy for Cosmos DB to PostgreSQL migration.

| Task | Status | Details |
|------|--------|---------|
| Add Ardalis.Specification 8.0.0 | Pending | Application layer |
| Create PostgreSQL Infrastructure.Data project | Pending | Mystira.App |
| Implement DualWriteRepository pattern | Pending | Phase-aware routing |
| Redis caching layer | Pending | StackExchange.Redis |
| Polly v8 resilience pipelines | Done | Mystira.Shared.Resilience |

### Priority 3: Wolverine Event-Driven Architecture (ADR-0015)

Replace MediatR with Wolverine for unified messaging.

| Task | Status | Details |
|------|--------|---------|
| Azure Service Bus provisioning | Pending | Terraform module |
| Add Wolverine packages | Pending | All .NET projects |
| Define domain events | Pending | AccountCreated, SessionCompleted, etc. |
| Migrate simple query handlers | Pending | GetAccountById, etc. |
| Migrate command handlers | Pending | CreateAccount, etc. |

---

## Q2 2026: Service Migrations

### Complete Service Migrations

All services adopt Mystira.Shared infrastructure:

| Service | Priority | Key Changes | Status |
|---------|----------|-------------|--------|
| Mystira.App | High | PostgreSQL dual-write, Wolverine | Pending |
| Mystira.Admin.Api | High | Read-only PostgreSQL access | Pending |
| Mystira.StoryGenerator | Medium | Shared PostgreSQL, Wolverine events | Pending |
| Mystira.Publisher | Medium | Event subscription, blockchain | Pending |
| Mystira.Chain | Medium | gRPC endpoints, event subscription | Pending |
| Mystira.Admin.UI | Low | API contract updates | Pending |
| Mystira.DevHub | Low | Minimal changes | Pending |

### Cross-Service Integration

| Task | Status | Details |
|------|--------|---------|
| Publish/subscribe events via Service Bus | Pending | Wolverine |
| Cross-instance cache invalidation | Pending | Redis pub/sub |
| Service-to-service auth (Managed Identity) | Done | Terraform |

---

## Q3 2026: PostgreSQL Primary & Optimization

### Data Migration Completion

| Task | Status | Details |
|------|--------|---------|
| Switch read operations to PostgreSQL | Pending | Phase 2→3 |
| Data reconciliation validation | Pending | 99.99% accuracy |
| Disable Cosmos DB writes | Pending | Phase 3 |
| Archive Cosmos DB data | Pending | Blob storage |
| Remove Cosmos code paths | Pending | Cleanup |

### Performance & Monitoring

| Task | Status | Details |
|------|--------|---------|
| Unified monitoring dashboards | Pending | Azure Monitor |
| Cache hit/miss metrics | Pending | Application Insights |
| Query performance optimization | Pending | PostgreSQL indexes |
| Load testing | Pending | Performance baselines |

---

## Technical Debt & Bug Fixes

### Critical (Fix in Q1)

| Issue | Source | Description |
|-------|--------|-------------|
| `Guid.Parse` crashes on string IDs | Repository pattern | Use string IDs consistently |
| Missing `CancellationToken` propagation | Async methods | Add to all async signatures |
| Fire-and-forget patterns | Background services | Add proper error handling |
| Dual-write transaction safety | Data sync | Wrap in TransactionScope |

### High Priority

| Issue | Source | Description |
|-------|--------|-------------|
| Missing rollback procedures | Documentation | Create per-phase guides |
| Performance baselines missing | Monitoring | Establish before migration |
| Feature flag documentation | Operations | Create usage guides |

---

## Architecture Decisions

All architecture decisions are documented as ADRs:

| ADR | Status | Description |
|-----|--------|-------------|
| ADR-0014 | Active | Polyglot Persistence (Ardalis.Specification) |
| ADR-0015 | Active | Event-Driven Architecture (Wolverine) |
| ADR-0017 | Implemented | Resource Group Organization |
| ADR-0019 | Implemented | Dockerfile Location Standardization |
| ADR-0020 | Active | Package Consolidation |

See [docs/architecture/adr/](./docs/architecture/adr/) for full list.

---

## Success Metrics

### Q1 2026

- [ ] All services reference Mystira.Core types
- [ ] OpenAPI spec validation in CI/CD
- [ ] Wolverine packages added to all .NET projects

### Q2 2026

- [ ] All services migrated to Mystira.Shared patterns
- [ ] Event-driven messaging operational
- [ ] Cross-service integration tested

### Q3 2026

- [ ] PostgreSQL serving 100% reads
- [ ] Cosmos DB decommissioned
- [ ] 30% reduction in database costs

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
- [Implementation Roadmap](./docs/planning/implementation-roadmap.md) - Detailed technical phases
- [Operations Runbooks](./docs/operations/runbooks/) - Operational procedures
