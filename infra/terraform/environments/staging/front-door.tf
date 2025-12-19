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

  # WAF Configuration - Staging Settings
  enable_waf           = true
  waf_mode             = "Detection"  # Detection mode for staging (non-blocking)
  rate_limit_threshold = 200          # Moderate threshold for staging

  # Caching Configuration
  enable_caching         = true
  cache_duration_seconds = 3600  # 1 hour for staging

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
