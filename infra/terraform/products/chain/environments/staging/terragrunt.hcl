# =============================================================================
# Chain - Staging Environment
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
  # Chain API
  api_replicas = 2

  # Use shared resources
  use_shared_cosmos        = true
  use_shared_servicebus    = true
  use_shared_log_analytics = true
}
