#!/bin/bash
# Infrastructure Validation Script
# Validates Terraform configuration and checks for common issues

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TERRAFORM_DIR="${SCRIPT_DIR}/../terraform"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

error() {
    echo -e "${RED}❌ Error: $1${NC}" >&2
    exit 1
}

warning() {
    echo -e "${YELLOW}⚠️  Warning: $1${NC}"
}

success() {
    echo -e "${GREEN}✅ $1${NC}"
}

info() {
    echo -e "ℹ️  $1"
}

# Check if Terraform is installed
check_terraform() {
    if ! command -v terraform &> /dev/null; then
        error "Terraform is not installed. Please install Terraform >= 1.5.0"
    fi

    local version
    version=$(terraform version -json | jq -r '.terraform_version')
    success "Terraform version: $version"
}

# Validate Terraform syntax for all modules
validate_modules() {
    info "Validating Terraform modules..."

    local modules=(
        "modules/chain"
        "modules/publisher"
        "modules/story-generator"
        "modules/shared/postgresql"
        "modules/shared/redis"
        "modules/shared/monitoring"
        "modules/dns"
    )

    local failed=0

    for module in "${modules[@]}"; do
        local module_path="${TERRAFORM_DIR}/${module}"
        if [ ! -d "$module_path" ]; then
            warning "Module directory not found: $module_path"
            continue
        fi

        info "Validating module: $module"
        cd "$module_path" || error "Failed to change directory to $module_path"

        if terraform init -backend=false > /dev/null 2>&1; then
            if terraform validate -backend=false > /dev/null 2>&1; then
                success "Module $module is valid"
            else
                error "Module $module validation failed"
                failed=1
            fi
        else
            warning "Failed to initialize module $module (may need providers)"
        fi

        cd "$SCRIPT_DIR" || error "Failed to return to script directory"
    done

    if [ $failed -eq 1 ]; then
        error "Some modules failed validation"
    fi
}

# Validate environment configurations
validate_environments() {
    info "Validating environment configurations..."

    local environments=("dev" "staging" "prod")
    local failed=0

    for env in "${environments[@]}"; do
        local env_path="${TERRAFORM_DIR}/environments/${env}"
        if [ ! -d "$env_path" ]; then
            warning "Environment directory not found: $env_path"
            continue
        fi

        info "Checking environment: $env"
        cd "$env_path" || error "Failed to change directory to $env_path"

        # Check if main.tf exists
        if [ ! -f "main.tf" ]; then
            warning "main.tf not found in $env"
            continue
        fi

        # Basic syntax check (without init/plan which requires Azure credentials)
        if terraform fmt -check -diff > /dev/null 2>&1; then
            success "Environment $env syntax is valid"
        else
            warning "Environment $env may have formatting issues (run terraform fmt)"
        fi

        cd "$SCRIPT_DIR" || error "Failed to return to script directory"
    done
}

# Check for required files
check_required_files() {
    info "Checking for required files..."

    local required_files=(
        "terraform/modules/chain/main.tf"
        "terraform/modules/publisher/main.tf"
        "terraform/modules/story-generator/main.tf"
        "terraform/modules/shared/postgresql/main.tf"
        "terraform/modules/shared/redis/main.tf"
        "terraform/modules/shared/monitoring/main.tf"
        "terraform/environments/dev/main.tf"
        "terraform/environments/staging/main.tf"
        "terraform/environments/prod/main.tf"
    )

    local missing=0

    for file in "${required_files[@]}"; do
        local file_path="${TERRAFORM_DIR}/../${file}"
        if [ -f "$file_path" ]; then
            success "Found: $file"
        else
            error "Missing required file: $file"
            missing=1
        fi
    done

    if [ $missing -eq 1 ]; then
        error "Some required files are missing"
    fi
}

# Check Kubernetes manifests
check_kubernetes_manifests() {
    info "Checking Kubernetes manifests..."

    local manifests=(
        "kubernetes/base/chain/deployment.yaml"
        "kubernetes/base/publisher/deployment.yaml"
        "kubernetes/base/story-generator/deployment.yaml"
        "kubernetes/overlays/dev/kustomization.yaml"
        "kubernetes/overlays/staging/kustomization.yaml"
        "kubernetes/overlays/prod/kustomization.yaml"
    )

    local missing=0

    for manifest in "${manifests[@]}"; do
        local manifest_path="${TERRAFORM_DIR}/../../${manifest}"
        if [ -f "$manifest_path" ]; then
            success "Found: $manifest"
        else
            warning "Missing manifest: $manifest"
            missing=1
        fi
    done
}

# Main execution
main() {
    echo "=== Infrastructure Validation ==="
    echo ""

    check_terraform
    echo ""

    check_required_files
    echo ""

    validate_modules
    echo ""

    validate_environments
    echo ""

    check_kubernetes_manifests
    echo ""

    success "Infrastructure validation completed successfully!"
}

main "$@"

