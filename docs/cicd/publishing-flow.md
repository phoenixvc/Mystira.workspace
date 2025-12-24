# Publishing & Deployment Flow

**Last Updated**: 2025-12-19
**Status**: ✅ Complete

This document describes how packages are built, published, and deployed across all Mystira services.

---

## Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           PUBLISHING DESTINATIONS                            │
├─────────────────────────────────────────────────────────────────────────────┤
│  Docker Images  →  Azure Container Registry (myssharedacr.azurecr.io)         │
│  NPM Packages   →  npmjs.org (via Changesets)                               │
│  NuGet Packages →  GitHub Packages / NuGet.org                              │
│  Deployments    →  Azure Kubernetes Service (AKS)                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Docker Image Publishing

### Registry

**Azure Container Registry**: `myssharedacr.azurecr.io`

### Images Published

| Image             | Source                     | Dockerfile                                | Notes |
| ----------------- | -------------------------- | ----------------------------------------- | ----- |
| `publisher`       | `packages/publisher`       | `infra/docker/publisher/Dockerfile`       | Node.js |
| `chain`           | `packages/chain`           | `infra/docker/chain/Dockerfile`           | Python |
| `story-generator` | `packages/story-generator` | `infra/docker/story-generator/Dockerfile` | .NET API (not Blazor WASM) |

> **Note**: The `story-generator` image builds `Mystira.StoryGenerator.Api`, following the same pattern as `Mystira.App` where API goes to Kubernetes and Web (Blazor WASM) would go to Static Web App.

### Tagging Strategy

| Branch        | Tags Applied                 |
| ------------- | ---------------------------- |
| `dev`         | `dev`, `latest`, `{sha}`     |
| `main`        | `staging`, `latest`, `{sha}` |
| Manual deploy | `prod`, `{sha}`              |

### Workflow Triggers

```yaml
# Triggered on push to dev or main
on:
  push:
    branches: [dev, main]
    paths:
      - "packages/{service}/**"
```

### Build Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│    Lint      │ ──▶ │    Test      │ ──▶ │    Build     │ ──▶ │ Docker Push  │
│              │     │              │     │              │     │   to ACR     │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

---

## NPM Package Publishing

### Registry

**npmjs.org** (public registry)

### Configuration

Uses **Changesets** for versioning and publishing.

### Workflow

```yaml
# .github/workflows/release.yml
- Triggers on push to main
- Creates version PRs via Changesets
- Publishes to npm on merge
```

### Required Secrets

| Secret      | Description                     |
| ----------- | ------------------------------- |
| `NPM_TOKEN` | npm access token for publishing |

### Publishing Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│ Push to main │ ──▶ │  Changesets  │ ──▶ │  Version PR  │ ──▶ │  Publish to  │
│              │     │   Action     │     │   Created    │     │    npmjs     │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

---

## NuGet Package Publishing

### Registry Options

- **GitHub Packages**: `https://nuget.pkg.github.com/phoenixvc/index.json` (private)
- **NuGet.org**: `https://api.nuget.org/v3/index.json` (public)

### Packages Published

| Package                            | Project               | Description                           |
| ---------------------------------- | --------------------- | ------------------------------------- |
| `Mystira.StoryGenerator.Contracts` | Shared contracts/DTOs | gRPC/API contracts                    |
| `Mystira.StoryGenerator.Client`    | Client library        | SDK for consuming Story Generator API |

### Required Secrets

| Secret          | Description                             |
| --------------- | --------------------------------------- |
| `NUGET_API_KEY` | NuGet.org API key (for public packages) |
| `GITHUB_TOKEN`  | Auto-provided (for GitHub Packages)     |

### Workflow Configuration

```yaml
# In story-generator-ci.yml
- name: Pack NuGet packages
  run: dotnet pack --configuration Release --output ./nupkg

- name: Push to GitHub Packages
  run: |
    dotnet nuget push ./nupkg/*.nupkg \
      --source "https://nuget.pkg.github.com/phoenixvc/index.json" \
      --api-key ${{ secrets.GITHUB_TOKEN }}

- name: Push to NuGet.org (optional)
  if: github.ref == 'refs/heads/main'
  run: |
    dotnet nuget push ./nupkg/*.nupkg \
      --source "https://api.nuget.org/v3/index.json" \
      --api-key ${{ secrets.NUGET_API_KEY }}
```

### Versioning

**Current Strategy**: Alpha pre-release versioning

