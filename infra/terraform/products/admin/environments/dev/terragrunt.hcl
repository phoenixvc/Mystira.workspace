# =============================================================================
# Admin Services - Dev Environment
# =============================================================================

locals {
  environment = "dev"
}

include "root" {
  path           = find_in_parent_folders()
  merge_strategy = "deep"
}

include "product" {
  path           = find_in_parent_folders("_product.hcl")
  merge_strategy = "deep"
}

# Dev-specific configuration
inputs = {
  # Admin UI (Static Web App)
  admin_ui_sku = "Free"

  # Custom domains
  enable_custom_domain = false

  # Admin API scaling (minimal for dev)
  admin_api_min_replicas = 1
  admin_api_max_replicas = 2
}
