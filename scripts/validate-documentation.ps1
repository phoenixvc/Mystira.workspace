#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Validate documentation files for proper structure and formatting

.DESCRIPTION
    This script validates that documentation files follow the required structure
    and contain all necessary sections for their type.

.PARAMETER FilePath
    Path to the documentation file to validate

.EXAMPLE
    ./validate-documentation.ps1 "docs/history/implementations/0001-2026-02-28-example-implementation.md"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath
)

if (-not (Test-Path $FilePath)) {
    Write-Error "Documentation file not found: $FilePath"
    exit 1
}

$Content = Get-Content $FilePath -Raw
$FileName = Split-Path $Leaf -Path $FilePath

# Determine document type from filename
if ($FileName -match "implementation\.md$") {
    $DocType = "implementation"
} elseif ($FileName -match "bugfix\.md$") {
    $DocType = "bugfix"
} elseif ($FileName -match "feature\.md$") {
    $DocType = "feature"
} else {
    Write-Error "Unknown document type. Expected filename format: XXXX-YYYY-MM-DD-title-type.md"
    exit 1
}

# Validate filename format
if ($FileName -notmatch "^\d{4}-\d{4}-\d{2}-[^-]+-(implementation|bugfix|feature)\.md$") {
    Write-Error "Invalid filename format. Expected: XXXX-YYYY-MM-DD-title-type.md"
    exit 1
}

# Define required sections for each type
$RequiredSections = switch ($DocType) {
    "implementation" {
        @(
            "^# .+ Implementation - Historical Summary",
            "^## Overview",
            "^## Implementation Summary",
            "^### Projects/Components Affected",
            "^## Results",
            "^## Lessons Learned",
            "^## Future Considerations"
        )
    }
    "bugfix" {
        @(
            "^# .+ Resolution - Historical Summary",
            "^## Problem Description",
            "^## Root Cause Analysis",
            "^## Solution Implemented",
            "^## Verification",
            "^## Impact Assessment",
            "^## Lessons Learned"
        )
    }
    "feature" {
        @(
            "^# .+ Launch - Historical Summary",
            "^## Feature Overview",
            "^## User Problem Solved",
            "^## Implementation Details",
            "^## User Experience",
            "^## Results",
            "^## Future Enhancements"
        )
    }
}

# Check for required sections
$MissingSections = @()
foreach ($Section in $RequiredSections) {
    if ($Content -notmatch $Section) {
        $MissingSections += $Section
    }
}

if ($MissingSections.Count -gt 0) {
    Write-Error "Missing required sections:"
    foreach ($Section in $MissingSections) {
        Write-Error "  - $Section"
    }
    exit 1
}

# Check for unfilled placeholders
$Placeholders = @(
    "\[.*?\]",
    "\{.*?\}",
    "TODO",
    "FIXME"
)

$UnfilledPlaceholders = @()
foreach ($Placeholder in $Placeholders) {
    $Matches = [regex]::Matches($Content, $Placeholder)
    if ($Matches.Count -gt 0) {
        $UnfilledPlaceholders += $Matches | ForEach-Object { $_.Value }
    }
}

if ($UnfilledPlaceholders.Count -gt 0) {
    Write-Warning "Found unfilled placeholders:"
    foreach ($Placeholder in $UnfilledPlaceholders) {
        Write-Warning "  - $Placeholder"
    }
}

# Validate document ID matches filename
if ($Content -match "\*\*Document ID\*\*: (\d{4})") {
    $DocId = $Matches[1]
    if ($FileName -notmatch "^$DocId-") {
        Write-Error "Document ID ($DocId) does not match filename prefix"
        exit 1
    }
} else {
    Write-Warning "Document ID not found in content"
}

# Validate date matches filename
if ($Content -match "\*\*Completed\*\*: (\d{4}-\d{2}-\d{2})") {
    $DocDate = $Matches[1]
    if ($FileName -notmatch "\d{4}-$DocDate-") {
        Write-Error "Document date ($DocDate) does not match filename"
        exit 1
    }
} else {
    Write-Warning "Completion date not found in content"
}

Write-Host "✅ Documentation validation passed: $FilePath" -ForegroundColor Green
Write-Host "📝 Type: $DocType" -ForegroundColor Cyan
Write-Host "📋 Required sections: All present" -ForegroundColor Green

if ($UnfilledPlaceholders.Count -gt 0) {
    Write-Host "⚠️  Unfilled placeholders: $($UnfilledPlaceholders.Count)" -ForegroundColor Yellow
}
