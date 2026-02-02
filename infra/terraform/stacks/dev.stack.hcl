# =============================================================================
# Mystira Platform Stack - Development Environment
# =============================================================================
# Deploy the complete Mystira platform to the dev environment.
#
# Usage:
#   cd infra/terraform
#   terragrunt stack run plan --stack-config stacks/dev.stack.hcl
#   terragrunt stack run apply --stack-config stacks/dev.stack.hcl
#
# Selective deployment:
#   terragrunt stack run plan --stack-config stacks/dev.stack.hcl --target shared-infra
#   terragrunt stack run plan --stack-config stacks/dev.stack.hcl --target admin
# =============================================================================

locals {
  environment = "dev"
  tf_root     = "${get_terragrunt_dir()}/.."

  # Dev environment configuration
  config = {
    # Resource sizing (cost-optimized for dev)
    postgresql_sku        = "B_Standard_B1ms"
    postgresql_storage_gb = 32
    redis_sku             = "Basic"
    redis_capacity        = 0

    # Scaling limits
    min_replicas = 1
    max_replicas = 2

    # Monitoring
    log_retention_days = 30
  }
}

# =============================================================================
# Layer 0: Shared Infrastructure
# =============================================================================
unit "shared-infra" {
  source = "${local.tf_root}/shared-infra/environments/${local.environment}"
  path   = "shared-infra-dev"

  inputs = {
    environment           = local.environment
    postgresql_sku        = local.config.postgresql_sku
    postgresql_storage_gb = local.config.postgresql_storage_gb
    redis_sku             = local.config.redis_sku
    redis_capacity        = local.config.redis_capacity
    log_retention_days    = local.config.log_retention_days
  }
}

# =============================================================================
# Layer 1: Application Products
# =============================================================================

unit "admin" {
  source = "${local.tf_root}/products/admin/environments/${local.environment}"
  path   = "admin-dev"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment  = local.environment
    min_replicas = local.config.min_replicas
    max_replicas = local.config.max_replicas
  }
}

unit "chain" {
  source = "${local.tf_root}/products/chain/environments/${local.environment}"
  path   = "chain-dev"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment  = local.environment
    min_replicas = local.config.min_replicas
    max_replicas = local.config.max_replicas
  }
}

unit "mystira-app" {
  source = "${local.tf_root}/products/mystira-app/environments/${local.environment}"
  path   = "mystira-app-dev"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment  = local.environment
    min_replicas = local.config.min_replicas
    max_replicas = local.config.max_replicas
  }
}

unit "publisher" {
  source = "${local.tf_root}/products/publisher/environments/${local.environment}"
  path   = "publisher-dev"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment  = local.environment
    min_replicas = local.config.min_replicas
    max_replicas = local.config.max_replicas
  }
}

unit "story-generator" {
  source = "${local.tf_root}/products/story-generator/environments/${local.environment}"
  path   = "story-generator-dev"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment  = local.environment
    min_replicas = local.config.min_replicas
    max_replicas = local.config.max_replicas
  }
}
