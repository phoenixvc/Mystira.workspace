# =============================================================================
# Publisher - Dev Environment
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
  # Publisher API
  api_replicas = 1

  # Use shared resources
  use_shared_servicebus    = true
  use_shared_log_analytics = true
}
