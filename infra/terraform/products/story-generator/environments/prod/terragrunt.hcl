# =============================================================================
# Story Generator - Production Environment
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

# Production-specific configuration
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

  # High availability
  enable_api_auto_scaling = true
  api_min_replicas        = 2
  api_max_replicas        = 10
}
