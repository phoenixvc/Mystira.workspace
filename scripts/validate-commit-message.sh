#!/bin/bash

# Validate commit message format according to our standards

commit_message="$1"

if [ -z "$commit_message" ]; then
    echo "Usage: $0 <commit_message>"
    exit 1
fi

# Define allowed types
types="feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert"

# Define allowed scopes
scopes="chain|app|story-generator|infra|workspace|deps"

# Check if commit message follows format
if [[ ! "$commit_message" =~ ^($types)\(($scopes)\): .+$ ]]; then
    echo "❌ Invalid commit message format"
    echo ""
    echo "Expected format: <type>(<scope>): <subject>"
    echo ""
    echo "Allowed types: $types"
    echo "Allowed scopes: $scopes"
    echo ""
    echo "Examples:"
    echo "  feat(app): add user authentication system"
    echo "  fix(story-generator): resolve null reference in LLM service"
    echo "  docs(workspace): update PR documentation guide"
    exit 1
fi

# Check subject length (max 50 characters)
subject=$(echo "$commit_message" | sed 's/^[^:]*: //')
if [ ${#subject} -gt 50 ]; then
    echo "❌ Subject line too long (max 50 characters)"
    echo "Current length: ${#subject} characters"
    echo "Subject: $subject"
    exit 1
fi

# Check if subject starts with lowercase
if [[ ! "$subject" =~ ^[a-z] ]]; then
    echo "❌ Subject should start with lowercase"
    echo "Subject: $subject"
    exit 1
fi

echo "✅ Commit message validation passed"
exit 0
