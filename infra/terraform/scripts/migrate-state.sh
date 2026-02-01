#!/bin/bash
# =============================================================================
# Terraform State Migration Script
# Migrates from monolithic to product-based state structure
# =============================================================================

set -euo pipefail

# Resolve script directory (handles symlinks correctly)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TERRAFORM_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

# Configuration
ENVIRONMENT="${1:-dev}"
STORAGE_ACCOUNT="${STORAGE_ACCOUNT:-mysterraformstate}"
CONTAINER="${CONTAINER:-tfstate}"
RESOURCE_GROUP="${RESOURCE_GROUP:-mys-terraform-state}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# =============================================================================
# Wave 1: Backup Current State
# =============================================================================
backup_state() {
    log_info "=== Wave 1: Backing up current state ==="

    BACKUP_DIR="${TERRAFORM_DIR}/backups/$(date +%Y%m%d_%H%M%S)"
    mkdir -p "$BACKUP_DIR"

    local env_dir="${TERRAFORM_DIR}/environments/${ENVIRONMENT}"
    if [[ ! -d "$env_dir" ]]; then
        log_error "Environment directory not found: $env_dir"
        exit 1
    fi

    log_info "Pulling current state..."
    (cd "$env_dir" && terraform state pull > "${BACKUP_DIR}/${ENVIRONMENT}_backup.tfstate")

    log_info "State backed up to ${BACKUP_DIR}/${ENVIRONMENT}_backup.tfstate"

    echo "$BACKUP_DIR" > "${TERRAFORM_DIR}/.last_backup_dir"
}

# =============================================================================
# Wave 2: Create New State Files in Azure
# =============================================================================
create_state_containers() {
    log_info "=== Wave 2: Creating state file structure in Azure ==="

    # Products to migrate
    PRODUCTS=("shared-infra" "mystira-app" "story-generator" "admin" "publisher" "chain")

    for product in "${PRODUCTS[@]}"; do
        log_info "Creating state path: ${product}/${ENVIRONMENT}.tfstate"

        # Azure Blob Storage doesn't have directories, but we can create empty blobs
        # The actual state files will be created when terraform init runs
    done

    log_info "State paths prepared"
}

