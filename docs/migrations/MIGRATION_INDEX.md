# Mystira Migration Index

**Last Updated**: December 2026
**Status**: Active
**Owner**: jurie@phoenixvc.tech (Technical), eben@phoenixvc.tech (Business)

## Overview

This document serves as the single source of truth for all migration activities in the Mystira platform. It consolidates and cross-references all migration documentation to prevent redundancy and ensure clarity.

## Quick Status Dashboard

| Area | Status | Progress | Primary Doc |
|------|--------|----------|-------------|
| **Infrastructure Foundation** | ✅ Complete | 100% | [MIGRATION_SUMMARY.md](../../MIGRATION_SUMMARY.md) |
| **Unified Type System** | 🔄 In Progress | 70% | [001-extract-mystira-core.md](../architecture/migrations/001-extract-mystira-core.md) |
| **API Domain Standardization** | ✅ Complete | 100% | [004-unified-api-domains.md](../architecture/migrations/004-unified-api-domains.md) |
| **Admin API/UI Extraction** | 🔄 In Progress | 70% | [phases.md](../migration/phases.md) |
| **Service Migrations** | 📋 Planned | 30% | [README.md](./README.md) |
| **Data Architecture** | ⚠️ Needs Review | 60% | See note below |

> **⚠️ Data Architecture Note**: The `repository-architecture.md` document contains known issues identified in code reviews. See [Action Items](#action-items) below.

## Document Hierarchy

### Tier 1: Authoritative Guides (Single Source of Truth)

These documents are the canonical references for their respective areas:

| Document | Purpose | Status |
|----------|---------|--------|
| [004-unified-api-domains.md](../architecture/migrations/004-unified-api-domains.md) | Domain naming convention | ✅ Definitive |
| [phases.md](../migration/phases.md) | Admin API/UI extraction progress | 🔄 Active |
| [001-extract-mystira-core.md](../architecture/migrations/001-extract-mystira-core.md) | Unified type system | 🔄 Active |

### Tier 2: Service-Specific Guides

One guide per service, following consistent structure:

| Service | Guide | Status |
|---------|-------|--------|
| Mystira.App | [mystira-app-migration.md](./mystira-app-migration.md) | 🔄 In Progress |
| Mystira.Admin | [mystira-admin-migration.md](./mystira-admin-migration.md) | 🔄 In Progress |
| Mystira.StoryGenerator | [mystira-storygenerator-migration.md](./mystira-storygenerator-migration.md) | 🔄 In Progress |
| Mystira.Publisher | [mystira-publisher-migration.md](./mystira-publisher-migration.md) | 📋 Planned |
| Mystira.Chain | [mystira-chain-migration.md](./mystira-chain-migration.md) | 📋 Planned |
| Mystira.DevHub | [mystira-devhub-migration.md](./mystira-devhub-migration.md) | 📋 Planned |
| Admin UI | [mystira-admin-ui-migration.md](./mystira-admin-ui-migration.md) | 📋 Planned |

### Tier 3: Technical Reference (Supporting)

These provide implementation details:

| Document | Purpose | Notes |
|----------|---------|-------|
| [user-domain-postgresql-migration.md](../architecture/migrations/user-domain-postgresql-migration.md) | PostgreSQL schema | ✅ Reference |
| [repository-architecture.md](../architecture/migrations/repository-architecture.md) | Dual-DB pattern | ⚠️ Contains known bugs |
| Subproject guides in `subprojects/` | Implementation details | Supporting only |

### Tier 4: Analysis Documents (Archive)

These informed decisions but are not implementation guides:

| Document | Purpose | Action |
|----------|---------|--------|
| [code-review-improvements.md](../architecture/migrations/code-review-improvements.md) | Bug identification | → Action items extracted below |
| [pr-analysis-hybrid-data-strategy.md](../architecture/migrations/pr-analysis-hybrid-data-strategy.md) | PR review | → Action items extracted |
| [mpr-analysis-review.md](../architecture/migrations/mpr-analysis-review.md) | Doc review | → Action items extracted |
| [remaining-issues-and-opportunities.md](../architecture/migrations/remaining-issues-and-opportunities.md) | Issue tracking | → Action items extracted |

---

## Current Phase: Q1 2026

### Active Work Items

1. **Complete Migration 001**: Unified Type System with OpenAPI + Mystira.Core
   - [x] Create OpenAPI spec structure
   - [x] Create Mystira.Core C# package
   - [x] Create @mystira/core-types TypeScript package
   - [x] Update CI/CD workflows
   - [ ] Migrate existing ErrorResponse types
   - [ ] Update consuming services

2. **API Domain Standardization**
   - [x] Define naming convention
   - [x] Update OpenAPI specs
   - [x] Create migration guides
   - [ ] Update Terraform DNS records (if needed)
   - [ ] Update application configurations

3. **Admin API/UI Extraction (Phase 3+)**
   - See [phases.md](../migration/phases.md) for detailed checklist

---

## Action Items (From Analysis Docs)

### Critical (Fix Now)

| Issue | Source | Action | Owner |
|-------|--------|--------|-------|
| `Guid.Parse` crashes on string IDs | code-review-improvements.md | Fix in Repository pattern | TBD |
| Missing `CancellationToken` propagation | code-review-improvements.md | Add to async methods | TBD |
| `SyncItem` mutability issues | code-review-improvements.md | Make immutable or redesign | TBD |
| Fire-and-forget patterns | code-review-improvements.md | Add proper error handling | TBD |

### High Priority

| Issue | Source | Action | Owner |
|-------|--------|--------|-------|
| Repository architecture doc has bugs | mpr-analysis-review.md | Update doc or mark as draft | TBD |
| Wolverine dependency on PostgreSQL | ADR analysis | Document sequence | TBD |
| Feature flag documentation missing | remaining-issues.md | Create docs | TBD |

### Medium Priority

| Issue | Source | Action | Owner |
|-------|--------|--------|-------|
| 40% documentation redundancy | This analysis | Consolidate as planned | TBD |
| Missing rollback procedures | remaining-issues.md | Create per-phase | TBD |
| Missing performance baselines | remaining-issues.md | Establish baseline | TBD |

---

## Deprecated Documents

The following documents are superseded or archived:

| Document | Superseded By | Reason |
|----------|---------------|--------|
| `admin-api-extraction-plan.md` | `phases.md` | Duplicate content |
| `002-app-api-migration.md` | `004-unified-api-domains.md` | Less comprehensive |
| `003-story-generator-migration.md` | `004-unified-api-domains.md` | Less comprehensive |

---

## Migration Timeline

```
Q4 2025 (Completed)
├── Infrastructure foundation (Azure naming v2.2)
├── Package publishing (Mystira.Shared, Contracts)
└── Initial service migrations started

Q1 2026 (Current)
├── Migration 001: Unified Type System
├── API Domain Standardization
├── Admin API/UI Extraction (Phase 3+)
└── Documentation consolidation

Q2 2026 (Planned)
├── Complete service migrations (all 7 services)
├── Wolverine event bus migration
└── Cross-service integration

Q3 2026 (Planned)
├── Legacy domain sunset
├── Full PostgreSQL migration
└── Performance optimization
```

---

## Contacts

| Role | Contact | Scope |
|------|---------|-------|
| Technical Lead | jurie@phoenixvc.tech | Architecture, implementation |
| Founder/Business | eben@phoenixvc.tech | Decisions, priorities |

---

## Related Documents

- [ADR Index](../architecture/adr/) - Architectural Decision Records
- [Guides](../guides/) - Implementation guides
- [Operations](../operations/) - Runbooks and procedures
