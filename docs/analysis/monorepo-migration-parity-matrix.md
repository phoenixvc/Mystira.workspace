# Monorepo Migration Parity Matrix

This document tracks migration parity from legacy PhoenixVC repos to the
`Mystira.workspace` monorepo using GitHub-first evidence.

## Scope and mapping

| Legacy repo                        | Monorepo target            |
| ---------------------------------- | -------------------------- |
| `phoenixvc/Mystira.App`            | `packages/app`             |
| `phoenixvc/Mystira.StoryGenerator` | `packages/story-generator` |
| `phoenixvc/Mystira.Publisher`      | `packages/publisher`       |
| `phoenixvc/Mystira.Chain`          | `packages/chain`           |
| `phoenixvc/Mystira.Devhub`         | `packages/devhub`          |
| `phoenixvc/Mystira.Admin.Api`      | `packages/admin-api`       |
| `phoenixvc/Mystira.Admin.UI`       | `packages/admin-ui`        |
| `phoenixvc/Mystira.Infra`          | `infra`                    |

## Artifact parity snapshot (manifest-backed)

Legend:

- `present` = artifact clearly present in monorepo target
- `centralized` = moved to workspace/shared location by design
- `migrate-doc` = valid/value-adding doc should be migrated
- `unexplained-gap` = potential functionality loss, needs recovery

| Legacy repo            | Source/tests                         | Docs/readme                                                                                        | Workflows                                              | Release/config                                                                           | Status  |
| ---------------------- | ------------------------------------ | -------------------------------------------------------------------------------------------------- | ------------------------------------------------------ | ---------------------------------------------------------------------------------------- | ------- |
| Mystira.App            | present                              | present + `migrate-doc` (`PR-ANALYSIS.md`)                                                         | centralized                                            | centralized (`renovate.json`, root `global.json`)                                        | Partial |
| Mystira.StoryGenerator | present                              | present + multiple `migrate-doc` files                                                             | centralized                                            | centralized (`renovate.json`)                                                            | Partial |
| Mystira.Publisher      | present (includes cypress in legacy) | present + `migrate-doc` tactical docs                                                              | centralized (`ci` path retained in workspace CI model) | partial (`eslint.config.js` -> `eslint.config.mjs` validated; lock/renovate centralized) | Partial |
| Mystira.Chain          | present                              | present                                                                                            | centralized                                            | centralized (`renovate.json`)                                                            | Strong  |
| Mystira.Devhub         | present                              | present                                                                                            | centralized                                            | no blocker observed                                                                      | Strong  |
| Mystira.Admin.Api      | present                              | present                                                                                            | centralized                                            | centralized (`renovate.json`)                                                            | Strong  |
| Mystira.Admin.UI       | present                              | present + `migrate-doc` tactical docs                                                              | centralized                                            | transformed (`vite.config.js` + `.d.ts` -> `vite.config.ts`, behavior aligned)           | Partial |
| Mystira.Infra          | present                              | partial (`AZURE_SETUP.md` appears transformed to `azure-setup.md`; `DNS_INGRESS_SETUP.md` missing) | no legacy workflows folder (`404` snapshot)            | partial                                                                                  | Partial |

## Concrete root-level gap classification from snapshots

### Centralized by design (not a loss)

- `.github` missing in package targets; workspace has centralized workflows
  under `.github/workflows`.
- `renovate.json` missing in package targets; treated as centralized policy.

### Docs to migrate (user policy: migrate valid docs)

- `Mystira.App`: `PR-ANALYSIS.md`
- `Mystira.StoryGenerator`:
  - `AGENT_ID_FORMAT_FIX.md`
  - `AGENT_ORCHESTRATOR_IMPLEMENTATION.md`
  - `DEPLOYMENTS_CONFIG_CHANGES.md`
  - `IMPLEMENTATION_SUMMARY.md`
  - `QUICKSTART_AGENT_IDS.md`
  - `RAG_MULTI_INDEX_IMPLEMENTATION.md`
  - `SCENARIO_CONSISTENCY_EVALUATION_IMPLEMENTATION.md`
- `Mystira.Publisher`:
  - `ANALYSIS_ISSUES.md`
  - `IMPLEMENTATION_STATUS.md`
  - `IMPROVEMENTS.md`
  - `PHASE_COMPLETION_SUMMARY.md`
- `Mystira.Admin.UI`:
  - `COMPLETION_STATUS.md`
  - `MIGRATION_ANALYSIS.md`
  - `MIGRATION_SUMMARY.md`

### Unexplained or needs-owner-confirmation gaps

- None open from the current priority concern set.

Notes:

- `Mystira.Infra`: `DNS_INGRESS_SETUP.md` has been migrated to
  `infra/dns-ingress-setup.md`.
- `Mystira.Publisher`: lockfile policy is workspace-driven (`pnpm-lock.yaml`
  at root) while package-local lockfiles may exist per package tooling.

## Recovery actions (execution scope)

1. Migrate the listed `migrate-doc` files into canonical docs taxonomy.
2. For each future `unexplained` item, either recover into target or record
   explicit intentional-removal rationale in recovery PR.
3. Keep centralized workflow and renovate decisions, but link evidence in
   workflow mapping and PR notes.

## Evidence sources

- `docs/analysis/evidence/github-manifests/*-legacy-root.json`
- `docs/analysis/evidence/github-manifests/*-legacy-workflows.json`
- `docs/analysis/evidence/github-manifests/*-monorepo-target.json`
- `docs/analysis/evidence/github-manifests/mystira-workspace-workflows.json`
- `.github/workflows/README.md` (current workflow inventory and trigger model)
