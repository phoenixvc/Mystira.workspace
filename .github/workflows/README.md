# Workflow Inventory

This document explains the CI/CD workflow organization for the Mystira monorepo.

## Current workflows

All code lives in a single monorepo. CI, linting, testing, building, and deployments are managed centrally via GitHub Actions workflows in this repository.

### Reusable workflow templates

### 🔧 Component CI Workflows

Per-package CI workflows trigger on path-based changes:

**Why in workspace?** All packages live in this monorepo. The workspace provides their CI/CD.

### 📱 App Component

### 🚀 Deployment Workflows

- **`staging-release.yml`** - Deploys all services to staging
  - Triggers: Pushes to main with infra/package changes
  - Environment: <https://staging.mystira.app>

- **`production-release.yml`** - Deploys all services to production
  - Triggers: Manual only (requires typing "DEPLOY TO PRODUCTION")
  - Environment: <https://mystira.app>

- **`mystira-app-api-cicd-prod.yml`** - Blue-green production deployment for App API

### 🏗️ Infrastructure Workflows

- **`infra-validate.yml`** - Validates Terraform, K8s manifests, Dockerfiles, security scans
- **`infra-deploy.yml`** - Deploys infrastructure (Terraform apply, Docker builds, K8s)

### 📋 Workspace-Level Workflows

- **`ci.yml`** - Workspace-wide CI (lint, test, build across all packages)

### 🔧 Utility Workflows

- **`utilities-link-checker.yml`** - Validates markdown links (weekly + on changes)

## Development Workflow

1. Make changes in `packages/{component}/`
2. Create PR to dev/main
3. CI runs automatically (path-filtered)
4. Merge to main triggers staging deployment
5. Production deployment via manual workflow dispatch

## Trigger Summary

| Workflow                | Push (dev/main) | Pull Request  | Manual | Schedule |
| ----------------------- | --------------- | ------------- | ------ | -------- |
| Component CIs           | Path-based      | Path-based    | Yes    | -        |
| Staging Release         | Main only       | -             | Yes    | -        |
| Production Release      | -               | -             | Yes    | -        |
| Infrastructure Validate | Path-based      | Path-based    | Yes    | -        |
| Infrastructure Deploy   | Main only       | -             | Yes    | -        |
| Workspace CI            | Yes             | Yes           | Yes    | -        |
| Link Checker            | Markdown only   | Markdown only | Yes    | Weekly   |

## Adding New Component Workflows

1. **Name format**: `Components: {Component Name} - CI`
2. **File naming**: `{component-name}-ci.yml`
3. **Include standard jobs**: lint, test, build
4. **Add path filters** to only trigger on relevant changes
