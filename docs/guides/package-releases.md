# Package Release Guide

**Last Updated**: 2025-12-24

This guide covers how to release packages in the Mystira workspace, including NPM packages, NuGet packages, and Docker images.

---

## Quick Reference

| Action | Command |
|--------|---------|
| Create a changeset | `pnpm changeset` |
| Check pending changesets | `pnpm changeset status` |
| Preview version bumps | `pnpm changeset version --dry-run` |
| Publish (CI only) | Automatic on merge to `main` |

---

## Understanding Changesets

[Changesets](https://github.com/changesets/changesets) is our version management tool. It tracks changes across packages and automates versioning and publishing.

### How It Works

1. **Developer creates a changeset** describing their change
2. **Changeset files accumulate** in `.changeset/` directory
3. **On merge to main**, the release workflow either:
   - Creates a "Version Packages" PR (if changesets exist)
   - Publishes packages (when Version PR is merged)

### Changeset File Structure

```
.changeset/
├── config.json          # Changesets configuration
├── README.md            # Instructions
├── fuzzy-tigers-jump.md # Example changeset (random name)
└── brave-lions-swim.md  # Another changeset
```

---

## Creating a Changeset

### Step 1: Make Your Code Changes

Develop your feature or fix as normal.

### Step 2: Run the Changeset Command

```bash
pnpm changeset
```

### Step 3: Select Affected Packages

Use arrow keys and space to select packages that were modified:

```
🦋  Which packages would you like to include?
   ◯ @mystira/publisher
   ◉ @mystira/shared-utils
   ◯ @mystira/admin-ui
```

### Step 4: Choose Version Bump Type

For each selected package, choose the appropriate bump:

| Type | When to Use | Example |
|------|-------------|---------|
| `patch` | Bug fixes, minor updates | `1.0.0` → `1.0.1` |
| `minor` | New features, backward compatible | `1.0.0` → `1.1.0` |
| `major` | Breaking changes | `1.0.0` → `2.0.0` |

```
🦋  Which packages should have a major bump?
   ◯ @mystira/shared-utils

🦋  Which packages should have a minor bump?
   ◉ @mystira/shared-utils

🦋  The following packages will be patch bumped:
   (none selected)
```

### Step 5: Write a Summary

Describe what changed. This becomes part of the CHANGELOG:

```
🦋  Please enter a summary for this change (this will be in the changelogs).
    (submit empty line to open external editor)

🦋  Summary: Add retry logic to API client for improved reliability
```

### Step 6: Commit the Changeset

The command creates a markdown file in `.changeset/`:

```bash
git add .changeset/fuzzy-tigers-jump.md
git commit -m "chore: add changeset for API retry logic"
```

---

## Changeset File Format

A changeset file looks like this:

```markdown
---
"@mystira/shared-utils": minor
"@mystira/publisher": patch
---

Add retry logic to API client for improved reliability.

- Implements exponential backoff for failed requests
- Adds configurable retry count (default: 3)
- Includes circuit breaker pattern for persistent failures
```

### Multiple Packages

If your change affects multiple packages, list them all:

```markdown
---
"@mystira/app": minor
"@mystira/app-contracts": minor
---

Add user preferences API endpoint and client types.
```

> **Note**: Linked packages (defined in `.changeset/config.json`) are versioned together automatically.

---

## Linked Packages

Some packages are "linked" - they always share the same version:

| Group | Packages |
|-------|----------|
| App | `@mystira/app`, `@mystira/app-contracts` |
| Story Generator | `@mystira/story-generator`, `@mystira/story-generator-contracts` |
| Publisher | `@mystira/publisher`, `@mystira/shared-utils` |

When you bump one package in a linked group, all packages in that group receive the same bump.

---

## Release Workflow

### Automatic Release (Recommended)

1. **Create PR** with your changes + changeset
2. **Merge to main** after approval
3. **Release workflow runs**:
   - If changesets exist → Creates "Version Packages" PR
   - If Version PR merged → Publishes packages

### Version Packages PR

The automated PR:
- Bumps versions in `package.json` files
- Updates `CHANGELOG.md` files
- Removes consumed changeset files

**Review and merge this PR to publish packages.**

### What Gets Published

| Trigger | NPM Packages | NuGet Packages | Docker Images |
|---------|--------------|----------------|---------------|
| Version PR merged | Published to npmjs.org | Triggered automatically | Tagged on next build |

---

## Pre-release Versions

### Creating a Pre-release

For testing before stable release:

```bash
# Enter pre-release mode
pnpm changeset pre enter beta

# Create changesets as normal
pnpm changeset

# Exit pre-release mode when ready
pnpm changeset pre exit
```

### Pre-release Tags

| Mode | Version Format | Use Case |
|------|---------------|----------|
| `alpha` | `1.0.0-alpha.0` | Early development |
| `beta` | `1.0.0-beta.0` | Feature complete, testing |
| `rc` | `1.0.0-rc.0` | Release candidate |

---

## NuGet Package Releases

NuGet packages are released through dedicated publish workflows with automatic dev/main branch handling:

### Workflows

| Workflow | Package | Version Source |
|----------|---------|----------------|
| `Packages - Contracts: Publish NuGet` | `Mystira.Contracts` | `package.json` (Changesets) |
| `Packages - Shared: Publish NuGet` | `Mystira.Shared` | `.csproj` file |
| `Packages - Legacy: Submodule NuGet` | Legacy submodule packages | ⚠️ Deprecated |

### Automatic Branch Publishing

Both `contracts-publish.yml` and `shared-publish.yml` automatically publish on pushes to dev or main:

| Branch | Version Format | Destination | Example |
|--------|---------------|-------------|---------|
| `dev` | `{base}-dev.{run_number}` | GitHub Packages only | `0.1.0-dev.123` |
| `main` | `{base}` (stable) | GitHub Packages + NuGet.org | `0.1.0` |

### Manual Trigger

Both workflows support manual dispatch with:
- **Version suffix** (optional): e.g., `beta.1`, `rc.1`
- **Publish to NuGet.org** checkbox: Controls whether to publish to public registry

### Unified Release (via Changesets)

When `@mystira/contracts` NPM package is published via Changesets, the corresponding NuGet package is triggered:

- `@mystira/contracts` (NPM) → `Mystira.Contracts` (NuGet)

### Version Synchronization

| Package | Version Source | Strategy |
|---------|----------------|----------|
| `Mystira.Contracts` | `packages/contracts/package.json` | Synced with NPM via Changesets |
| `Mystira.Shared` | `.csproj` Version property | Independent versioning |

### Legacy Submodule Dispatch

> ⚠️ **Deprecated**: The `Packages - Legacy: Submodule NuGet` workflow handles old submodule-based packages. Use `Mystira.Contracts` for new development.

For legacy packages, submodule CI can trigger publishing via `repository_dispatch`:

```yaml
- uses: peter-evans/repository-dispatch@v3
  with:
    token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
    repository: phoenixvc/Mystira.workspace
    event-type: nuget-publish
    client-payload: |
      {
        "package": "app-contracts",
        "version_suffix": "dev.${{ github.run_number }}",
        "is_prerelease": "true"
      }
```

---

## Docker Image Releases

Docker images are built and pushed by service-specific CI workflows.

### Tagging Strategy

| Environment | Tags |
|-------------|------|
| Development | `dev`, `dev-{sha}` |
| Staging | `staging`, `staging-{sha}` |
| Production | `prod`, `prod-{sha}`, `latest` |

### Manual Build

```bash
# Build locally
docker build -f infra/docker/publisher/Dockerfile -t publisher:local .

# Push to ACR (requires login)
az acr login --name myssharedacr
docker tag publisher:local myssharedacr.azurecr.io/publisher:dev
docker push myssharedacr.azurecr.io/publisher:dev
```

---

## Checking Release Status

### Pending Changesets

```bash
# View pending changesets
pnpm changeset status

# Preview what versions would be bumped
pnpm changeset version --dry-run
```

### Published Packages

```bash
# Check NPM package
npm view @mystira/publisher versions

# Check NuGet package
dotnet package search Mystira.StoryGenerator --source github

# Check Docker images
az acr repository show-tags --name myssharedacr --repository publisher
```

### GitHub Actions

Monitor releases at: `https://github.com/phoenixvc/Mystira.workspace/actions`

| Workflow | Purpose |
|----------|---------|
| `Workspace: Release` | NPM publishing via Changesets + trigger NuGet |
| `Packages - Contracts: Publish NuGet` | Unified contracts NuGet package |
| `Packages - Shared: Publish NuGet` | Shared utilities NuGet package |
| `Packages - Legacy: Submodule NuGet` | ⚠️ Deprecated submodule packages |
| `*-ci.yml` | Docker image builds |

---

## Troubleshooting

### Changeset Not Detected

**Problem**: CI says "No changesets found"

**Solution**: Ensure the changeset file is committed:
```bash
git status  # Check for untracked files in .changeset/
git add .changeset/*.md
git commit -m "chore: add changeset"
```

### Version PR Not Created

**Problem**: Merged to main but no Version PR appeared

**Possible causes**:
1. No changesets in the merge
2. Workflow failed (check Actions tab)
3. All changesets were for ignored packages

### Package Not Publishing

**Problem**: Version PR merged but package not on npm

**Check**:
1. `NPM_TOKEN` secret is valid
2. Package `access` is set to `public` in config
3. No npm publish errors in workflow logs

### NuGet Publish Failed

**Problem**: NuGet package not appearing after release

**Check**:
1. `NUGET_API_KEY` secret is valid
2. Package version doesn't already exist
3. Check workflow logs:
   - `contracts-publish.yml` for Mystira.Contracts
   - `shared-publish.yml` for Mystira.Shared
   - `nuget-publish.yml` for legacy packages

---

## Best Practices

### Do

- Create changesets for every user-facing change
- Write clear, user-focused summaries
- Include context about why the change was made
- Group related changes in a single changeset

### Don't

- Create changesets for internal refactoring (unless it affects the API)
- Use technical jargon in summaries (users read changelogs)
- Forget to commit the changeset file
- Manually edit version numbers in `package.json`

### Changeset Summary Examples

**Good**:
```
Add retry logic to API client for improved reliability under network issues.
```

**Bad**:
```
fix: refactor ApiClient.ts to use async/await with try/catch
```

---

## Package Structure

### Current Packages

| Package | Type | Location | Purpose |
|---------|------|----------|---------|
| `@mystira/app` | NPM | `packages/app` | Core app package |
| `@mystira/app-contracts` | NPM | `packages/app` | App API types |
| `@mystira/story-generator` | NPM | `packages/story-generator` | Story generator package |
| `@mystira/story-generator-contracts` | NPM | `packages/story-generator` | Story generator API types |
| `@mystira/publisher` | NPM | `packages/publisher` | Publishing service |
| `@mystira/shared-utils` | NPM | `packages/publisher` | Shared utilities |
| `Mystira.Contracts` | NuGet | `packages/contracts/dotnet` | Unified .NET API contracts |
| `Mystira.Shared` | NuGet | `packages/shared` | .NET shared utilities |
| `Mystira.App.Contracts` | NuGet | `packages/app` | ⚠️ Deprecated - use Mystira.Contracts |
| `Mystira.StoryGenerator.Contracts` | NuGet | `packages/story-generator` | ⚠️ Deprecated - use Mystira.Contracts |

### Linked Packages

Packages in linked groups always version together:

```
Group 1: @mystira/app ↔ @mystira/app-contracts
Group 2: @mystira/story-generator ↔ @mystira/story-generator-contracts
Group 3: @mystira/publisher ↔ @mystira/shared-utils
```

### Consolidation Status

**Phase 1 Complete** - New unified packages are available:

| New Package | Replaces | Status |
|-------------|----------|--------|
| `@mystira/contracts` (NPM) | `@mystira/app-contracts`, `@mystira/story-generator-contracts` | ✅ Available |
| `Mystira.Contracts` (NuGet) | `Mystira.App.Contracts`, `Mystira.StoryGenerator.Contracts` | ✅ Available |
| `Mystira.Shared` (NuGet) | N/A (new shared utilities) | ✅ Available |
| `@mystira/shared-utils` | (moved from Publisher) | ✅ Available |

**Migration Period Active** - Old packages still work but are deprecated.

### NuGet Workflow Names

All NuGet workflows use the "Packages - " prefix for grouping in GitHub Actions UI:

| Workflow Name | Purpose |
|---------------|---------|
| `Packages - Contracts: Publish NuGet` | Unified contracts package |
| `Packages - Shared: Publish NuGet` | Shared utilities package |
| `Packages - Legacy: Submodule NuGet` | ⚠️ Deprecated submodule packages |

See [Contracts Migration Guide](./contracts-migration.md) for upgrade instructions.

See [ADR-0020: Package Consolidation Strategy](../architecture/adr/0020-package-consolidation-strategy.md) for details.

---

## Related Documentation

- [Contracts Migration Guide](./contracts-migration.md) - Migrate to unified contracts
- [Publishing & Deployment Flow](../cicd/publishing-flow.md) - Complete publishing overview
- [Package Inventory](../analysis/package-inventory.md) - Full package analysis with consolidation recommendations
- [CI/CD Setup](../cicd/cicd-setup.md) - CI configuration
- [Commit Conventions](./commit-conventions.md) - Commit message format
- [ADR-0003: Release Pipeline Strategy](../architecture/adr/0003-release-pipeline-strategy.md) - Release architecture
- [ADR-0020: Package Consolidation Strategy](../architecture/adr/0020-package-consolidation-strategy.md) - Package consolidation plan

---

## Quick Cheat Sheet

```bash
# Create a changeset
pnpm changeset

# Check status
pnpm changeset status

# Preview version changes
pnpm changeset version --dry-run

# Enter pre-release mode
pnpm changeset pre enter beta

# Exit pre-release mode
pnpm changeset pre exit

# View changesets config
cat .changeset/config.json
```
