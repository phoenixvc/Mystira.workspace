# =============================================================================
# Admin Services - Staging Environment
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

# Staging-specific configuration
inputs = {
  # Admin UI (Static Web App)
  admin_ui_sku = "Standard"

  # Custom domains
  enable_custom_domain = true
}