```
1.0.0-alpha.{build_number}
```

Examples:

- `1.0.0-alpha.42`
- `1.0.0-alpha.43`

The build number increments automatically with each CI run (`github.run_number`).

**Progression**:
| Phase | Version Pattern | When |
|-------|-----------------|------|
| Alpha | `1.0.0-alpha.{n}` | Current development |
| Beta | `1.0.0-beta.{n}` | Feature complete, testing |
| RC | `1.0.0-rc.{n}` | Release candidate |
| Stable | `1.0.0` | Production release |

To update version stage, modify the workflow:

```yaml
# In story-generator-ci.yml
-p:PackageVersion=1.0.0-alpha.${{ github.run_number }}
```

---

## Deployment Flow

### Environment URLs

| Environment | Publisher                       | Chain                       | API                       |
| ----------- | ------------------------------- | --------------------------- | ------------------------- |
| Dev         | `dev.publisher.mystira.app`     | `dev.chain.mystira.app`     | `dev.api.mystira.app`     |
| Staging     | `staging.publisher.mystira.app` | `staging.chain.mystira.app` | `staging.api.mystira.app` |
| Production  | `publisher.mystira.app`         | `chain.mystira.app`         | `api.mystira.app`         |

### Complete Flow Diagram

```
                              CODE CHANGES
                                   │
                    ┌──────────────┴──────────────┐
                    │                             │
                    ▼                             ▼
             ┌─────────────┐               ┌─────────────┐
             │ Feature     │               │   Hotfix    │
             │ Branch      │               │   Branch    │
             └──────┬──────┘               └──────┬──────┘
                    │                             │
                    │ PR                          │ PR
                    ▼                             ▼
             ┌─────────────┐               ┌─────────────┐
             │  dev branch │               │ main branch │
             │             │               │             │
             │ • CI runs   │               │ • CI runs   │
             │ • Docker →  │               │ • Docker →  │
             │   ACR:dev   │               │   ACR:staging│
             └──────┬──────┘               └──────┬──────┘
                    │                             │
                    │ PR (1 approval)             │
                    ▼                             │
             ┌─────────────┐                      │
             │ main branch │◀─────────────────────┘
             │             │
             │ • Auto      │
             │   staging   │
             │   deploy    │
             └──────┬──────┘
                    │
                    │ staging-release.yml
                    ▼
             ┌─────────────┐
             │   STAGING   │
             │   AKS       │
             │ mys-staging │
             └──────┬──────┘
                    │
                    │ Manual trigger
                    │ "DEPLOY TO PRODUCTION"
                    ▼
             ┌─────────────┐
             │ PRODUCTION  │
             │   AKS       │
             │  mys-prod   │
             └─────────────┘
```

### Workflow Files

| Workflow                              | Trigger                    | Action                                |
| ------------------------------------- | -------------------------- | ------------------------------------- |
| `publisher-ci.yml`                    | Push/PR to dev/main        | Lint, test, build, Docker push        |
| `chain-ci.yml`                        | Push/PR to dev/main        | Lint, test, build, Docker push        |
| `story-generator-ci.yml`              | Push/PR to dev/main        | Lint, test, build, Docker push        |
| `submodule-deploy-dev.yml`            | `repository_dispatch`      | Deploy submodule to dev K8s           |
| `submodule-deploy-dev-appservice.yml` | `repository_dispatch`      | Deploy Mystira.App to dev App Service |
| `nuget-publish.yml`                   | `repository_dispatch`      | Publish NuGet packages                |
| `release.yml`                         | Push to main               | NPM publish via Changesets            |
| `staging-release.yml`                 | Push to main               | Auto-deploy to staging AKS            |
| `production-release.yml`              | Manual                     | Deploy to production AKS              |
| `infra-deploy.yml`                    | Manual/Push                | Full infrastructure deployment        |

---

## Submodule Deployment (Dev Only)

Submodule repositories (Admin.Api, Admin.UI, etc.) use `repository_dispatch` to trigger deployments to the **dev environment only**.

> **Full Setup Guide**: See [Submodule CI/CD Setup](./submodule-cicd-setup.md) for complete instructions including workflow templates and secrets configuration.

### Trigger Behavior

| Event | CI (lint/test/build) | Deploy to Dev |
|-------|---------------------|---------------|
| PR opened/updated | ✅ | ❌ |
| Draft PR | ❌ skip | ❌ |
| Push/merge to dev | ✅ | ✅ |
| Push to main | ✅ | ❌ |
| Manual dispatch | ✅ | ✅ |

