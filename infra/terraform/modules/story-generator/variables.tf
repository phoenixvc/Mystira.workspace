# Mystira Story-Generator Module - Variable Definitions
# Module: infra/terraform/modules/story-generator
#
# This file contains all input variable declarations for the story-generator module.
# See main.tf for resource definitions and outputs.tf for output values.

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "org" {
  description = "Organization prefix (e.g., mys)"
  type        = string
  default     = "mys"
}

variable "location" {
  description = "Azure region for deployment"
  type        = string
  default     = "eastus"
}

variable "region_code" {
  description = "Short region code (eus, euw, etc.) - defaults to 'eus' for eastus"
  type        = string
  default     = "eus"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "vnet_id" {
  description = "Virtual Network ID for story-generator deployment"
  type        = string
  default     = null
}

variable "subnet_id" {
  description = "Subnet ID for story-generator service"
  type        = string
  default     = null
}

variable "shared_postgresql_server_id" {
  description = "ID of shared PostgreSQL server (from shared/postgresql module)"
  type        = string
  default     = null
}

variable "shared_redis_cache_id" {
  description = "ID of shared Redis cache (from shared/redis module)"
  type        = string
  default     = null
}

variable "shared_log_analytics_workspace_id" {
  description = "ID of shared Log Analytics workspace for monitoring integration"
  type        = string
  default     = null
}

variable "use_shared_cosmos" {
  description = "Whether to use shared Cosmos DB instead of a dedicated account"
  type        = bool
  default     = false
}

variable "shared_cosmos_connection_string" {
  description = "Shared Cosmos DB connection string (AccountEndpoint=...;AccountKey=...;). Required when use_shared_cosmos = true."
  type        = string
  sensitive   = true
  default     = null

  validation {
    condition     = var.use_shared_cosmos == false || (var.shared_cosmos_connection_string != null && trimspace(var.shared_cosmos_connection_string) != "")
    error_message = "shared_cosmos_connection_string must be set (non-empty) when use_shared_cosmos is true."
  }
}

variable "cosmos_database_id" {
  description = "Cosmos DB database id for story session storage"
  type        = string
  default     = "MystiraStoryGenerator"
}

variable "cosmos_container_id" {
  description = "Cosmos DB container id for story session storage"
  type        = string
  default     = "StorySessions"
}

variable "use_shared_postgresql" {
  description = "Whether to use shared PostgreSQL instead of creating dedicated one"
  type        = bool
  default     = false
}

variable "use_shared_redis" {
  description = "Whether to use shared Redis instead of creating dedicated one"
  type        = bool
  default     = false
}

variable "use_shared_log_analytics" {
  description = "Whether to use shared Log Analytics instead of creating dedicated one"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

# -----------------------------------------------------------------------------
# Static Web App Settings (Blazor WASM Frontend)
# -----------------------------------------------------------------------------

variable "enable_static_web_app" {
  description = "Deploy Static Web App for Blazor WASM frontend"
  type        = bool
  default     = false
}

variable "static_web_app_sku" {
  description = "SKU for Static Web App (Free or Standard)"
  type        = string
  default     = "Free"
}

variable "fallback_location" {
  description = "Fallback region for resources not available in primary location (e.g., Static Web Apps not available in South Africa North)"
  type        = string
  default     = "eastus2"
}

variable "swa_custom_domain" {
  description = "Custom domain for Static Web App (e.g., story.mystira.app)"
  type        = string
  default     = ""
}

variable "enable_swa_custom_domain" {
  description = "Enable custom domain for Static Web App"
  type        = bool
  default     = false
}

# -----------------------------------------------------------------------------
# Lifecycle Settings
# -----------------------------------------------------------------------------

variable "enable_prevent_destroy" {
  description = "Enable prevent_destroy lifecycle rule on critical resources. Set to true for production."
  type        = bool
  default     = true
}
