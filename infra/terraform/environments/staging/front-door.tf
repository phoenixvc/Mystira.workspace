# Azure Front Door Configuration for Staging Environment

module "front_door" {
  source = "../../modules/front-door"

  environment         = "staging"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = "mystira"

  # Backend addresses (current NGINX ingress endpoints)
  publisher_backend_address = "staging.publisher.mystira.app"
  chain_backend_address     = "staging.chain.mystira.app"

  # Custom domains
  custom_domain_publisher = "staging.publisher.mystira.app"
  custom_domain_chain     = "staging.chain.mystira.app"

  # Admin services
  enable_admin_services     = true
  admin_api_backend_address = "staging.admin-api.mystira.app"
  admin_ui_backend_address  = "staging.admin.mystira.app"
  custom_domain_admin_api   = "staging.admin-api.mystira.app"
  custom_domain_admin_ui    = "staging.admin.mystira.app"

  # Story Generator services (API + SWA)
  enable_story_generator              = true
  story_generator_api_backend_address = "staging.story-api.mystira.app"
  story_generator_swa_backend_address = module.story_generator.static_web_app_default_hostname
  custom_domain_story_generator_api   = "staging.story-api.mystira.app"
  custom_domain_story_generator_swa   = "staging.story.mystira.app"

  # Mystira.App services (API + SWA/PWA)
  enable_mystira_app              = true
  mystira_app_api_backend_address = module.mystira_app.app_service_default_hostname
  mystira_app_swa_backend_address = module.mystira_app.static_web_app_default_hostname
  custom_domain_mystira_app_api   = "staging.api.mystira.app"
  custom_domain_mystira_app_swa   = "staging.app.mystira.app"

  # WAF Configuration - Staging Settings
  enable_waf           = true
  waf_mode             = "Detection" # Detection mode for staging (non-blocking)
  rate_limit_threshold = 200         # Moderate threshold for staging

  # Caching Configuration
  enable_caching         = true
  cache_duration_seconds = 3600 # 1 hour for staging

  # Health Probes
  health_probe_path     = "/health"
  health_probe_interval = 30

  # Session Affinity
  session_affinity_enabled = false

  tags = merge(local.common_tags, {
    Component = "front-door"
    Purpose   = "cdn-waf"
  })
}

# Outputs for DNS configuration
output "front_door_publisher_endpoint" {
  description = "Front Door endpoint for Publisher - use this for CNAME"
  value       = module.front_door.publisher_endpoint_hostname
}

output "front_door_chain_endpoint" {
  description = "Front Door endpoint for Chain - use this for CNAME"
  value       = module.front_door.chain_endpoint_hostname
}

output "front_door_custom_domain_verification" {
  description = "Custom domain verification instructions"
  value       = module.front_door.custom_domain_verification
}

output "front_door_admin_api_endpoint" {
  description = "Front Door endpoint for Admin API - use this for CNAME"
  value       = module.front_door.admin_api_endpoint_hostname
}

output "front_door_admin_ui_endpoint" {
  description = "Front Door endpoint for Admin UI - use this for CNAME"
  value       = module.front_door.admin_ui_endpoint_hostname
}

output "front_door_story_generator_api_endpoint" {
  description = "Front Door endpoint for Story Generator API - use this for CNAME"
  value       = module.front_door.story_generator_api_endpoint_hostname
}

output "front_door_story_generator_swa_endpoint" {
  description = "Front Door endpoint for Story Generator SWA - use this for CNAME"
  value       = module.front_door.story_generator_swa_endpoint_hostname
}

output "front_door_mystira_app_api_endpoint" {
  description = "Front Door endpoint for Mystira.App API - use this for CNAME"
  value       = module.front_door.mystira_app_api_endpoint_hostname
}

output "front_door_mystira_app_swa_endpoint" {
  description = "Front Door endpoint for Mystira.App SWA - use this for CNAME"
  value       = module.front_door.mystira_app_swa_endpoint_hostname
}

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
# DNS Configuration Requirements for Custom Domains (Staging)
# =============================================================================
#
# IMPORTANT: Custom domains require proper DNS configuration before SSL certificates
# can be provisioned by Front Door. Without this, you'll see ERR_CERT_COMMON_NAME_INVALID.
#
# For each custom domain, you need:
# 1. CNAME record pointing to the Front Door endpoint
# 2. TXT record for domain validation (_dnsauth.<subdomain>)
#
# Required DNS Records for Staging Environment:
# =============================================
#
# Mystira.App (PWA + API):
# | Type  | Name                         | Value                                    |
# |-------|------------------------------|------------------------------------------|
# | CNAME | staging.app                  | <front_door_mystira_app_swa_endpoint>    |
# | TXT   | _dnsauth.staging.app         | <validation_token from terraform output> |
# | CNAME | staging.api                  | <front_door_mystira_app_api_endpoint>    |
# | TXT   | _dnsauth.staging.api         | <validation_token from terraform output> |
#
# Admin Services:
# | Type  | Name                         | Value                                    |
# |-------|------------------------------|------------------------------------------|
# | CNAME | staging.admin                | <front_door_admin_ui_endpoint>           |
# | TXT   | _dnsauth.staging.admin       | <validation_token from terraform output> |
# | CNAME | staging.admin-api            | <front_door_admin_api_endpoint>          |
# | TXT   | _dnsauth.staging.admin-api   | <validation_token from terraform output> |
#
# Story Generator:
# | Type  | Name                         | Value                                    |
# |-------|------------------------------|------------------------------------------|
# | CNAME | staging.story                | <front_door_story_generator_swa_endpoint>|
# | TXT   | _dnsauth.staging.story       | <validation_token from terraform output> |
# | CNAME | staging.story-api            | <front_door_story_generator_api_endpoint>|
# | TXT   | _dnsauth.staging.story-api   | <validation_token from terraform output> |
#
# Publisher/Chain (Legacy AKS Services):
# | Type  | Name                         | Value                                    |
# |-------|------------------------------|------------------------------------------|
# | CNAME | staging.publisher            | <front_door_publisher_endpoint>          |
# | TXT   | _dnsauth.staging.publisher   | <validation_token from terraform output> |
# | CNAME | staging.chain                | <front_door_chain_endpoint>              |
# | TXT   | _dnsauth.staging.chain       | <validation_token from terraform output> |
#
# After deploying, get validation tokens with:
#   terraform output -json | jq '.front_door_custom_domain_verification'
#
# =============================================================================
