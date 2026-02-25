# CI/CD Documentation

This directory contains documentation for CI/CD pipelines, GitHub Actions workflows, and DevOps practices.

> Source of truth for current workflow file inventory and trigger matrix:
> `.github/workflows/README.md`.

## Contents

- [Publishing & Deployment Flow](./publishing-flow.md) - **Complete guide to package publishing and deployment**
- [CI/CD Setup](./cicd-setup.md) - CI/CD pipelines and branch protection configuration
- [Branch Protection](./branch-protection.md) - Branch protection rules for `dev` and `main`
- [Workflow Permissions](./workflow-permissions.md) - Explanation of GitHub workflow permissions and token management

## Quick Reference

### Publishing Destinations

| Artifact Type  | Destination                 | Trigger                        |
| -------------- | --------------------------- | ------------------------------ |
| Docker Images  | `myssharedacr.azurecr.io`   | Push to `dev`/`main`           |
| NPM Packages   | `npmjs.org`                 | Changesets on `main`           |
| NuGet Packages | GitHub Packages / NuGet.org | Push to `main`                 |
| Deployments    | Azure Kubernetes Service    | Auto (staging) / Manual (prod) |

### Deployment Environments

| Environment | URL Pattern             | Trigger                  |
| ----------- | ----------------------- | ------------------------ |
| Dev         | `dev.*.mystira.app`     | `infra-deploy` workflow  |
| Staging     | `staging.*.mystira.app` | Auto on merge to `main`  |
| Production  | `*.mystira.app`         | Manual with confirmation |

> **Note**: Dev deployments can be triggered via `infra-deploy` workflow dispatch.

## Azure Permissions

Infrastructure deployments require specific Azure permissions for the service principal. See [Azure Setup Guide](../../infra/azure-setup.md#step-2-required-permissions) for:

- Required Azure RBAC roles
- Azure AD / Entra ID permissions
- Complete permission setup script
- Automated permission validation

## Related Documentation

- [Azure Setup Guide](../../infra/azure-setup.md) - Service principal and permission setup
- [Infrastructure Deployment Checklist](../../DEPLOYMENT_CHECKLIST.md) - Full deployment guide
- [ADR-0004: Branching Strategy and CI/CD](../architecture/adr/0004-branching-strategy-and-cicd.md) - Architecture decision on branching
- [ADR-0003: Release Pipeline Strategy](../architecture/adr/0003-release-pipeline-strategy.md) - Release pipeline architecture