### Flow Diagram

```
┌─────────────────────────┐     ┌─────────────────────────────┐
│   Submodule Repository  │     │      Mystira.workspace      │
│   (e.g., Admin.Api)     │     │                             │
├─────────────────────────┤     ├─────────────────────────────┤
│ 1. PR merged to dev     │     │                             │
│    (push event fires)   │     │                             │
│         ↓               │     │                             │
│ 2. Build Docker image   │     │                             │
│ 3. Push to ACR          │────►│ 4. repository_dispatch      │
│         ↓               │     │    event received           │
│ 4. Trigger dispatch     │     │         ↓                   │
│    (admin-api-deploy)   │     │ 5. Update K8s deployment    │
└─────────────────────────┘     │    with new image tag       │
                                │         ↓                   │
                                │ 6. Rollout to dev AKS       │
                                │         ↓                   │
                                │ 7. Update submodule ref     │
                                │    and commit to dev        │
                                └─────────────────────────────┘
```

> **Note**: After successful deployment, the workspace automatically updates the submodule reference to the deployed commit and pushes to `dev`. This keeps the workspace in sync with deployed versions.

### Supported Event Types

| Event Type               | Target                      | Handler Workflow                      |
| ------------------------ | --------------------------- | ------------------------------------- |
| `admin-api-deploy`       | `mys-admin-api` (K8s)       | `submodule-deploy-dev.yml`            |
| `admin-ui-deploy`        | `mys-admin-ui` (K8s)        | `submodule-deploy-dev.yml`            |
| `story-generator-deploy` | `mys-story-generator` (K8s) | `submodule-deploy-dev.yml`            |
| `publisher-deploy`       | `mys-publisher` (K8s)       | `submodule-deploy-dev.yml`            |
| `chain-deploy`           | `mys-chain` (K8s)           | `submodule-deploy-dev.yml`            |
| `app-deploy`             | App Service                 | `submodule-deploy-dev-appservice.yml` |
| `app-swa-deploy`         | Static Web App              | `submodule-deploy-dev-appservice.yml` |
| `devhub-deploy`          | Static Web App              | `submodule-deploy-dev-appservice.yml` |
| `story-generator-swa-deploy` | Static Web App          | `submodule-deploy-dev-appservice.yml` |
| `nuget-publish`          | GitHub/NuGet.org            | `nuget-publish.yml`                   |

> **Note**: `Mystira.StoryGenerator` follows the same API/Web pattern as `Mystira.App`:
> - **API** (`Mystira.StoryGenerator.Api`) → Kubernetes via `story-generator-deploy`
> - **Web** (`Mystira.StoryGenerator.Web`, Blazor WASM) → Static Web App via `story-generator-swa-deploy`

### Client Payload Format

Submodules should send the following payload:

```json
{
  "environment": "dev",
  "ref": "<commit-sha>",
  "triggered_by": "<github-actor>",
  "run_id": "<workflow-run-id>",
  "repository": "<owner/repo>",
  "image_tag": "dev-<commit-sha>",
  "pr_number": "<pr-number-or-empty>"
}
```

### Example Submodule Workflow (Dispatch Job Only)

```yaml
# In submodule repository - add this job to your CI workflow
# Full template: docs/cicd/submodule-cicd-setup.md

trigger-workspace-deploy:
  name: Trigger Workspace Deployment
  runs-on: ubuntu-latest
  needs: build-and-push
  # Only deploy on push to dev (includes merges) or manual dispatch
  if: |
    needs.build-and-push.result == 'success' &&
    github.ref == 'refs/heads/dev' &&
    (github.event_name == 'push' || github.event_name == 'workflow_dispatch')
  steps:
    - name: Trigger deployment via workspace
      uses: peter-evans/repository-dispatch@v3
      with:
        token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
        repository: phoenixvc/Mystira.workspace
        event-type: admin-api-deploy  # Change per service
        client-payload: |
          {
            "environment": "dev",
            "ref": "${{ github.sha }}",
            "triggered_by": "${{ github.actor }}",
            "run_id": "${{ github.run_id }}",
            "repository": "${{ github.repository }}",
            "image_tag": "dev-${{ github.sha }}",
            "pr_number": ""
          }
```

### Required Secrets (Submodule Repositories)

