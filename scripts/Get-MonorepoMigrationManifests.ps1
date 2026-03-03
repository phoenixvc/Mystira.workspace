[CmdletBinding()]
param(
    [string]$Organization = "phoenixvc",
    [string]$Monorepo = "Mystira.workspace",
    [string]$MonorepoRef = "dev",
    [string]$OutputDirectory = "docs/analysis/evidence/manifest-captures"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RepositoryRoot {
    return (Resolve-Path (Join-Path $PSScriptRoot ".."))
}

function Convert-ToArray {
    param($InputObject)

    if ($null -eq $InputObject) {
        return @()
    }

    if ($InputObject -is [array]) {
        return $InputObject
    }

    return @($InputObject)
}

function Save-WorkflowContentFiles {
    param(
        [string]$OutputRoot,
        [string]$RepoSlug,
        $WorkflowItems,
        [hashtable]$Headers
    )

    $items = Convert-ToArray -InputObject $WorkflowItems
    if ($items.Count -eq 0) {
        return
    }

    $baseDir = Join-Path $OutputRoot (Join-Path "workflow-files" $RepoSlug)
    if (-not (Test-Path -LiteralPath $baseDir)) {
        New-Item -ItemType Directory -Path $baseDir -Force | Out-Null
    }

    foreach ($item in $items) {
        if ($null -eq $item.download_url) {
            continue
        }

        try {
            $content = Invoke-RestMethod -Method Get -Uri $item.download_url -Headers $Headers
            $name = [System.IO.Path]::GetFileName($item.path)
            $target = Join-Path $baseDir $name
            Set-Content -LiteralPath $target -Value $content -Encoding UTF8
        }
        catch {
            $name = if ($item.path) { [System.IO.Path]::GetFileName($item.path) } else { "unknown" }
            $target = Join-Path $baseDir ($name + ".error.json")
            Save-ErrorSnapshot -Path $target -Message $_.Exception.Message
        }
    }
}

function Get-EnvFileValues {
    param([string]$FilePath)

    $values = @{}
    if (-not (Test-Path -LiteralPath $FilePath)) {
        return $values
    }

    foreach ($line in Get-Content -LiteralPath $FilePath) {
        $trimmed = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
        if ($trimmed.StartsWith("#")) { continue }
        if (-not $trimmed.Contains("=")) { continue }

        $pair = $trimmed -split "=", 2
        $key = $pair[0].Trim()
        $value = $pair[1].Trim().Trim('"').Trim("'")

        if (-not [string]::IsNullOrWhiteSpace($key)) {
            $values[$key] = $value
        }
    }

    return $values
}

function Import-LocalEnvironment {
    param([string]$EnvFilePath)

    $values = Get-EnvFileValues -FilePath $EnvFilePath
    foreach ($key in $values.Keys) {
        [Environment]::SetEnvironmentVariable($key, $values[$key], "Process")
    }
}

function Invoke-GitHubGet {
    param(
        [string]$Path,
        [hashtable]$Headers
    )

    $uri = "https://api.github.com$Path"
    return Invoke-RestMethod -Method Get -Uri $uri -Headers $Headers
}

function Get-RepoMetadata {
    param(
        [string]$Organization,
        [string]$Repository,
        [hashtable]$Headers
    )

    return Invoke-GitHubGet -Path "/repos/$Organization/$Repository" -Headers $Headers
}

function Resolve-RepoRef {
    param(
        [string]$Organization,
        [string]$Repository,
        [string]$PreferredRef,
        [hashtable]$Headers
    )

    $metadata = Get-RepoMetadata -Organization $Organization -Repository $Repository -Headers $Headers
    $resolvedRef = $PreferredRef
    if ([string]::IsNullOrWhiteSpace($resolvedRef)) {
        $resolvedRef = $metadata.default_branch
    }

    try {
        $ref = Invoke-GitHubGet -Path "/repos/$Organization/$Repository/git/ref/heads/$resolvedRef" -Headers $Headers
        return @{
            ref           = $resolvedRef
            sha           = $ref.object.sha
            defaultBranch = $metadata.default_branch
        }
    }
    catch {
        $fallbackRef = $metadata.default_branch
        $fallback = Invoke-GitHubGet -Path "/repos/$Organization/$Repository/git/ref/heads/$fallbackRef" -Headers $Headers
        return @{
            ref           = $fallbackRef
            sha           = $fallback.object.sha
            defaultBranch = $metadata.default_branch
        }
    }
}

function Get-RepoTreeRecursive {
    param(
        [string]$Organization,
        [string]$Repository,
        [string]$Sha,
        [hashtable]$Headers
    )

    return Invoke-GitHubGet -Path "/repos/$Organization/$Repository/git/trees/$Sha`?recursive=1" -Headers $Headers
}

function Save-Json {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Data
    )

    $dir = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }

    $json = $Data | ConvertTo-Json -Depth 100
    Set-Content -LiteralPath $Path -Value $json -Encoding UTF8
}

