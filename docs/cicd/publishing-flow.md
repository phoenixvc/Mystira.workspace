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
│  Docker Images  →  Azure Container Registry (mysprodacr.azurecr.io)         │
│  NPM Packages   →  npmjs.org (via Changesets)                               │
│  NuGet Packages →  GitHub Packages / NuGet.org                              │
│  Deployments    →  Azure Kubernetes Service (AKS)                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Docker Image Publishing

### Registry
**Azure Container Registry**: `mysprodacr.azurecr.io`

### Images Published

| Image | Source | Dockerfile |
|-------|--------|------------|
| `publisher` | `packages/publisher` | `infra/docker/publisher/Dockerfile` |
| `chain` | `packages/chain` | `infra/docker/chain/Dockerfile` |
| `story-generator` | `packages/story-generator` | `infra/docker/story-generator/Dockerfile` |

### Tagging Strategy

| Branch | Tags Applied |
|--------|--------------|
| `dev` | `dev`, `latest`, `{sha}` |
| `main` | `staging`, `latest`, `{sha}` |
| Manual deploy | `prod`, `{sha}` |

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
| Secret | Description |
|--------|-------------|
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

| Package | Project | Description |
|---------|---------|-------------|
| `Mystira.StoryGenerator.Contracts` | Shared contracts/DTOs | gRPC/API contracts |
| `Mystira.StoryGenerator.Client` | Client library | SDK for consuming Story Generator API |

### Required Secrets

| Secret | Description |
|--------|-------------|
| `NUGET_API_KEY` | NuGet.org API key (for public packages) |
| `GITHUB_TOKEN` | Auto-provided (for GitHub Packages) |

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

| Environment | Publisher | Chain | API |
|-------------|-----------|-------|-----|
| Dev | `dev.publisher.mystira.app` | `dev.chain.mystira.app` | `dev.api.mystira.app` |
| Staging | `staging.publisher.mystira.app` | `staging.chain.mystira.app` | `staging.api.mystira.app` |
| Production | `publisher.mystira.app` | `chain.mystira.app` | `api.mystira.app` |

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

| Workflow | Trigger | Action |
|----------|---------|--------|
| `publisher-ci.yml` | Push/PR to dev/main | Lint, test, build, Docker push |
| `chain-ci.yml` | Push/PR to dev/main | Lint, test, build, Docker push |
| `story-generator-ci.yml` | Push/PR to dev/main | Lint, test, build, Docker push, NuGet |
| `release.yml` | Push to main | NPM publish via Changesets |
| `staging-release.yml` | Push to main | Auto-deploy to staging AKS |
| `production-release.yml` | Manual | Deploy to production AKS |
| `infra-deploy.yml` | Manual/Push | Full infrastructure deployment |

---

## GitHub Secrets Required

| Secret | Purpose | Required For |
|--------|---------|--------------|
| `AZURE_CREDENTIALS` | Azure service principal JSON | All Azure operations |
| `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` | PAT for private submodules | CI checkout |
| `NPM_TOKEN` | npm publish token | NPM releases |
| `NUGET_API_KEY` | NuGet.org API key | NuGet releases |
| `AZURE_CLIENT_ID` | Azure client ID (OIDC) | Federated auth |
| `AZURE_TENANT_ID` | Azure tenant ID (OIDC) | Federated auth |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID | Federated auth |

---

## Quick Reference Commands

### Check Published Docker Images
```bash
az acr repository list --name mysprodacr
az acr repository show-tags --name mysprodacr --repository publisher
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
