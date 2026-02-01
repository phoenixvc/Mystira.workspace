# =============================================================================
# Mystira Platform Stack - Production Environment
# =============================================================================
# Deploy the complete Mystira platform to production.
#
# Usage:
#   cd infra/terraform
#   terragrunt stack run plan --stack-config stacks/prod.stack.hcl
#   terragrunt stack run apply --stack-config stacks/prod.stack.hcl
#
# IMPORTANT: Production deployments require:
#   1. Successful staging deployment and testing
#   2. Approval via GitHub environment protection rules
#   3. Off-peak deployment window (recommended)
#
# Rollback:
#   terragrunt stack run plan --stack-config stacks/prod.stack.hcl --target <unit> -- -refresh-only
# =============================================================================

locals {
  environment = "prod"
  tf_root     = "${get_terragrunt_dir()}/.."

  # Production environment configuration (high availability, production scale)
  config = {
    # Resource sizing (production-grade)
    postgresql_sku         = "GP_Standard_D4s_v3"
    postgresql_storage_gb  = 256
    postgresql_ha_mode     = "ZoneRedundant"
    postgresql_backup_days = 35
    redis_sku              = "Premium"
    redis_capacity         = 1

    # Scaling limits (auto-scale enabled)
    min_replicas = 2
    max_replicas = 10

    # Monitoring (extended retention)
    log_retention_days = 90

    # High availability
    ha_enabled    = true
    zone_redundant = true
  }
}

# =============================================================================
# Layer 0: Shared Infrastructure
# =============================================================================
unit "shared-infra" {
  source = "${local.tf_root}/shared-infra/environments/${local.environment}"
  path   = "shared-infra-prod"

  inputs = {
    environment            = local.environment
    postgresql_sku         = local.config.postgresql_sku
    postgresql_storage_gb  = local.config.postgresql_storage_gb
    postgresql_ha_mode     = local.config.postgresql_ha_mode
    postgresql_backup_days = local.config.postgresql_backup_days
    redis_sku              = local.config.redis_sku
    redis_capacity         = local.config.redis_capacity
    log_retention_days     = local.config.log_retention_days
    ha_enabled             = local.config.ha_enabled
    zone_redundant         = local.config.zone_redundant
  }
}

# =============================================================================
# Layer 1: Application Products
# =============================================================================

unit "admin" {
  source = "${local.tf_root}/products/admin/environments/${local.environment}"
  path   = "admin-prod"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment    = local.environment
    min_replicas   = local.config.min_replicas
    max_replicas   = local.config.max_replicas
    zone_redundant = local.config.zone_redundant
  }
}

unit "chain" {
  source = "${local.tf_root}/products/chain/environments/${local.environment}"
  path   = "chain-prod"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment    = local.environment
    min_replicas   = local.config.min_replicas
    max_replicas   = local.config.max_replicas
    zone_redundant = local.config.zone_redundant
  }
}

unit "mystira-app" {
  source = "${local.tf_root}/products/mystira-app/environments/${local.environment}"
  path   = "mystira-app-prod"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment    = local.environment
    min_replicas   = local.config.min_replicas
    max_replicas   = local.config.max_replicas
    zone_redundant = local.config.zone_redundant
  }
}

unit "publisher" {
  source = "${local.tf_root}/products/publisher/environments/${local.environment}"
  path   = "publisher-prod"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment    = local.environment
    min_replicas   = local.config.min_replicas
    max_replicas   = local.config.max_replicas
    zone_redundant = local.config.zone_redundant
  }
}

unit "story-generator" {
  source = "${local.tf_root}/products/story-generator/environments/${local.environment}"
  path   = "story-generator-prod"

  dependency {
    config       = unit.shared-infra
    skip_outputs = true
  }

  inputs = {
    environment    = local.environment
    min_replicas   = local.config.min_replicas
    max_replicas   = local.config.max_replicas
    zone_redundant = local.config.zone_redundant
  }
}
