# =============================================================================
# Mystira.App - Dev Environment
# =============================================================================

include "root" {
  path = find_in_parent_folders()
}

include "product" {
  path = find_in_parent_folders("terragrunt.hcl", "${get_terragrunt_dir()}/../../terragrunt.hcl")
}

terraform {
  source = "${get_terragrunt_dir()}/."
}

# Dev-specific configuration
inputs = {
  # App Service
  app_service_sku = "B1"  # Basic tier for dev

  # Static Web App
  enable_static_web_app = true
  static_web_app_sku    = "Free"

  # Use shared resources (cost optimization for dev)
  use_shared_cosmos       = true
  use_shared_storage      = true
  use_shared_monitoring   = true
  use_shared_postgresql   = true

  # Communication Services
  enable_communication_services = false  # Disabled for dev

  # Custom domains
  enable_app_custom_domain = false
  enable_api_custom_domain = false

  # CORS
  cors_allowed_origins = [
    "http://localhost:5000",
    "http://localhost:5173",
    "https://dev.mystira.app"
  ]
}
