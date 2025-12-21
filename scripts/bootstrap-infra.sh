#!/bin/bash
# Bootstrap Infrastructure Script
# This script sets up all prerequisites for deploying Mystira infrastructure
# Run once before first deployment, or to verify/fix infrastructure prerequisites

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
# All resources deploy to South Africa North (fresh deployment)
TERRAFORM_RG="mys-shared-terraform-rg-san"
TERRAFORM_STORAGE="myssharedtfstatesan"
TERRAFORM_CONTAINER="tfstate"
LOCATION="southafricanorth"
ACR_NAME="myssharedacr"
DNS_ZONE="mystira.app"
DNS_ZONE_RG="mys-prod-core-rg-san"

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

check_prerequisites() {
    log_info "Checking prerequisites..."

    # Check Azure CLI
    if ! command -v az &> /dev/null; then
        log_error "Azure CLI not installed. Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi

    # Check if logged in
    if ! az account show &> /dev/null; then
        log_error "Not logged into Azure. Run: az login"
        exit 1
    fi

    # Check Terraform
    if ! command -v terraform &> /dev/null; then
        log_warn "Terraform not installed locally (OK if using CI/CD only)"
    fi

    log_success "Prerequisites check passed"
}

get_subscription_info() {
    log_info "Getting subscription info..."
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
    log_info "Using subscription: $SUBSCRIPTION_NAME ($SUBSCRIPTION_ID)"
}

create_terraform_backend() {
    log_info "Setting up Terraform backend..."

    # Check if resource group exists
    if az group show --name "$TERRAFORM_RG" &> /dev/null; then
        log_info "Resource group $TERRAFORM_RG already exists"
    else
        log_info "Creating resource group $TERRAFORM_RG..."
        az group create --name "$TERRAFORM_RG" --location "$LOCATION" --output none
        log_success "Created resource group $TERRAFORM_RG"
    fi

    # Check if storage account exists
    if az storage account show --name "$TERRAFORM_STORAGE" --resource-group "$TERRAFORM_RG" &> /dev/null; then
        log_info "Storage account $TERRAFORM_STORAGE already exists"
    else
        log_info "Creating storage account $TERRAFORM_STORAGE..."
        az storage account create \
            --name "$TERRAFORM_STORAGE" \
            --resource-group "$TERRAFORM_RG" \
            --location "$LOCATION" \
            --sku Standard_LRS \
            --kind StorageV2 \
            --output none
        log_success "Created storage account $TERRAFORM_STORAGE"
    fi

    # Get storage key and create container
    STORAGE_KEY=$(az storage account keys list \
        --resource-group "$TERRAFORM_RG" \
        --account-name "$TERRAFORM_STORAGE" \
        --query "[0].value" -o tsv)

    if az storage container show --name "$TERRAFORM_CONTAINER" --account-name "$TERRAFORM_STORAGE" --account-key "$STORAGE_KEY" &> /dev/null; then
        log_info "Container $TERRAFORM_CONTAINER already exists"
    else
        log_info "Creating container $TERRAFORM_CONTAINER..."
        az storage container create \
            --name "$TERRAFORM_CONTAINER" \
            --account-name "$TERRAFORM_STORAGE" \
            --account-key "$STORAGE_KEY" \
            --output none
        log_success "Created container $TERRAFORM_CONTAINER"
    fi

    log_success "Terraform backend ready"
}

create_shared_acr() {
    log_info "Setting up shared Azure Container Registry..."

    # Check if ACR exists
    if az acr show --name "$ACR_NAME" &> /dev/null; then
        log_info "Container Registry $ACR_NAME already exists"
    else
        log_info "Creating Container Registry $ACR_NAME..."
        az acr create \
            --name "$ACR_NAME" \
            --resource-group "$TERRAFORM_RG" \
            --location "$LOCATION" \
            --sku Basic \
            --admin-enabled true \
            --output none
        log_success "Created Container Registry $ACR_NAME"
    fi

    log_success "Container Registry ready"
}

setup_dns_zone() {
    log_info "Setting up DNS Zone for $DNS_ZONE..."

    # Ensure DNS zone resource group exists
    if ! az group show --name "$DNS_ZONE_RG" &> /dev/null; then
        log_info "Creating DNS zone resource group $DNS_ZONE_RG..."
        az group create --name "$DNS_ZONE_RG" --location "$LOCATION" --output none
        log_success "Created resource group $DNS_ZONE_RG"
    fi

    # Check if DNS zone exists
    if az network dns zone show --name "$DNS_ZONE" --resource-group "$DNS_ZONE_RG" &> /dev/null; then
        log_info "DNS Zone $DNS_ZONE already exists"
    else
        log_info "Creating DNS Zone $DNS_ZONE..."
        az network dns zone create \
            --name "$DNS_ZONE" \
            --resource-group "$DNS_ZONE_RG" \
            --output none
        log_success "Created DNS Zone $DNS_ZONE"

        log_warn "IMPORTANT: Update your domain registrar's nameservers to:"
        az network dns zone show --name "$DNS_ZONE" --resource-group "$DNS_ZONE_RG" --query nameServers -o tsv
    fi

    log_success "DNS Zone ready"
}

create_service_principal() {
    log_info "Checking service principal for GitHub Actions..."

    SP_NAME="mystira-github-actions"

    # Check if SP exists
    if az ad sp list --display-name "$SP_NAME" --query "[0].appId" -o tsv 2>/dev/null | grep -q .; then
        log_info "Service principal $SP_NAME already exists"
        APP_ID=$(az ad sp list --display-name "$SP_NAME" --query "[0].appId" -o tsv)
        log_info "App ID: $APP_ID"
    else
        log_info "Creating service principal $SP_NAME..."
        SP_OUTPUT=$(az ad sp create-for-rbac \
            --name "$SP_NAME" \
            --role contributor \
            --scopes "/subscriptions/$SUBSCRIPTION_ID" \
            --sdk-auth)

        log_success "Created service principal"
        log_warn "IMPORTANT: Save this output as GitHub secret AZURE_CREDENTIALS:"
        echo "$SP_OUTPUT"
    fi
}

print_summary() {
    echo ""
    echo "=============================================="
    echo -e "${GREEN}Infrastructure Bootstrap Complete${NC}"
    echo "=============================================="
    echo ""
    echo "Resources created/verified:"
    echo "  - Resource Group: $TERRAFORM_RG"
    echo "  - Storage Account: $TERRAFORM_STORAGE"
    echo "  - Container: $TERRAFORM_CONTAINER"
    echo "  - Container Registry: $ACR_NAME"
    echo "  - DNS Zone: $DNS_ZONE"
    echo ""
    echo "Next steps:"
    echo "  1. Verify DNS nameservers are configured at your registrar"
    echo "  2. Ensure AZURE_CREDENTIALS secret is set in GitHub"
    echo "  3. Run the Infrastructure Deploy workflow from GitHub Actions"
    echo ""
    echo "To deploy manually:"
    echo "  cd infra/terraform/environments/dev"
    echo "  terraform init"
    echo "  terraform plan"
    echo "  terraform apply"
    echo ""
}

# Main
main() {
    echo "=============================================="
    echo "Mystira Infrastructure Bootstrap"
    echo "=============================================="
    echo ""

    check_prerequisites
    get_subscription_info
    create_terraform_backend
    create_shared_acr
    setup_dns_zone
    create_service_principal
    print_summary
}

main "$@"
