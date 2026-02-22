<#
.SYNOPSIS
    Creates Azure AI Foundry agents for Mystira Story Generator

.DESCRIPTION
    This script creates the four required agents (Writer, Judge, Refiner, RubricSummary)
    in Azure AI Foundry and outputs their IDs for configuration.

.PARAMETER Endpoint
    Azure AI Foundry endpoint URL (e.g., "https://your-project.azure.com/api/projects/your-project")

.PARAMETER ModelDeployment
    The model deployment name to use (e.g., "gpt-4", "gpt-4-turbo")

.EXAMPLE
    .\setup-agents.ps1 -Endpoint "https://mys-shared-ai-san.services.ai.azure.com/api/projects/mys-shared-ai-san-project" -ModelDeployment "gpt-4.1"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Endpoint,

    [Parameter(Mandatory=$true)]
    [string]$ModelDeployment
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Azure AI Foundry Agent Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet is available
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: .NET SDK not found. Please install .NET 8.0 or later." -ForegroundColor Red
    exit 1
}

Write-Host "Creating agents in Azure AI Foundry..." -ForegroundColor Yellow
Write-Host "Endpoint: $Endpoint" -ForegroundColor Gray
Write-Host "Model: $ModelDeployment" -ForegroundColor Gray
Write-Host ""

# Build and run the agent setup tool
$projectPath = Join-Path $PSScriptRoot ".." "src" "Mystira.StoryGenerator.AgentSetup"

if (!(Test-Path $projectPath)) {
    Write-Host "ERROR: Agent setup project not found at: $projectPath" -ForegroundColor Red
    Write-Host "This script requires the AgentSetup project to be created." -ForegroundColor Red
    exit 1
}

Write-Host "Building agent setup tool..." -ForegroundColor Yellow
dotnet build $projectPath --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to build agent setup tool" -ForegroundColor Red
    exit 1
}

Write-Host "Running agent setup..." -ForegroundColor Yellow
dotnet run --project $projectPath --no-build --configuration Release -- `
    --endpoint "$Endpoint" `
    --model "$ModelDeployment"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Agent setup failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Agent Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Copy the agent IDs above" -ForegroundColor White
Write-Host "2. Update src/Mystira.StoryGenerator.Api/appsettings.json" -ForegroundColor White
Write-Host "3. Replace the placeholder agent IDs with the actual IDs" -ForegroundColor White
Write-Host ""
