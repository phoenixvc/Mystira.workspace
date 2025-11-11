#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

dotnet restore Mystira.StoryGenerator.sln
dotnet build Mystira.StoryGenerator.sln
