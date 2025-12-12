#!/bin/bash

# Setup script for Mystira workspace submodules

set -e

echo "🚀 Setting up Mystira workspace submodules..."

# Check if .gitmodules exists
if [ ! -f .gitmodules ]; then
    echo "❌ .gitmodules file not found. Please ensure it exists."
    exit 1
fi

# Initialize and update submodules
echo "📦 Initializing git submodules..."
git submodule update --init --recursive

# Check each submodule
echo "🔍 Checking submodule status..."

submodules=("packages/chain" "packages/app" "packages/story-generator" "infra")

for submodule in "${submodules[@]}"; do
    if [ -d "$submodule" ] && [ -f "$submodule/.git" ]; then
        echo "✅ $submodule is initialized"
    else
        echo "⚠️  $submodule is not properly initialized"
    fi
done

echo ""
echo "✨ Submodule setup complete!"
echo ""
echo "Next steps:"
echo "  1. Run 'pnpm install' to install dependencies"
echo "  2. Run 'pnpm build' to build all packages"
echo "  3. See docs/SUBMODULES.md for more information"