function Save-ErrorSnapshot {
    param(
        [string]$Path,
        [string]$Message
    )

    Save-Json -Path $Path -Data @{
        status        = "error"
        message       = $Message
        capturedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    }
}

$repoRoot = Get-RepositoryRoot
$envPath = Join-Path $repoRoot ".env.local"
Import-LocalEnvironment -EnvFilePath $envPath

$token = $env:PHOENIXVC_GITHUB_PAT
if ([string]::IsNullOrWhiteSpace($token)) {
    $token = $env:GITHUB_TOKEN
}

if ([string]::IsNullOrWhiteSpace($token)) {
    throw "Missing PAT. Add PHOENIXVC_GITHUB_PAT or GITHUB_TOKEN to .env.local."
}

$headers = @{
    Accept                 = "application/vnd.github+json"
    Authorization          = "Bearer $token"
    "User-Agent"           = "mystira-migration-parity-script"
    "X-GitHub-Api-Version" = "2022-11-28"
}

$legacyRepos = @(
    "Mystira.App",
    "Mystira.StoryGenerator",
    "Mystira.Publisher",
    "Mystira.Chain",
    "Mystira.Devhub",
    "Mystira.Admin.Api",
    "Mystira.Admin.UI",
    "Mystira.Infra"
)

$targetMap = @{
    "Mystira.App"            = "packages/app"
    "Mystira.StoryGenerator" = "packages/story-generator"
    "Mystira.Publisher"      = "packages/publisher"
    "Mystira.Chain"          = "packages/chain"
    "Mystira.Devhub"         = "packages/devhub"
    "Mystira.Admin.Api"      = "packages/admin-api"
    "Mystira.Admin.UI"       = "packages/admin-ui"
    "Mystira.Infra"          = "infra"
}

$resolvedOutputDir = Join-Path $repoRoot $OutputDirectory
if (-not (Test-Path -LiteralPath $resolvedOutputDir)) {
    New-Item -ItemType Directory -Path $resolvedOutputDir -Force | Out-Null
}

$monorepoResolvedRef = Resolve-RepoRef -Organization $Organization -Repository $Monorepo -PreferredRef $MonorepoRef -Headers $headers
$monorepoTree = Get-RepoTreeRecursive -Organization $Organization -Repository $Monorepo -Sha $monorepoResolvedRef.sha -Headers $headers
Save-Json -Path (Join-Path $resolvedOutputDir "mystira-workspace-tree-recursive.json") -Data @{
    repo          = $Monorepo
    ref           = $monorepoResolvedRef.ref
    sha           = $monorepoResolvedRef.sha
    defaultBranch = $monorepoResolvedRef.defaultBranch
    tree          = $monorepoTree.tree
}

