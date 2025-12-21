# Mystira Scripts

Utility scripts for managing the Mystira workspace and repositories.

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
