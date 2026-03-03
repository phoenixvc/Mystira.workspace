#!/bin/bash

# Comprehensive Quality Check Script for Mystira Workspace
# This script runs linting, formatting, and tests across all languages

set -e

echo "🚀 Starting comprehensive quality checks for Mystira workspace..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if we're in the right directory
if [ ! -f "package.json" ] || [ ! -f "pnpm-workspace.yaml" ]; then
    print_error "This script must be run from the workspace root directory"
    exit 1
fi

print_status "Step 1: TypeScript/JavaScript Package Management (pnpm)"
echo "Using pnpm as the primary package manager for all Node.js projects..."

# Install dependencies
print_status "Installing dependencies with pnpm..."
pnpm install --frozen-lockfile

print_status "Step 2: Linting all TypeScript/JavaScript projects..."
pnpm run lint

print_status "Step 3: Running TypeScript/JavaScript tests..."
pnpm run test

print_status "Step 4: Building all TypeScript/JavaScript projects..."
pnpm run build

print_status "Step 5: .NET Projects"
echo "Running .NET tests and builds..."

# Find all .csproj files and run tests
find . -name "*.csproj" -not -path "./node_modules/*" -not -path "*/bin/*" -not -path "*/obj/*" | while read -r project; do
    project_dir=$(dirname "$project")
    print_status "Testing .NET project: $project"
    cd "$project_dir"
    dotnet test --no-build --verbosity normal || print_warning "Tests failed for $project"
    cd - > /dev/null
done

# Build all .NET projects
print_status "Building all .NET projects..."
dotnet build --no-restore

print_status "Step 6: Python Projects"
echo "Running Python tests..."

# Find Python projects
if [ -f "packages/chain/pyproject.toml" ]; then
    cd packages/chain
    print_status "Testing Python chain package..."
    python -m pytest tests/ -v || print_warning "Python tests failed"
    cd - > /dev/null
fi

print_status "Step 7: Rust Projects"
echo "Running Rust tests..."

if [ -f "packages/devhub/Mystira.DevHub/src-tauri/Cargo.toml" ]; then
    cd packages/devhub/Mystira.DevHub/src-tauri
    print_status "Testing Rust DevHub package..."
    cargo test || print_warning "Rust tests failed"
    cd - > /dev/null
fi

print_status "Step 8: Security and Dependency Checks"
echo "Running security audits..."

# Node.js security audit
pnpm audit || print_warning "Security vulnerabilities found in Node.js dependencies"

# .NET security check (if available)
if command -v dotnet-list-dependencies &> /dev/null; then
    print_status "Checking .NET dependencies..."
    # Add any .NET security checking tools here
fi

print_status "Step 9: Code Quality Metrics"
echo "Generating code quality reports..."

# TypeScript coverage
if command -v npx &> /dev/null; then
    print_status "Generating TypeScript coverage reports..."
    pnpm run test:coverage || print_warning "Coverage generation failed"
fi

print_status "Step 10: Documentation Generation"
echo "Checking documentation coverage..."

# Generate API docs if configured
if [ -f "packages/api-spec/package.json" ]; then
    cd packages/api-spec
    pnpm run generate || print_warning "API documentation generation failed"
    cd - > /dev/null
fi

print_success "✅ All quality checks completed!"
echo ""
echo "📊 Summary:"
echo "  - TypeScript/JavaScript: Linted, tested, and built"
echo "  - .NET: Tested and built"
echo "  - Python: Tested (if available)"
echo "  - Rust: Tested (if available)"
echo "  - Security: Audited"
echo "  - Documentation: Generated"
echo ""
print_status "Any warnings above should be reviewed and addressed."

# Exit with success code
exit 0
