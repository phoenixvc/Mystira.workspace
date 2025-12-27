# Deployment Guide

## Quick Reference

| Environment | URL | How to Deploy |
|-------------|-----|---------------|
| **Dev** | dev.mystira.app | Push to submodule `dev` branch |
| **Staging** | staging.mystira.app | Merge PR to `main` |
| **Production** | mystira.app | Manual: Actions → "Deployment: Production" |

## Dev Deployments (Automatic)

Push to a submodule's `dev` branch triggers deployment automatically:

| Service | Submodule | Target |
|---------|-----------|--------|
| App API | `packages/app` | App Service |
| Admin API | `packages/admin-api` | Kubernetes |
| Admin UI | `packages/admin-ui` | Kubernetes |
| Story Generator | `packages/story-generator` | Kubernetes + SWA |
| Publisher | `packages/publisher` | Kubernetes |
| Chain | `packages/chain` | Kubernetes |
| DevHub | `packages/devhub` | Static Web App |

## Staging Deployments (Automatic)

Merging a PR to `main` automatically deploys to staging via `staging-release.yml`.

## Production Deployments (Manual)

1. Go to **Actions** → **"Deployment: Production"**
2. Click **Run workflow**
3. Type `DEPLOY TO PRODUCTION` to confirm

### App API (Blue-Green)

For the App API specifically:
1. **Actions** → **"Mystira App API: Production Blue-Green Deployment"**
2. Type `DEPLOY TO PRODUCTION`

### Rollback

1. **Actions** → **"Mystira App API: Manual Rollback"**
2. Type `ROLLBACK PRODUCTION`

## Workflows Overview

### Core (use these)
| Workflow | Purpose |
|----------|---------|
| `ci.yml` | PR checks, tests |
| `staging-release.yml` | Auto-deploy to staging |
| `production-release.yml` | Manual prod deploy |

### Dev Deploy (automatic via submodule push)
| Workflow | Services |
|----------|----------|
| `submodule-deploy-dev.yml` | K8s: admin-api, admin-ui, publisher, chain, story-generator |
| `submodule-deploy-dev-appservice.yml` | App Service/SWA: app, devhub |

### Packages
| Workflow | Purpose |
|----------|---------|
| `shared-publish.yml` | Mystira.Shared NuGet |
| `contracts-publish.yml` | Mystira.Contracts NuGet |

### Infrastructure
| Workflow | Purpose |
|----------|---------|
| `infra-validate.yml` | PR validation (terraform plan) |
| `infra-deploy.yml` | Apply terraform changes |

### Utility (run automatically)
| Workflow | Purpose |
|----------|---------|
| `security-scan-scheduled.yml` | Weekly security scans |
| `check-submodules.yml` | Verify submodule state |

## Troubleshooting

### "Network error" on dev.mystira.app
The deployment may have failed. Check:
1. **Actions** → Recent `submodule-deploy-dev-appservice.yml` runs
2. Look for NuGet restore errors (401 = missing `source-url` in setup-dotnet)

### Deployment not triggering
Submodule repos must dispatch events. Verify:
1. Submodule CI passed
2. `repository_dispatch` event was sent
3. Check workspace Actions for incoming dispatches
