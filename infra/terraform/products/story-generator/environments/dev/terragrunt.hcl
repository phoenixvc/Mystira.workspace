# =============================================================================
# Story Generator - Dev Environment
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
  # Static Web App
  enable_static_web_app = true
  static_web_app_sku    = "Free"

  # Use shared resources
  use_shared_postgresql    = true
  use_shared_redis         = true
  use_shared_log_analytics = true

  # Custom domains
  enable_swa_custom_domain = false
}
