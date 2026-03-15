# Mystira Workspace Plan

**Last Updated**: 2026-03-15
**Branch**: `dev`
**Backlog**: [BACKLOG.md](../../BACKLOG.md)

---

## Overview

Three parallel streams of work, executed in dependency order.

Stream A (workspace consolidation) is functionally complete on `dev`, but `main`
is still behind and requires a final cleanup + merge.

Stream B (infrastructure) and Stream C (service migrations) are already partially
underway on `dev` and will be finalized after Stream A merges to `main`.

---

## Stream A: Workspace Consolidation

**Branch**: `refactor/workspace-consolidation` → `refactor/port-consolidation` → `dev`

Phase 0 (branch setup) and Phase 1 (folder restructure) are complete. Phases 2-7
eliminate duplicate domain stacks left from an incomplete migration.

**Rules**: Never delete without confirming functionality exists elsewhere. Each phase
is one atomic commit. Gates are mandatory.

| Step | What                                                           | Status                                                                                                                                                                                    | Gate                    |
| ---- | -------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------- |
| A.0  | Pop `stash@{0}` — 24-file dep update (NuGet, SDK, Dockerfiles) | **DONE** `921606886`                                                                                                                                                                      | `dotnet build` succeeds |
| A.1  | Project Restructure & Cleanup                                  | Standardized `packages/core`, `packages/domain`, `packages/shared`, `packages/infrastructure`. Removed all legacy `Mystira.Shared` duplication from `apps/app`.                           | **DONE**                |
| A.2  | Port Interface Consolidation                                   | Consolidated service (`IMessagingService`, `IBlobService`, etc.) and repository (`IAccountRepository`, etc.) ports into `Mystira.Core`. Legacy adapters in `apps/app` removed or bridged. | **DONE**                |
| A.3  | Domain Model Consolidation                                     | 539 files migrated to `Mystira.Domain` DDD types (value objects, definition models, EF mappings, tests). Legacy models in `apps/app` orphaned.                                            | **DONE**                |
| A.4  | CQRS Handler Deduplication                                     | Consolidated handlers for accounts, user profiles, etc. into `Mystira.Core` using Wolverine static handler pattern.                                                                       | **DONE**                |
| A.5  | Auth Extraction & Identity Decoupling                          | Extracted shared authentication logic into `Mystira.Shared`. Standardized JWT validation and identity gateway across all services.                                                        | **DONE**                |
| A.6  | Final Cleanup + PR to main                                     | Merge `dev` → `main` and remove remaining legacy duplicates under `apps/app` (e.g., `Mystira.App.Domain/Models`, `Mystira.App.Application` bridge where no longer needed).                | **NEXT**                |

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

| Step | What                                                                                                                            | Status          |
| ---- | ------------------------------------------------------------------------------------------------------------------------------- | --------------- |
| B.1  | Align code with CLI changes already applied (AKS stopped, SKUs downsized, .NET 10 upgrade).                                     | **DONE**        |
| B.2  | Terraform naming standardization (v2.2): fix prefixes (remove "mystira" duplication), consolidate Log Analytics 3→1.            | **DONE**        |
| B.3  | Terraform state split with Terragrunt: `shared-infra/` foundation + `products/*` per-product states.                            | **IN PROGRESS** |
| B.4  | Resource group reorganization (ADR-0017): core vs per-service RGs and cross-env shared RGs.                                     | **IN PROGRESS** |
| B.5  | Mystira resource consolidation: converge on Terragrunt `shared-infra/` + `products/*` entrypoints (avoid direct use of legacy). | **IN PROGRESS** |
| B.6  | Shared resource reuse: one-per-env Redis/Cosmos/Storage/LA where possible; products consume shared-infra outputs consistently.  | **IN PROGRESS** |
| B.7  | Edge/networking migration: DNS, Front Door, and networking moved out of legacy environments with minimal disruption.            | **NEXT**        |

Cost notes: see [infra-consolidation-cost-estimate.md](../analysis/infra-consolidation-cost-estimate.md).

Non-scope plan: [infra-edge-networking-plan.md](./infra-edge-networking-plan.md).

Operational notes:

- Pre-seed inventory snapshot: [azure-inventory-preseed.md](../analysis/azure-inventory-preseed.md)
- Terragrunt rationale + founder adoption: [terragrunt-for-founders.md](../analysis/terragrunt-for-founders.md)
- Other repo migration plan: [other-repo-migration-plan.md](./other-repo-migration-plan.md)
- DevHub: keep “infra validate/preview” UX aligned with Terragrunt stacks and shared-infra outputs
- Prod safety: Terragrunt blocks prod apply/destroy unless `ALLOW_PROD_APPLY=true`
- External infra standards to evaluate (post-stabilization): https://github.com/phoenixvc/azure-infrastructure, https://github.com/phoenixvc/phoenixvc-actions-runner

---

## Stream C: Service Migrations

Mystira.Shared is an internal package (ProjectReference, not NuGet).

Notes:

