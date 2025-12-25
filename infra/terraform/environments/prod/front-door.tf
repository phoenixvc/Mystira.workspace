# Azure Front Door Example Configuration for Production Environment
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

  # Backend addresses (current NGINX ingress endpoints)
  publisher_backend_address = "publisher.mystira.app"
  chain_backend_address     = "chain.mystira.app"

  # Custom domains
  custom_domain_publisher = "publisher.mystira.app"
  custom_domain_chain     = "chain.mystira.app"

  # Admin services
  enable_admin_services     = true
  admin_api_backend_address = "admin-api.mystira.app"
  admin_ui_backend_address  = "admin.mystira.app"
  custom_domain_admin_api   = "admin-api.mystira.app"
  custom_domain_admin_ui    = "admin.mystira.app"

  # Story Generator services (API + SWA)
  enable_story_generator              = true
  story_generator_api_backend_address = "story-api.mystira.app"
  story_generator_swa_backend_address = module.story_generator.static_web_app_default_hostname
  custom_domain_story_generator_api   = "story-api.mystira.app"
  custom_domain_story_generator_swa   = "story.mystira.app"

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

# After deploying, update DNS with:
# 1. Change publisher.mystira.app A record to CNAME pointing to Front Door endpoint
# 2. Change chain.mystira.app A record to CNAME pointing to Front Door endpoint
# 3. Add _dnsauth TXT records for domain validation
#
# Example DNS changes:
# Before: publisher.mystira.app -> A record -> <NGINX LB IP>
# After:  publisher.mystira.app -> CNAME -> mystira-prod-publisher.azurefd.net
