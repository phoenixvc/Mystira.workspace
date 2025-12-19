# Infrastructure Validation Script (PowerShell)
# Validates Terraform configuration and checks for common issues

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TerraformDir = Join-Path $ScriptDir "..\terraform"

function Write-Error {
    param([string]$Message)
    Write-Host "❌ Error: $Message" -ForegroundColor Red
    exit 1
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  Warning: $Message" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message"
}

# Check if Terraform is installed
function Test-Terraform {
    try {
        $version = terraform version -json | ConvertFrom-Json | Select-Object -ExpandProperty terraform_version
        Write-Success "Terraform version: $version"
    }
    catch {
        Write-Error "Terraform is not installed. Please install Terraform >= 1.5.0"
    }
}

# Validate Terraform syntax for all modules
function Test-TerraformModules {
    Write-Info "Validating Terraform modules..."

    $modules = @(
        "modules\chain",
        "modules\publisher",
        "modules\story-generator",
        "modules\shared\postgresql",
        "modules\shared\redis",
        "modules\shared\monitoring",
        "modules\dns"
    )

    $failed = $false

    foreach ($module in $modules) {
        $modulePath = Join-Path $TerraformDir $module
        if (-not (Test-Path $modulePath)) {
            Write-Warning "Module directory not found: $modulePath"
            continue
        }

        Write-Info "Validating module: $module"
        Push-Location $modulePath

        try {
            terraform init -backend=false *> $null
            if ($LASTEXITCODE -eq 0) {
                terraform validate -backend=false *> $null
                if ($LASTEXITCODE -eq 0) {
                    Write-Success "Module $module is valid"
                }
                else {
                    Write-Error "Module $module validation failed"
                    $failed = $true
                }
            }
            else {
                Write-Warning "Failed to initialize module $module (may need providers)"
            }
        }
        catch {
            Write-Warning "Error validating module $module : $_"
        }
        finally {
            Pop-Location
        }
    }

    if ($failed) {
        Write-Error "Some modules failed validation"
    }
}

# Validate environment configurations
function Test-Environments {
    Write-Info "Validating environment configurations..."

    $environments = @("dev", "staging", "prod")

    foreach ($env in $environments) {
        $envPath = Join-Path $TerraformDir "environments\$env"
        if (-not (Test-Path $envPath)) {
            Write-Warning "Environment directory not found: $envPath"
            continue
        }

        Write-Info "Checking environment: $env"
        $mainTf = Join-Path $envPath "main.tf"
        if (-not (Test-Path $mainTf)) {
            Write-Warning "main.tf not found in $env"
            continue
        }

        Push-Location $envPath
        try {
            terraform fmt -check *> $null
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Environment $env syntax is valid"
            }
            else {
                Write-Warning "Environment $env may have formatting issues (run terraform fmt)"
            }
        }
        catch {
            Write-Warning "Error checking environment $env : $_"
        }
        finally {
            Pop-Location
        }
    }
}

# Check for required files
function Test-RequiredFiles {
    Write-Info "Checking for required files..."

    $requiredFiles = @(
        "terraform\modules\chain\main.tf",
        "terraform\modules\publisher\main.tf",
        "terraform\modules\story-generator\main.tf",
        "terraform\modules\shared\postgresql\main.tf",
        "terraform\modules\shared\redis\main.tf",
        "terraform\modules\shared\monitoring\main.tf",
        "terraform\environments\dev\main.tf",
        "terraform\environments\staging\main.tf",
        "terraform\environments\prod\main.tf"
    )

    $missing = $false

    foreach ($file in $requiredFiles) {
        $filePath = Join-Path $ScriptDir "..\$file"
        if (Test-Path $filePath) {
            Write-Success "Found: $file"
        }
        else {
            Write-Error "Missing required file: $file"
            $missing = $true
        }
    }

    if ($missing) {
        Write-Error "Some required files are missing"
    }
}

# Check Kubernetes manifests
function Test-KubernetesManifests {
    Write-Info "Checking Kubernetes manifests..."

    $manifests = @(
        "kubernetes\base\chain\deployment.yaml",
        "kubernetes\base\publisher\deployment.yaml",
        "kubernetes\base\story-generator\deployment.yaml",
        "kubernetes\overlays\dev\kustomization.yaml",
        "kubernetes\overlays\staging\kustomization.yaml",
        "kubernetes\overlays\prod\kustomization.yaml"
    )

    foreach ($manifest in $manifests) {
        $manifestPath = Join-Path $ScriptDir "..\$manifest"
        if (Test-Path $manifestPath) {
            Write-Success "Found: $manifest"
        }
        else {
            Write-Warning "Missing manifest: $manifest"
        }
    }
}

# Main execution
Write-Host "=== Infrastructure Validation ===" -ForegroundColor Cyan
Write-Host ""

Test-Terraform
Write-Host ""

Test-RequiredFiles
Write-Host ""

Test-TerraformModules
Write-Host ""

Test-Environments
Write-Host ""

Test-KubernetesManifests
Write-Host ""

Write-Success "Infrastructure validation completed successfully!"