# =============================================================================
# Wave 3: Extract Resource Lists for Each Product
# =============================================================================
list_resources_by_product() {
    log_info "=== Wave 3: Analyzing resources by product ==="

    local env_dir="${TERRAFORM_DIR}/environments/${ENVIRONMENT}"
    local state_file="${TERRAFORM_DIR}/.state_resources.txt"

    log_info "Getting current state resources..."
    (cd "$env_dir" && terraform state list > "$state_file")

    # Categorize resources
    log_info "Categorizing resources..."

    # Shared Infrastructure
    grep -E '^module\.(shared_|identity)' "$state_file" > "${TERRAFORM_DIR}/.shared_infra_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_resource_group\.(main|shared)' "$state_file" >> "${TERRAFORM_DIR}/.shared_infra_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_virtual_network\.' "$state_file" >> "${TERRAFORM_DIR}/.shared_infra_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_subnet\.' "$state_file" >> "${TERRAFORM_DIR}/.shared_infra_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_kubernetes_cluster\.' "$state_file" >> "${TERRAFORM_DIR}/.shared_infra_resources.txt" 2>/dev/null || true

    # Story Generator
    grep -E '^module\.story_generator' "$state_file" > "${TERRAFORM_DIR}/.story_generator_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_resource_group\.story' "$state_file" >> "${TERRAFORM_DIR}/.story_generator_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_key_vault_secret\.story' "$state_file" >> "${TERRAFORM_DIR}/.story_generator_resources.txt" 2>/dev/null || true

    # Mystira App
    grep -E '^module\.mystira_app' "$state_file" > "${TERRAFORM_DIR}/.mystira_app_resources.txt" 2>/dev/null || true
    grep -E '^module\.mystira[^_]' "$state_file" >> "${TERRAFORM_DIR}/.mystira_app_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_resource_group\.mystira' "$state_file" >> "${TERRAFORM_DIR}/.mystira_app_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_key_vault_secret\.mystira' "$state_file" >> "${TERRAFORM_DIR}/.mystira_app_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_static_site\.mystira' "$state_file" >> "${TERRAFORM_DIR}/.mystira_app_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_communication_service\.mystira' "$state_file" >> "${TERRAFORM_DIR}/.mystira_app_resources.txt" 2>/dev/null || true

    # Admin (API + UI)
    grep -E '^module\.admin_(api|ui)' "$state_file" > "${TERRAFORM_DIR}/.admin_resources.txt" 2>/dev/null || true
    grep -E '^module\.entra_id' "$state_file" >> "${TERRAFORM_DIR}/.admin_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_resource_group\.admin' "$state_file" >> "${TERRAFORM_DIR}/.admin_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_key_vault_secret\.admin' "$state_file" >> "${TERRAFORM_DIR}/.admin_resources.txt" 2>/dev/null || true

    # Publisher
    grep -E '^module\.publisher' "$state_file" > "${TERRAFORM_DIR}/.publisher_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_resource_group\.publisher' "$state_file" >> "${TERRAFORM_DIR}/.publisher_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_key_vault_secret\.publisher' "$state_file" >> "${TERRAFORM_DIR}/.publisher_resources.txt" 2>/dev/null || true

    # Chain
    grep -E '^module\.chain' "$state_file" > "${TERRAFORM_DIR}/.chain_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_resource_group\.chain' "$state_file" >> "${TERRAFORM_DIR}/.chain_resources.txt" 2>/dev/null || true
    grep -E '^azurerm_key_vault_secret\.chain' "$state_file" >> "${TERRAFORM_DIR}/.chain_resources.txt" 2>/dev/null || true

    # GitHub OIDC (stays with shared-infra as it's cross-cutting)
    grep -E '^module\.github_oidc' "$state_file" >> "${TERRAFORM_DIR}/.shared_infra_resources.txt" 2>/dev/null || true

    log_info "Resource categorization complete:"
    log_info "  Shared Infra: $(wc -l < "${TERRAFORM_DIR}/.shared_infra_resources.txt") resources"
    log_info "  Mystira App: $(wc -l < "${TERRAFORM_DIR}/.mystira_app_resources.txt") resources"
    log_info "  Story Generator: $(wc -l < "${TERRAFORM_DIR}/.story_generator_resources.txt") resources"
    log_info "  Admin: $(wc -l < "${TERRAFORM_DIR}/.admin_resources.txt") resources"
    log_info "  Publisher: $(wc -l < "${TERRAFORM_DIR}/.publisher_resources.txt") resources"
    log_info "  Chain: $(wc -l < "${TERRAFORM_DIR}/.chain_resources.txt") resources"
}

