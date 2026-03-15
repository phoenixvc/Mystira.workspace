# Monorepo Work Summary (Features + Effort)

This document summarizes notable features and modernization work across all projects since the monorepo migration, and provides a rough effort estimate broken down by project.

## Baseline + Method

- Baseline commit: `4767ce579` (chore(workspace): finalize monorepo migration cleanup)
- Range: `4767ce579..HEAD` (up to 2026-03-14 on `dev`)
- Metrics:
  - Commits per project are estimated from commits that touched files under that path.
  - LOC numbers use `git diff --numstat --no-renames`, which means large directory moves can look like huge “changes”.
  - Treat LOC as directional (especially for `apps/app` vs `packages/app`, and similar “packages → apps” moves).

## Quantitative Summary (by monorepo project path)

| Project                       | Commits | Insertions | Deletions | Changed | Effort (est.)                                |
| ----------------------------- | ------: | ---------: | --------: | ------: | -------------------------------------------- |
| apps/app                      |      15 |    218,698 |         0 | 218,698 | 12–25 days (move-heavy + consolidation tail) |
| packages/app                  |      23 |          0 |   176,821 | 176,821 | included above (mostly “moved out”)          |
| apps/story-generator          |       7 |     66,245 |         0 |  66,245 | 4–10 days                                    |
| packages/story-generator      |      15 |          0 |    63,654 |  63,654 | included above (mostly “moved out”)          |
| apps/devhub                   |       4 |     54,955 |         0 |  54,955 | 3–8 days                                     |
| packages/devhub               |      29 |          0 |    56,854 |  56,854 | included above (mostly “moved out”)          |
| docs                          |      23 |     38,326 |     6,730 |  45,056 | 3–7 days                                     |
| apps/admin/api                |       8 |     23,603 |         0 |  23,603 | 2–5 days                                     |
| packages/admin-api            |      18 |          0 |    25,334 |  25,334 | included above (mostly “moved out”)          |
| apps/admin/ui                 |       2 |     14,679 |         0 |  14,679 | 1–3 days                                     |
| packages/admin-ui             |      17 |          0 |    20,341 |  20,341 | included above (mostly “moved out”)          |
| apps/publisher/ui             |       2 |     11,747 |         0 |  11,747 | 1–3 days                                     |
| packages/publisher            |      14 |          0 |    12,753 |  12,753 | included above (mostly “moved out”)          |
| apps/identity                 |       8 |      1,488 |         0 |   1,488 | 1–2 days                                     |
| infra/terraform               |       9 |        946 |     1,604 |   2,550 | 1–3 days (but high leverage)                 |
| .github                       |      34 |      2,816 |     4,582 |   7,398 | 2–6 days                                     |
| packages/core                 |      11 |     19,228 |       908 |  20,136 | 2–6 days                                     |
| packages/shared               |      24 |      1,311 |    15,264 |  16,575 | 2–5 days                                     |
| packages/shared-observability |       2 |      7,505 |         0 |   7,505 | 1–2 days                                     |
| packages/shared-messaging     |       3 |      6,679 |         0 |   6,679 | 1–2 days                                     |
| packages/shared-ts            |       2 |      4,494 |         0 |   4,494 | 0.5–1.5 days                                 |
| packages/domain               |       6 |      1,043 |        34 |   1,077 | 0.5–1.5 days                                 |
| scripts                       |       9 |      1,691 |        10 |   1,701 | 0.5–1.5 days                                 |
| infra/other                   |      16 |        335 |        65 |     400 | 0.5–1.5 days                                 |
| apps/publisher/api            |       1 |        247 |         0 |     247 | 0.5–1 day                                    |
| packages/contracts            |      12 |        246 |       134 |     380 | 0.5–2 days                                   |
| packages/infrastructure       |      16 |        523 |       380 |     903 | 0.5–2 days                                   |
| packages/design-tokens        |       7 |        417 |       305 |     722 | 0.5–2 days                                   |
| packages/core-types           |       6 |          0 |     3,205 |   3,205 | included in shared-ts consolidation          |
| packages/shared-utils         |      12 |          0 |     1,555 |   1,555 | included in shared-ts consolidation          |
| packages/shared-graph         |       1 |      1,099 |         0 |   1,099 | 0.5–1.5 days                                 |
| packages/chain                |      10 |         47 |        24 |      71 | 0.5–1 day                                    |
| infra/kubernetes              |       1 |         22 |        17 |      39 | <0.5 day                                     |
| packages/api-spec             |       5 |          8 |         8 |      16 | <0.5 day                                     |
| packages/authoring            |       4 |          7 |         1 |       8 | <0.5 day                                     |
| packages/tests                |       3 |          3 |         3 |       6 | <0.5 day                                     |
| packages/ai                   |       4 |          3 |         2 |       5 | <0.5 day                                     |

