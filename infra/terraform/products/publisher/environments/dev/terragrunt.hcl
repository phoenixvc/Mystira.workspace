# =============================================================================
# Publisher - Dev Environment
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

# Dev-specific configuration
inputs = {
  # Publisher API
  api_replicas = 1

  # Use shared resources
  use_shared_servicebus    = true
  use_shared_log_analytics = true
}