# =============================================================================
# Wave 4: Generate State Move Commands
# =============================================================================
generate_move_commands() {
    log_info "=== Wave 4: Generating state move commands ==="

    MOVE_SCRIPT="${SCRIPT_DIR}/execute_state_moves.sh"

    cat > "$MOVE_SCRIPT" << 'HEADER'
#!/bin/bash
# Auto-generated state migration commands
# This script uses the import-based approach for Azure remote state
#
# IMPORTANT: Review before executing!
# The approach:
#   1. Pull source state to local file
#   2. For each product, init and import resources
#   3. Remove resources from source state
#   4. Push updated source state

set -euo pipefail

ENVIRONMENT="${1:-dev}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TERRAFORM_DIR="$(dirname "$SCRIPT_DIR")"

cd "$TERRAFORM_DIR"

# Pull current state to local file
echo "Pulling current state..."
cd "environments/${ENVIRONMENT}"
terraform state pull > "../${ENVIRONMENT}_current.tfstate"
cd ../..

HEADER

    # Generate import commands for each product
    for product_file in "${TERRAFORM_DIR}/.shared_infra_resources.txt" "${TERRAFORM_DIR}/.mystira_app_resources.txt" "${TERRAFORM_DIR}/.story_generator_resources.txt" "${TERRAFORM_DIR}/.admin_resources.txt" "${TERRAFORM_DIR}/.publisher_resources.txt" "${TERRAFORM_DIR}/.chain_resources.txt"; do
        [[ ! -f "$product_file" ]] && continue

        product_name=$(basename "$product_file" _resources.txt | sed 's/^\.//' | sed 's/_/-/g')

        if [[ -s "$product_file" ]]; then
            cat >> "$MOVE_SCRIPT" << EOF

# =============================================================================
# ${product_name}
# =============================================================================
echo "Processing ${product_name}..."

# Determine product directory
if [[ "${product_name}" == "shared-infra" ]]; then
    PRODUCT_DIR="\${TERRAFORM_DIR}/shared-infra/environments/\${ENVIRONMENT}"
else
    PRODUCT_DIR="\${TERRAFORM_DIR}/products/${product_name}/environments/\${ENVIRONMENT}"
fi

if [[ -d "\$PRODUCT_DIR" ]]; then
    cd "\$PRODUCT_DIR"

    # Initialize with backend
    terragrunt init --terragrunt-non-interactive

    # Import each resource (you'll need to map resource addresses to Azure IDs)
    echo "Resources to import for ${product_name}:"
EOF

            while IFS= read -r resource; do
                [[ -z "$resource" ]] && continue
                echo "    echo \"  - ${resource}\"" >> "$MOVE_SCRIPT"
                echo "    # terragrunt import '${resource}' '<AZURE_RESOURCE_ID>'" >> "$MOVE_SCRIPT"
            done < "$product_file"

            echo "" >> "$MOVE_SCRIPT"
            echo "    cd \"\$TERRAFORM_DIR\"" >> "$MOVE_SCRIPT"
            echo "else" >> "$MOVE_SCRIPT"
            echo "    echo \"Warning: Product directory not found: \$PRODUCT_DIR\"" >> "$MOVE_SCRIPT"
            echo "fi" >> "$MOVE_SCRIPT"
        fi
    done

    cat >> "$MOVE_SCRIPT" << 'FOOTER'

echo ""
echo "=============================================="
echo "Migration commands generated!"
echo ""
echo "MANUAL STEPS REQUIRED:"
echo "1. Get Azure resource IDs for each resource using:"
echo "   az resource list --query \"[?contains(name,'mystira')].{name:name,id:id}\""
echo ""
echo "2. Replace '<AZURE_RESOURCE_ID>' placeholders with actual IDs"
echo ""
echo "3. Run terragrunt import commands for each product"
echo ""
echo "4. Verify with: terragrunt run-all plan"
echo "=============================================="
FOOTER

    chmod +x "$MOVE_SCRIPT"
    log_info "Move commands generated: $MOVE_SCRIPT"
}

# =============================================================================
# Wave 5: Validate New State Structure
# =============================================================================
validate_states() {
    log_info "=== Wave 5: Validating new state structure ==="

    local products=("shared-infra" "mystira-app" "story-generator" "admin" "publisher" "chain")

    for product in "${products[@]}"; do
        local product_dir
        # Handle shared-infra differently from other products
        if [[ "$product" == "shared-infra" ]]; then
            product_dir="${TERRAFORM_DIR}/shared-infra/environments/${ENVIRONMENT}"
        else
            product_dir="${TERRAFORM_DIR}/products/${product}/environments/${ENVIRONMENT}"
        fi

        if [[ -d "$product_dir" ]]; then
            log_info "Validating ${product}..."

            if [[ -f "${product_dir}/terragrunt.hcl" ]]; then
                (cd "$product_dir" && terragrunt validate) || log_warn "Validation issues in ${product}"
            fi
        else
            log_warn "Directory not found: ${product_dir}"
        fi
    done

    log_info "Validation complete"
}

# =============================================================================
# Main Execution
# =============================================================================
main() {
    log_info "Starting Terraform State Migration for environment: ${ENVIRONMENT}"
    log_info "=================================================="

    echo ""
    echo "This script will:"
    echo "  1. Backup current state"
    echo "  2. Create new state file structure"
    echo "  3. Analyze and categorize resources"
    echo "  4. Generate state move commands"
    echo ""
    echo "The actual state moves will NOT be executed automatically."
    echo "Review the generated scripts before running them."
    echo ""

    read -p "Continue? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_warn "Migration cancelled"
        exit 1
    fi

    backup_state
    create_state_containers
    list_resources_by_product
    generate_move_commands

    log_info "=================================================="
    log_info "Migration preparation complete!"
    log_info ""
    log_info "Next steps:"
    log_info "  1. Review generated files in scripts/"
    log_info "  2. Review resource categorization (.*.txt files)"
    log_info "  3. Execute: ./scripts/execute_state_moves.sh ${ENVIRONMENT}"
    log_info "  4. Run: terragrunt run-all plan (verify no changes)"
    log_info "  5. If issues, restore from: $(cat .last_backup_dir)"
}

# Run main function
main "$@"
