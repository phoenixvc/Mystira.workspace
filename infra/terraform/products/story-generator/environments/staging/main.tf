# =============================================================================
# Story Generator - Staging Environment
# =============================================================================
# AI-powered story generation service with Blazor WASM frontend
# =============================================================================

variable "environment" {
  type = string
}

variable "location" {
  type = string
}

variable "fallback_location" {
  type    = string
  default = "eastus2"
}

variable "resource_group_name" {
  type = string
}

variable "tags" {
  type = map(string)
}

# Module feature flags / options (passed from Terragrunt)
variable "enable_static_web_app" {
  type    = bool
  default = false
}

variable "static_web_app_sku" {
  type    = string
  default = "Free"
}

variable "enable_swa_custom_domain" {
  type    = bool
  default = false
}

variable "swa_custom_domain" {
  type    = string
  default = ""
}

variable "use_shared_postgresql" {
  type    = bool
  default = true
}

variable "use_shared_redis" {
  type    = bool
  default = true
}

variable "use_shared_log_analytics" {
  type    = bool
  default = true
}

# Shared infrastructure inputs (optional - use defaults when not provided)
variable "shared_postgresql_server_id" {
  type    = string
  default = null
}

variable "shared_redis_cache_id" {
  type    = string
  default = null
}

variable "shared_log_analytics_workspace_id" {
  type    = string
  default = null
}

variable "shared_cosmos_connection_string" {
  type      = string
  sensitive = true
  default   = null

  validation {
    condition     = var.use_shared_cosmos == false || (var.shared_cosmos_connection_string != null && trim(var.shared_cosmos_connection_string) != "")
    error_message = "shared_cosmos_connection_string must be set (non-empty) when use_shared_cosmos is true."
  }
}

variable "use_shared_cosmos" {
  type    = bool
  default = true
}

variable "cosmos_database_id" {
  type    = string
  default = "MystiraStoryGenerator"
}

variable "cosmos_container_id" {
  type    = string
  default = "StorySessions"
}

# =============================================================================
# Story Generator Module
# =============================================================================

module "story_generator" {
  source = "../../../../modules/story-generator"

  environment         = var.environment
  location            = var.location
  fallback_location   = var.fallback_location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  enable_static_web_app    = var.enable_static_web_app
  static_web_app_sku       = var.static_web_app_sku
  enable_swa_custom_domain = var.enable_swa_custom_domain
  swa_custom_domain        = var.swa_custom_domain

  use_shared_postgresql    = var.use_shared_postgresql
  use_shared_redis         = var.use_shared_redis
  use_shared_log_analytics = var.use_shared_log_analytics

  # Pass shared infrastructure references
  shared_postgresql_server_id       = var.shared_postgresql_server_id
  shared_redis_cache_id             = var.shared_redis_cache_id
  shared_log_analytics_workspace_id = var.shared_log_analytics_workspace_id

  use_shared_cosmos               = var.use_shared_cosmos
  shared_cosmos_connection_string = var.shared_cosmos_connection_string
  cosmos_database_id              = var.cosmos_database_id
  cosmos_container_id             = var.cosmos_container_id
}

# =============================================================================
# Outputs
# =============================================================================

output "static_web_app_url" {
  description = "Static Web App URL"
  value       = module.story_generator.static_web_app_url
}

output "static_web_app_hostname" {
  description = "Static Web App default hostname"
  value       = module.story_generator.static_web_app_default_hostname
}

output "key_vault_uri" {
  description = "Key Vault URI for secrets"
  value       = module.story_generator.key_vault_uri
}