| Secret                               | Description                                      |
| ------------------------------------ | ------------------------------------------------ |
| `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` | GitHub PAT with `repo` scope to trigger dispatch |
| `AZURE_CLIENT_ID`                    | Azure service principal client ID                |
| `AZURE_TENANT_ID`                    | Azure tenant ID                                  |
| `AZURE_SUBSCRIPTION_ID`              | Azure subscription ID                            |
| `GH_PACKAGES_TOKEN`                  | GitHub PAT for NuGet package restore             |

> **Token Setup**: `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` is the standard PAT used across all Mystira repositories. It must have `repo` scope to trigger `repository_dispatch` events in the workspace.

### Why Dev Only?

Submodule-triggered deployments are **intentionally limited to the dev environment**:

- **Staging/Production**: Managed through workspace release workflows (`staging-release.yml`, `production-release.yml`)
- **Rationale**: Production deployments require coordinated releases, approval gates, and testing
- **Dev**: Enables rapid iteration and testing of individual services

---

## NuGet Package Publishing

Submodules with shared contracts (e.g., `Mystira.App.Contracts`) can trigger NuGet package publishing via the workspace.

### Version Strategy

| Branch | Version | Destination |
|--------|---------|-------------|
| `dev`  | `1.0.0-dev.{run_number}` | GitHub Packages only |
| `main` | `1.0.0` (stable) | GitHub Packages + NuGet.org |

### Triggering NuGet Publish

Add this job to your submodule workflow (after successful build):

```yaml
trigger-nuget-publish:
  name: Trigger NuGet Publish
  runs-on: ubuntu-latest
  needs: build-and-push
  if: |
    needs.build-and-push.result == 'success' &&
    (github.ref == 'refs/heads/dev' || github.ref == 'refs/heads/main')
  steps:
    - name: Trigger NuGet publish
      uses: peter-evans/repository-dispatch@v3
      with:
        token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
        repository: phoenixvc/Mystira.workspace
        event-type: nuget-publish
        client-payload: |
          {
            "package": "app-contracts",
            "ref": "${{ github.sha }}",
            "triggered_by": "${{ github.actor }}",
            "run_id": "${{ github.run_id }}",
            "version_suffix": "${{ github.ref == 'refs/heads/dev' && format('dev.{0}', github.run_number) || '' }}",
            "is_prerelease": "${{ github.ref == 'refs/heads/dev' }}"
          }
```

### Supported Packages

| Package Name | Event Payload `package` Value |
|--------------|-------------------------------|
| Mystira.App.Contracts | `app-contracts` |
| Mystira.StoryGenerator.Contracts | `story-generator-contracts` |

---

## GitHub Secrets Required

| Secret                                  | Purpose                      | Required For         |
| --------------------------------------- | ---------------------------- | -------------------- |
| `AZURE_CREDENTIALS`                     | Azure service principal JSON | All Azure operations |
| `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` | PAT for private submodules   | CI checkout          |
| `NPM_TOKEN`                             | npm publish token            | NPM releases         |
| `NUGET_API_KEY`                         | NuGet.org API key            | NuGet releases       |
| `AZURE_CLIENT_ID`                       | Azure client ID (OIDC)       | Federated auth       |
| `AZURE_TENANT_ID`                       | Azure tenant ID (OIDC)       | Federated auth       |
| `AZURE_SUBSCRIPTION_ID`                 | Azure subscription ID        | Federated auth       |

---

## Quick Reference Commands

### Check Published Docker Images

```bash
az acr repository list --name myssharedacr
az acr repository show-tags --name myssharedacr --repository publisher
```

### Check NPM Package

```bash
npm view @mystira/publisher versions
```

### Check NuGet Package

```bash
dotnet nuget list source
dotnet package search Mystira.StoryGenerator --source github
```

### Trigger Manual Deployment

```bash
# Via GitHub CLI
gh workflow run "Infrastructure Deploy" -f environment=dev
gh workflow run "Production Release" -f confirm="DEPLOY TO PRODUCTION"
```

---

## Related Documentation

- [CI/CD Setup](./cicd-setup.md) - Branch protection and CI configuration
- [Infrastructure Deployment Checklist](../../DEPLOYMENT_CHECKLIST.md) - Full deployment guide
- [ADR-0003: Release Pipeline Strategy](../architecture/adr/0003-release-pipeline-strategy.md)
- [ADR-0004: Branching Strategy](../architecture/adr/0004-branching-strategy-and-cicd.md)
