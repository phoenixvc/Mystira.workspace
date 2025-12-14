# CI/CD Setup & Branch Protection

**Last Updated**: 2025-12-14  
**Status**: ✅ Complete

## Summary

All CI/CD pipelines and branch protection have been successfully configured according to [ADR-0004: Branching Strategy and CI/CD](./architecture/adr/0004-branching-strategy-and-cicd.md).

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

## Related Documentation

- [Branch Protection](./branch-protection.md)
- [ADR-0004: Branching Strategy and CI/CD](./architecture/adr/0004-branching-strategy-and-cicd.md)
- [ADR-0003: Release Pipeline Strategy](./architecture/adr/0003-release-pipeline-strategy.md)
