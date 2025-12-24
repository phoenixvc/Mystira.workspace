#!/bin/bash
# Deploy Anthropic Claude models via Azure ML Serverless Endpoints
# This script deploys Claude models from the Azure AI Model Catalog
#
# Prerequisites:
#   - Azure CLI installed and logged in
#   - Azure ML extension installed: az extension add -n ml
#   - Marketplace terms accepted for Anthropic models
#   - Sufficient quota in the target region
#
# Usage:
#   ./deploy-claude-models.sh [environment]
#
# Examples:
#   ./deploy-claude-models.sh dev
#   ./deploy-claude-models.sh prod
#   AZURE_LOCATION=uksouth ./deploy-claude-models.sh staging

set -e

# =============================================================================
# Configuration
# =============================================================================

ENVIRONMENT="${1:-dev}"
LOCATION="${AZURE_LOCATION:-uksouth}"  # Claude only available in specific regions
RESOURCE_GROUP="${AZURE_RESOURCE_GROUP:-mys-${ENVIRONMENT}-core-rg-san}"
AI_SERVICES_NAME="${AZURE_AI_SERVICES_NAME:-mys-shared-ai-san}"

# Claude models to deploy
declare -A CLAUDE_MODELS=(
  ["claude-haiku-4-5"]="Anthropic-claude-3-5-haiku"
  ["claude-sonnet-4-5"]="Anthropic-claude-sonnet-4-5"
  ["claude-opus-4-5"]="Anthropic-claude-opus-4-5"
)

# =============================================================================
# Helper Functions
# =============================================================================

print_header() {
  echo ""
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  echo "$1"
  echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
}

print_success() {
  echo "✓ $1"
}

print_info() {
  echo "→ $1"
}

print_warning() {
  echo "⚠ $1"
}

print_error() {
  echo "✗ $1"
}

# Error handler
error_handler() {
  local exit_code=$?
  if [ $exit_code -ne 0 ]; then
    print_header "DEPLOYMENT FAILED"
    echo ""
    echo "The Claude model deployment failed. Common causes:"
    echo ""
    echo "1. Marketplace Terms Not Accepted:"
    echo "   Accept terms at: https://ai.azure.com/explore/models"
    echo "   Or via CLI:"
    echo "   az vm image terms accept --publisher Anthropic --offer claude --plan <model>"
    echo ""
    echo "2. Region Not Supported:"
    echo "   Claude models are only available in: UK South, East US 2, Sweden Central"
    echo "   Current region: $LOCATION"
    echo ""
    echo "3. Insufficient Quota:"
    echo "   Request quota increase at: https://aka.ms/oai/quotaincrease"
    echo ""
    echo "4. Azure ML Extension Missing:"
    echo "   Install with: az extension add -n ml"
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
  fi
}

trap error_handler EXIT

# =============================================================================
# Pre-flight Checks
# =============================================================================

check_prerequisites() {
  print_header "PRE-FLIGHT CHECKS"

  # Check Azure CLI
  if ! command -v az &> /dev/null; then
    print_error "Azure CLI not found. Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
  fi
  print_success "Azure CLI found"

  # Check logged in
  if ! az account show &> /dev/null; then
    print_error "Not logged in to Azure. Run: az login"
    exit 1
  fi
  print_success "Logged in to Azure"

  # Check ML extension
  if ! az extension show -n ml &> /dev/null; then
    print_info "Installing Azure ML extension..."
    az extension add -n ml --yes
    print_success "Azure ML extension installed"
  else
    print_success "Azure ML extension found"
  fi

  # Check resource group exists
  if ! az group show --name "$RESOURCE_GROUP" &> /dev/null; then
    print_error "Resource group '$RESOURCE_GROUP' not found"
    print_info "Run Terraform first to create infrastructure"
    exit 1
  fi
  print_success "Resource group '$RESOURCE_GROUP' exists"

  # Check AI Services account exists
  if ! az cognitiveservices account show --name "$AI_SERVICES_NAME" --resource-group "$RESOURCE_GROUP" &> /dev/null; then
    print_error "AI Services account '$AI_SERVICES_NAME' not found"
    print_info "Run Terraform first to create AI Foundry account"
    exit 1
  fi
  print_success "AI Services account '$AI_SERVICES_NAME' exists"

  echo ""
}

