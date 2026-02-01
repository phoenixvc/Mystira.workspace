# =============================================================================
# Chain - Production Environment
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
  # Chain API
  api_min_replicas    = 2
  api_max_replicas    = 10
  enable_auto_scaling = true

  # Use shared resources
  use_shared_cosmos        = true
  use_shared_servicebus    = true
  use_shared_log_analytics = true
}
