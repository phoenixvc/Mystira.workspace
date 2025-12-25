# App Repository Workflow Cleanup Guide

This document provides detailed instructions for cleaning up workflows in the [Mystira.App](https://github.com/phoenixvc/Mystira.App) repository.

## Summary

**Goal:** Remove duplicate and unnecessary workflows now that workspace handles staging/production releases.

**Impact:** Reduce from 18 workflows to 3-7 workflows (depending on dev deployment preference).

## Quick Command Reference

### Minimum Cleanup (Delete 11 workflows)

```bash
cd /path/to/Mystira.App

# Delete staging workflows (6 files)
rm .github/workflows/infrastructure-deploy-staging.yml
rm .github/workflows/mystira-app-admin-api-cicd-staging.yml
rm .github/workflows/mystira-app-api-cicd-staging.yml
rm .github/workflows/mystira-app-pwa-cicd-staging.yml
rm .github/workflows/mystira-app-pwa-cicd-staging.yml.disabled
rm .github/workflows/staging-automated-setup.yml

# Delete production workflows (4 files)
rm .github/workflows/infrastructure-deploy-prod.yml
rm .github/workflows/mystira-app-admin-api-cicd-prod.yml
rm .github/workflows/mystira-app-api-cicd-prod.yml
rm .github/workflows/mystira-app-pwa-cicd-prod.yml

# Delete package publishing (1 file)
rm .github/workflows/publish-shared-packages.yml

# Commit changes
git add .github/workflows/
git commit -m "refactor: Remove duplicate staging/prod workflows

- Workspace now handles staging deployments via staging-release.yml
- Workspace now handles production deployments via production-release.yml
- Workspace now handles NuGet package publishing
- Eliminates duplication and centralizes release control
- App repo focuses on dev CI and preview environments"
```

### Full Centralization (Delete 15 workflows)

If you want **all** deployments controlled from workspace:

```bash
cd /path/to/Mystira.App

# Delete all previous files plus dev workflows (4 more files)
rm .github/workflows/infrastructure-deploy-dev.yml
rm .github/workflows/mystira-app-admin-api-cicd-dev.yml
rm .github/workflows/mystira-app-api-cicd-dev.yml
rm .github/workflows/mystira-app-pwa-cicd-dev.yml

# Commit changes
git add .github/workflows/
git commit -m "refactor: Move all deployment workflows to workspace

- Workspace handles all deployments (dev/staging/prod)
- App repo focuses solely on CI testing and preview environments
- Centralized deployment control across all environments"
```

## Detailed Workflow Analysis

### ✅ KEEP (3 workflows) - Development & Testing

These workflows provide fast feedback for developers and should stay:

#### 1. ci-tests-codecov.yml

**Purpose:** Run tests and collect code coverage on PRs
**Keep because:** Fast dev feedback, PR validation
**Triggers:** pull_request, push to dev/main

#### 2. swa-preview-tests.yml

**Purpose:** Deploy and test preview environments for PRs
**Keep because:** Review app changes before merging
**Triggers:** pull_request, workflow_dispatch

#### 3. swa-cleanup-staging-environments.yml

**Purpose:** Clean up preview environments
**Keep because:** Resource management, cost control
**Triggers:** schedule, workflow_dispatch

### ❌ DELETE (6 workflows) - Staging Deployments

Workspace `staging-release.yml` now handles staging deployments:

#### 4. infrastructure-deploy-staging.yml

**Replaced by:** Workspace staging-release.yml
**Why delete:** Duplicate, workspace has centralized control

#### 5. mystira-app-admin-api-cicd-staging.yml

**Replaced by:** Workspace staging-release.yml
**Why delete:** Workspace orchestrates multi-service deployment

#### 6. mystira-app-api-cicd-staging.yml

**Replaced by:** Workspace staging-release.yml
**Why delete:** Workspace orchestrates multi-service deployment

#### 7. mystira-app-pwa-cicd-staging.yml

**Replaced by:** Workspace staging-release.yml
**Why delete:** Workspace orchestrates multi-service deployment

#### 8. mystira-app-pwa-cicd-staging.yml.disabled

**Why delete:** Already disabled, no longer needed

#### 9. staging-automated-setup.yml

**Replaced by:** Workspace staging-release.yml
**Why delete:** Workspace handles infrastructure setup

### ❌ DELETE (4 workflows) - Production Deployments

Workspace `production-release.yml` now handles production deployments:

#### 10. infrastructure-deploy-prod.yml

**Replaced by:** Workspace production-release.yml
**Why delete:** Prevents accidental prod deployments, centralized control

#### 11. mystira-app-admin-api-cicd-prod.yml

**Replaced by:** Workspace production-release.yml
**Why delete:** Workspace orchestrates multi-service deployment

#### 12. mystira-app-api-cicd-prod.yml

**Replaced by:** Workspace production-release.yml
**Why delete:** Workspace orchestrates multi-service deployment

#### 13. mystira-app-pwa-cicd-prod.yml

**Replaced by:** Workspace production-release.yml
**Why delete:** Workspace orchestrates multi-service deployment

### ❌ DELETE (1 workflow) - Package Publishing

Workspace will handle NuGet package publishing:

#### 14. publish-shared-packages.yml

**Replaced by:** Workspace workflow (to be added)
**Why delete:** Centralized release control, consistent versioning

### 🔄 OPTIONAL (4 workflows) - Dev Deployments

**Recommendation: KEEP** for fast development iterations

#### Option A: Keep Dev Workflows (Recommended)

**Advantages:**

- ✅ Fast deployment after merging to dev branch
- ✅ No workspace dependency for dev work
- ✅ Quick testing in dev environment
- ✅ Independent development velocity

**Keep these:**

- infrastructure-deploy-dev.yml
- mystira-app-admin-api-cicd-dev.yml
- mystira-app-api-cicd-dev.yml
- mystira-app-pwa-cicd-dev.yml

#### Option B: Delete Dev Workflows (More Centralized)

**Advantages:**

- ✅ All deployments in one place
- ✅ Consistent with staging/prod approach
- ✅ Single source of truth

**Disadvantages:**

- ⚠️ Slower dev feedback (must update workspace submodule)
- ⚠️ More steps to deploy to dev
- ⚠️ Couples dev work to workspace updates

## Verification

After cleanup, verify the remaining workflows:

```bash
# Should show 3-7 files depending on your choice
ls .github/workflows/*.yml | wc -l

# List remaining workflows
ls -1 .github/workflows/*.yml
```

**Expected result (if keeping dev workflows):**

```
.github/workflows/ci-tests-codecov.yml
.github/workflows/infrastructure-deploy-dev.yml
.github/workflows/mystira-app-admin-api-cicd-dev.yml
.github/workflows/mystira-app-api-cicd-dev.yml
.github/workflows/mystira-app-pwa-cicd-dev.yml
.github/workflows/swa-cleanup-staging-environments.yml
.github/workflows/swa-preview-tests.yml
.github/workflows/templates/
```

## Next Steps

1. **Execute cleanup** using commands above
2. **Test remaining workflows** work correctly
3. **Update workspace** to handle staging/prod/NuGet publishing
4. **Document changes** in App repo README
5. **Notify team** about new workflow strategy

## Benefits After Cleanup

✅ **No duplication** - Single source for staging/prod deployments
✅ **Controlled releases** - Only workspace can deploy to staging/prod
✅ **Fast development** - App repo CI still provides quick feedback
✅ **Clear boundaries** - Dev in App repo, releases from workspace
✅ **Easier maintenance** - Fewer workflows to manage
✅ **Cost savings** - Fewer unnecessary workflow runs

## Questions?

If you have questions about which workflows to delete, refer to:

- This guide
- `.github/workflows/README.md` in workspace
- Team discussions

## Related Documentation

- [Workspace Workflow Documentation](../../Mystira.workspace/.github/workflows/README.md)
- [Development Workflow Guide](../../docs/development-workflow.md)
