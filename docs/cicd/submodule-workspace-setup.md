# Submodule Workspace Setup Instructions

**Last Updated**: 2025-12-24

This document provides instructions to run in each submodule repository to ensure proper integration with the Mystira.workspace monorepo.

---

## Overview

Each submodule needs to be configured to:
1. Allow the workspace to access it via HTTPS with tokens
2. Support the unified package publishing flow
3. Enable proper CI/CD integration

---

## Instructions by Submodule

### All Submodules - Common Setup

Run these commands in each submodule repository:

```bash
# Ensure the repo allows being used as a submodule
# (No action needed - just verify .gitmodules in workspace points to correct URL)

# Verify the repo has proper package.json (if NPM package)
cat package.json | jq '.name, .version'
```

---

### Mystira.App (packages/app)

```bash
# 1. Ensure GitHub Actions can trigger workspace workflows
# Add to .github/workflows/ci.yml (if dispatching to workspace):

# on:
#   push:
#     branches: [main, dev]
#
# jobs:
#   notify-workspace:
#     runs-on: ubuntu-latest
#     if: github.ref == 'refs/heads/main'
#     steps:
#       - name: Trigger workspace NuGet publish
#         uses: peter-evans/repository-dispatch@v3
#         with:
#           token: ${{ secrets.MYSTIRA_WORKSPACE_TOKEN }}
#           repository: phoenixvc/Mystira.workspace
#           event-type: nuget-publish
#           client-payload: |
#             {
#               "package": "app-contracts",
#               "ref": "${{ github.sha }}",
#               "triggered_by": "app-submodule"
#             }

# 2. Verify NuGet package configuration
cat src/Mystira.App.Contracts/Mystira.App.Contracts.csproj | grep -E "<PackageId>|<Version>"

# 3. Ensure proper .npmrc for workspace integration (if applicable)
echo "//registry.npmjs.org/:_authToken=\${NPM_TOKEN}" > .npmrc
```

---

### Mystira.StoryGenerator (packages/story-generator)

```bash
# 1. Same workflow dispatch setup as App
# Add repository_dispatch trigger to CI workflow

# 2. Verify NuGet package configuration
cat src/Mystira.StoryGenerator.Contracts/Mystira.StoryGenerator.Contracts.csproj | grep -E "<PackageId>|<Version>"

# 3. Ensure proper .npmrc for workspace integration
echo "//registry.npmjs.org/:_authToken=\${NPM_TOKEN}" > .npmrc
```

---

### Mystira.Publisher (packages/publisher)

```bash
# 1. Note: @mystira/shared-utils has been MOVED to workspace level
# If this submodule still has shared-utils, it should be deprecated

# 2. Update any internal references to use workspace shared-utils
# Change: import { ... } from './packages/shared-utils'
# To:     import { ... } from '@mystira/shared-utils'

# 3. Verify package.json dependencies
cat package.json | jq '.dependencies, .devDependencies'
```

---

### Mystira.Admin.UI (packages/admin-ui)

```bash
# 1. Ensure the submodule is accessible with the PAT
# The workspace uses MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN

# 2. Verify the submodule can be cloned
git remote -v

# 3. If private, ensure the PAT has repo access
```

---

### Mystira.Admin.Api (packages/admin-api)

```bash
# 1. Same as Admin.UI - ensure PAT access

# 2. Verify .NET build works
dotnet build --configuration Release
```

---

### Mystira.Chain (packages/chain)

```bash
# 1. Verify Docker build works
docker build -f Dockerfile -t chain:test .

# 2. Ensure any workspace integrations are configured
```

---

### Mystira.DevHub (packages/devhub)

```bash
# Standard setup - no special configuration needed
```

---

## GitHub Secrets Required

Each submodule that dispatches to the workspace needs:

| Secret | Purpose | Where to Add |
|--------|---------|--------------|
| `MYSTIRA_WORKSPACE_TOKEN` | PAT to trigger workspace workflows | Submodule repo settings |

The workspace needs:

| Secret | Purpose |
|--------|---------|
| `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` | PAT to clone private submodules |
| `NPM_TOKEN` | Publish to npmjs.org |
| `NUGET_API_KEY` | Publish to NuGet.org |
| `GH_PACKAGES_TOKEN` | Publish to GitHub Packages |

---

## Verifying Submodule Integration

Run from the workspace root:

```bash
# 1. Update all submodules
git submodule update --init --recursive

# 2. Check submodule status
git submodule status

# 3. Verify each submodule is on expected commit
git submodule foreach 'echo "$name: $(git rev-parse HEAD)"'

# 4. Test build with submodules
pnpm install
pnpm build
```

---

## Troubleshooting

### "could not read Username for 'https://github.com'"

**Cause**: Missing or invalid PAT for private submodules.

**Fix**: Ensure `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` is set and has `repo` scope.

```yaml
# In workflow:
- uses: actions/checkout@v4
  with:
    submodules: recursive
    token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
```

### "Fetched in submodule path but it did not contain [commit]"

**Cause**: Submodule is pointing to a commit that doesn't exist in the remote.

**Fix**:
```bash
# In the workspace:
cd packages/<submodule>
git fetch origin
git checkout main  # or the expected branch
cd ../..
git add packages/<submodule>
git commit -m "chore: update submodule reference"
```

### Submodule not building in CI

**Check**:
1. Is the submodule in `pnpm-workspace.yaml`?
2. Does the submodule have a valid `package.json`?
3. Are dependencies installed correctly?

```bash
# Verify workspace includes submodule
cat pnpm-workspace.yaml

# Check package.json exists
ls packages/<submodule>/package.json
```

---

## Migration Notes

### shared-utils Migration

The `@mystira/shared-utils` package has been moved from `packages/publisher` to workspace-level `packages/shared-utils`.

**For Publisher submodule:**
```bash
# 1. Remove local shared-utils (if still exists)
rm -rf packages/shared-utils

# 2. Update imports in code
# From: import { withRetry } from './packages/shared-utils'
# To:   import { withRetry } from '@mystira/shared-utils'

# 3. Add as dependency
pnpm add @mystira/shared-utils
```

### contracts Migration

Legacy contracts packages are deprecated. See [Contracts Migration Guide](../guides/contracts-migration.md).

---

## Related Documentation

- [Submodule CI/CD Setup](./submodule-cicd-setup.md)
- [Publishing Flow](./publishing-flow.md)
- [Package Releases Guide](../guides/package-releases.md)
- [Contracts Migration Guide](../guides/contracts-migration.md)
