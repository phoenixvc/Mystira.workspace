# Mystira Workspace Backlog

Single source of truth for open work items in the Mystira monorepo.

## Active Work Items

### Documentation & Organization

- [ ] Update `.github/workflows/README.md` to reflect current 13 workflows
- [ ] Flatten workflow trigger table and add reusable workflow tracking
- [ ] Remove stale migration/review docs (consolidated into backlog)

### Features In Progress

- [ ] App test gaps refactor
- [ ] Devhub Leptos Tauri integration

### Dependencies (Renovate)

- [ ] rust:1.93-slim-bookworm
- [ ] python:3.14-slim
- [ ] python:3.11-slim
- [ ] github-actions updates
- [ ] node.js updates
- [ ] debian:bookworm-slim
- [ ] @testing-library/react to v16
- [ ] microsoft packages v3 (major)
- [ ] linting packages
- [ ] all non-major dependencies

### Build Performance (NEW P0 Priority)

- [ ] BUILD-1: Fix TypeScript localStorage DOM types in publisher package
- [ ] BUILD-2: Fix IAuthService.EnsureTokenValidAsync interface implementations
- [ ] BUILD-3: Fix turbo.json missing output paths for chain package
- [ ] BUILD-4: Enable MSBuild parallelism and CPU optimization
- [ ] BUILD-5: Optimize NuGet package caching and incremental builds
- [ ] BUILD-6: Audit and optimize .NET inter-project references
- [ ] BUILD-7: Enable deterministic builds and optimize Roslyn analyzers
- [ ] BUILD-8: Refine turbo.json caching strategy and output patterns
- [ ] BUILD-9: Implement selective build strategies for affected packages
- [ ] BUILD-10: Optimize task dependency graph for maximum parallelism
- [ ] BUILD-11: Standardize and optimize output directory structures
- [ ] BUILD-12: Optimize pnpm-workspace.yaml dependency resolution
- [ ] BUILD-13: Centralize common devDependencies and remove duplication
- [ ] BUILD-14: Create targeted build commands (full vs incremental)
- [ ] BUILD-15: Ensure optimal HMR configuration for dev cycles
- [ ] BUILD-16: Implement affected-only testing strategies
- [ ] BUILD-17: Configure CI/CD build caching and parallel execution

## Completed

- [x] Submodules converted to true monorepo structure
- [x] .NET 10.0 upgrade across all packages
- [x] ESLint converted to modular configuration format
- [x] Build time optimization plan integrated into backlog

## Notes

- All migration docs consolidated in `docs/migrations/`
- Package-specific CI runs via turbo in workspace root
- Release workflows run centrally from workspace
- Build performance prioritized as P0 due to blocking all development

This file consolidates:

- Open implementation work still remaining.
- Documentation cleanup/triage needed to align with current monorepo reality.

Completed items are intentionally omitted.

## Consolidated sources

- `docs/planning/master-implementation-checklist.md`
- `docs/planning/implementation-roadmap.md`

## Documentation reality cleanup queue

1. Delete/update stale workflow cleanup guidance and references
   - Delete candidate: `docs/app-workflow-cleanup.md` (completed)
   - Replace reference in: `docs/analysis/monorepo-migration-parity-matrix.md`
2. Revalidate date/status-heavy docs for present-day accuracy
   - `docs/setup/setup-status.md` (consolidated/deleted)
   - `docs/planning/master-implementation-checklist.md` (consolidated/deleted)
   - `docs/planning/implementation-roadmap.md` (consolidated/deleted)
3. Keep workflow docs aligned with actual workflow files
   - `.github/workflows/README.md`
   - `docs/cicd/README.md`

## P0: Build Performance & Developer Experience

_Build performance currently blocks all development work with failing builds and 146+ second build times._

