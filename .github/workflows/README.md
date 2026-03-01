# Workflow Inventory

This document tracks the current GitHub Actions workflows in this repo.
It reflects the workflow files present under `.github/workflows/`.

## Current workflows (13)

### Deployment

- **Development workflows** run per-package with path-based triggers for fast feedback
- **Release and deployment workflows** run centrally from the workspace for controlled releases
- `deploy-app-api-production.yml` - App API blue/green production deploy
- `deploy-app-api-rollback.yml` - App API manual rollback

### Reusable workflow templates

- `reusable-docker-build.yml`
- `reusable-security-scan.yml`

### Security

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

| Workflow           | Push (dev/main)  | Pull Request     | Manual  | Schedule  |
| ------------------ | ---------------- | ---------------- | ------- | --------- |
| Component CIs (6)  | ✅ Path-based    | ✅ Path-based    | ✅      | -         |
| Staging Release    | ✅ Main only     | -                | ✅      | -         |
| Production Release | -                | -                | ✅ Only | -         |
| Workspace CI       | ✅               | ✅               | ✅      | -         |
| Workspace Release  | ✅ Main only     | -                | ✅      | -         |
| Link Checker       | ✅ Markdown only | ✅ Markdown only | ✅      | ✅ Weekly |

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

## Related Packages

- **Mystira.workspace** (this repo) - Monorepo workspace, centralized releases
- **packages/Mystira.App** - App package with independent CI/CD
- **packages/admin-api** - Admin API package (no CI/CD, uses workspace)
- **packages/admin-ui** - Admin UI package (no CI/CD, uses workspace)
- **packages/Mystira.Chain** - Chain package (no CI/CD, uses workspace)
- **packages/Mystira.DevHub** - DevHub package (no CI/CD, uses workspace)
- **packages/Mystira.Publisher** - Publisher package (no CI/CD, uses workspace)
- **packages/Mystira.StoryGenerator** - Story Generator package (no CI/CD, uses workspace)

## Questions?

If you have questions about workflow organization or need to add new workflows, refer to this document or discuss in the team.

- `security-keyvault-secrets.yml` - Key Vault secret sync/validation (manual)
- `security-scan-scheduled.yml` - Weekly security scan + on-demand run

### Utilities

- `utilities-link-checker.yml` - Markdown link checking

### Workspace

- `workspace-ci.yml` - Main workspace CI for dev/main

## Trigger summary

| Workflow                        | Push                    | Pull Request                   | Manual | Schedule | Reusable        |
| ------------------------------- | ----------------------- | ------------------------------ | ------ | -------- | --------------- |
| `workspace-ci.yml`              | `dev`, `main`           | `dev`, `main`                  | Yes    | -        | -               |
| `deploy-staging.yml`            | `main` (path-filtered)  | -                              | Yes    | -        | -               |
| `deploy-production.yml`         | -                       | -                              | Yes    | -        | -               |
| `deploy-app-api-production.yml` | -                       | -                              | Yes    | -        | -               |
| `deploy-app-api-rollback.yml`   | -                       | -                              | Yes    | -        | -               |
| `security-scan-scheduled.yml`   | -                       | -                              | Yes    | Weekly   | -               |
| `security-keyvault-secrets.yml` | -                       | -                              | Yes    | -        | -               |
| `utilities-link-checker.yml`    | `main` (markdown paths) | `dev`, `main` (markdown paths) | Yes    | Weekly   | -               |
| `reusable-docker-build.yml`     | -                       | -                              | -      | -        | `workflow_call` |
| `reusable-security-scan.yml`    | -                       | -                              | -      | -        | `workflow_call` |

## Usage notes

1. Prefer reusing `reusable-*` templates for shared CI logic.
2. Use path filters to avoid unnecessary workflow runs.
3. Require explicit confirmations for production-impacting workflows.
4. Keep workflow names in `[Category] Name` format for consistency.
