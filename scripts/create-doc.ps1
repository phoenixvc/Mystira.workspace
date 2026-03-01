#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Create documentation template for completed PRs or implementations with sequential numbering

.DESCRIPTION
    This script generates standardized documentation templates for different types of work:
    - Implementation: Major changes, ADR implementations, refactoring
    - Bugfix: Complex bug resolutions
    - Feature: New feature launches

    Each document gets a sequential number (XXXX-YYYY-MM-DD-title-type.md) for proper ordering.

.PARAMETER Type
    Type of documentation to create (implementation, bugfix, feature)

.PARAMETER Title
    Title of the work being documented

.PARAMETER PR
    Pull request number (optional)

.PARAMETER Date
    Date of completion (defaults to today)

.EXAMPLE
    ./create-doc.ps1 implementation "TreatWarningsAsErrors=true" 1234

.EXAMPLE
    ./create-doc.ps1 bugfix "Null Reference Exception in User Service"

.EXAMPLE
    ./create-doc.ps1 feature "User Authentication System"
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("implementation", "bugfix", "feature")]
    [string]$Type,

    [Parameter(Mandatory=$true)]
    [string]$Title,

    [Parameter(Mandatory=$false)]
    [string]$PR = "",

    [Parameter(Mandatory=$false)]
    [string]$Date = (Get-Date -Format "yyyy-MM-dd")
)

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir

# Determine output directory
$OutputDir = switch ($Type) {
    "implementation" { "$RepoRoot/docs/history/implementations" }
    "bugfix" { "$RepoRoot/docs/history/bug-fixes" }
    "feature" { "$RepoRoot/docs/history/features" }
    default { "$RepoRoot/docs/history" }
}

# Create directory if it doesn't exist
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Get or create index file
$IndexPath = "$RepoRoot/docs/history/.index.json"
$Index = @{}

if (Test-Path $IndexPath) {
    try {
        $Index = Get-Content $IndexPath | ConvertFrom-Json -AsHashtable
    } catch {
        Write-Warning "Could not read index file, creating new one"
        $Index = @{}
    }
}

# Initialize type counter if not exists
$TypeKey = "$($Type)Counter"
if (-not $Index.ContainsKey($TypeKey)) {
    $Index[$TypeKey] = 0
}

# Increment counter
$Index[$TypeKey]++
$SequentialNumber = "{0:D4}" -f $Index[$TypeKey]

# Save updated index
try {
    $Index | ConvertTo-Json -Depth 10 | Out-File -FilePath $IndexPath -Encoding UTF8
} catch {
    Write-Warning "Could not save index file: $($_.Exception.Message)"
}

# Generate filename (sanitize title)
$SanitizedTitle = $Title -replace '[^a-zA-Z0-9\s-]', '' -replace '\s+', '-'
$Filename = "$SequentialNumber-$Date-$SanitizedTitle-$Type.md"
$OutputPath = Join-Path $OutputDir $Filename

