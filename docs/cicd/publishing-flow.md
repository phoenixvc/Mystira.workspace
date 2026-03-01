# Publishing & Deployment Flow

**Last Updated**: February 2026
**Status**: ✅ Complete (updated for monorepo)

This document describes how packages are built, published, and deployed across all Mystira services in the monorepo.

---

## Overview

```
┌──────────────────────────────────────────────────────────────────────────┐
│                        PUBLISHING DESTINATIONS                          │
├──────────────────────────────────────────────────────────────────────────┤
│  Docker Images  →  Azure Container Registry (myssharedacr.azurecr.io)  │
│  NPM Packages   →  npmjs.org (via Changesets, external only)           │
│  NuGet Packages →  GitHub Packages / NuGet.org (external only)         │
│  Deployments    →  Azure Kubernetes Service (AKS)                      │
└──────────────────────────────────────────────────────────────────────────┘
```

> **Monorepo note**: Internal `@mystira/*` TypeScript packages and internal Mystira .NET libraries are consumed via workspace protocol (`workspace:*`) and `<ProjectReference>` respectively — they are **not** published to any registry.

---

## Version Strategy

### Internal Packages (Not Published)

Internal packages are consumed directly within the monorepo:

| Package Type                                                   | Consumption Method   | Registry     |
| -------------------------------------------------------------- | -------------------- | ------------ |
| TypeScript (`@mystira/contracts`, `@mystira/core-types`, etc.) | pnpm `workspace:*`   | None (local) |
| .NET (`Mystira.Shared`, `Mystira.Domain`, etc.)                | `<ProjectReference>` | None (local) |

### External Packages (Published)

| Package Type                | Version Source                 | Registry                    |
| --------------------------- | ------------------------------ | --------------------------- |
| `Mystira.Contracts` (NuGet) | Synced with NPM via Changesets | GitHub Packages / NuGet.org |
| `Mystira.Shared` (NuGet)    | `.csproj` Version property     | GitHub Packages / NuGet.org |
| Docker Images               | Git SHA + environment tag      | Azure Container Registry    |

### Pre-release Strategy

| Environment | NPM Version   | NuGet Version | Docker Tag             |
| ----------- | ------------- | ------------- | ---------------------- |
| Development | `1.0.0-dev.N` | `1.0.0-dev.N` | `dev-{sha}`            |
| Staging     | `1.0.0-rc.N`  | `1.0.0-rc.N`  | `staging-{sha}`        |
| Production  | `1.0.0`       | `1.0.0`       | `prod-{sha}`, `latest` |

---

## Docker Image Publishing

### Registry

**Azure Container Registry**: `myssharedacr.azurecr.io`

### Images Published

| Image             | Source                     | Dockerfile                                | Notes    |
| ----------------- | -------------------------- | ----------------------------------------- | -------- |
| `publisher`       | `packages/publisher`       | `infra/docker/publisher/Dockerfile`       | Node.js  |
| `chain`           | `packages/chain`           | `infra/docker/chain/Dockerfile`           | Python   |
| `story-generator` | `packages/story-generator` | `infra/docker/story-generator/Dockerfile` | .NET API |
| `admin-api`       | `packages/admin-api`       | `infra/docker/admin-api/Dockerfile`       | .NET API |
| `app`             | `packages/app`             | `infra/docker/app/Dockerfile`             | .NET API |

### Tagging Strategy

| Branch        | Tags Applied                 |
| ------------- | ---------------------------- |
| `dev`         | `dev`, `latest`, `{sha}`     |
| `main`        | `staging`, `latest`, `{sha}` |
| Manual deploy | `prod`, `{sha}`              |

### Workflow Triggers

