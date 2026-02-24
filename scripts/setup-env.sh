#!/usr/bin/env bash
set -euo pipefail

# Copy all .env.example files to .env for local development.
# Existing .env files are never overwritten.

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

created=0
skipped=0

while IFS= read -r example; do
  env_file="${example%.example}"
  if [ -f "$env_file" ]; then
    echo "  skip  $env_file (already exists)"
    skipped=$((skipped + 1))
  else
    cp "$example" "$env_file"
    echo "  create $env_file"
    created=$((created + 1))
  fi
done < <(find "$ROOT_DIR" -name ".env.example" -not -path "*/node_modules/*" | sort)

echo ""
echo "Done — $created created, $skipped skipped."
