# =============================================================================
# Mystira Platform Stack - Root Stack Configuration
# =============================================================================
# Terragrunt Stacks allow deploying multiple units as a cohesive platform.
#
# Stack Layers:
#   Layer 0: shared-infra     - Databases, caches, messaging, monitoring
#   Layer 1: products         - Application services (can deploy in parallel)
#
# Usage:
#   terragrunt stack run plan --stack-config stacks/terragrunt.stack.hcl
#   terragrunt stack run apply --stack-config stacks/terragrunt.stack.hcl
#
# Environment-specific:
#   terragrunt stack run plan --stack-config stacks/dev.stack.hcl
#   terragrunt stack run plan --stack-config stacks/staging.stack.hcl
#   terragrunt stack run plan --stack-config stacks/prod.stack.hcl
# =============================================================================

locals {
  # Default environment from env var or fallback
  environment = get_env("TF_VAR_environment", "dev")

  # Root terraform directory
  tf_root = "${get_terragrunt_dir()}/.."

  # Products that depend on shared-infra
  products = [
    "admin",
    "chain",
    "mystira-app",
    "publisher",
    "story-generator",
  ]
}

# =============================================================================
# Layer 0: Shared Infrastructure (Foundation)
# =============================================================================
# Must be deployed first - all products depend on these resources
unit "shared-infra" {
  source = "${local.tf_root}/shared-infra/environments/${local.environment}"

  # No dependencies - this is the foundation layer
  path = "shared-infra"

  # Stack-level inputs
  inputs = {
    environment = local.environment
  }
}

# =============================================================================
# Layer 1: Application Products (Can deploy in parallel after shared-infra)
# =============================================================================

unit "admin" {
  source = "${local.tf_root}/products/admin/environments/${local.environment}"
  path   = "products/admin"

  dependency {
    config = unit.shared-infra
    # Skip validation failures during planning without deployed shared-infra
    skip_outputs = true
  }

  inputs = {
    environment = local.environment
  }
}

unit "chain" {
  source = "${local.tf_root}/products/chain/environments/${local.environment}"
  path   = "products/chain"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment = local.environment
  }
}

unit "mystira-app" {
  source = "${local.tf_root}/products/mystira-app/environments/${local.environment}"
  path   = "products/mystira-app"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment = local.environment
  }
}

unit "publisher" {
  source = "${local.tf_root}/products/publisher/environments/${local.environment}"
  path   = "products/publisher"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment = local.environment
  }
}

unit "story-generator" {
  source = "${local.tf_root}/products/story-generator/environments/${local.environment}"
  path   = "products/story-generator"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment = local.environment
  }
}
