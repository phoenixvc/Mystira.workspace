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

variable "resource_group_name" {
  type = string
}

variable "tags" {
  type = map(string)
}

# Shared infrastructure inputs
variable "shared_postgresql_server_fqdn" {
  type = string
}

variable "shared_redis_connection_string" {
  type      = string
  sensitive = true
}

variable "shared_azure_ai_endpoint" {
  type = string
}

variable "shared_log_analytics_workspace_id" {
  type = string
}

variable "shared_application_insights_connection_string" {
  type      = string
  sensitive = true
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
  resource_group_name = var.resource_group_name
  tags                = var.tags

  # Pass shared infrastructure references
  postgresql_server_fqdn                 = var.shared_postgresql_server_fqdn
  redis_connection_string                = var.shared_redis_connection_string
  azure_ai_endpoint                      = var.shared_azure_ai_endpoint
  log_analytics_workspace_id             = var.shared_log_analytics_workspace_id
  application_insights_connection_string = var.shared_application_insights_connection_string
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