- Baseline approach: extract from the current Mystira identity/auth implementation first, then generalize behind a stable “login contract” (faster + fewer unknowns).
- Greenfield approach: only if you want a product-agnostic platform repo immediately; otherwise it’s slower and risks over-design.
- Safety: keep per-product app registrations/scopes even if the login API is shared, so tokens and permissions remain product-bounded.
- External platform candidates (post-stabilization): https://github.com/phoenixvc/ai-gateway (OpenAI-compatible gateway on Azure Container Apps), https://github.com/phoenixvc/pvc-costops-analytics (FinOps platform: ADX + Grafana + Terraform + FastAPI)
- ai-gateway integration plan: [ai-gateway-integration-plan.md](./ai-gateway-integration-plan.md)

| Step | What                                                                                                                                                                 | Status          |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------- |
| C.1  | Each service adds Wolverine handlers + internal Mystira.Shared ProjectReference (priority: App → Admin.Api → StoryGenerator → Publisher → Chain → Admin.UI → DevHub) | **IN PROGRESS** |
| C.2  | Cross-service integration: Wolverine + Azure Service Bus pub/sub, Redis cache invalidation, domain/integration event catalog                                         | **IN PROGRESS** |
| C.3  | Performance baselines + load testing                                                                                                                                 | **TODO**        |
| C.4  | Shared login platform: Entra-backed login API baseline for all products                                                                                              | **PLANNED**     |
| C.5  | Shared login UI kit: reusable auth screens/components package (optional)                                                                                             | **PLANNED**     |
| C.6  | AI platform adoption: adopt `phoenixvc/agentkit-forge` + `ai-gateway` (https://github.com/phoenixvc/ai-gateway) after stabilization                                  | **PLANNED**     |
| C.7  | PVC CostOps integration: integrate `pvc-costops-analytics` (https://github.com/phoenixvc/pvc-costops-analytics) after repo review                                    | **PLANNED**     |

---

## Dependencies

```text
Stream A ──► Stream B (requires clean main)
Stream A ──► Stream C (requires consolidated packages)
B.1 ──► B.2 ──► B.3 ──► B.4 (sequential within infra)
C.1 ──► C.2 ──► C.3 (sequential within services)
```

---

## Milestones (next)

- Adopt `phoenixvc/agentkit-forge` and `ai-gateway` once both repos are stable: https://github.com/phoenixvc/ai-gateway
- Integrate PVC CostOps (FinOps platform) after repo review: https://github.com/phoenixvc/pvc-costops-analytics
- Evaluate org-wide standards/tooling after stabilization: https://github.com/phoenixvc/azure-infrastructure, https://github.com/phoenixvc/phoenixvc-actions-runner
- Optional future eval (only if it fits Mystira’s direction): https://github.com/phoenixvc/cognitive-mesh, https://github.com/JustAGhosT/codeflow-engine, https://github.com/JustAGhosT/content_creation

---

## Archived Plans

All previous planning documents have been archived to `docs/history/plans/`
with ARCHIVED headers. See that directory for historical context and detailed
implementation procedures referenced above.

---

## Change Log (since `main@71bbb2368`)

- Date range: 2026-03-10..2026-03-14
- Unmerged commits on `dev`: 8 (including 1 merge commit)
- PR references in commit subjects: 5 (#642, #721, #722, #724, #735)
- Files changed vs `main`: 76
- Approx LOC delta vs `main`: +11308 / -2288 (net +9020)
- Estimated effort: ~2–4 engineer-days (infrastructure + build fixes + consolidation tail)

---

## Work Done Since Monorepo Migration

Baseline commit: `4767ce579` (chore(workspace): finalize monorepo migration cleanup)

See [monorepo-features-effort-summary.md](../analysis/monorepo-features-effort-summary.md) for a cross-project feature summary and per-project effort estimates.

### Change Summary

- Date range: 2025-12-20..2026-03-14
- Commits: 145 (including 18 merge commits)
- Files changed: 4375
- Approx LOC delta: +241856 / -158893 (net +82963)
- PR references in commit subjects: 70 (example: #605, #609, #610, #613, #614, #617, #618, #619, #620, #621, #631, #642, #735)

### Major Milestones (high-level)

- Workspace/Build: flattened submodules, moved deployables under `apps/`, standardized CI/CD + CODEOWNERS + quality gates.
- Shared packages: converted internal dependencies to ProjectReferences, split/merged shared packages (e.g., shared-ts consolidation), and aligned shared contracts.
- Platform upgrades: broad .NET 10 upgrade and dependency/security updates across .NET + Node + Rust surfaces.
- Infrastructure: introduced shared-infra vs product-layer Terragrunt structure; evolved Terraform modules toward shared resources (monitoring, data, ACR, comms).
- Mystira.App: completed migration phases 5–9, aligned API/PWA builds, removed/bridged duplicated ports and domain stacks, adopted shared auth/exception patterns.
- Admin (API/UI): migrated admin services into monorepo, standardized exception handling + resilience, and stabilized integration test infrastructure.
- Identity: migrated into monorepo and aligned shared authentication/authorization patterns for cross-service identity workflows.
- Story Generator: brought API into monorepo, expanded service wiring to shared auth + Wolverine patterns, and kept contract-driven API surfaces consistent.
- Publisher: migrated publisher UI and added/extended publisher API surface to support contributors/stories/users flows.
- Chain: migrated chain tooling, fixed CI lint/test stability for Python/Rust components, and aligned infra/service boundaries with shared resources.
- DevHub: imported and integrated Leptos/Tauri tooling into the workspace, aligning shared configs and build conventions.
