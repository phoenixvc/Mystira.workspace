# Azure AI Search Module Variables

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
}

variable "region_code" {
  description = "Short region code for resource naming (e.g., 'san' for South Africa North)"
  type        = string
  default     = "san"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

# =============================================================================
# Search Service Configuration
# =============================================================================

variable "sku" {
  description = "SKU for Azure AI Search (free, basic, standard, standard2, standard3)"
  type        = string
  default     = "basic"

  validation {
    condition     = contains(["free", "basic", "standard", "standard2", "standard3"], var.sku)
    error_message = "SKU must be one of: free, basic, standard, standard2, standard3"
  }
}

variable "replica_count" {
  description = "Number of replicas (1-12, standard tier only)"
  type        = number
  default     = 1

  validation {
    condition     = var.replica_count >= 1 && var.replica_count <= 12
    error_message = "Replica count must be between 1 and 12"
  }
}

variable "partition_count" {
  description = "Number of partitions (1, 2, 3, 4, 6, or 12, standard tier only)"
  type        = number
  default     = 1

  validation {
    condition     = contains([1, 2, 3, 4, 6, 12], var.partition_count)
    error_message = "Partition count must be one of: 1, 2, 3, 4, 6, 12"
  }
}

variable "semantic_search_sku" {
  description = "Semantic search SKU (free, standard, or disabled)"
  type        = string
  default     = "disabled"

  validation {
    condition     = contains(["disabled", "free", "standard"], var.semantic_search_sku)
    error_message = "Semantic search SKU must be 'disabled', 'free', or 'standard'"
  }
}

# =============================================================================
# Security Configuration
# =============================================================================

variable "public_network_access_enabled" {
  description = "Enable public network access"
  type        = bool
  default     = true
}

variable "local_authentication_enabled" {
  description = "Enable API key authentication (disable for managed identity only)"
  type        = bool
  default     = true
}

# =============================================================================
# Private Endpoint Configuration
# =============================================================================

variable "enable_private_endpoint" {
  description = "Enable private endpoint"
  type        = bool
  default     = false
}

variable "private_endpoint_subnet_id" {
  description = "Subnet ID for private endpoint"
  type        = string
  default     = ""
}

variable "create_private_dns_zone" {
  description = "Create private DNS zone for private endpoint"
  type        = bool
  default     = true
}

variable "vnet_id" {
  description = "VNet ID for private DNS zone link"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