foreach ($legacyRepo in $legacyRepos) {
    $safeName = $legacyRepo.ToLowerInvariant().Replace(".", "-")
    $resolvedLegacyRef = $null

    try {
        $resolvedLegacyRef = Resolve-RepoRef -Organization $Organization -Repository $legacyRepo -PreferredRef "dev" -Headers $headers
        Save-Json -Path (Join-Path $resolvedOutputDir "$safeName-legacy-ref.json") -Data $resolvedLegacyRef
    }
    catch {
        Save-ErrorSnapshot -Path (Join-Path $resolvedOutputDir "$safeName-legacy-ref.json") -Message $_.Exception.Message
    }

    if ($null -ne $resolvedLegacyRef) {
        try {
            $legacyTree = Get-RepoTreeRecursive -Organization $Organization -Repository $legacyRepo -Sha $resolvedLegacyRef.sha -Headers $headers
            Save-Json -Path (Join-Path $resolvedOutputDir "$safeName-legacy-tree-recursive.json") -Data @{
                repo          = $legacyRepo
                ref           = $resolvedLegacyRef.ref
                sha           = $resolvedLegacyRef.sha
                defaultBranch = $resolvedLegacyRef.defaultBranch
                tree          = $legacyTree.tree
            }
        }
        catch {
            Save-ErrorSnapshot -Path (Join-Path $resolvedOutputDir "$safeName-legacy-tree-recursive.json") -Message $_.Exception.Message
        }
    }

    try {
        $legacyRef = if ($null -ne $resolvedLegacyRef) { $resolvedLegacyRef.ref } else { "dev" }
        $legacyRoot = Invoke-GitHubGet -Path "/repos/$Organization/$legacyRepo/contents`?ref=$legacyRef" -Headers $headers
        Save-Json -Path (Join-Path $resolvedOutputDir "$safeName-legacy-root.json") -Data $legacyRoot
    }
    catch {
        Save-ErrorSnapshot -Path (Join-Path $resolvedOutputDir "$safeName-legacy-root.json") -Message $_.Exception.Message
    }

    $legacyWorkflows = $null
    try {
        $legacyRef = if ($null -ne $resolvedLegacyRef) { $resolvedLegacyRef.ref } else { "dev" }
        $legacyWorkflows = Invoke-GitHubGet -Path "/repos/$Organization/$legacyRepo/contents/.github/workflows`?ref=$legacyRef" -Headers $headers
        Save-Json -Path (Join-Path $resolvedOutputDir "$safeName-legacy-workflows.json") -Data $legacyWorkflows
    }
    catch {
        Save-ErrorSnapshot -Path (Join-Path $resolvedOutputDir "$safeName-legacy-workflows.json") -Message $_.Exception.Message
    }

    if ($null -ne $legacyWorkflows) {
        Save-WorkflowContentFiles -OutputRoot $resolvedOutputDir -RepoSlug "$safeName-legacy" -WorkflowItems $legacyWorkflows -Headers $headers
    }

    $targetPath = $targetMap[$legacyRepo]
    try {
        $monorepoTarget = Invoke-GitHubGet -Path "/repos/$Organization/$Monorepo/contents/$targetPath`?ref=$($monorepoResolvedRef.ref)" -Headers $headers
        Save-Json -Path (Join-Path $resolvedOutputDir "$safeName-monorepo-target.json") -Data $monorepoTarget
    }
    catch {
        Save-ErrorSnapshot -Path (Join-Path $resolvedOutputDir "$safeName-monorepo-target.json") -Message $_.Exception.Message
    }

    try {
        $targetPrefix = "$targetPath/"
        $monorepoTargetTree = @($monorepoTree.tree | Where-Object {
                $_.path -eq $targetPath -or $_.path.StartsWith($targetPrefix)
            })
        Save-Json -Path (Join-Path $resolvedOutputDir "$safeName-monorepo-target-tree-recursive.json") -Data @{
            repo       = $Monorepo
            ref        = $monorepoResolvedRef.ref
            sha        = $monorepoResolvedRef.sha
            targetPath = $targetPath
            tree       = $monorepoTargetTree
        }
    }
    catch {
        Save-ErrorSnapshot -Path (Join-Path $resolvedOutputDir "$safeName-monorepo-target-tree-recursive.json") -Message $_.Exception.Message
    }
}

try {
    $monorepoWorkflows = Invoke-GitHubGet -Path "/repos/$Organization/$Monorepo/contents/.github/workflows`?ref=$($monorepoResolvedRef.ref)" -Headers $headers
    Save-Json -Path (Join-Path $resolvedOutputDir "mystira-workspace-workflows.json") -Data $monorepoWorkflows
    Save-WorkflowContentFiles -OutputRoot $resolvedOutputDir -RepoSlug "mystira-workspace" -WorkflowItems $monorepoWorkflows -Headers $headers
}
catch {
    Save-ErrorSnapshot -Path (Join-Path $resolvedOutputDir "mystira-workspace-workflows.json") -Message $_.Exception.Message
}

Write-Host "Saved GitHub manifest snapshots to: $resolvedOutputDir"
