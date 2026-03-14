# =============================================================================
# Root Terragrunt Configuration
# =============================================================================
# This file contains common configuration inherited by all child configs.
#
# Structure:
#   shared-infra/     - Shared resources (databases, caches, AI services)
#   products/         - Individual product deployments
#     mystira-app/
#     story-generator/
#     admin/
#     publisher/
#     chain/
# =============================================================================

locals {
  # Parse the path to extract product and environment
  # Example path: products/mystira-app/environments/dev
  path_parts = split("/", path_relative_to_include())

  # Determine if this is shared-infra or a product
  is_shared = length(local.path_parts) > 0 && local.path_parts[0] == "shared-infra"

  # Extract product name (e.g., "mystira-app", "story-generator")
  product = local.is_shared ? "shared-infra" : (
    length(local.path_parts) >= 2 ? local.path_parts[1] : "unknown"
  )

  # Extract environment (dev, staging, prod) with safe fallback
  environment = length(local.path_parts) >= 2 ? element(local.path_parts, length(local.path_parts) - 1) : "default"

  # Azure configuration
  azure_subscription_id = get_env("ARM_SUBSCRIPTION_ID", "")
  azure_tenant_id       = get_env("ARM_TENANT_ID", "")

  # State storage configuration
  state_resource_group  = "mys-shared-terraform-rg-san"
  state_storage_account = "myssharedtfstatesan"
  state_container       = "tfstate"

  region_code = "san"

  default_resource_group_name = local.is_shared ? "mys-${local.environment}-core-rg-${local.region_code}" : (
    local.product == "mystira-app" ? (
      local.environment == "prod" ? "mys-prod-mystira-rg-${local.region_code}" : "mys-${local.environment}-app-rg-${local.region_code}"
    ) : local.product == "story-generator" ? "mys-${local.environment}-story-rg-${local.region_code}" : (
      local.product == "admin" ? "mys-${local.environment}-admin-rg-${local.region_code}" : (
        local.product == "publisher" ? "mys-${local.environment}-publisher-rg-${local.region_code}" : (
          local.product == "chain" ? "mys-${local.environment}-chain-rg-${local.region_code}" : "mys-${local.environment}-rg-${local.region_code}"
        )
      )
    )
  )

  # Common tags applied to all resources
  common_tags = {
    ManagedBy   = "terraform"
    Project     = "Mystira"
    Environment = local.environment
    Product     = local.product
  }
}

# =============================================================================
# Remote State Configuration
# =============================================================================
# Each product/environment gets its own state file for isolation.
#
# STATE LOCKING: Azure Blob Storage provides automatic state locking via blob
# leases when use_azuread_auth is enabled. This prevents concurrent operations
# that could corrupt state. If a lock is held for >15 minutes, it will timeout.
# To force unlock: terragrunt force-unlock <LOCK_ID>

remote_state {
  backend = "azurerm"

  config = {
    resource_group_name  = local.state_resource_group
    storage_account_name = local.state_storage_account
    container_name       = local.state_container
    key                  = "${local.product}/${local.environment}.tfstate"
    use_azuread_auth     = true # Enable state locking
  }

  generate = {
    path      = "backend.tf"
    if_exists = "overwrite_terragrunt"
  }
}

# =============================================================================
# Provider Configuration
# =============================================================================
# Generate provider configuration for all child modules

generate "provider" {
  path      = "provider.tf"
  if_exists = "overwrite_terragrunt"
  contents  = <<EOF
terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"
    }
  }
}

provider "azurerm" {
  subscription_id = "${local.azure_subscription_id}"

  features {
    key_vault {
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }
    resource_group {
      prevent_deletion_if_contains_resources = true
    }
  }
}

provider "azapi" {}
EOF
}

# =============================================================================
# Common Inputs
# =============================================================================
# These inputs are available to all child configurations

inputs = {
  environment = local.environment
  tags        = local.common_tags

  # Location configuration
  location          = "southafricanorth"
  fallback_location = "eastus2" # For resources not available in primary region

  # Resource group naming
  resource_group_name = local.default_resource_group_name
}

# =============================================================================
# Hooks
# =============================================================================

terraform {
  # Run terraform fmt before plan/apply
  before_hook "terraform_fmt" {
    commands = ["plan", "apply"]
    execute  = ["terraform", "fmt", "-recursive", "-write=false", "-diff"]
  }

  before_hook "prod_apply_guard" {
    commands = ["apply", "destroy"]
    execute  = ["pwsh", "-NoProfile", "-Command", "if ('${local.environment}' -eq 'prod' -and $env:ALLOW_PROD_APPLY -ne 'true') { Write-Error 'Blocked: prod apply/destroy requires ALLOW_PROD_APPLY=true'; exit 1 }"]
  }

  # NOTE: terraform validate is NOT included as a before_hook because
  # it requires initialization. Validation is handled by CI/CD after init.
}
