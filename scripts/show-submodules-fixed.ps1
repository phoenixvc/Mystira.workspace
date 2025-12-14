#!/usr/bin/env pwsh
# Script to display Git submodules in a readable format
# Usage: ./scripts/show-submodules.ps1 [submodule-path]

param(
    [string]$SubmodulePath = "."
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-SubmoduleInfo {
    param(
        [string]$RepoPath,
        [string]$SubmodulePath
    )

    Push-Location $RepoPath
    
    try {
        # Get the commit hash that the parent repo is pointing to
        $submoduleCommit = git ls-tree HEAD -- "$SubmodulePath" | ForEach-Object {
            ($_ -split '\s+')[2]
        }

        if (-not $submoduleCommit) {
            return $null
        }

        Push-Location $SubmodulePath
        try {
            # Get commit info
            $commitInfo = git log -1 --pretty=format:"%h|%s|%cr|%an" $submoduleCommit 2>$null
            if (-not $commitInfo) {
                $commitInfo = "$submoduleCommit|(commit not found in submodule)|unknown|unknown"
            }
            
            $commitParts = $commitInfo -split '\|'
            $shortHash = $commitParts[0]
            $subject = $commitParts[1]
            $relativeDate = $commitParts[2]
            $author = $commitParts[3]
            
            # Get current branch or detached HEAD info
            $currentBranch = git rev-parse --abbrev-ref HEAD 2>$null
            if ($currentBranch -eq "HEAD") {
                $currentBranch = "detached"
            }
            
            # Check if submodule is modified
            $status = git status --porcelain 2>$null
            $isModified = $status -and $status.Count -gt 0
            
            # Check if submodule commit matches current HEAD
            $currentHead = git rev-parse HEAD 2>$null
            $isUpToDate = $submoduleCommit -eq $currentHead
            
            # Check if submodule is behind/ahead of remote
            $behind = 0
            $ahead = 0
            try {
                $upstreamRef = '@{u}'
                $remoteBranch = git rev-parse --abbrev-ref --symbolic-full-name $upstreamRef 2>$null 3>$null
                if ($remoteBranch -and $remoteBranch -ne 'HEAD' -and $LASTEXITCODE -eq 0) {
                    git fetch --quiet 2>$null 3>$null
                    if ($LASTEXITCODE -eq 0) {
                        $behindStr = git rev-list --count "HEAD..$upstreamRef" 2>$null 3>$null
                        $aheadStr = git rev-list --count "$upstreamRef..HEAD" 2>$null 3>$null
                        if ($behindStr -and $LASTEXITCODE -eq 0) { $behind = [int]$behindStr }
                        if ($aheadStr -and $LASTEXITCODE -eq 0) { $ahead = [int]$aheadStr }
                    }
                }
            }
            catch {
                # No upstream configured, that's okay
            }
            
            return @{
                Path = $SubmodulePath
                Commit = $submoduleCommit
                ShortHash = $shortHash
                Subject = $subject
                RelativeDate = $relativeDate
                Author = $author
                Branch = $currentBranch
                IsModified = $isModified
                IsUpToDate = $isUpToDate
                Behind = $behind
                Ahead = $ahead
            }
        }
        finally {
            Pop-Location
        }
    }
    finally {
        Pop-Location
    }
}

# Main execution
Push-Location $SubmodulePath

try {
    # Get all submodules
    $submodules = git config --file .gitmodules --get-regexp path | ForEach-Object {
        ($_ -replace '^submodule\..+\.path ', '')
    }

    if (-not $submodules) {
        Write-Host "No submodules found." -ForegroundColor Yellow
        exit 0
    }

    Write-Host "`nSubmodule Status" -ForegroundColor Cyan
    Write-Host ("=" * 100) -ForegroundColor Cyan
    Write-Host ""

    $allInfo = @()
    foreach ($submodule in $submodules) {
        $info = Get-SubmoduleInfo -RepoPath $PWD -SubmodulePath $submodule
        if ($info) {
            $allInfo += $info
        }
    }

    # Display formatted output
    foreach ($info in $allInfo) {
        Write-Host " $($info.Path)" -ForegroundColor Green
        Write-Host "   Commit:    " -NoNewline
        Write-Host "$($info.ShortHash)" -ForegroundColor Yellow -NoNewline
        Write-Host " ($($info.Commit.Substring(0, 12)))"
        Write-Host "   Message:   $($info.Subject)"
        Write-Host "   Date:      $($info.RelativeDate) by $($info.Author)"
        Write-Host "   Branch:    " -NoNewline
        if ($info.Branch -ne "detached") {
            Write-Host "$($info.Branch)" -ForegroundColor Cyan
        } else {
            Write-Host "detached HEAD" -ForegroundColor Yellow
        }
        
        # Status indicators
        $statusParts = @()
        if ($info.IsModified) {
            $statusParts += "Modified"
        }
        if (-not $info.IsUpToDate) {
            $statusParts += "Not at referenced commit"
        }
        if ($info.Behind -gt 0) {
            $statusParts += "$($info.Behind) behind remote"
        }
        if ($info.Ahead -gt 0) {
            $statusParts += "$($info.Ahead) ahead of remote"
        }
        if ($statusParts.Count -eq 0) {
            $statusParts += "Up to date"
        }
        
        Write-Host "   Status:    " -NoNewline
        if ($statusParts.Count -eq 1 -and $statusParts[0] -eq "Up to date") {
            Write-Host "[OK] $($statusParts[0])" -ForegroundColor Green
        } else {
            Write-Host ($statusParts -join ", ") -ForegroundColor Yellow
        }
        
        Write-Host ""
    }

    Write-Host ("=" * 100) -ForegroundColor Cyan
    Write-Host ""
    
    # Summary
    $modifiedCount = ($allInfo | Where-Object { $_.IsModified }).Count
    $outOfDateCount = ($allInfo | Where-Object { -not $_.IsUpToDate }).Count
    $behindCount = ($allInfo | Where-Object { $_.Behind -gt 0 }).Count
    
    if ($modifiedCount -eq 0 -and $outOfDateCount -eq 0 -and $behindCount -eq 0) {
        Write-Host "[OK] All submodules are up to date" -ForegroundColor Green
    } else {
        Write-Host "Summary:" -ForegroundColor Yellow
        if ($modifiedCount -gt 0) {
            Write-Host "  - $modifiedCount submodule(s) have uncommitted changes" -ForegroundColor Yellow
        }
        if ($outOfDateCount -gt 0) {
            Write-Host "  - $outOfDateCount submodule(s) not at referenced commit" -ForegroundColor Yellow
        }
        if ($behindCount -gt 0) {
            Write-Host "  - $behindCount submodule(s) behind remote" -ForegroundColor Yellow
        }
    }
    Write-Host ""
}
finally {
    Pop-Location
}

