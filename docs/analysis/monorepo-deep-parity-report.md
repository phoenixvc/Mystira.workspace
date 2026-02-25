# Monorepo Deep Parity Report

This report evaluates deep parity (recursive file trees), behavioral parity, and
nested config parity using GitHub manifest snapshots in
`docs/analysis/evidence/github-manifests`.

## Coverage set

- `src/**`, `tests/**`
- `.github/workflows/**` and reusable workflow templates
- `infra/**` (modules, env overlays, scripts, manifests)
- `scripts/**`, `tools/**`
- `migrations/**`, `db/**`, `schema/**`
- `config/**` plus nested tool configs (`eslint`, `vite`, `tsconfig`,
  `docker`, etc.)
- `contracts/**`, generated artifacts, API specs
- deployment assets (`k8s/**`, `helm/**`, `terraform/**`, `bicep/**`)
- runtime/static assets when behavior-relevant

## Recursive parity summary (snapshot-derived)

| Repo                   | Legacy covered files | Monorepo covered files | Missing | Added | Assessment           |
| ---------------------- | -------------------: | ---------------------: | ------: | ----: | -------------------- |
| Mystira.App            |                 1112 |                   1101 |      18 |     7 | Partial              |
| Mystira.StoryGenerator |                  383 |                    373 |      10 |     0 | Partial              |
| Mystira.Publisher      |                  132 |                    131 |       2 |     1 | Partial              |
| Mystira.Chain          |                   13 |                      8 |       7 |     2 | Workflow-centralized |
| Mystira.Devhub         |                   16 |                     12 |       5 |     1 | Workflow-centralized |
| Mystira.Admin.Api      |                  130 |                    122 |       9 |     1 | Partial              |
| Mystira.Admin.UI       |                   96 |                     89 |       7 |     0 | Partial              |
| Mystira.Infra          |                   38 |                    159 |       1 |   122 | Expanded scope       |

## Non-workflow deep gaps (highest priority)

### Changed-intentional (resolved with rationale)

1. `Mystira.StoryGenerator`
   - Legacy interface: `src/Mystira.StoryGenerator.Domain/Commands/ICommandHandler.cs`
   - Monorepo replacement: orchestrator-driven command pipeline via
     `IAgentOrchestrator` + concrete application `*CommandHandler.cs` classes.
   - Rationale: MediatR adapter interface removed as part of Wolverine/convention
     migration; behavior covered through orchestrator and handler conventions.

### Likely intentional/excluded (non-functional or generated)

1. `Mystira.Infra`
   - Missing: `terraform/environments/dev/.terraform/modules/modules.json`
   - Classification: generated local Terraform metadata, should not be tracked
     for parity.

### Docs migrated (valid/value-adding)

- `Mystira.App`:
  - `packages/app/docs/operations/docker-fix-summary.md`
  - `packages/app/docs/setup/nuget-feed-setup.md`
  - `packages/app/docs/setup/nuget-setup.md`
- `Mystira.StoryGenerator`:
  - `packages/story-generator/docs/rag-indexer/enhanced-schema-support.md`
  - `packages/story-generator/docs/rag-indexer/solid-dry-improvements.md`
- `Mystira.Admin.Api`:
  - `packages/admin-api/docs/setup/nuget-feed-configuration.md`
  - canonical ADR retained at
    `docs/architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md`
- `Mystira.Infra`:
  - `infra/dns-ingress-setup.md`

## Behavioral parity assessment

| Capability                              | Assessment                                              | Evidence                                                                                                       |
| --------------------------------------- | ------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| Build/test/security workflows           | Equivalent via centralized workspace workflows          | `mystira-workspace-workflows.json` plus legacy workflow manifests                                              |
| Rollback coverage                       | Equivalent for app/admin-api/chain/story-generator      | `mystira-app-api-rollback.yml`, `rollback-admin-api.yml`, `rollback-chain.yml`, `rollback-story-generator.yml` |
| Dev deployment workflows                | Intentionally reduced/centralized; acceptable by policy | user policy decision + workflow mappings                                                                       |
| Admin UI build config semantics         | Equivalent after typed config migration                 | `vite.config.js`/`.d.ts` -> `packages/admin-ui/vite.config.ts` with aligned runtime settings                   |
| Publisher lint config semantics         | Equivalent after ESM extension migration                | identical blob SHA for legacy `eslint.config.js` and target `packages/publisher/eslint.config.mjs`             |
| StoryGenerator command-handler behavior | Equivalent via orchestrator-driven command pipeline     | `ICommandHandler.cs` (legacy) replaced by `IAgentOrchestrator` + concrete `*CommandHandler.cs` implementation  |

## Nested config parity findings

1. **Config transformations detected (likely intentional)**
   - `eslint.config.js` -> `eslint.config.mjs` (Publisher)
   - `vite.config.js` + `vite.config.d.ts` -> `vite.config.ts` (Admin UI)
2. **Config centralization confirmed**
   - `global.json` is centralized at monorepo root (workspace policy), not
     package-local.
3. **Infrastructure nested config expansion**
   - Monorepo infra target includes substantially more deployment and
     environment files than legacy snapshot (expected expansion, not loss).

## Recovery backlog (actionable)

- None open from current parity concern set.
