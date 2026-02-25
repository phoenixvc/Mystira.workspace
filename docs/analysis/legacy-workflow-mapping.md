# Legacy Workflow Mapping

This document maps legacy repository workflow responsibilities to current
monorepo workflows.

## Monorepo workflow baseline

Current workspace workflows under `.github/workflows` include:

- `workspace-ci.yml`
- `deploy-staging.yml`
- `deploy-production.yml`
- `deploy-app-api-production.yml`
- `deploy-app-api-rollback.yml`
- `infra-validate.yml`
- `infra-deploy.yml`
- `reusable-docker-build.yml`
- `reusable-security-scan.yml`
- `reusable-terraform.yml`
- `security-keyvault-secrets.yml`
- `security-scan-scheduled.yml`
- `utilities-link-checker.yml`

## Mapping table (file-level dispositions)

Legend:

- `mapped`: legacy workflow capability exists in monorepo workflows
- `retired-intentional`: intentionally removed/centralized (dev parity optional)
- `needs-confirmation`: parity not proven from current evidence

### Mystira.App

| Legacy workflow                | Monorepo workflow(s)                                  | Disposition         |
| ------------------------------ | ----------------------------------------------------- | ------------------- |
| `ci-tests.yml`                 | `ci.yml`, `_ci-gate.yml`                              | mapped              |
| `test-unit.yml`                | `_test-unit.yml`, `test-contract.yml`                 | mapped              |
| `test-load.yml`                | `test-load.yml`                                       | mapped              |
| `test-e2e.yml`                 | `test-e2e.yml`                                        | mapped              |
| `security-scanning.yml`        | `_security-scan.yml`, `security-scan-scheduled.yml`   | mapped              |
| `monitor-sla.yml`              | `monitor-sla.yml`                                     | mapped              |
| `api-rollback-prod.yml`        | `mystira-app-api-rollback.yml`                        | mapped              |
| `deploy-trigger-workspace.yml` | `staging-release.yml`, `production-release.yml`       | mapped              |
| `api-ci-dev.yml`               | n/a (dev parity not mandatory)                        | retired-intentional |
| `pwa-ci-dev.yml`               | n/a (dev parity not mandatory)                        | retired-intentional |
| `pwa-deploy-dev.yml`           | n/a (dev parity not mandatory)                        | retired-intentional |
| `pwa-cleanup-preview.yml`      | n/a (dev/preview parity intentionally reduced)        | retired-intentional |
| `pwa-smoke-tests.yml`          | covered by centralized quality gates (`test-e2e.yml`) | retired-intentional |
| `templates/`                   | reusable workflow pattern (`_*.yml`)                  | mapped              |

### Mystira.StoryGenerator

| Legacy workflow        | Monorepo workflow(s)                                | Disposition         |
| ---------------------- | --------------------------------------------------- | ------------------- |
| `ci.yml`               | `ci.yml`, `_ci-gate.yml`                            | mapped              |
| `test-unit.yml`        | `_test-unit.yml`                                    | mapped              |
| `test-load.yml`        | `test-load.yml`                                     | mapped              |
| `test-integration.yml` | `_test-integration.yml`                             | mapped              |
| `security-scan.yml`    | `_security-scan.yml`, `security-scan-scheduled.yml` | mapped              |
| `rollback.yml`         | `rollback-story-generator.yml`                      | mapped              |
| `build-deploy-dev.yml` | n/a (dev parity not mandatory)                      | retired-intentional |

### Mystira.Publisher

| Legacy workflow | Monorepo workflow(s)                                    | Disposition |
| --------------- | ------------------------------------------------------- | ----------- |
| `ci.yml`        | `ci.yml`, `packages-authoring.yml`, `_docker-build.yml` | mapped      |

### Mystira.Chain

| Legacy workflow        | Monorepo workflow(s)                                | Disposition |
| ---------------------- | --------------------------------------------------- | ----------- |
| `ci.yml`               | `ci.yml`, `_ci-gate.yml`                            | mapped      |
| `test-unit.yml`        | `_test-unit.yml`                                    | mapped      |
| `test-load.yml`        | `test-load.yml`                                     | mapped      |
| `test-integration.yml` | `_test-integration.yml`                             | mapped      |
| `security-scan.yml`    | `_security-scan.yml`, `security-scan-scheduled.yml` | mapped      |
| `rollback.yml`         | `rollback-chain.yml`                                | mapped      |
| `chain-cicd.yml`       | `release.yml`, `production-release.yml`             | mapped      |

### Mystira.Devhub

| Legacy workflow     | Monorepo workflow(s)                                | Disposition         |
| ------------------- | --------------------------------------------------- | ------------------- |
| `ci.yml`            | `ci.yml`, `_ci-gate.yml`                            | mapped              |
| `test-unit.yml`     | `_test-unit.yml`                                    | mapped              |
| `test-e2e.yml`      | `test-e2e.yml`                                      | mapped              |
| `security-scan.yml` | `_security-scan.yml`, `security-scan-scheduled.yml` | mapped              |
| `build-desktop.yml` | n/a (desktop pipeline retired in workspace model)   | retired-intentional |

### Mystira.Admin.Api

| Legacy workflow        | Monorepo workflow(s)                                | Disposition         |
| ---------------------- | --------------------------------------------------- | ------------------- |
| `ci.yml`               | `ci.yml`, `_build-dotnet.yml`                       | mapped              |
| `test-unit.yml`        | `_test-unit.yml`                                    | mapped              |
| `test-integration.yml` | `_test-integration.yml`                             | mapped              |
| `security-scan.yml`    | `_security-scan.yml`, `security-scan-scheduled.yml` | mapped              |
| `rollback.yml`         | `rollback-admin-api.yml`                            | mapped              |
| `monitor-health.yml`   | `monitor-health.yml`                                | mapped              |
| `deploy-dev.yml`       | n/a (dev parity not mandatory)                      | retired-intentional |

### Mystira.Admin.UI

| Legacy workflow  | Monorepo workflow(s)                                | Disposition         |
| ---------------- | --------------------------------------------------- | ------------------- |
| `ci.yml`         | `ci.yml`                                            | mapped              |
| `test-unit.yml`  | `_test-unit.yml`                                    | mapped              |
| `test-e2e.yml`   | `test-e2e.yml`                                      | mapped              |
| `security.yml`   | `_security-scan.yml`, `security-scan-scheduled.yml` | mapped              |
| `deploy-dev.yml` | n/a (dev parity not mandatory)                      | retired-intentional |

### Mystira.Infra

`mystira-infra-legacy-workflows.json` captured a GitHub `404` for
`.github/workflows`, indicating no legacy workflow folder at that path.
Infra capability is represented in monorepo by `infra-validate.yml`,
`infra-deploy.yml`, `infra-promote.yml`, and terragrunt workflows.

## Remaining validation items

1. Validate trigger-level parity (push/PR/schedule/manual/release) for mapped
   workflows where release-critical behavior changed.
2. Confirm secrets/environment parity for staging/prod and rollback paths.

## Verification checklist

- [x] Every legacy workflow has a disposition (`mapped` /
  `retired-intentional` / `needs-confirmation`).
- [ ] Every `mapped` row links to concrete monorepo workflow file and job.
- [x] Every `retired-intentional` row contains replacement/deprecation note.
- [x] No staging/prod/rollback critical workflow remains `needs-confirmation`.

## Evidence note

This mapping is based on captured manifests in
`docs/analysis/evidence/github-manifests/` and the workspace workflow set at
capture time.
