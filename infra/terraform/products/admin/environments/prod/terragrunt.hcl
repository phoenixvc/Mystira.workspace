# =============================================================================
# Admin Services - Production Environment
# =============================================================================

locals {
  environment = "prod"
}

include "root" {
  path           = find_in_parent_folders()
  merge_strategy = "deep"
}

include "product" {
  path           = find_in_parent_folders("_product.hcl")
  merge_strategy = "deep"
}

terraform {
  source = "${get_terragrunt_dir()}/."
}

# Production-specific configuration
inputs = {
  # Admin UI (Static Web App)
  admin_ui_sku = "Standard"

  # Custom domains
  enable_custom_domain = true

  # Admin API scaling
  admin_api_min_replicas = 2
  admin_api_max_replicas = 5
}
