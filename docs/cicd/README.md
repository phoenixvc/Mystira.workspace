# CI/CD Documentation

This directory contains documentation for CI/CD pipelines, GitHub Actions workflows, and DevOps practices.

## Contents

- [Publishing & Deployment Flow](./publishing-flow.md) - **Complete guide to package publishing and deployment**
- [CI/CD Setup](./cicd-setup.md) - CI/CD pipelines and branch protection configuration
- [Branch Protection](./branch-protection.md) - Branch protection rules for `dev` and `main`
- [Submodule Access](./submodule-access.md) - Troubleshooting guide for submodule access in CI/CD
- [Workflow Permissions](./workflow-permissions.md) - Explanation of GitHub workflow permissions and token management

## Quick Reference

### Publishing Destinations

| Artifact Type | Destination | Trigger |
|---------------|-------------|---------|
| Docker Images | `myssharedacr.azurecr.io` | Push to `dev`/`main` |
| NPM Packages | `npmjs.org` | Changesets on `main` |
| NuGet Packages | GitHub Packages / NuGet.org | Push to `main` |
| Deployments | Azure Kubernetes Service | Auto (staging) / Manual (prod) |

### Deployment Environments

| Environment | URL Pattern | Trigger |
|-------------|-------------|---------|
| Dev | `dev.*.mystira.app` | Manual via `infra-deploy` |
| Staging | `staging.*.mystira.app` | Auto on merge to `main` |
| Production | `*.mystira.app` | Manual with confirmation |

## Related Documentation

- [Infrastructure Deployment Checklist](../../DEPLOYMENT_CHECKLIST.md) - Full deployment guide
- [ADR-0004: Branching Strategy and CI/CD](../architecture/adr/0004-branching-strategy-and-cicd.md) - Architecture decision on branching
- [ADR-0003: Release Pipeline Strategy](../architecture/adr/0003-release-pipeline-strategy.md) - Release pipeline architecture