```yaml
# Triggered on push to dev or main with path-based filtering
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

### Workflows

NuGet packages are published through dedicated workflows:

| Workflow                              | File                    | Package             | Status    |
| ------------------------------------- | ----------------------- | ------------------- | --------- |
| `Packages - Contracts: Publish NuGet` | `contracts-publish.yml` | `Mystira.Contracts` | ✅ Active |
| `Packages - Shared: Publish NuGet`    | `shared-publish.yml`    | `Mystira.Shared`    | ✅ Active |

### Automatic Branch Publishing

```
┌──────────────────────────────────────────────────────────────────────────┐
│                        NuGet Publishing Triggers                        │
├──────────────────────────────────────────────────────────────────────────┤
│  Push to dev   →  Pre-release to GitHub Packages (e.g., 0.1.0-dev.123) │
│  Push to main  →  Stable to GitHub Packages + NuGet.org (e.g., 0.1.0)  │
│  Changesets    →  Triggered after @mystira/contracts NPM publish        │
│  Manual        →  workflow_dispatch with version suffix + NuGet toggle  │
└──────────────────────────────────────────────────────────────────────────┘
```

### Registry Options

- **GitHub Packages**: `https://nuget.pkg.github.com/phoenixvc/index.json` (all builds)
- **NuGet.org**: `https://api.nuget.org/v3/index.json` (stable releases only)

### Packages Published

| Package             | NPM Counterpart      | Description           | Workflow                |
| ------------------- | -------------------- | --------------------- | ----------------------- |
| `Mystira.Contracts` | `@mystira/contracts` | Unified API contracts | `contracts-publish.yml` |
| `Mystira.Shared`    | N/A                  | .NET shared utilities | `shared-publish.yml`    |

### Version Synchronization

| Package             | Version Source                    | Strategy                       |
| ------------------- | --------------------------------- | ------------------------------ |
| `Mystira.Contracts` | `packages/contracts/package.json` | Synced with NPM via Changesets |
| `Mystira.Shared`    | `.csproj` Version property        | Independent versioning         |

### Required Secrets

| Secret          | Description                             |
| --------------- | --------------------------------------- |
| `NUGET_API_KEY` | NuGet.org API key (for public packages) |
| `GITHUB_TOKEN`  | Auto-provided (for GitHub Packages)     |

### Manual Trigger Options

Both contracts and shared workflows support manual dispatch with:

- **Version suffix** (optional): e.g., `beta.1`, `rc.1`
- **Publish to NuGet.org** checkbox: Controls public registry publishing

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

| Workflow                 | Trigger                      | Action                           |
| ------------------------ | ---------------------------- | -------------------------------- |
| `publisher-ci.yml`       | Push/PR to dev/main          | Lint, test, build, Docker push   |
| `chain-ci.yml`           | Push/PR to dev/main          | Lint, test, build, Docker push   |
| `story-generator-ci.yml` | Push/PR to dev/main          | Lint, test, build, Docker push   |
| `contracts-publish.yml`  | Push to dev/main, Changesets | Publish Mystira.Contracts NuGet  |
| `shared-publish.yml`     | Push to dev/main             | Publish Mystira.Shared NuGet     |
| `release.yml`            | Push to main                 | NPM + NuGet publish (Changesets) |
| `staging-release.yml`    | Push to main                 | Auto-deploy to staging AKS       |
| `production-release.yml` | Manual                       | Deploy to production AKS         |
| `infra-deploy.yml`       | Manual/Push                  | Full infrastructure deployment   |

---

## GitHub Secrets Required

| Secret                  | Purpose                      | Required For          |
| ----------------------- | ---------------------------- | --------------------- |
| `AZURE_CREDENTIALS`     | Azure service principal JSON | All Azure operations  |
| `NPM_TOKEN`             | npm publish token            | NPM releases          |
| `NUGET_API_KEY`         | NuGet.org API key            | NuGet releases        |
| `AZURE_CLIENT_ID`       | Azure client ID (OIDC)       | Federated auth        |
| `AZURE_TENANT_ID`       | Azure tenant ID (OIDC)       | Federated auth        |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID        | Federated auth        |
| `GH_PACKAGES_TOKEN`     | GitHub PAT for packages      | NuGet package restore |

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
dotnet package search Mystira.Contracts --source github
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
- [ADR-0003: Release Pipeline Strategy](../architecture/adr/0003-release-pipeline-strategy.md)
- [ADR-0004: Branching Strategy](../architecture/adr/0004-branching-strategy-and-cicd.md)
