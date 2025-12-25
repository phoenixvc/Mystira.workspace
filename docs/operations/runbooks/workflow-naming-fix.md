# Runbook: Fix GitHub Actions Workflow Naming

**Last Updated**: 2025-12-22
**Owner**: DevOps Team
**Estimated Time**: 5 minutes per submodule

## Problem

GitHub Actions workflows in submodule repositories are displaying as file paths (e.g., `.github/workflows/admin-api-ci.yml`) instead of descriptive names in the Actions UI.

## Affected Repositories

The following submodule repositories need their workflow names updated:

| Repository | File Path | Current Display |
|------------|-----------|-----------------|
| admin-api | `.github/workflows/ci.yml` | `.github/workflows/admin-api-ci.yml` |
| admin-ui | `.github/workflows/ci.yml` | `.github/workflows/admin-ui-ci.yml` |
| app | `.github/workflows/ci.yml` | `.github/workflows/app-ci.yml` |
| chain | `.github/workflows/ci.yml` | `.github/workflows/chain-ci.yml` |
| devhub | `.github/workflows/ci.yml` | `.github/workflows/devhub-ci.yml` |
| publisher | `.github/workflows/ci.yml` | `.github/workflows/publisher-ci.yml` |
| story-generator | `.github/workflows/ci.yml` | `.github/workflows/story-generator-ci.yml` |

## Root Cause

Workflow files are missing the `name:` field at the top of the YAML file. Without this field, GitHub displays the file path instead of a friendly name.

## Solution

Add a descriptive `name:` field to the top of each workflow file.

### Naming Convention

Use the following naming convention for consistency:

```
[Component Name]: [Workflow Type]
```

Examples:
- `Admin API: CI`
- `Admin UI: CI`
- `Chain: CI`
- `Publisher: Build & Test`
- `Story Generator: CI`

### Fix Procedure

For each submodule repository:

1. **Clone or navigate to the repository**
   ```bash
   cd packages/admin-api  # Example
   git checkout main
   git pull origin main
   ```

2. **Edit the workflow file**
   ```bash
   # Open .github/workflows/ci.yml (or the relevant workflow file)
   ```

3. **Add the name field** at the very top of the file:
   ```yaml
   name: "Admin API: CI"  # Add this line FIRST

   on:
     push:
       branches: [main, dev]
     pull_request:
       branches: [main]
   # ... rest of workflow
   ```

4. **Commit and push**
   ```bash
   git add .github/workflows/ci.yml
   git commit -m "fix: Add descriptive name to CI workflow"
   git push origin main
   ```

5. **Repeat** for each affected repository

### Example Before/After

**Before:**
```yaml
on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    # ...
```

**After:**
```yaml
name: "Admin API: CI"

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    # ...
```

## Recommended Workflow Names

| Repository | Suggested Name |
|------------|----------------|
| admin-api | `Admin API: CI` |
| admin-ui | `Admin UI: CI` |
| app | `Mystira App: CI` |
| chain | `Chain: CI` |
| devhub | `DevHub: CI` |
| publisher | `Publisher: CI` |
| story-generator | `Story Generator: CI` |

## Verification

After updating, the workflows will appear with friendly names in the GitHub Actions UI:

1. Go to the repository's Actions tab
2. Verify workflows show the new names
3. Run a test workflow to confirm it still functions

## Notes

- The `name:` field must be at the very top of the YAML file
- Quotes around the name are optional but recommended for names with colons
- Changes take effect immediately after pushing
- The parent workspace repository doesn't need changes - just the submodules

## Related Documentation

- [GitHub Actions Workflow Syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#name)
- [ADR-0004: Branching Strategy & CI/CD](../../architecture/adr/0004-branching-strategy-and-cicd.md)