- BUILD-1: Fix TypeScript localStorage DOM types in publisher package
- BUILD-2: Fix IAuthService.EnsureTokenValidAsync interface implementations across test projects
- BUILD-3: Fix turbo.json missing output paths for chain package build task
- BUILD-4: Enable MSBuild parallelism (/m switch) and optimize max CPU count
- BUILD-5: Optimize NuGet package caching and ensure proper --no-restore flag usage
- BUILD-6: Audit and optimize .NET inter-project references to reduce unnecessary rebuilds
- BUILD-7: Enable deterministic builds and optimize Roslyn analyzer usage
- BUILD-8: Refine turbo.json caching strategy and output patterns for better caching
- BUILD-9: Implement selective build strategies for affected packages only
- BUILD-10: Optimize task dependency graph for maximum parallelism
- BUILD-11: Standardize and optimize output directory structures across packages
- BUILD-12: Optimize pnpm-workspace.yaml for efficient dependency resolution
- BUILD-13: Centralize common devDependencies and remove duplication across packages
- BUILD-14: Create targeted build commands for different scenarios (full vs incremental)
- BUILD-15: Ensure optimal HMR configuration for faster development cycles
- BUILD-16: Implement affected-only testing strategies to reduce test execution time
- BUILD-17: Configure CI/CD build caching and parallel execution in pipelines

## P0: Critical correctness and migration safety

- CRIT-1: Cosmos FindAsync signature and partition key correctness
- CRIT-2: PostgreSQL JSONB serialization correctness
- CRIT-3: Dual-write transaction safety and rollback guarantees
- MED-6: ILike SQL injection hardening

## P0: Technical debt from migration docs (consolidated)

- BUG-A1: Sync service uses Guid.Parse on string IDs (will throw for ULID/non-Guid)
- BUG-A2: UpdateAsync return type mismatch across repositories (Task vs Task<T>)
- BUG-A3: SyncItem record has mutable properties with `set` instead of `init`
- BUG-A4: ConcurrentBag used for failed items (wrong ordering, use ConcurrentQueue)
- BUG-A5: Missing CancellationToken in domain methods (GetByEmail, GetByAuth0UserId)
- BUG-B1: No transaction around primary write + queue (data inconsistency risk)
- BUG-PR1: PostgreSQL connection string format (Trust Server Certificate case)
- BUG-PR2: Storage lifecycle rule prefix mismatch (blobs vs containers)
- INF-1: Redis cache key collisions (missing InstanceName prefix for env isolation)

## P1: Data migration completion

- Create EF Core migrations and deploy initial schema
- Implement Redis-backed sync queue
- Complete `DataSyncBackgroundService`
- Implement reconciliation/data comparison reporting
- Finalize phase-based read cutover and rollback-tested traffic shift
- Complete Cosmos deprecation steps (disable writes, archive, monitoring)

## P1: Eventing and handler migration

- Complete Wolverine setup (topics/subscriptions/outbox/common config)
- Migrate remaining query/command handlers to Wolverine
- Remove MediatR usage and pipeline behaviors where still present
- Ensure controller/message bus integration is complete

## P1: Library and pattern migrations

- **Ardalis.Specification 8.0.0 migration**: Implement specification pattern across all services
  - Status: Implementation Ready, detailed guide available
  - Reference: `docs/architecture/specifications/ardalis-specification-migration.md`
- **Contracts package consolidation**: Complete migration to unified @mystira/contracts packages
  - Status: Phase 1 Complete, migration period active
  - Reference: `docs/guides/contracts-migration.md`
  - Actions: Update imports, remove old packages, validate TypeScript/NuGet flows

## P1: Security and identity completion

- Deploy Entra ID app registrations
- Configure MSAL in Admin UI
- Add Microsoft.Identity.Web in Admin API
- Configure group-to-role mapping and Conditional Access policies
- Complete External ID tenant setup and social provider integration
- Test admin and consumer auth end-to-end
- Test service-to-service managed identity auth end-to-end
- Store social secrets in Key Vault
- Implement secrets rotation and access auditing
- Create compliance checklists and incident response procedures
- Complete IAM policy audit, least-privilege, RBAC, and access reviews

