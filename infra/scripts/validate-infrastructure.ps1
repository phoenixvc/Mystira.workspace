# Infrastructure Validation Script (PowerShell)
# Validates Terraform configuration and checks for common issues

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TerraformDir = Join-Path $ScriptDir "..\terraform"

$script:ValidationFailed = $false

function Write-Error {
    param([string]$Message)
    Write-Host "[ERR] $Message" -ForegroundColor Red
    $script:ValidationFailed = $true
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message"
}

# Check if Terraform is installed
function Test-Terraform {
    try {
        $version = terraform version -json | ConvertFrom-Json | Select-Object -ExpandProperty terraform_version
        Write-Success "Terraform version: $version"
    }
    catch {
        Write-Error "Terraform is not installed. Please install Terraform >= 1.5.0"
        exit 1
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
                terraform validate *> $null
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
    $products = @("mystira-app", "story-generator", "admin", "publisher", "chain")
    $failed = $false

    foreach ($env in $environments) {
        $targets = @()
        $targets += Join-Path $TerraformDir "shared-infra\environments\$env"
        foreach ($product in $products) {
            $targets += Join-Path $TerraformDir "products\$product\environments\$env"
        }

        foreach ($target in $targets) {
            if (-not (Test-Path $target)) {
                Write-Warning "Environment directory not found: $target"
                continue
            }

            $mainTf = Join-Path $target "main.tf"
            if (-not (Test-Path $mainTf)) {
                Write-Warning "main.tf not found in: $target"
                continue
            }

            Write-Info "Validating: $target"
            Push-Location $target
            try {
                $targetFailed = $false

                terraform fmt -check *> $null
                if ($LASTEXITCODE -ne 0) {
                    Write-Error "Validation failed (fmt): $target"
                    $failed = $true
                    $targetFailed = $true
                }

                terraform init -backend=false *> $null
                if ($LASTEXITCODE -ne 0) {
                    Write-Error "Validation failed (init): $target"
                    $failed = $true
                    $targetFailed = $true
                }

                terraform validate *> $null
                if ($LASTEXITCODE -ne 0) {
                    Write-Error "Validation failed (validate): $target"
                    $failed = $true
                    $targetFailed = $true
                }
                if (-not $targetFailed) {
                    Write-Success "Valid: $target"
                }
            }
            catch {
                Write-Warning "Error validating $target : $_"
                $failed = $true
                $script:ValidationFailed = $true
            }
            finally {
                Pop-Location
            }
        }
    }

    if ($failed) {
        Write-Error "Some environment targets failed validation"
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
        "terraform\shared-infra\environments\dev\main.tf",
        "terraform\shared-infra\environments\staging\main.tf",
        "terraform\shared-infra\environments\prod\main.tf",
        "terraform\products\mystira-app\environments\dev\main.tf",
        "terraform\products\mystira-app\environments\staging\main.tf",
        "terraform\products\mystira-app\environments\prod\main.tf",
        "terraform\products\story-generator\environments\dev\main.tf",
        "terraform\products\story-generator\environments\staging\main.tf",
        "terraform\products\story-generator\environments\prod\main.tf",
        "terraform\products\admin\environments\dev\main.tf",
        "terraform\products\admin\environments\staging\main.tf",
        "terraform\products\admin\environments\prod\main.tf",
        "terraform\products\publisher\environments\dev\main.tf",
        "terraform\products\publisher\environments\staging\main.tf",
        "terraform\products\publisher\environments\prod\main.tf",
        "terraform\products\chain\environments\dev\main.tf",
        "terraform\products\chain\environments\staging\main.tf",
        "terraform\products\chain\environments\prod\main.tf"
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

if ($script:ValidationFailed) {
    Write-Host "[ERR] Infrastructure validation failed." -ForegroundColor Red
    exit 1
}

Write-Success "Infrastructure validation completed successfully!"

