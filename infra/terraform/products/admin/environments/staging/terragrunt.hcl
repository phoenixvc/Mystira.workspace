# =============================================================================
# Admin Services - Staging Environment
# =============================================================================

locals {
  environment = "staging"
}

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

# Staging-specific configuration
inputs = {
  # Admin UI (Static Web App)
  admin_ui_sku = "Standard"

  # Custom domains
  enable_custom_domain = true
}
