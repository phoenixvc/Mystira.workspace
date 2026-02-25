# Workflow Inventory

This document tracks the current GitHub Actions workflows in this repo.
It reflects the workflow files present under `.github/workflows/`.

## Current workflows (13)

### Deployment

- `deploy-app-api-production.yml` - App API blue/green production deploy
- `deploy-app-api-rollback.yml` - App API manual rollback
- `deploy-production.yml` - Full production deployment (manual confirmation)
- `deploy-staging.yml` - Staging deployment

### Infrastructure

- `infra-deploy.yml` - Terraform apply + infra deployment pipeline
- `infra-validate.yml` - Terraform/K8s/Docker/security validation

### Reusable workflow templates

- `reusable-docker-build.yml`
- `reusable-security-scan.yml`
- `reusable-terraform.yml`

### Security

- `security-keyvault-secrets.yml` - Key Vault secret sync/validation (manual)
- `security-scan-scheduled.yml` - Weekly security scan + on-demand run

### Utilities

- `utilities-link-checker.yml` - Markdown link checking

### Workspace

- `workspace-ci.yml` - Main workspace CI for dev/main

## Trigger summary

| Workflow                        | Push                              | Pull Request                      | Manual | Schedule | Reusable        |
| ------------------------------- | --------------------------------- | --------------------------------- | ------ | -------- | --------------- |
| `workspace-ci.yml`              | `dev`, `main`                     | `dev`, `main`                     | Yes    | -        | -               |
| `deploy-staging.yml`            | `main` (path-filtered)            | -                                 | Yes    | -        | -               |
| `deploy-production.yml`         | -                                 | -                                 | Yes    | -        | -               |
| `deploy-app-api-production.yml` | -                                 | -                                 | Yes    | -        | -               |
| `deploy-app-api-rollback.yml`   | -                                 | -                                 | Yes    | -        | -               |
| `infra-validate.yml`            | `main`, `staging` (path-filtered) | `main`, `staging` (path-filtered) | Yes    | -        | -               |
| `infra-deploy.yml`              | `main` (path-filtered)            | -                                 | Yes    | -        | -               |
| `security-scan-scheduled.yml`   | -                                 | -                                 | Yes    | Weekly   | -               |
| `security-keyvault-secrets.yml` | -                                 | -                                 | Yes    | -        | -               |
| `utilities-link-checker.yml`    | `main` (markdown paths)           | `dev`, `main` (markdown paths)    | Yes    | Weekly   | -               |
| `reusable-docker-build.yml`     | -                                 | -                                 | -      | -        | `workflow_call` |
| `reusable-security-scan.yml`    | -                                 | -                                 | -      | -        | `workflow_call` |
| `reusable-terraform.yml`        | -                                 | -                                 | -      | -        | `workflow_call` |

## Usage notes

1. Prefer reusing `reusable-*` templates for shared CI logic.
2. Use path filters to avoid unnecessary workflow runs.
3. Require explicit confirmations for production-impacting workflows.
4. Keep workflow names in `Category: Name` format for consistency.
