# Azure Front Door Example Configuration for Dev Environment
# Rename this file to front-door.tf to enable Front Door

# IMPORTANT: Before enabling, ensure:
# 1. Backend services (Publisher, Chain) are deployed and healthy
# 2. DNS is properly configured
# 3. Budget is allocated (~$150/month for dev)

module "front_door" {
  source = "../../modules/front-door"

  environment         = "dev"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = "mystira"

  # Backend addresses (current NGINX ingress endpoints)
  publisher_backend_address = "dev.publisher.mystira.app"
  chain_backend_address     = "dev.chain.mystira.app"

  # Custom domains (same as backend for dev)
  custom_domain_publisher = "dev.publisher.mystira.app"
  custom_domain_chain     = "dev.chain.mystira.app"

  # WAF Configuration
  enable_waf           = true
  waf_mode             = "Detection"  # Use Detection mode for dev to avoid blocking legitimate test traffic
  rate_limit_threshold = 100          # Lower threshold for dev

  # Caching Configuration
  enable_caching         = true
  cache_duration_seconds = 1800  # 30 minutes for dev

  # Health Probes
  health_probe_path     = "/health"
  health_probe_interval = 30

  # Session Affinity
  session_affinity_enabled = false  # Disabled for stateless apps

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

# After deploying, update DNS with:
# 1. Change dev.publisher.mystira.app A record to CNAME pointing to Front Door endpoint
# 2. Change dev.chain.mystira.app A record to CNAME pointing to Front Door endpoint
# 3. Add _dnsauth TXT records for domain validation
#
# Example DNS changes:
# Before: dev.publisher.mystira.app -> A record -> <NGINX LB IP>
# After:  dev.publisher.mystira.app -> CNAME -> mystira-dev-publisher.azurefd.net
