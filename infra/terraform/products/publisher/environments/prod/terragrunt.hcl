# =============================================================================
# Publisher - Production Environment
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

# Production-specific configuration
inputs = {
  # Publisher API
  api_min_replicas    = 2
  api_max_replicas    = 10
  enable_auto_scaling = true

  # Use shared resources
  use_shared_servicebus    = true
  use_shared_log_analytics = true
}
