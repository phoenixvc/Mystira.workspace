# =============================================================================
# Story Generator - Staging Environment
# =============================================================================

include "root" {
  path           = find_in_parent_folders()
  merge_strategy = "deep"
}

include "product" {
  path           = find_in_parent_folders("_product.hcl")
  merge_strategy = "deep"
}

# Staging-specific configuration
inputs = {
  # Static Web App
  enable_static_web_app = true
  static_web_app_sku    = "Standard"

  # Use shared resources
  use_shared_postgresql    = true
  use_shared_redis         = true
  use_shared_log_analytics = true

  # Custom domains
  enable_swa_custom_domain = true
}
