# =============================================================================
# Mystira App - Staging Environment
# =============================================================================

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

# Staging-specific configuration
inputs = {
  # App Service Plan
  app_service_sku = "B2"

  # Static Web App
  static_web_app_sku = "Standard"

  # Use shared resources
  use_shared_cosmos        = true
  use_shared_redis         = true
  use_shared_storage       = true
  use_shared_log_analytics = true

  # Custom domains
  enable_custom_domain     = true
  enable_swa_custom_domain = true
}