# Generate template based on type
$Template = switch ($Type) {
    "implementation" {
@"
# $Title Implementation - Historical Summary

**Completed**: $Date
**Duration**: [Time period]
**Status**: ✅ **SUCCESSFULLY COMPLETED**
**PR**: $PR - [PR Title]
**Document ID**: $SequentialNumber

## Overview
[Brief description of what was implemented and why]

## Implementation Summary

### Projects/Components Affected
- ✅ **[Component 1]** - [Description]
- ✅ **[Component 2]** - [Description]
- ...

### Key Changes Made
1. **[Change 1]** - [Description and impact]
2. **[Change 2]** - [Description and impact]
3. ...

### Issues Resolved
- **[Issue 1]**: [Description of problem and solution]
- **[Issue 2]**: [Description of problem and solution]

## Implementation Approach
[Description of the methodology and phases]

### Phase 1: [Phase Name]
[What was done in this phase]

### Phase 2: [Phase Name]
[What was done in this phase]

## Results
[Quantitative and qualitative results]

### Metrics
- **Build Status**: [Status]
- **Performance**: [Improvements]
- **Coverage**: [Changes]
- **Tests**: [Pass/fail status]

### Impact
[Description of the impact on the system/users]

## Lessons Learned

### Technical Insights
[Technical lessons and discoveries]

### Process Improvements
[Process improvements made during implementation]

### Best Practices Established
[Best practices that can be applied elsewhere]

## Future Considerations
[Maintenance needs, potential enhancements, follow-up work]

## Related Documentation
- **[ADR-XXXX]**: [Related architecture decision]
- **[Other docs]**: [Related documentation]

---

**Implementation Team**: [Team/Individual]
**Review Status**: [Status]
**Next Steps**: [What should happen next]
**Document Index**: $SequentialNumber
"@
    }
    "bugfix" {
@"
# $Title Resolution - Historical Summary

**Completed**: $Date
**Bug ID**: [Issue/Tracking Number]
**PR**: $PR
**Severity**: [Critical/High/Medium/Low]
**Document ID**: $SequentialNumber

## Problem Description
[What was the bug and its impact]

## Root Cause Analysis
[Why the bug occurred]

## Solution Implemented
[How the bug was fixed]

### Code Changes
- **File 1**: [Description of changes]
- **File 2**: [Description of changes]

### Testing
- **Unit Tests**: [Test coverage added]
- **Integration Tests**: [Test scenarios]
- **Manual Testing**: [Manual verification steps]

## Verification
[How the fix was validated]

### Before/After Comparison
[Metrics or behavior comparison]

### Regression Testing
[Steps taken to prevent regression]

## Impact Assessment
[Who was affected and how]

## Prevention Measures
[How to prevent similar bugs]

## Lessons Learned
[What we learned from this bug]

---

**Fix Author**: [Author]
**Reviewer**: [Reviewer]
**Status**: [Resolved/Monitoring]
**Document Index**: $SequentialNumber
"@
    }
    "feature" {
@"
# $Title Launch - Historical Summary

**Launched**: $Date
**PR**: $PR
**Feature Type**: [New Feature/Enhancement]
**Document ID**: $SequentialNumber

## Feature Overview
[Description of the new feature]

## User Problem Solved
[What user problem this addresses]

## Implementation Details

### Architecture
[High-level architecture description]

### Components
- **[Component 1]**: [Description]
- **[Component 2]**: [Description]

### API Changes
[New endpoints, parameters, etc.]

### Database Changes
[Schema changes, migrations, etc.]

## User Experience
[How users interact with the feature]

### UI Changes
[Interface changes]

### Documentation
[User-facing documentation created]

## Rollout Plan
[How the feature was rolled out]

### Phasing
- **Phase 1**: [Description]
- **Phase 2**: [Description]

### Monitoring
[What metrics are being tracked]

## Results
[Success metrics and user feedback]

### Usage Statistics
[Adoption and usage metrics]

### User Feedback
[Summary of user reactions]

## Future Enhancements
[Planned improvements or follow-up features]

## Related Work
[Related features or dependencies]

---

**Product Manager**: [PM]
**Tech Lead**: [Lead]
**Status**: [Live/Beta/Coming Soon]
**Document Index**: $SequentialNumber
"@
    }
}

# Write the template to file
$Template | Out-File -FilePath $OutputPath -Encoding UTF8

Write-Host "✅ Documentation template created: $OutputPath" -ForegroundColor Green
Write-Host "📝 Type: $Type" -ForegroundColor Cyan
Write-Host "📋 Title: $Title" -ForegroundColor Cyan
Write-Host "🔗 PR: $PR" -ForegroundColor Cyan
Write-Host "🔢 Document ID: $SequentialNumber" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open the file and fill in the details" -ForegroundColor White
Write-Host "2. Update the [bracketed] sections with actual content" -ForegroundColor White
Write-Host "3. Review and finalize the documentation" -ForegroundColor White
Write-Host "4. Reference in related ADRs or other documentation" -ForegroundColor White
Write-Host "5. Link the document in the PR description" -ForegroundColor White