## Feature / Work Summary (by former “repo”)

### Mystira.App (API + PWA)

Paths: `apps/app/*` (previously `packages/app/*`)

- App API now consumes workspace packages (`Mystira.Core`, `Mystira.Domain`, `Mystira.Shared`) instead of local duplicates.
- Auth + exception handling aligned to shared patterns (JWT validation, standardized error responses).
- UI work continued in the PWA (notably the hero redesign and portal visual work).
- Build stability improvements + .NET 10 upgrade alignment across the app solution.

### Mystira.Admin (Admin API + Admin UI)

Paths: `apps/admin/api/*`, `apps/admin/ui/*` (previously `packages/admin-api/*`, `packages/admin-ui/*`)

- Admin API exception migration to shared exceptions + global exception handler.
- Resilience patterns added/standardized (Polly v8 usage).
- Integration test/build stability fixes during migration.
- Admin UI moved into monorepo and aligned with workspace tooling.

### Mystira.Identity

Path: `apps/identity/*`

- Identity API aligned to shared auth primitives and Wolverine patterns already used across other services.
- Consolidated dependencies and configuration patterns for cross-service identity workflows.

### Mystira.StoryGenerator

Path: `apps/story-generator/*` (previously `packages/story-generator/*`)

- Service migrated into monorepo and aligned with shared auth + Wolverine usage.
- API/controller surfaces maintained during the move; contracts and wiring brought closer to the workspace conventions.

### Mystira.Publisher (Publisher UI + Publisher API)

Paths: `apps/publisher/src/*`, `apps/publisher/api/*` (previously `packages/publisher/*`)

- Publisher UI migrated into monorepo and dependency maintenance performed (front-end library updates).
- Publisher API surface added/expanded (contributors/stories/users endpoints introduced).

### Mystira.Chain

Path: `packages/chain/*` and related infra where applicable

- CI stability work (Python tooling/lint/test wiring) and workspace integration alignment.
- Continued convergence toward shared infra/service boundaries (shared monitoring, shared resources).

### Mystira.DevHub

Path: `apps/devhub/*` (previously `packages/devhub/*`)

- DevHub Leptos/Tauri tooling imported and integrated into the workspace.
- Build/test config alignment with the rest of the monorepo.

### Mystira.Infra (Terraform + Kubernetes + ops glue)

Paths: `infra/terraform/*`, `infra/kubernetes/*`, `scripts/*`, `.github/*`

- Terraform updated to reflect .NET 10 + monorepo deployment patterns.
- Terragrunt layering present (`shared-infra/` foundation + `products/*` per-product).
- Shared resource direction reinforced (monitoring/data/ACR/comms patterns).
- CI/CD workflows consolidated and hardened (security checks, dependency review scope, formatting gates).

### Workspace Packages (Core/Domain/Shared/\*)

Paths: `packages/*`

- Core: consolidated CQRS handlers and “ports” interfaces for reuse across services.
- Domain: DDD model consolidation and shared value-object primitives.
- Shared: shared auth, exception primitives, and shared messaging/observability packages.
- shared-ts: consolidated duplicate TS packages into a single shared package.

## Total Effort (rough)

- Overall (monorepo migration + follow-on consolidation/infra + service alignment): ~25–60 engineer-days.
- Heaviest workstreams (by coordination/impact rather than pure LOC): Mystira.App migration + workspace package consolidation + CI/infra wiring.
