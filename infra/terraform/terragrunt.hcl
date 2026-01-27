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

  # Extract environment (dev, staging, prod)
  environment = element(local.path_parts, length(local.path_parts) - 1)

  # Azure configuration
  azure_subscription_id = get_env("ARM_SUBSCRIPTION_ID", "")
  azure_tenant_id       = get_env("ARM_TENANT_ID", "")

  # State storage configuration
  state_resource_group  = "mys-terraform-state"
  state_storage_account = "mysterraformstate"
  state_container       = "tfstate"

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
# Each product/environment gets its own state file for isolation

remote_state {
  backend = "azurerm"

  config = {
    resource_group_name  = local.state_resource_group
    storage_account_name = local.state_storage_account
    container_name       = local.state_container
    key                  = "${local.product}/${local.environment}.tfstate"

    # Enable state locking
    use_azuread_auth = true
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
  fallback_location = "eastus2"  # For resources not available in primary region

  # Resource group naming
  resource_group_name = "mys-${local.environment}-rg"
}

# =============================================================================
# Hooks
# =============================================================================

terraform {
  # Run terraform fmt before plan/apply
  before_hook "terraform_fmt" {
    commands = ["plan", "apply"]
    execute  = ["terraform", "fmt", "-recursive"]
  }

  # Validate terraform files before plan/apply
  before_hook "terraform_validate" {
    commands = ["plan", "apply"]
    execute  = ["terraform", "validate"]
  }
}
