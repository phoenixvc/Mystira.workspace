# Azure Front Door Configuration for Production Environment
# Rename this file to front-door.tf to enable Front Door

# IMPORTANT: Before enabling, ensure:
# 1. Backend services (Publisher, Chain) are deployed and healthy
# 2. DNS is properly configured
# 3. Budget is allocated (~$200-300/month for production)
# 4. Test in dev/staging first!

module "front_door" {
  source = "../../modules/front-door"

  environment         = "prod"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = "mystira"

  # ==========================================================================
  # Production Configuration
  # ==========================================================================
  #
  # IMPORTANT: Backend addresses use the "-k8s" hostnames which have A records
  # pointing to K8s ingress. Custom domains use the public hostnames which have
  # CNAME records pointing to Front Door. This separation is required because:
  # - Front Door custom domains need CNAME -> Front Door endpoint
  # - Front Door backends need to reach the actual K8s services (A record -> IP)
  # - You cannot have both A and CNAME for the same hostname
  # ==========================================================================

  # Backend addresses (K8s backend hostnames with A records to ingress IP)
  publisher_backend_address = "publisher-k8s.mystira.app"
  chain_backend_address     = "chain-k8s.mystira.app"

  # Custom domains (public hostnames with CNAME to Front Door)
  custom_domain_publisher = "publisher.mystira.app"
  custom_domain_chain     = "chain.mystira.app"

  # Admin services
  enable_admin_services     = true
  admin_api_backend_address = "admin-api-k8s.mystira.app"
  admin_ui_backend_address  = "admin-k8s.mystira.app"
  custom_domain_admin_api   = "admin-api.mystira.app"
  custom_domain_admin_ui    = "admin.mystira.app"

  # Story Generator services (API + SWA)
  enable_story_generator              = true
  story_generator_api_backend_address = "story-api-k8s.mystira.app"
  story_generator_swa_backend_address = module.story_generator.static_web_app_default_hostname
  custom_domain_story_generator_api   = "story-api.mystira.app"
  custom_domain_story_generator_swa   = "story.mystira.app"

  # Mystira.App services (API + SWA/PWA)
  enable_mystira_app              = true
  mystira_app_api_backend_address = module.mystira_app.app_service_default_hostname
  mystira_app_swa_backend_address = module.mystira_app.static_web_app_default_hostname
  custom_domain_mystira_app_api   = "api.mystira.app"
  custom_domain_mystira_app_swa   = "app.mystira.app"

  # WAF Configuration - PRODUCTION SETTINGS
  enable_waf           = true
  waf_mode             = "Prevention" # BLOCK malicious traffic in production
  rate_limit_threshold = 500          # Higher threshold for production traffic

  # Caching Configuration
  enable_caching         = true
  cache_duration_seconds = 7200 # 2 hours for production

  # Health Probes
  health_probe_path     = "/health"
  health_probe_interval = 30

  # Session Affinity
  session_affinity_enabled = false # Disabled for stateless apps

  tags = merge(local.common_tags, {
    Component   = "front-door"
    Purpose     = "cdn-waf"
    Environment = "prod"
    CostCenter  = "Infrastructure"
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

output "front_door_waf_policy_id" {
  description = "WAF policy ID for monitoring"
  value       = module.front_door.waf_policy_id
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
# DNS Configuration Requirements for Custom Domains (Production)
# =============================================================================
#
# IMPORTANT: Custom domains require proper DNS configuration before SSL certificates
# can be provisioned by Front Door. Without this, you'll see ERR_CERT_COMMON_NAME_INVALID.
#
# For each custom domain, you need:
# 1. CNAME record pointing to the Front Door endpoint
# 2. TXT record for domain validation (_dnsauth.<subdomain>)
#
# Required DNS Records for Production Environment:
# ================================================
#
# Mystira.App (PWA + API):
# | Type  | Name              | Value                                    |
# |-------|-------------------|------------------------------------------|
# | CNAME | app               | <front_door_mystira_app_swa_endpoint>    |
# | TXT   | _dnsauth.app      | <validation_token from terraform output> |
# | CNAME | api               | <front_door_mystira_app_api_endpoint>    |
# | TXT   | _dnsauth.api      | <validation_token from terraform output> |
#
# Admin Services:
# | Type  | Name              | Value                                    |
# |-------|-------------------|------------------------------------------|
# | CNAME | admin             | <front_door_admin_ui_endpoint>           |
# | TXT   | _dnsauth.admin    | <validation_token from terraform output> |
# | CNAME | admin-api         | <front_door_admin_api_endpoint>          |
# | TXT   | _dnsauth.admin-api| <validation_token from terraform output> |
#
# Story Generator:
# | Type  | Name              | Value                                    |
# |-------|-------------------|------------------------------------------|
# | CNAME | story             | <front_door_story_generator_swa_endpoint>|
# | TXT   | _dnsauth.story    | <validation_token from terraform output> |
# | CNAME | story-api         | <front_door_story_generator_api_endpoint>|
# | TXT   | _dnsauth.story-api| <validation_token from terraform output> |
#
# Publisher/Chain (Legacy AKS Services):
# | Type  | Name              | Value                                    |
# |-------|-------------------|------------------------------------------|
# | CNAME | publisher         | <front_door_publisher_endpoint>          |
# | TXT   | _dnsauth.publisher| <validation_token from terraform output> |
# | CNAME | chain             | <front_door_chain_endpoint>              |
# | TXT   | _dnsauth.chain    | <validation_token from terraform output> |
#
# After deploying, get validation tokens with:
#   terraform output -json | jq '.front_door_custom_domain_verification'
#
# Example DNS Migration:
# Before: publisher.mystira.app -> A record -> <NGINX LB IP>
# After:  publisher.mystira.app -> CNAME -> mystira-prod-publisher.azurefd.net
#
# Before: api.mystira.app -> CNAME -> mys-prod-app-api-san.azurewebsites.net
# After:  api.mystira.app -> CNAME -> mystira-prod-app-api.azurefd.net
#
# =============================================================================
