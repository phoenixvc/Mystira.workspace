# Mystira Workspace Plan

**Last Updated**: 2026-03-10
**Branch**: `refactor/workspace-consolidation`
**Backlog**: [BACKLOG.md](../../BACKLOG.md)

---

## Overview

Three parallel streams of work, executed in dependency order. Stream A (workspace
consolidation) is in progress on the current branch. Streams B and C begin after
Stream A merges to main.

---

## Stream A: Workspace Consolidation

**Branch**: `refactor/workspace-consolidation`

Phase 0 (branch setup) and Phase 1 (folder restructure) are complete. Phases 2-7
eliminate duplicate domain stacks left from an incomplete migration.

**Rules**: Never delete without confirming functionality exists elsewhere. Each phase
is one atomic commit. Gates are mandatory.

| Step | What                                                                                              | Key Files                                                                                                                                           | Gate                                               |
| ---- | ------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| A.0  | Pop `stash@{0}` — 24-file dep update (NuGet, SDK, Dockerfiles)                                    | `global.json`, ~20 `.csproj`                                                                                                                        | `dotnet build` succeeds                            |
| A.1  | Entity Base Class Consolidation — audit 4 Entity impls, migrate all consumers to Domain           | `packages/shared/Mystira.Shared/Data/Entities/`, `packages/domain/Mystira.Domain/Entities/`, `apps/app/src/Mystira.App.Domain/Models/BaseEntity.cs` | Zero refs to `Mystira.Shared.Data.Entities.Entity` |
| A.2  | Port Interface Consolidation — reconcile 50+ duplicated ports                                     | `apps/app/src/Mystira.App.Application/Ports/` vs `packages/application/Mystira.Application/Ports/`                                                  | Zero duplicate port refs                           |
| A.3  | Domain Model Consolidation — merge Account, Scenario, GameSession, Badge, UserProfile, MediaAsset | Domain versions are richer; App versions are DTOs. Keep Domain, migrate App consumers                                                               | All duplicates documented + build passes           |
| A.4  | CQRS Handler Deduplication — merge enhanced handlers into workspace                               | `apps/app/src/Mystira.App.Application/CQRS/` vs `packages/application/Mystira.Application/CQRS/`                                                    | No business logic lost                             |
| A.5  | Auth Extraction + Identity Decoupling — create `packages/contracts/auth/`                         | Decouple Identity.Api from App.Application, add missing infra (PendingSignupRepository, AzureEmailService)                                          | Identity builds without App.Application refs       |
| A.6  | Final Cleanup + PR to main                                                                        | Audit remaining App.Application, clean configs, CI pass                                                                                             | Full build+test suite green                        |

### Post-Consolidation Architecture

```text
apps/                              # 6 deployable groups
  admin/{api,ui}                   # .NET API + React UI
  app/                             # .NET API + Blazor PWA
  devhub/                          # Tauri desktop (Rust + .NET)
  identity/                        # .NET auth API
  publisher/                       # React UI
  story-generator/                 # .NET API

packages/                          # 14 library packages
  ai, api-spec, application, authoring, chain, contracts,
  design-tokens, domain, infrastructure (7 csproj),
  shared (kernel), shared-graph, shared-messaging,
  shared-observability, shared-ts, tests
```

**Dependency flow**: Apps → Application → Domain. Infrastructure → Application (ports). Shared-\* → Domain.

---

## Stream B: Infrastructure

Begins after Stream A merges to main.

| Step | What                                                                                                                                                                | Archived Reference                                                 |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| B.1  | Terraform scale-down — align code with CLI changes already applied (AKS stopped, SKUs downsized)                                                                    | `docs/history/plans/2026-03-scaledown-plan.md`                     |
| B.2  | Terraform naming standardization (v2.2) — fix prefixes (remove "mystira" duplication), consolidate Log Analytics 3→1 (~$100/mo savings), consolidate email services | `docs/history/plans/2025-12-dev-resources-standardization-plan.md` |
| B.3  | Data migration — dev: direct cutover, staging: pg_dump/restore, prod: blue-green with automated rollback                                                            | `docs/history/plans/2025-12-data-migration-plan.md`                |
| B.4  | Terraform state splitting — monolithic per-environment state to product-based states with Terragrunt                                                                | `docs/history/plans/2025-12-terraform-migration-plan.md`           |

---

## Stream C: Service Migrations

Begins after Stream A merges to main. Mystira.Shared is an internal package
(ProjectReference, not NuGet).

| Step | What                                                                                                                                                                 |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C.1  | Each service adds Wolverine handlers + internal Mystira.Shared ProjectReference (priority: App → Admin.Api → StoryGenerator → Publisher → Chain → Admin.UI → DevHub) |
| C.2  | Cross-service integration — Wolverine + Azure Service Bus pub/sub, Redis cache invalidation, 178+ domain events across 26 categories                                 |
| C.3  | Performance baselines + load testing                                                                                                                                 |

---

## Dependencies

```text
Stream A ──► Stream B (requires clean main)
Stream A ──► Stream C (requires consolidated packages)
B.1 ──► B.2 ──► B.3 ──► B.4 (sequential within infra)
C.1 ──► C.2 ──► C.3 (sequential within services)
```

---

## Archived Plans

All previous planning documents have been archived to `docs/history/plans/`
with ARCHIVED headers. See that directory for historical context and detailed
implementation procedures referenced above.
