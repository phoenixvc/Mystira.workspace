# =============================================================================
# Azure Front Door Configuration for Staging Environment
# =============================================================================
#
# IMPORTANT: Staging Front Door is now handled by the shared non-prod Front Door
# deployed from the dev environment. This saves ~$150/month in Azure costs.
#
# The shared Front Door in dev handles:
# - dev.* domains (primary environment)
# - staging.* domains (secondary environment)
#
# For DNS configuration and validation tokens, use the dev environment outputs:
#   cd ../dev && terraform output front_door_staging_*
#
# To deploy staging services behind the shared Front Door:
# 1. Deploy staging infrastructure normally
# 2. The dev terraform plan/apply will create the Front Door resources for staging
# 3. Configure DNS to point staging.* domains to the shared Front Door endpoints
#
# =============================================================================

# Front Door module is NOT deployed from staging
# All Front Door resources are managed from the dev environment
# See: ../dev/front-door.tf

# If you need to see staging Front Door outputs, run:
#   cd ../dev && terraform output | grep staging

# Validation token outputs for TXT records
output "front_door_publisher_validation_token" {
  description = "Validation token for Publisher custom domain"
  value       = module.front_door.publisher_custom_domain_validation_token
  sensitive   = true
}

output "front_door_chain_validation_token" {
  description = "Validation token for Chain custom domain"
  value       = module.front_door.chain_custom_domain_validation_token
  sensitive   = true
}

output "front_door_admin_api_validation_token" {
  description = "Validation token for Admin API custom domain"
  value       = module.front_door.admin_api_custom_domain_validation_token
  sensitive   = true
}

output "front_door_admin_ui_validation_token" {
  description = "Validation token for Admin UI custom domain"
  value       = module.front_door.admin_ui_custom_domain_validation_token
  sensitive   = true
}

output "front_door_story_generator_api_validation_token" {
  description = "Validation token for Story Generator API custom domain"
  value       = module.front_door.story_generator_api_custom_domain_validation_token
  sensitive   = true
}

output "front_door_story_generator_swa_validation_token" {
  description = "Validation token for Story Generator SWA custom domain"
  value       = module.front_door.story_generator_swa_custom_domain_validation_token
  sensitive   = true
}

output "front_door_mystira_app_api_validation_token" {
  description = "Validation token for Mystira.App API custom domain"
  value       = module.front_door.mystira_app_api_custom_domain_validation_token
  sensitive   = true
}

output "front_door_mystira_app_swa_validation_token" {
  description = "Validation token for Mystira.App SWA custom domain"
  value       = module.front_door.mystira_app_swa_custom_domain_validation_token
  sensitive   = true
}

# =============================================================================
# Legacy Configuration (commented out for reference)
# =============================================================================
# If you need to switch back to a dedicated staging Front Door, uncomment the
# module below and remove the enable_secondary_environment from dev/front-door.tf
#
# module "front_door" {
#   source = "../../modules/front-door"
#
#   environment         = "staging"
#   resource_group_name = azurerm_resource_group.main.name
#   location            = var.location
#   project_name        = "mystira"
#
#   # Backend addresses (current NGINX ingress endpoints)
#   publisher_backend_address = "staging.publisher.mystira.app"
#   chain_backend_address     = "staging.chain.mystira.app"
#
#   # Custom domains
#   custom_domain_publisher = "staging.publisher.mystira.app"
#   custom_domain_chain     = "staging.chain.mystira.app"
#
#   # Admin services
#   enable_admin_services     = true
#   admin_api_backend_address = "staging.admin-api.mystira.app"
#   admin_ui_backend_address  = "staging.admin.mystira.app"
#   custom_domain_admin_api   = "staging.admin-api.mystira.app"
#   custom_domain_admin_ui    = "staging.admin.mystira.app"
#
#   # Story Generator services (API + SWA)
#   enable_story_generator              = true
#   story_generator_api_backend_address = "staging.story-api.mystira.app"
#   story_generator_swa_backend_address = module.story_generator.static_web_app_default_hostname
#   custom_domain_story_generator_api   = "staging.story-api.mystira.app"
#   custom_domain_story_generator_swa   = "staging.story.mystira.app"
#
#   # Mystira.App services (API + SWA/PWA)
#   enable_mystira_app              = true
#   mystira_app_api_backend_address = module.mystira_app.app_service_default_hostname
#   mystira_app_swa_backend_address = module.mystira_app.static_web_app_default_hostname
#   custom_domain_mystira_app_api   = "staging.api.mystira.app"
#   custom_domain_mystira_app_swa   = "staging.app.mystira.app"
#
#   # WAF Configuration - Staging Settings
#   enable_waf           = true
#   waf_mode             = "Detection" # Detection mode for staging (non-blocking)
#   rate_limit_threshold = 200         # Moderate threshold for staging
#
#   # Caching Configuration
#   enable_caching         = true
#   cache_duration_seconds = 3600 # 1 hour for staging
#
#   # Health Probes
#   health_probe_path     = "/health"
#   health_probe_interval = 30
#
#   # Session Affinity
#   session_affinity_enabled = false
#
#   tags = merge(local.common_tags, {
#     Component = "front-door"
#     Purpose   = "cdn-waf"
#   })
# }
