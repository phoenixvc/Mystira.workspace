# Workflow Organization Strategy

This document explains the CI/CD workflow organization for the Mystira monorepo.

## Overview

All code lives in a single monorepo. CI, linting, testing, building, and deployments are managed centrally via GitHub Actions workflows in this repository.

## Workflow Categories

### 🔧 Component CI Workflows

Per-package CI workflows trigger on path-based changes:

- **`admin-api-ci.yml`** - Admin API (.NET) linting, testing, building
- **`admin-ui-ci.yml`** - Admin UI (React/TypeScript) linting, testing, building
- **`chain-ci.yml`** - Chain (Python) linting, testing, Docker builds, K8s validation
- **`devhub-ci.yml`** - Devhub (Node.js) linting, testing, building
- **`publisher-ci.yml`** - Publisher (Node.js) linting, testing, Docker builds, K8s validation
- **`story-generator-ci.yml`** - Story Generator (.NET) linting, testing, Docker builds, NuGet publishing

**Trigger:** Changes to `packages/{component}/**` on dev/main branches

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
- **`release.yml`** - NPM package releases via Changesets

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
| Workspace Release       | Main only       | -             | Yes    | -        |
| Link Checker            | Markdown only   | Markdown only | Yes    | Weekly   |

## Adding New Component Workflows

1. **Name format**: `Components: {Component Name} - CI`
2. **File naming**: `{component-name}-ci.yml`
3. **Include standard jobs**: lint, test, build
4. **Add path filters** to only trigger on relevant changes
