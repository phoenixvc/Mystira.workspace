#!/bin/bash

# Pre-commit validation script

echo "🔍 Running pre-commit checks..."

# Check for documentation requirement based on staged files
staged_files=$(git diff --cached --name-only)
docs_required=false
high_impact=false

for file in $staged_files; do
    # Check for high impact changes
    if [[ $file == *.csproj ]] || [[ $file == package.json ]] || [[ $file == Cargo.toml ]]; then
        high_impact=true
        docs_required=true
        break
    elif [[ $file == src/**/*.cs ]] || [[ $file == src/**/*.ts ]] || [[ $file == src/**/*.js ]]; then
        docs_required=true
    fi
done

if [ "$docs_required" = true ]; then
    echo "📋 Documentation may be required for this change"
    echo "   Use: ./scripts/create-doc.ps1 <type> <title> <pr>"
    
    if [ "$high_impact" = true ]; then
        echo "⚠️  High impact change detected - documentation is required"
    fi
fi

# Validate commit message if this is a commit (not just pre-commit hook)
if [ -n "$1" ] && [ "$1" = "validate-commit" ]; then
    commit_msg=$(git log -1 --pretty=%B)
    echo "📝 Validating commit message..."
    ./scripts/validate-commit-message.sh "$commit_msg"
fi

echo "✅ Pre-commit checks completed"
