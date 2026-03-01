#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Validate commit message format according to our standards

.DESCRIPTION
    This script validates that commit messages follow the required format:
    <type>(<scope>): <subject>

.PARAMETER Message
    The commit message to validate

.EXAMPLE
    ./validate-commit-message.ps1 "feat(app): add user authentication system"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Message
)

# Define allowed types
$Types = @("feat", "fix", "docs", "style", "refactor", "perf", "test", "build", "ci", "chore", "revert")

# Define allowed scopes
$Scopes = @("chain", "app", "story-generator", "infra", "workspace", "deps")

# Check if commit message follows format
$Pattern = "^($($Types -join '|'))\(($($Scopes -join '|'))\): .+$"
if ($Message -notmatch $Pattern) {
    Write-Host "❌ Invalid commit message format" -ForegroundColor Red
    Write-Host ""
    Write-Host "Expected format: <type>(<scope>): <subject>" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Allowed types: $($Types -join ', ')" -ForegroundColor Cyan
    Write-Host "Allowed scopes: $($Scopes -join ', ')" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  feat(app): add user authentication system" -ForegroundColor White
    Write-Host "  fix(story-generator): resolve null reference in LLM service" -ForegroundColor White
    Write-Host "  docs(workspace): update PR documentation guide" -ForegroundColor White
    exit 1
}

# Extract subject
$Subject = ($Message -split ":", 2)[1].Trim()

# Check subject length (max 50 characters)
if ($Subject.Length -gt 50) {
    Write-Host "❌ Subject line too long (max 50 characters)" -ForegroundColor Red
    Write-Host "Current length: $($Subject.Length) characters" -ForegroundColor Yellow
    Write-Host "Subject: $Subject" -ForegroundColor White
    exit 1
}

# Check if subject starts with lowercase
if ($Subject -notmatch "^[a-z]") {
    Write-Host "❌ Subject should start with lowercase" -ForegroundColor Red
    Write-Host "Subject: $Subject" -ForegroundColor White
    exit 1
}

Write-Host "✅ Commit message validation passed" -ForegroundColor Green
exit 0
