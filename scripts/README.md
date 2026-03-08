# Mystira Scripts

Utility scripts for managing the Mystira workspace and repositories.

## Documentation Generation

### `create-doc.ps1`

Creates standardized documentation templates for completed PRs and implementations.

**Usage:**

```powershell
# Create implementation documentation
./create-doc.ps1 implementation "TreatWarningsAsErrors=true" 1234

# Create bug fix documentation
./create-doc.ps1 bugfix "Null Reference Exception in User Service"

# Create feature launch documentation
./create-doc.ps1 feature "User Authentication System"
```

**Parameters:**

- `Type`: Documentation type (implementation, bugfix, feature)
- `Title`: Title of the work being documented
- `PR`: Pull request number (optional)
- `Date`: Date of completion (defaults to today)

**Output:**

- Implementation docs: `docs/history/implementations/YYYY-MM-DD-title-implementation.md`
- Bug fix docs: `docs/history/bug-fixes/YYYY-MM-DD-title-bugfix.md`
- Feature docs: `docs/history/features/YYYY-MM-DD-title-feature.md`

## Validation Scripts

### `validate-documentation.ps1`

Validates that documentation files follow the required structure and contain all necessary sections.

**Usage:**

```powershell
./validate-documentation.ps1 "docs/history/implementations/0001-2026-03-01-example-implementation.md"
```

**Features:**

- Checks required sections by document type
- Validates filename format and sequential numbering
- Identifies unfilled placeholders
- Ensures document ID matches filename

### `validate-commit-message.ps1`

Validates commit message format according to our standards.

**Usage:**

```powershell
./validate-commit-message.ps1 "feat(app): add user authentication system"
```

**Validates:**

- Proper format: `<type>(<scope>): <subject>`
- Allowed types: feat, fix, docs, style, refactor, perf, test, build, ci, chore, revert
- Allowed scopes: chain, app, story-generator, infra, workspace, deps
- Subject length (max 50 characters)
- Lowercase subject start

### `validate-commit-message.sh`

Bash version of commit message validation (for Unix/Linux environments).

**Usage:**

```bash
./validate-commit-message.sh "feat(app): add user authentication system"
```

### `pre-commit-check.sh`

Runs pre-commit validation including documentation requirement assessment.

**Usage:**

```bash
# Run as standalone check
./pre-commit-check.sh

# Run with commit message validation
./pre-commit-check.sh validate-commit
```

**Features:**

- Analyzes staged files for documentation requirements
- Identifies high-impact changes
- Validates commit message format
- Provides guidance for documentation creation

## Migration Parity Manifest Capture

### `Get-MonorepoMigrationManifests.ps1`

Captures GitHub API manifests for legacy PhoenixVC repositories and the mapped
`Mystira.workspace` targets to support migration parity audits.

**Reads token from:**

- `.env.local` key `PHOENIXVC_GITHUB_PAT` (preferred)
- `.env.local` key `GITHUB_TOKEN` (fallback)

**Output:**

- `docs/analysis/evidence/manifest-captures/*.json`
- Archived parity manifests should be retrieved from git history when needed.

**Usage (PowerShell):**

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Get-MonorepoMigrationManifests.ps1
```

**Optional parameters:**

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\Get-MonorepoMigrationManifests.ps1 `
  -Organization "phoenixvc" `
  -Monorepo "Mystira.workspace" `
  -MonorepoRef "dev"
```

## Repository Metadata Sync

### `sync-repo-metadata.sh`

Synchronizes repository metadata (descriptions, topics, homepage) across all Mystira repositories using the GitHub API.

**Features:**

- Updates repository descriptions
- Manages repository topics/labels
- Sets homepage URLs
- Supports dry-run mode for testing
- Handles archived repositories

**Prerequisites:**

```bash
# Install GitHub CLI
# macOS
brew install gh

# Ubuntu/Debian
sudo apt install gh

# Login to GitHub
gh auth login

# Install jq for JSON parsing
# macOS
brew install jq

# Ubuntu/Debian
sudo apt install jq
```

**Usage:**

Dry run (preview changes without applying):

```bash
./scripts/sync-repo-metadata.sh --dry-run
```

Apply changes:

```bash
./scripts/sync-repo-metadata.sh
```

**Configuration:**

Edit `scripts/repo-metadata.json` to update repository metadata:

```json
{
  "organization": "phoenixvc",
  "repositories": {
    "Mystira.App": {
      "description": "Your repository description",
      "topics": ["mystira", "dotnet", "csharp"],
      "homepage": "https://mystira.app"
    }
  }
}
```

**Supported Fields:**

- `description` - Repository description (required)
- `topics` - Array of topic labels (optional)
- `homepage` - Homepage URL (optional)
- `archived` - Mark repository as archived (optional, default: false)

**Examples:**

1. Update all repositories:

   ```bash
   ./scripts/sync-repo-metadata.sh
   ```

2. Preview changes:

   ```bash
   ./scripts/sync-repo-metadata.sh --dry-run
   ```

3. Update topics for a repository:
   ```json
   {
     "Mystira.Chain": {
       "description": "Blockchain integration service",
       "topics": ["mystira", "python", "blockchain", "web3"]
     }
   }
   ```

**Troubleshooting:**

- **Authentication errors**: Run `gh auth login` and ensure you have proper permissions
- **Repository not found**: Check that the repository name matches exactly
- **Topics not updating**: Requires admin or maintain access to the repository
- **jq not found**: Install jq using your package manager

**Notes:**

- The script skips repositories that don't exist or aren't accessible
- Archived repositories can still have their metadata updated
- Changes are applied immediately (no undo, use dry-run first!)
