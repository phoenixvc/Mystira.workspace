# CI/CD Documentation

This directory contains documentation for CI/CD pipelines, GitHub Actions workflows, and DevOps practices.

## Contents

- [Publishing & Deployment Flow](./publishing-flow.md) - Complete guide to deployment pipelines
- [CI/CD Setup](./cicd-setup.md) - CI/CD pipeline configuration
- [Branch Protection](./branch-protection.md) - Branch protection rules for `dev` and `main`
- [Workflow Permissions](./workflow-permissions.md) - GitHub workflow permissions and token management

## Quick Reference

### Deployment Environments

| Environment | URL Pattern             | Trigger                  |
| ----------- | ----------------------- | ------------------------ |
| Dev         | `dev.*.mystira.app`     | Push to `dev`            |
| Staging     | `staging.*.mystira.app` | Auto on merge to `main`  |
| Production  | `*.mystira.app`         | Manual with confirmation |

### Build Pipeline

All packages are built from the monorepo root:

```bash
pnpm build          # TypeScript packages (Turbo)
dotnet build        # .NET packages (MSBuild)
pnpm test           # All tests
```

## Related Documentation

- [Azure Setup Guide](../../infra/azure-setup.md) - Service principal and permission setup
- [ADR-0004: Branching Strategy and CI/CD](../architecture/adr/0004-branching-strategy-and-cicd.md) - Architecture decision on branching
