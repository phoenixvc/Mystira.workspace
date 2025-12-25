# CI/CD Setup & Branch Protection

**Last Updated**: 2025-12-22
**Status**: ✅ Complete

## Summary

All CI/CD pipelines and branch protection have been successfully configured according to [ADR-0004: Branching Strategy and CI/CD](./architecture/adr/0004-branching-strategy-and-cicd.md).

## Distributed CI Model (December 2025)

As of December 2025, we migrated to a **distributed CI model** where:

- **Dev CI** runs in each component's own repository for fast feedback
- **Staging/Production deployments** are managed from this workspace

### Component Dev CI Workflows

Each component repository now has its own `ci.yml` workflow:

| Component | Repository | Runtime | PR Merged |
|-----------|------------|---------|-----------|
| Admin API | [Mystira.Admin.Api](https://github.com/phoenixvc/Mystira.Admin.Api) | .NET 9.0 | #7 |
| Admin UI | [Mystira.Admin.UI](https://github.com/phoenixvc/Mystira.Admin.UI) | Node.js 20 | #12 |
| Chain | [Mystira.Chain](https://github.com/phoenixvc/Mystira.Chain) | Python 3.11 | #1 |
| DevHub | [Mystira.DevHub](https://github.com/phoenixvc/Mystira.DevHub) | Node.js 20 | #1 |
| Publisher | [Mystira.Publisher](https://github.com/phoenixvc/Mystira.Publisher) | Node.js 20 | #13 |
| Story Generator | [Mystira.StoryGenerator](https://github.com/phoenixvc/Mystira.StoryGenerator) | .NET 9.0 | #56 |
| App | [Mystira.App](https://github.com/phoenixvc/Mystira.App) | .NET 9.0 | (existing) |

### Workspace Workflows

This workspace now focuses on:

- **Infrastructure workflows** (`infra-deploy.yml`, `infra-validate.yml`)
- **Deployment workflows** (`staging-release.yml`, `production-release.yml`)
- **Workspace CI** (`ci.yml`) - workspace-level validation
- **Utilities** (`check-submodules.yml`, `utilities-link-checker.yml`)

## Branch Protection Configuration

### `dev` Branch Protection

- ✅ **Require pull request before merging**: Enabled
- ✅ **Required approvals**: 0 (for fast iteration)
- ✅ **Require status checks to pass**: Enabled
  - All CI checks required: `CI / lint`, `CI / test`, `CI / build`, `Chain CI / lint`, `Chain CI / test`, `Chain CI / build`, `Publisher CI / lint`, `Publisher CI / test`, `Publisher CI / build`
- ✅ **Require branches to be up to date**: Enabled
- ✅ **Allow force pushes**: Disabled
- ✅ **Allow deletions**: Disabled

### `main` Branch Protection

- ✅ **Require pull request before merging**: Enabled
- ✅ **Required approvals**: 1
- ✅ **Require code owner reviews**: Enabled
- ✅ **Dismiss stale reviews**: Enabled
- ✅ **Require status checks to pass**: Enabled (all CI checks required)
- ✅ **Require conversation resolution**: Enabled
- ✅ **Allow force pushes**: Disabled
- ✅ **Allow deletions**: Disabled

## Branch Protection Status

| Branch | Protected | PR Required | Approvals | Code Owners | Conversation Resolution | CI Checks |
| ------ | --------- | ----------- | --------- | ----------- | ----------------------- | --------- |
| `dev`  | ✅ Yes    | ✅ Yes      | 0         | ❌ No       | ❌ No                   | ✅ 9      |
| `main` | ✅ Yes    | ✅ Yes      | 1         | ✅ Yes      | ✅ Yes                  | ✅ 9      |

## GitHub Environments

### Staging Environment

- ✅ Created and configured
- No approval required (can be configured via GitHub UI)
- Used by automatic staging release workflow on merge to `main`

### Production Environment

- ✅ Created and configured
- Reviewers can be added via GitHub UI (Settings → Environments → production → Required reviewers)
- Used by manual production release workflow

## Workflow Configuration

### Current Workflow

```
Feature Branch
  ↓
PR to dev
  ↓ (CI checks run, must pass)
Merge to dev
  ↓ (Docker images pushed to ACR)
  ↓
PR to main
  ↓ (CI checks run, must pass)
  ↓ (1 approval required)
Merge to main
  ↓ (Automatic staging deployment)
  ↓
Manual Production Deployment
  ↓ (Approval required)
Production
```

### Workflow Triggers

- **CI Workflows**: Trigger on pushes to `dev`/`main` and PRs to `dev`/`main`
- **Staging Release**: Automatic on merge to `main`
- **Production Release**: Manual with environment approval

## Completed Actions

### ✅ PR #29 Merged

- **PR**: #29 - "feat: update CI/CD pipelines for PR workflow to dev"
- **Status**: Merged to `main` (2025-12-14)
- **Changes**: Updated all CI workflows to support PRs to `dev` branch

### ✅ Dev Branch Created

- **Branch**: `dev`
- **Created from**: `main`
- **Status**: Pushed to remote and tracking set up

### ✅ Branch Protection Configured

Both `dev` and `main` branch protection rules configured as above.

### ✅ GitHub Environments Created

Both `staging` and `production` environments created and configured.

## Verification Commands

### Check Branch Protection

```bash
# Check dev branch protection
gh api repos/phoenixvc/Mystira.workspace/branches/dev/protection

# Check main branch protection
gh api repos/phoenixvc/Mystira.workspace/branches/main/protection
```

### Check Environments

```bash
# List environments
gh api repos/phoenixvc/Mystira.workspace/environments
```

## Azure Service Principal Permissions

The infrastructure deployment workflows require specific Azure permissions. Before running infrastructure deployments, ensure the service principal has:

1. **Contributor** role at subscription level (basic resource management)
2. **User Access Administrator** role (for RBAC role assignments)
3. **Azure AD permissions** (for app registrations and managed identities)
4. **Storage Blob Data Contributor** on Terraform state storage account

For detailed setup instructions, see the [Azure Setup Guide](../../infra/azure-setup.md#step-2-required-permissions).

### Automated Permission Checks

The `infra-deploy.yml` workflow includes automated permission validation that runs before Terraform. If permissions are missing, check the "Check Service Principal Permissions" step in the workflow run for:

- Specific missing permissions
- Commands to grant the required permissions
- Links to relevant documentation

## Related Documentation

- [Azure Setup Guide](../../infra/azure-setup.md) - Service principal and permission setup
- [Branch Protection](./branch-protection.md)
- [ADR-0004: Branching Strategy and CI/CD](../architecture/adr/0004-branching-strategy-and-cicd.md)
- [ADR-0003: Release Pipeline Strategy](../architecture/adr/0003-release-pipeline-strategy.md)
