# Workflow Organization Strategy

This document explains the CI/CD workflow organization for the Mystira workspace monorepo.

## Overview

The workspace uses a **hybrid approach** where:

- **Development workflows** run per-package with path-based triggers for fast feedback
- **Release and deployment workflows** run centrally from the workspace for controlled releases

## Workflow Categories

### 🔧 Component CI Workflows (6 components)

These workflows run when respective component files change in workspace PRs and pushes:

- **`admin-api-ci.yml`** - Admin API (.NET) linting, testing, building
- **`admin-ui-ci.yml`** - Admin UI (React/TypeScript) linting, testing, building
- **`chain-ci.yml`** - Chain (Python) linting, testing, Docker builds, K8s validation
- **`devhub-ci.yml`** - Devhub (Node.js) linting, testing, building
- **`publisher-ci.yml`** - Publisher (Node.js) linting, testing, Docker builds, K8s validation
- **`story-generator-ci.yml`** - Story Generator (.NET) linting, testing, Docker builds, NuGet publishing

**Trigger:** Changes to `packages/{component}/**` on dev/main branches

**Why in workspace?** All packages live in this monorepo. The workspace provides their CI/CD.

### 📱 App Component

- **`app-ci.yml`** - App (C#, .NET) linting, testing, building
- **Trigger:** Changes to `packages/app/**` on dev/main branches

### 🚀 Deployment Workflows

- **`staging-release.yml`** - Deploys all services to staging environment
  - Triggers: Pushes to main branch with infrastructure/package changes
  - Deploys: Infrastructure + Kubernetes services to staging
  - Environment: https://staging.mystira.app

- **`production-release.yml`** - Deploys all services to production environment
  - Triggers: Manual only (workflow_dispatch with confirmation)
  - Requires: Typing "DEPLOY TO PRODUCTION" to confirm
  - Environment: https://mystira.app

### 🏗️ Infrastructure Workflows

- **`infra-validate.yml`** - Validates infrastructure configurations
  - Validates: Terraform, Kubernetes manifests, Dockerfiles
  - Runs: Security scans (Checkov), format checks
  - Environments: dev, staging, prod

- **`infra-deploy.yml`** - Deploys infrastructure to environments
  - Handles: Terraform apply, Docker builds, Kubernetes deployments
  - Supports: Selective component deployment, manual triggers
  - Environments: dev (auto), staging/prod (manual)

### 📋 Workspace-Level Workflows

- **`ci.yml`** - Workspace-wide CI pipeline
  - Runs: Linting, testing, building across all packages
  - Triggers: All PRs and pushes to dev/main
  - Purpose: Validates workspace-level integration

- **`release.yml`** - NPM package releases
  - Manages: Package versioning via Changesets
  - Publishes: NPM packages to registry
  - Triggers: Pushes to main branch

### 🔧 Utility Workflows

- **`utilities-link-checker.yml`** - Checks documentation links
  - Validates: All links in markdown files
  - Triggers: Changes to `**/*.md`, weekly schedule
  - Creates: Issues for broken links (on schedule)

## Development Workflow

### Working on Components (admin-api, admin-ui, chain, devhub, publisher, story-generator)

1. Make changes in workspace: `packages/{component}/`
2. Create PR to workspace
3. Workspace CI runs automatically
4. Tests, linting, builds validate changes
5. Merge to main triggers deployments (if configured)

### Working on App

1. Make changes in workspace: `packages/app/`
2. Create PR to workspace
3. Workspace CI runs automatically
4. Tests, linting, builds validate changes
5. Merge to main triggers deployments (if configured)

### Releasing to Production

1. Ensure all component CIs pass
2. Merge to workspace main branch
3. For staging: Automatic deployment via `staging-release.yml`
4. For production: Manual trigger of `production-release.yml` with confirmation

## Trigger Summary

| Workflow                | Push (dev/main)  | Pull Request     | Manual  | Schedule  |
| ----------------------- | ---------------- | ---------------- | ------- | --------- |
| Component CIs (6)       | ✅ Path-based    | ✅ Path-based    | ✅      | -         |
| Staging Release         | ✅ Main only     | -                | ✅      | -         |
| Production Release      | -                | -                | ✅ Only | -         |
| Infrastructure Validate | ✅ Path-based    | ✅ Path-based    | ✅      | -         |
| Infrastructure Deploy   | ✅ Main only     | -                | ✅      | -         |
| Workspace CI            | ✅               | ✅               | ✅      | -         |
| Workspace Release       | ✅ Main only     | -                | ✅      | -         |
| Link Checker            | ✅ Markdown only | ✅ Markdown only | ✅      | ✅ Weekly |

## Advantages of This Approach

✅ **Fast development feedback** - App developers get immediate CI results in their repo
✅ **Controlled releases** - Only workspace can deploy to staging/production
✅ **No duplication** - Clear separation of concerns by environment
✅ **Consistent CI** - Other 6 components have uniform CI from workspace
✅ **Clear ownership** - Development in packages, releases from workspace

## Migration Notes

### Previous Setup

- Workspace had `app-ci.yml` which duplicated App repo's CI
- Caused confusion about which workflow runs when
- Wasted CI resources with duplicate runs

### Current Setup (After This PR)

- Removed workspace `app-ci.yml`
- App uses its own comprehensive CI/CD
- Clear documentation of workflow strategy
- No duplication, clear boundaries

### App Repo Cleanup Required

**Action Required:** Delete these 11+ workflows from [Mystira.App](https://github.com/phoenixvc/Mystira.App) repository:

#### Must Delete - Staging Deployments (6 workflows)

```bash
# Workspace staging-release.yml now handles these
rm .github/workflows/infrastructure-deploy-staging.yml
rm .github/workflows/mystira-app-admin-api-cicd-staging.yml
rm .github/workflows/mystira-app-api-cicd-staging.yml
rm .github/workflows/mystira-app-pwa-cicd-staging.yml
rm .github/workflows/mystira-app-pwa-cicd-staging.yml.disabled
rm .github/workflows/staging-automated-setup.yml
```

#### Must Delete - Production Deployments (4 workflows)

```bash
# Workspace production-release.yml now handles these
rm .github/workflows/infrastructure-deploy-prod.yml
rm .github/workflows/mystira-app-admin-api-cicd-prod.yml
rm .github/workflows/mystira-app-api-cicd-prod.yml
rm .github/workflows/mystira-app-pwa-cicd-prod.yml
```

#### Must Delete - Package Publishing (1 workflow)

```bash
# Move to workspace for centralized control
rm .github/workflows/publish-shared-packages.yml
```

#### Optional - Dev Deployments (4 workflows)

**Recommendation: KEEP** for fast dev iterations

If you want centralized control, delete these:

```bash
rm .github/workflows/infrastructure-deploy-dev.yml
rm .github/workflows/mystira-app-admin-api-cicd-dev.yml
rm .github/workflows/mystira-app-api-cicd-dev.yml
rm .github/workflows/mystira-app-pwa-cicd-dev.yml
```

#### Keep in App Repo (3 workflows)

```bash
# These should stay in App repo for fast dev feedback
✅ .github/workflows/ci-tests-codecov.yml
✅ .github/workflows/swa-preview-tests.yml
✅ .github/workflows/swa-cleanup-staging-environments.yml
```

### Summary of Changes

**Before:** 18 workflows in App repo
**After:** 3-7 workflows in App repo (depending on dev deployment choice)
**Removed:** 11-15 duplicate/unnecessary workflows
**Result:** Clear separation, no duplication, controlled releases

## Related Repositories

- **Mystira.workspace** (this repo) - Monorepo workspace, centralized releases
- **Mystira.App** - App submodule with independent CI/CD
- **Mystira.Admin.Api** - Admin API submodule (no CI/CD, uses workspace)
- **Mystira.Admin.UI** - Admin UI submodule (no CI/CD, uses workspace)
- **Mystira.Chain** - Chain submodule (no CI/CD, uses workspace)
- **Mystira.DevHub** - DevHub submodule (no CI/CD, uses workspace)
- **Mystira.Publisher** - Publisher submodule (no CI/CD, uses workspace)
- **Mystira.StoryGenerator** - Story Generator submodule (no CI/CD, uses workspace)

## Questions?

If you have questions about workflow organization or need to add new workflows, refer to this document or discuss in the team.