# =============================================================================
# Accept Marketplace Terms
# =============================================================================

accept_marketplace_terms() {
  print_header "MARKETPLACE TERMS"

  local publisher="anthropic"

  for deployment_name in "${!CLAUDE_MODELS[@]}"; do
    local model_id="${CLAUDE_MODELS[$deployment_name]}"
    print_info "Checking marketplace terms for $deployment_name..."

    # Try to accept terms (idempotent - safe to run multiple times)
    # Note: This uses the Azure Marketplace terms acceptance
    # The actual command may vary based on how Anthropic publishes to Azure
    if az term accept --publisher "$publisher" --product "claude" --plan "${deployment_name}" 2>/dev/null; then
      print_success "Terms accepted for $deployment_name"
    else
      print_warning "Could not auto-accept terms for $deployment_name"
      print_info "Please accept manually at: https://ai.azure.com/explore/models"
    fi
  done

  echo ""
}

# =============================================================================
# Deploy Claude Models
# =============================================================================

deploy_model() {
  local deployment_name="$1"
  local model_id="$2"

  print_info "Deploying $deployment_name..."

  # Check if endpoint already exists
  if az ml serverless-endpoint show \
    --name "$deployment_name" \
    --resource-group "$RESOURCE_GROUP" \
    --workspace-name "$AI_SERVICES_NAME" &> /dev/null 2>&1; then
    print_success "$deployment_name already deployed"
    return 0
  fi

  # Create serverless endpoint
  # Using az rest for more control over the API call
  local endpoint_url="https://management.azure.com/subscriptions/$(az account show --query id -o tsv)/resourceGroups/${RESOURCE_GROUP}/providers/Microsoft.MachineLearningServices/workspaces/${AI_SERVICES_NAME}/serverlessEndpoints/${deployment_name}?api-version=2024-10-01"

  local body=$(cat <<EOF
{
  "location": "$LOCATION",
  "properties": {
    "modelSettings": {
      "modelId": "azureml://registries/azure-openai/models/${model_id}"
    },
    "authMode": "Key"
  },
  "sku": {
    "name": "Consumption"
  }
}
EOF
)

  # Alternative: Use az ml serverless-endpoint create if workspace is set up
  # First try with az ml command
  if az ml serverless-endpoint create \
    --name "$deployment_name" \
    --model-id "azureml://registries/azure-openai/models/${model_id}" \
    --resource-group "$RESOURCE_GROUP" \
    --workspace-name "$AI_SERVICES_NAME" 2>/dev/null; then
    print_success "$deployment_name deployed successfully"
    return 0
  fi

  # Fallback to az rest if az ml doesn't work (different API structure)
  print_info "Trying alternative deployment method..."

  if az rest --method PUT \
    --url "$endpoint_url" \
    --body "$body" \
    --headers "Content-Type=application/json" 2>/dev/null; then
    print_success "$deployment_name deployed successfully"
    return 0
  fi

  # Final fallback: Deploy via Cognitive Services deployment API
  print_info "Trying Cognitive Services deployment API..."

  local cs_endpoint_url="https://management.azure.com/subscriptions/$(az account show --query id -o tsv)/resourceGroups/${RESOURCE_GROUP}/providers/Microsoft.CognitiveServices/accounts/${AI_SERVICES_NAME}/deployments/${deployment_name}?api-version=2024-10-01"

  local cs_body=$(cat <<EOF
{
  "sku": {
    "name": "Standard",
    "capacity": 1
  },
  "properties": {
    "model": {
      "format": "Anthropic",
      "name": "${model_id#Anthropic-}",
      "version": "latest"
    }
  }
}
EOF
)

  if az rest --method PUT \
    --url "$cs_endpoint_url" \
    --body "$cs_body" \
    --headers "Content-Type=application/json"; then
    print_success "$deployment_name deployed successfully"
    return 0
  fi

  print_error "Failed to deploy $deployment_name"
  print_info "Please deploy manually via Azure AI Foundry portal: https://ai.azure.com"
  return 1
}

