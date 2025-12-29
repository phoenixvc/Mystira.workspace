# Shared Storage Infrastructure Module - Azure
# Terraform module for deploying shared blob storage infrastructure
# Used by: Mystira.App, Story Generator, future services

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "storage_sku" {
  description = "Storage account SKU"
  type        = string
  default     = "Standard_LRS"
}

variable "cors_allowed_origins" {
  description = "CORS allowed origins for blob storage"
  type        = list(string)
  default     = []
}

variable "containers" {
  description = "Map of containers to create"
  type = map(object({
    access_type = string # "private", "blob", or "container"
  }))
  default = {
    "mystira-app-media" = { access_type = "blob" }
    "avatars"           = { access_type = "blob" }
    "audio"             = { access_type = "blob" }
    "content-bundles"   = { access_type = "blob" }
    "archives"          = { access_type = "private" }
    "uploads"           = { access_type = "private" }
  }
}

variable "enable_tiering" {
  description = "Enable storage lifecycle tiering"
  type        = bool
  default     = true
}

variable "tier_to_cool_days" {
  description = "Days before moving to cool tier"
  type        = number
  default     = 30
}

variable "tier_to_archive_days" {
  description = "Days before moving to archive tier"
  type        = number
  default     = 90
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  region_code = lookup({
    "southafricanorth" = "san"
    "eastus2"          = "eus2"
    "westeurope"       = "weu"
    "northeurope"      = "neu"
  }, var.location, substr(var.location, 0, 4))

  # Storage account names can't have dashes and max 24 chars
  storage_account_name = substr(replace("mys${var.environment}corest${local.region_code}", "-", ""), 0, 24)

  common_tags = merge(var.tags, {
    Component   = "shared-storage"
    Environment = var.environment
    Service     = "core"
    ManagedBy   = "terraform"
    Project     = "Mystira"
    SharedBy    = "app,story-generator"
  })
}

# Storage Account
resource "azurerm_storage_account" "shared" {
  name                     = local.storage_account_name
  location                 = var.location
  resource_group_name      = var.resource_group_name
  account_tier             = "Standard"
  account_replication_type = replace(var.storage_sku, "Standard_", "")
  account_kind             = "StorageV2"
  min_tls_version          = "TLS1_2"

  blob_properties {
    dynamic "cors_rule" {
      for_each = length(var.cors_allowed_origins) > 0 ? [1] : []
      content {
        allowed_headers    = ["*"]
        allowed_methods    = ["GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS"]
        allowed_origins    = var.cors_allowed_origins
        exposed_headers    = ["*"]
        max_age_in_seconds = 3600
      }
    }
  }

  tags = local.common_tags
}

# Storage Containers
resource "azurerm_storage_container" "containers" {
  for_each = var.containers

  name                  = each.key
  storage_account_id    = azurerm_storage_account.shared.id
  container_access_type = each.value.access_type
}

# Storage Lifecycle Management Policy
resource "azurerm_storage_management_policy" "tiering" {
  count = var.enable_tiering ? 1 : 0

  storage_account_id = azurerm_storage_account.shared.id

  # Rule 1: Audio content tiering
  rule {
    name    = "audio-tiering"
    enabled = true

    filters {
      prefix_match = ["audio/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = var.tier_to_cool_days
        tier_to_archive_after_days_since_modification_greater_than = var.tier_to_archive_days
      }
    }
  }

  # Rule 2: Content bundles tiering
  rule {
    name    = "content-bundle-tiering"
    enabled = true

    filters {
      prefix_match = ["content-bundles/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = 14
        tier_to_archive_after_days_since_modification_greater_than = 60
      }
    }
  }

  # Rule 3: Archives tiering
  rule {
    name    = "archive-tiering"
    enabled = true

    filters {
      prefix_match = ["archives/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_archive_after_days_since_modification_greater_than = 7
      }
    }
  }

  # Rule 4: Uploads cleanup
  rule {
    name    = "uploads-cleanup"
    enabled = true

    filters {
      prefix_match = ["uploads/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        delete_after_days_since_modification_greater_than = 7
      }
    }
  }

  # Rule 5: Snapshot cleanup
  rule {
    name    = "snapshot-cleanup"
    enabled = true

    filters {
      blob_types = ["blockBlob"]
    }

    actions {
      snapshot {
        delete_after_days_since_creation_greater_than = 30
      }
    }
  }

  # Rule 6: General media tiering
  rule {
    name    = "general-media-tiering"
    enabled = var.tier_to_cool_days > 0

    filters {
      prefix_match = ["mystira-app-media/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = var.tier_to_cool_days
        tier_to_archive_after_days_since_modification_greater_than = var.tier_to_archive_days
      }
    }
  }

  # Rule 7: Avatar tiering (keep hot longer)
  rule {
    name    = "avatar-tiering"
    enabled = true

    filters {
      prefix_match = ["avatars/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        tier_to_cool_after_days_since_modification_greater_than    = 90
        tier_to_archive_after_days_since_modification_greater_than = 365
      }
    }
  }
}

# Outputs
output "storage_account_id" {
  description = "Storage account ID"
  value       = azurerm_storage_account.shared.id
}

output "storage_account_name" {
  description = "Storage account name"
  value       = azurerm_storage_account.shared.name
}

output "primary_blob_endpoint" {
  description = "Storage primary blob endpoint"
  value       = azurerm_storage_account.shared.primary_blob_endpoint
}

output "primary_connection_string" {
  description = "Storage account primary connection string"
  value       = azurerm_storage_account.shared.primary_connection_string
  sensitive   = true
}

output "primary_access_key" {
  description = "Storage account primary access key"
  value       = azurerm_storage_account.shared.primary_access_key
  sensitive   = true
}

output "container_urls" {
  description = "Map of container names to URLs"
  value = { for name, container in azurerm_storage_container.containers :
    name => "${azurerm_storage_account.shared.primary_blob_endpoint}${name}"
  }
}
