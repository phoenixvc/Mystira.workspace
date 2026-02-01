# =============================================================================
# Mystira Platform Stack - Staging Environment
# =============================================================================
# Deploy the complete Mystira platform to staging for pre-production testing.
#
# Usage:
#   cd infra/terraform
#   terragrunt stack run plan --stack-config stacks/staging.stack.hcl
#   terragrunt stack run apply --stack-config stacks/staging.stack.hcl
#
# Note: Staging mirrors production configuration at reduced scale for
# cost optimization while maintaining feature parity.
# =============================================================================

locals {
  environment = "staging"
  tf_root     = "${get_terragrunt_dir()}/.."

  # Staging environment configuration (production-like, reduced scale)
  config = {
    # Resource sizing (balanced for staging)
    postgresql_sku        = "GP_Standard_D2s_v3"
    postgresql_storage_gb = 64
    redis_sku             = "Standard"
    redis_capacity        = 1

    # Scaling limits
    min_replicas = 1
    max_replicas = 3

    # Monitoring
    log_retention_days = 60

    # High availability (disabled for cost savings)
    ha_enabled = false
  }
}

# =============================================================================
# Layer 0: Shared Infrastructure
# =============================================================================
unit "shared-infra" {
  source = "${local.tf_root}/shared-infra/environments/${local.environment}"
  path   = "shared-infra-staging"

  inputs = {
    environment           = local.environment
    postgresql_sku        = local.config.postgresql_sku
    postgresql_storage_gb = local.config.postgresql_storage_gb
    redis_sku             = local.config.redis_sku
    redis_capacity        = local.config.redis_capacity
    log_retention_days    = local.config.log_retention_days
    ha_enabled            = local.config.ha_enabled
  }
}

# =============================================================================
# Layer 1: Application Products
# =============================================================================

unit "admin" {
  source = "${local.tf_root}/products/admin/environments/${local.environment}"
  path   = "admin-staging"

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
  path   = "chain-staging"

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
  path   = "mystira-app-staging"

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
  path   = "publisher-staging"

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
  path   = "story-generator-staging"

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
