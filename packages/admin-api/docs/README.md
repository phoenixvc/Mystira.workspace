# Mystira Admin API Documentation

This directory contains architecture, planning, and operational documentation for the Mystira Admin API.

## Directory Structure

```
docs/
├── README.md                    # This file
├── ALIGNMENT_REPORT.md          # Cross-repo alignment status
├── architecture/
│   ├── adr/                     # Architecture Decision Records
│   │   ├── 0006-admin-api-repository-extraction.md
│   │   ├── 0007-nuget-feed-strategy-for-shared-libraries.md
│   │   ├── 0013-data-management-and-storage-strategy.md
│   │   ├── 0014-polyglot-persistence-framework-selection.md
│   │   └── 0015-event-driven-architecture-framework.md
│   └── migrations/              # Migration guides and architecture docs
│       ├── mystira-app-admin-api-migration.md
│       ├── repository-architecture.md
│       ├── remaining-issues-and-opportunities.md
│       └── code-review-improvements.md
├── cicd/                        # CI/CD documentation
│   └── README.md
├── guides/                      # Developer guides
│   └── contracts-migration.md
├── planning/                    # Planning and roadmap documents
│   ├── master-implementation-checklist.md
│   ├── hybrid-data-strategy-roadmap.md
│   └── admin-api-extraction-plan.md
└── operations/                  # Operational procedures
    ├── DATA_MIGRATION_PLAN.md
    ├── DEPLOYMENT_STRATEGY.md
    ├── TESTING_CHECKLIST.md
    └── ROLLBACK_PROCEDURE.md
```

## Quick Links

### Architecture

- **[ADR-0006: Admin API Repository Extraction](architecture/adr/0006-admin-api-repository-extraction.md)** - Decision to extract Admin API to separate repository
- **[ADR-0007: NuGet Feed Strategy](architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md)** - Internal package management strategy
- **[ADR-0013: Data Management Strategy](architecture/adr/0013-data-management-and-storage-strategy.md)** - Cosmos DB → PostgreSQL migration strategy
- **[ADR-0014: Polyglot Persistence](architecture/adr/0014-polyglot-persistence-framework-selection.md)** - Ardalis.Specification and EF Core patterns
- **[ADR-0015: Event-Driven Architecture](architecture/adr/0015-event-driven-architecture-framework.md)** - Wolverine framework selection

### Migration

- **[Admin API Migration Guide](architecture/migrations/mystira-app-admin-api-migration.md)** - Specific changes for Admin API
- **[Repository Architecture](architecture/migrations/repository-architecture.md)** - Repository patterns and dual-write strategy
- **[Issues & Opportunities](architecture/migrations/remaining-issues-and-opportunities.md)** - Known issues and enhancement opportunities
- **[Code Review Improvements](architecture/migrations/code-review-improvements.md)** - Bug fixes and modern C# improvements

### Planning

- **[Master Implementation Checklist](planning/master-implementation-checklist.md)** - Complete task tracking
- **[Hybrid Data Strategy Roadmap](planning/hybrid-data-strategy-roadmap.md)** - Phased migration plan
- **[Admin API Extraction Plan](planning/admin-api-extraction-plan.md)** - Step-by-step extraction process

### Guides

- **[Contracts Migration Guide](guides/contracts-migration.md)** - Migrating to unified @mystira/contracts packages

### CI/CD

- **[CI/CD Documentation](cicd/README.md)** - Workflows, secrets, local development
- **[Alignment Report](ALIGNMENT_REPORT.md)** - Cross-repository alignment status

### Operations

- **[Data Migration Plan](operations/DATA_MIGRATION_PLAN.md)** - Production data migration procedures
- **[Deployment Strategy](operations/DEPLOYMENT_STRATEGY.md)** - Deployment approach and environments
- **[Testing Checklist](operations/TESTING_CHECKLIST.md)** - Pre-deployment testing requirements
- **[Rollback Procedure](operations/ROLLBACK_PROCEDURE.md)** - Emergency rollback steps

## Current Status

| Phase | Status | Description |
|-------|--------|-------------|
| Phase 1: Extraction | ✅ Complete | Repository created, NuGet packages configured |
| Phase 2: CI/CD | ✅ Complete | GitHub Actions workflows in place |
| Phase 3: PostgreSQL Setup | 🔄 In Progress | Adding dual-write infrastructure |
| Phase 4: Wolverine Events | ⏳ Pending | Event-driven architecture |
| Phase 5: Production Cutover | ⏳ Pending | Full PostgreSQL migration |

## Key Differences from Mystira.App.Api

| Aspect | Public API | Admin API |
|--------|------------|-----------|
| User Data | Read/Write | **Read-Only** |
| Content Data | Read-Only | **Read/Write** |
| Migration Phase | Full dual-write | Read-only from PostgreSQL |
| Redis Caching | User sessions | Content caching |
| Security | Public auth | Admin/SuperAdmin roles |

## Related Repositories

- **[Mystira.App](https://github.com/phoenixvc/Mystira.App)** - Source of shared NuGet packages
- **[Mystira.Admin.UI](https://github.com/phoenixvc/Mystira.Admin.UI)** - Admin frontend application
- **[mystira.workspace](https://github.com/phoenixvc/mystira.workspace)** - Mono-repo workspace
