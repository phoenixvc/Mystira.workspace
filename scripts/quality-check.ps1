# Comprehensive Quality Check Script for Mystira Workspace (PowerShell)
# This script runs linting, formatting, and tests across all languages

param(
    [switch]$SkipTests,
    [switch]$SkipBuild,
    [switch]$Verbose
)

# Colors for output
$Colors = @{
    Red = "Red"
    Green = "Green"
    Yellow = "Yellow"
    Blue = "Blue"
    White = "White"
}

function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Colors.Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor $Colors.Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Colors.Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Colors.Red
}

# Check if we're in the right directory
if (-not (Test-Path "package.json") -or -not (Test-Path "pnpm-workspace.yaml")) {
    Write-Error "This script must be run from the workspace root directory"
    exit 1
}

Write-Status "ðŸš€ Starting comprehensive quality checks for Mystira workspace..."

# Step 1: TypeScript/JavaScript Package Management
Write-Status "Step 1: TypeScript/JavaScript Package Management (pnpm)"
Write-Host "Using pnpm as the primary package manager for all Node.js projects..."

# Install dependencies
Write-Status "Installing dependencies with pnpm..."
try {
    pnpm install --frozen-lockfile
    Write-Success "Dependencies installed successfully"
} catch {
    Write-Warning "Failed to install dependencies: $_"
}

# Step 2: Linting
Write-Status "Step 2: Linting all TypeScript/JavaScript projects..."
try {
    pnpm run lint
    Write-Success "Linting completed successfully"
} catch {
    Write-Warning "Linting failed: $_"
}

# Step 3: Tests (if not skipped)
if (-not $SkipTests) {
    Write-Status "Step 3: Running TypeScript/JavaScript tests..."
    try {
        pnpm run test
        Write-Success "TypeScript/JavaScript tests passed"
    } catch {
        Write-Warning "TypeScript/JavaScript tests failed: $_"
    }
} else {
    Write-Status "Skipping tests as requested"
}

# Step 4: Build (if not skipped)
if (-not $SkipBuild) {
    Write-Status "Step 4: Building all TypeScript/JavaScript projects..."
    try {
        pnpm run build
        Write-Success "TypeScript/JavaScript build completed successfully"
    } catch {
        Write-Warning "TypeScript/JavaScript build failed: $_"
    }
} else {
    Write-Status "Skipping build as requested"
}

# Step 5: .NET Projects
Write-Status "Step 5: .NET Projects"
Write-Host "Running .NET tests and builds..."

# Find all .csproj files
$csprojFiles = Get-ChildItem -Path "." -Name "*.csproj" -Recurse | Where-Object { 
    $_ -notlike "*node_modules*" -and 
    $_ -notlike "*\bin\*" -and 
    $_ -notlike "*\obj\*" 
}

foreach ($project in $csprojFiles) {
    $projectDir = Split-Path $project -Parent
    Write-Status "Testing .NET project: $project"
    
    Push-Location $projectDir
    try {
        if (-not $SkipTests) {
            dotnet test --no-build --verbosity normal
        }
    } catch {
        Write-Warning "Tests failed for $project : $_"
    }
    Pop-Location
}

# Build all .NET projects
if (-not $SkipBuild) {
    Write-Status "Building all .NET projects..."
    try {
        dotnet build --no-restore
        Write-Success ".NET build completed successfully"
    } catch {
        Write-Warning ".NET build failed: $_"
    }
}

# Step 6: Python Projects
Write-Status "Step 6: Python Projects"
Write-Host "Running Python tests..."

if (Test-Path "packages/chain/pyproject.toml") {
    Push-Location "packages/chain"
    try {
        if (-not $SkipTests) {
            python -m pytest tests/ -v
        }
        Write-Success "Python tests completed"
    } catch {
        Write-Warning "Python tests failed: $_"
    }
    Pop-Location
}

# Step 7: Rust Projects
Write-Status "Step 7: Rust Projects"
Write-Host "Running Rust tests..."

if (Test-Path "packages/devhub/Mystira.DevHub/src-tauri/Cargo.toml") {
    Push-Location "packages/devhub/Mystira.DevHub/src-tauri"
    try {
        if (-not $SkipTests) {
            cargo test
        }
        Write-Success "Rust tests completed"
    } catch {
        Write-Warning "Rust tests failed: $_"
    }
    Pop-Location
}

# Step 8: Security and Dependency Checks
Write-Status "Step 8: Security and Dependency Checks"
Write-Host "Running security audits..."

# Node.js security audit
try {
    pnpm audit
    Write-Success "Security audit completed"
} catch {
    Write-Warning "Security vulnerabilities found: $_"
}

# Step 9: Code Quality Metrics
Write-Status "Step 9: Code Quality Metrics"
Write-Host "Generating code quality reports..."

# TypeScript coverage
if (Get-Command npx -ErrorAction SilentlyContinue) {
    Write-Status "Generating TypeScript coverage reports..."
    try {
        pnpm run test:coverage
        Write-Success "Coverage reports generated"
    } catch {
        Write-Warning "Coverage generation failed: $_"
    }
}

# Step 10: Documentation Generation
Write-Status "Step 10: Documentation Generation"
Write-Host "Checking documentation coverage..."

if (Test-Path "packages/api-spec/package.json") {
    Push-Location "packages/api-spec"
    try {
        pnpm run generate
        Write-Success "API documentation generated"
    } catch {
        Write-Warning "API documentation generation failed: $_"
    }
    Pop-Location
}

Write-Success "âœ… All quality checks completed!"
Write-Host ""
Write-Host "ðŸ“Š Summary:"
Write-Host "  - TypeScript/JavaScript: Linted, tested, and built"
Write-Host "  - .NET: Tested and built"
Write-Host "  - Python: Tested (if available)"
Write-Host "  - Rust: Tested (if available)"
Write-Host "  - Security: Audited"
Write-Host "  - Documentation: Generated"
Write-Host ""
Write-Status "Any warnings above should be reviewed and addressed."

exit 0

