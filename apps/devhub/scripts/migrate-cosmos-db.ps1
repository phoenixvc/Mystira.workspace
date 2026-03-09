# Cosmos DB Migration Script (PowerShell)
# Migrates data from legacy prodwusappmystiracosmos to new environment-specific accounts
#
# Source: prodwusappmystiracosmos (prod-wus-rg-mystira)
# Destinations:
#   - Production: mys-prod-core-cosmos-san (mys-prod-core-rg-san)
#   - Development: mys-dev-core-cosmos-san (mys-dev-core-rg-san)

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "prod")]
    [string]$Environment,

    [Parameter(Mandatory = $false)]
    [ValidateSet("scenarios", "bundles", "media-metadata", "user-profiles", "game-sessions",
                 "accounts", "compass-trackings", "character-maps", "badge-configurations",
                 "blobs", "master-data", "all")]
    [string]$Type = "all",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$CliProject = Join-Path $ProjectRoot "Mystira.DevHub.CLI"
$SourceDatabase = "MystiraAppDb"
$DestDatabase = "MystiraAppDb"

function Write-Info { param([string]$Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Success { param([string]$Message) Write-Host "[SUCCESS] $Message" -ForegroundColor Green }
function Write-Warn { param([string]$Message) Write-Host "[WARNING] $Message" -ForegroundColor Yellow }
function Write-Err { param([string]$Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

function Test-Prerequisites {
    Write-Info "Checking prerequisites..."

    # Check Azure CLI
    try {
        $null = Get-Command az -ErrorAction Stop
    }
    catch {
        Write-Err "Azure CLI is not installed. Please install it from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    }

    # Check Azure login
    try {
        $null = az account show 2>$null
        if ($LASTEXITCODE -ne 0) { throw }
    }
    catch {
        Write-Err "Not logged in to Azure. Please run 'az login' first."
        exit 1
    }

    # Check .NET
    try {
        $null = Get-Command dotnet -ErrorAction Stop
    }
    catch {
        Write-Err ".NET SDK is not installed. Please install .NET 9.0 or later."
        exit 1
    }

    Write-Success "Prerequisites check passed"
}

function Get-ConnectionStrings {
    Write-Info "Retrieving connection strings from Azure..."

    # Source Cosmos DB (legacy)
    if (-not $env:SOURCE_COSMOS_CONNECTION) {
        Write-Info "Fetching source Cosmos DB connection string..."
        $script:SourceCosmosConnection = az cosmosdb keys list `
            --name prodwusappmystiracosmos `
            --resource-group prod-wus-rg-mystira `
            --type connection-strings `
            --query "connectionStrings[0].connectionString" `
            -o tsv 2>$null

        if (-not $script:SourceCosmosConnection) {
            Write-Err "Failed to retrieve source Cosmos DB connection string. Please set SOURCE_COSMOS_CONNECTION environment variable."
            exit 1
        }
    }
    else {
        $script:SourceCosmosConnection = $env:SOURCE_COSMOS_CONNECTION
    }

    # Destination Cosmos DB
    if (-not $env:DEST_COSMOS_CONNECTION) {
        $destAccount = ""
        $destRg = ""

        if ($Environment -eq "prod") {
            $destAccount = "mys-prod-core-cosmos-san"
            $destRg = "mys-prod-core-rg-san"
        }
        else {
            $destAccount = "mys-dev-core-cosmos-san"
            $destRg = "mys-dev-core-rg-san"
        }

        Write-Info "Fetching destination Cosmos DB connection string for $Environment..."
        $script:DestCosmosConnection = az cosmosdb keys list `
            --name $destAccount `
            --resource-group $destRg `
            --type connection-strings `
            --query "connectionStrings[0].connectionString" `
            -o tsv 2>$null

        if (-not $script:DestCosmosConnection) {
            Write-Err "Failed to retrieve destination Cosmos DB connection string for $Environment."
            Write-Err "Please set DEST_COSMOS_CONNECTION environment variable or ensure you have access to $destAccount in $destRg."
            exit 1
        }
    }
    else {
        $script:DestCosmosConnection = $env:DEST_COSMOS_CONNECTION
    }

    Write-Success "Connection strings retrieved successfully"
}

function Build-Cli {
    Write-Info "Building CLI project..."

    Push-Location $CliProject
    try {
        if ($VerbosePreference -eq "Continue") {
            dotnet build --configuration Release
        }
        else {
            dotnet build --configuration Release --verbosity quiet
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Build failed"
        }

        Write-Success "CLI project built successfully"
    }
    finally {
        Pop-Location
    }
}

function Invoke-Migration {
    Write-Info "Starting migration..."

    $dryRunFlag = if ($DryRun) { "true" } else { "false" }

    if ($DryRun) {
        Write-Warn "Running in DRY-RUN mode - no data will be migrated"
    }

    Write-Host ""
    Write-Host "Migration Configuration:" -ForegroundColor Cyan
    Write-Host "  Environment: $Environment"
    Write-Host "  Migration Type: $Type"
    Write-Host "  Source Database: $SourceDatabase"
    Write-Host "  Destination Database: $DestDatabase"
    Write-Host "  Dry Run: $dryRunFlag"
    Write-Host ""

    # Create the JSON command
    $jsonCommand = @"
{
    "command": "migration.run",
    "args": {
        "type": "$Type",
        "sourceCosmosConnection": "$($script:SourceCosmosConnection -replace '"', '\"')",
        "destCosmosConnection": "$($script:DestCosmosConnection -replace '"', '\"')",
        "sourceDatabaseName": "$SourceDatabase",
        "destDatabaseName": "$DestDatabase",
        "dryRun": $dryRunFlag,
        "maxRetries": 3,
        "useBulkOperations": true
    }
}
"@

    Push-Location $CliProject
    try {
        Write-Info "Executing migration command..."

        $result = $jsonCommand | dotnet run --configuration Release --no-build 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "Migration Results:" -ForegroundColor Green

            try {
                $resultObj = $result | ConvertFrom-Json
                $result | ConvertFrom-Json | ConvertTo-Json -Depth 10
            }
            catch {
                Write-Host $result
            }

            Write-Host ""

            if ($DryRun) {
                Write-Success "Dry-run completed successfully!"
            }
            else {
                Write-Success "Migration completed successfully!"
            }
        }
        else {
            Write-Err "Migration failed!"
            Write-Host $result
            exit 1
        }
    }
    finally {
        Pop-Location
    }
}

# Main execution
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Cosmos DB Migration Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Test-Prerequisites
Get-ConnectionStrings
Build-Cli
Invoke-Migration

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   Migration Process Complete" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
