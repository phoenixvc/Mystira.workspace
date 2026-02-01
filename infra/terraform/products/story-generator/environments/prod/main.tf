# =============================================================================
# Story Generator - Production Environment
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

# =============================================================================
# Story Generator Module
# =============================================================================
#
# PRODUCTION PROMOTION CHECKLIST:
# Before applying this configuration to production, ensure:
#
# 1. Staging Validation:
#    - [ ] Staging environment terraform apply completed successfully
#    - [ ] Staging smoke tests passed (API health, story generation flow)
#    - [ ] No configuration drift detected in staging
#
# 2. Required CI/CD Gates:
#    - [ ] infra-terragrunt-validate.yml passed for this change
#    - [ ] infra-drift-detection.yml shows no unexpected drift
#    - [ ] Manual approval obtained for production deployment
#
# 3. Rollback Plan:
#    - [ ] Previous working state identified in tfstate history
#    - [ ] Rollback procedure documented (see infra/README.md)
#
# =============================================================================

module "story_generator" {
  source = "../../../../modules/story-generator"

  environment         = var.environment
  location            = var.location
  fallback_location   = var.fallback_location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  # Pass shared infrastructure references
  shared_postgresql_server_id       = var.shared_postgresql_server_id
  shared_redis_cache_id             = var.shared_redis_cache_id
  shared_log_analytics_workspace_id = var.shared_log_analytics_workspace_id
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
