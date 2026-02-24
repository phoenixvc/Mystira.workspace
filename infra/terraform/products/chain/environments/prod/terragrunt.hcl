# =============================================================================
# Chain - Production Environment
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
  source = "${get_repo_root()}/infra/terraform//${path_relative_to_include("root")}"
}

# Production-specific configuration
inputs = {
  # Chain API
  api_min_replicas    = 2
  api_max_replicas    = 10
  enable_auto_scaling = true

  # Use shared resources
  use_shared_cosmos        = true
  use_shared_servicebus    = true
  use_shared_log_analytics = true
}