## P2: CI/CD and deployment hardening

- Implement consistent error handling across pipelines
- Add pipeline metrics/reporting and failure dashboards
- Expand CI test coverage and performance testing where missing
- Add automated promotion/rollback triggers and deployment notifications

## P2: Monitoring and operations

- Integrate all services with shared monitoring/log aggregation
- Build service health, infra, performance, and deployment dashboards
- Create service runbooks, troubleshooting, DR, escalation, and checklists

## P2: Documentation and API docs

- Audit all documentation for stale references and misplaced files
- Update cross-references and documentation templates/process
- Generate API docs, host them, and add integration guides/examples
- Create/finish operational runbook and rollback procedures

## P2: Dev resource naming standardization (consolidated)

- Apply v2.2 naming updates consistently in Terraform module prefixes
  (`core`, `chain`, `publisher`, `story`) where drift remains.
- Ensure shared Log Analytics workspace usage is enforced for chain/publisher
  modules (remove dedicated per-module workspace resources where still present).
- Confirm ACR naming/scope policy (env-specific vs shared-global) and encode in
  Terraform + docs.
- Consolidate duplicate email communication resources and document final target
  naming pattern.
- Validate migration safety for naming-related resource replacement
  (backup/restore, rollback, post-cutover verification).

## P2: Legacy migration plan hygiene

- Review `docs/operations/DATA_MIGRATION_PLAN.md` (draft, 2025-12) for current
  naming/command validity and either:
  - refresh to current infrastructure reality, or
  - archive and replace with concise current runbook.

## P3: Performance, DX, and advanced features

- Performance baseline analysis, bottleneck fixes, and benchmarks
- Auto-scaling/load testing/capacity planning and scaling runbooks
- Database optimization and maintenance procedures
- Local DX improvements (docker-compose, scripts, debugging, validation)
- Developer tools/automation/utilities and workflow docs
- Onboarding checklist/training/knowledge base
- Feature flags infrastructure and usage procedures
- Canary deployment infrastructure/workflows
- Chaos engineering experiments/runbooks
- _Build time optimization and performance improvements (see P0: Build Performance)_

## P3: C# modernization (consolidated from code-review-improvements.md)

- Use primary constructors (C# 12) for simple repository/service classes
- Use collection expressions `[]` instead of `new()` for empty collections
- Use `required` keyword for non-nullable entity properties
- Convert DTOs to records with positional parameters
- Add IEntity marker interface with string Id constraint
- Split IRepository into IReadRepository + IRepository (CQRS-friendly)
- Add IAsyncEnumerable streaming methods for large datasets
- Use TimeProvider for testable timestamp generation
- Consider strongly-typed IDs (AccountId, ProfileId structs) to prevent mixing
- Use source generators for mapper boilerplate reduction
- Proper nullable reference type annotations (string? vs required string)
- Add generic CachedRepository decorator with DI registration

## Outstanding backlog IDs from migration docs

- MED-1, MED-2, MED-3, MED-4, MED-5, MED-7, MED-8
- INC-1 through INC-12
- DOC-1 through DOC-7
- ENH-1 through ENH-15

## Deferred item from review

- Add options validation (deferred future PR)
- Consolidated from deleted `docs/reviews/pr-analysis-mystira-shared.md`

## Recently removed / already acted on

- `docs/analysis/package-inventory.md` (deleted)
- `docs/pr-analysis.md` (deleted)
- `configs/` override directory (deleted)
- `docs/app-workflow-cleanup.md` (deleted)
- `docs/architecture/migrations/remaining-issues-and-opportunities.md` (consolidated into backlog)
- `docs/reviews/pr-analysis-mystira-shared.md` (consolidated into backlog)
- 12 planning documents archived to `docs/history/plans/` (2026-03-10) — see `docs/planning/PLAN.md`

## Maintenance rule

Treat this file as the single active backlog artifact. Move all new open
items here and avoid creating parallel task trackers.
