#!/bin/bash
set -e

# ============================================
# Sync Repository Metadata
# ============================================
# Updates GitHub repository descriptions, topics, and settings
# from the repo-metadata.json configuration file.
#
# Usage:
#   ./scripts/sync-repo-metadata.sh [--dry-run]
#
# Requirements:
#   - GitHub CLI (gh) must be installed and authenticated
#   - jq must be installed for JSON parsing
#
# Environment Variables:
#   GITHUB_TOKEN - GitHub personal access token (optional, uses gh auth)
# ============================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/repo-metadata.json"
DRY_RUN=false

# Parse arguments
if [[ "$1" == "--dry-run" ]]; then
  DRY_RUN=true
  echo "🔍 DRY RUN MODE - No changes will be made"
  echo ""
fi

# Check dependencies
if ! command -v gh &> /dev/null; then
  echo "❌ Error: GitHub CLI (gh) is not installed"
  echo "Install it from: https://cli.github.com/"
  exit 1
fi

if ! command -v jq &> /dev/null; then
  echo "❌ Error: jq is not installed"
  echo "Install it with: sudo apt-get install jq (Ubuntu) or brew install jq (macOS)"
  exit 1
fi

# Check if authenticated
if ! gh auth status &> /dev/null; then
  echo "❌ Error: Not authenticated with GitHub CLI"
  echo "Run: gh auth login"
  exit 1
fi

# Check if config file exists
if [[ ! -f "$CONFIG_FILE" ]]; then
  echo "❌ Error: Configuration file not found: $CONFIG_FILE"
  exit 1
fi

echo "📋 Reading configuration from: $CONFIG_FILE"
echo ""

# Read organization from config
ORG=$(jq -r '.organization' "$CONFIG_FILE")
echo "🏢 Organization: $ORG"
echo ""

# Get list of repositories from config
REPOS=$(jq -r '.repositories | keys[]' "$CONFIG_FILE")

# Track statistics
TOTAL=0
SUCCESS=0
FAILED=0
SKIPPED=0

# Process each repository
for REPO in $REPOS; do
  ((TOTAL++))
  FULL_NAME="$ORG/$REPO"

  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "📦 Processing: $FULL_NAME"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

  # Extract metadata from config
  DESCRIPTION=$(jq -r ".repositories.\"$REPO\".description" "$CONFIG_FILE")
  HOMEPAGE=$(jq -r ".repositories.\"$REPO\".homepage // empty" "$CONFIG_FILE")
  ARCHIVED=$(jq -r ".repositories.\"$REPO\".archived // false" "$CONFIG_FILE")
  TOPICS=$(jq -r ".repositories.\"$REPO\".topics[]" "$CONFIG_FILE" | tr '\n' ',' | sed 's/,$//')

  # Check if repository exists
  if ! gh repo view "$FULL_NAME" &> /dev/null; then
    echo "⚠️  Repository does not exist or is not accessible: $FULL_NAME"
    ((SKIPPED++))
    echo ""
    continue
  fi

  # Display planned changes
  echo "Description: $DESCRIPTION"
  if [[ -n "$HOMEPAGE" ]]; then
    echo "Homepage: $HOMEPAGE"
  fi
  if [[ "$ARCHIVED" == "true" ]]; then
    echo "⚠️  Archived: true"
  fi
  if [[ -n "$TOPICS" ]]; then
    echo "Topics: $TOPICS"
  fi
  echo ""

  if [[ "$DRY_RUN" == "true" ]]; then
    echo "🔍 [DRY RUN] Would update repository metadata"
    ((SUCCESS++))
  else
    # Update repository metadata
    echo "📝 Updating repository..."

    # Build gh repo edit command
    CMD="gh repo edit $FULL_NAME --description \"$DESCRIPTION\""

    if [[ -n "$HOMEPAGE" ]]; then
      CMD="$CMD --homepage \"$HOMEPAGE\""
    fi

    # Execute repository update
    if eval "$CMD" 2>/dev/null; then
      echo "✅ Updated description and homepage"
    else
      echo "⚠️  Failed to update repository settings"
      ((FAILED++))
      echo ""
      continue
    fi

    # Update topics (requires separate API call)
    if [[ -n "$TOPICS" ]]; then
      # Convert comma-separated topics to space-separated for gh
      TOPICS_ARRAY=$(echo "$TOPICS" | tr ',' ' ')

      # GitHub CLI doesn't have a direct command for topics, use API
      if gh api "repos/$FULL_NAME/topics" -X PUT -f names="[\"$(echo $TOPICS | sed 's/,/","/g')\"]" &> /dev/null; then
        echo "✅ Updated topics"
      else
        echo "⚠️  Failed to update topics (may require admin access)"
      fi
    fi

    ((SUCCESS++))
    echo "✅ Repository updated successfully"
  fi

  echo ""
done

# Print summary
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📊 Summary"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Total repositories: $TOTAL"
echo "✅ Successfully processed: $SUCCESS"
if [[ $FAILED -gt 0 ]]; then
  echo "❌ Failed: $FAILED"
fi
if [[ $SKIPPED -gt 0 ]]; then
  echo "⚠️  Skipped: $SKIPPED"
fi
echo ""

if [[ "$DRY_RUN" == "true" ]]; then
  echo "🔍 DRY RUN COMPLETE - No changes were made"
  echo "Run without --dry-run to apply changes"
else
  echo "✅ Metadata sync complete!"
fi

exit 0