deploy_all_models() {
  print_header "DEPLOYING CLAUDE MODELS"

  local failed=0

  for deployment_name in "${!CLAUDE_MODELS[@]}"; do
    local model_id="${CLAUDE_MODELS[$deployment_name]}"
    if ! deploy_model "$deployment_name" "$model_id"; then
      ((failed++))
    fi
  done

  echo ""

  if [ $failed -gt 0 ]; then
    print_warning "$failed model(s) failed to deploy"
    return 1
  fi

  return 0
}

# =============================================================================
# Verify Deployments
# =============================================================================

verify_deployments() {
  print_header "VERIFYING DEPLOYMENTS"

  # List all deployments
  print_info "Current deployments in $AI_SERVICES_NAME:"
  echo ""

  az cognitiveservices account deployment list \
    --name "$AI_SERVICES_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --output table 2>/dev/null || print_warning "Could not list deployments"

  echo ""

  # Get endpoint details
  print_info "Endpoint URL:"
  local endpoint=$(az cognitiveservices account show \
    --name "$AI_SERVICES_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query "properties.endpoint" -o tsv 2>/dev/null)
  echo "  $endpoint"

  echo ""
}

# =============================================================================
# Generate Usage Examples
# =============================================================================

print_usage_examples() {
  print_header "USAGE EXAMPLES"

  local endpoint=$(az cognitiveservices account show \
    --name "$AI_SERVICES_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query "properties.endpoint" -o tsv 2>/dev/null)

  cat <<EOF

# Get API Key
az cognitiveservices account keys list \\
  --name $AI_SERVICES_NAME \\
  --resource-group $RESOURCE_GROUP \\
  --query "key1" -o tsv

# Test Claude Sonnet
curl -X POST "${endpoint}openai/deployments/claude-sonnet-4-5/chat/completions?api-version=2024-10-01" \\
  -H "Content-Type: application/json" \\
  -H "api-key: \$API_KEY" \\
  -d '{
    "messages": [{"role": "user", "content": "Hello Claude!"}],
    "max_tokens": 100
  }'

# Python Example
from openai import AzureOpenAI

client = AzureOpenAI(
    azure_endpoint="${endpoint}",
    api_key=os.getenv("AZURE_AI_KEY"),
    api_version="2024-10-01"
)

response = client.chat.completions.create(
    model="claude-sonnet-4-5",
    messages=[{"role": "user", "content": "Analyze this code..."}],
    max_tokens=1000
)

EOF
}

# =============================================================================
# Main
# =============================================================================

main() {
  print_header "CLAUDE MODEL DEPLOYMENT"
  echo ""
  echo "Environment:    $ENVIRONMENT"
  echo "Location:       $LOCATION"
  echo "Resource Group: $RESOURCE_GROUP"
  echo "AI Services:    $AI_SERVICES_NAME"
  echo ""
  echo "Models to deploy:"
  for deployment_name in "${!CLAUDE_MODELS[@]}"; do
    echo "  - $deployment_name (${CLAUDE_MODELS[$deployment_name]})"
  done

  check_prerequisites
  # accept_marketplace_terms  # Uncomment when marketplace terms API is available
  deploy_all_models
  verify_deployments
  print_usage_examples

  print_header "DEPLOYMENT COMPLETE"
  echo ""
  print_success "Claude models deployed successfully!"
  echo ""
  echo "Next steps:"
  echo "  1. Get API key: az cognitiveservices account keys list --name $AI_SERVICES_NAME --resource-group $RESOURCE_GROUP"
  echo "  2. Test endpoint with curl or SDK"
  echo "  3. Update application configuration with deployment names"
  echo ""

  # Clear trap on success
  trap - EXIT
}

main "$@"
