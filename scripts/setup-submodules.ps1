# Setup script for Mystira workspace submodules (PowerShell)

Write-Host "🚀 Setting up Mystira workspace submodules..." -ForegroundColor Cyan

# Check if .gitmodules exists
if (-not (Test-Path .gitmodules)) {
    Write-Host "❌ .gitmodules file not found. Please ensure it exists." -ForegroundColor Red
    exit 1
}

# Initialize and update submodules
Write-Host "📦 Initializing git submodules..." -ForegroundColor Yellow
git submodule update --init --recursive

# Check each submodule
Write-Host "🔍 Checking submodule status..." -ForegroundColor Yellow

$submodules = @("packages/chain", "packages/app", "packages/story-generator", "infra")

foreach ($submodule in $submodules) {
    if ((Test-Path $submodule) -and (Test-Path "$submodule/.git")) {
        Write-Host "✅ $submodule is initialized" -ForegroundColor Green
    } else {
        Write-Host "⚠️  $submodule is not properly initialized" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "✨ Submodule setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run 'pnpm install' to install dependencies"
Write-Host "  2. Run 'pnpm build' to build all packages"
Write-Host "  3. See docs/SUBMODULES.md for more information"

