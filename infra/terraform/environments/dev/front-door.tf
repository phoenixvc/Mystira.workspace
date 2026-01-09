# Azure Front Door Configuration for Non-Prod Environment (dev + staging)
# This shared Front Door handles both dev and staging traffic to save costs.

# IMPORTANT: Before enabling, ensure:
# 1. Backend services (Publisher, Chain) are deployed and healthy
# 2. DNS is properly configured
# 3. Budget is allocated (~$150/month for non-prod)

module "front_door" {
  source = "../../modules/front-door"

  # Using "nonprod" to match existing Azure resource name: mystira-nonprod-fd
  environment         = "nonprod"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = "mystira"

  # ==========================================================================
  # Primary Environment (Dev) Configuration
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
  publisher_backend_address = "dev.publisher-k8s.mystira.app"
  chain_backend_address     = "dev.chain-k8s.mystira.app"

  # Custom domains (public hostnames with CNAME to Front Door)
  custom_domain_publisher = "dev.publisher.mystira.app"
  custom_domain_chain     = "dev.chain.mystira.app"

  # Admin services
  enable_admin_services     = true
  admin_api_backend_address = "dev.admin-api-k8s.mystira.app"
  admin_ui_backend_address  = module.admin_ui.static_web_app_default_hostname # SWA instead of K8s
  custom_domain_admin_api   = "dev.admin-api.mystira.app"
  custom_domain_admin_ui    = "dev.admin.mystira.app"

  # Story Generator services (API + SWA)
  enable_story_generator              = true
  story_generator_api_backend_address = "dev.story-api-k8s.mystira.app"
  story_generator_swa_backend_address = module.story_generator.static_web_app_default_hostname
  custom_domain_story_generator_api   = "dev.story-api.mystira.app"
  custom_domain_story_generator_swa   = "dev.story.mystira.app"

  # Mystira.App services (API + SWA/PWA)
  enable_mystira_app              = true
  mystira_app_api_backend_address = module.mystira_app.app_service_default_hostname
  mystira_app_swa_backend_address = module.mystira_app.static_web_app_default_hostname
  custom_domain_mystira_app_api   = "dev.api.mystira.app"
  custom_domain_mystira_app_swa   = "dev.mystira.app"

  # ==========================================================================
  # Secondary Environment (Staging) Configuration
  # ==========================================================================
  # Enable this to also handle staging domains from this Front Door instance
  #
  # IMPORTANT: Same pattern as dev - backend addresses use "-k8s" hostnames
  # with A records, custom domains use public hostnames with CNAME to Front Door.

  enable_secondary_environment = true
  secondary_environment        = "staging"

  # Staging backend addresses (K8s backend hostnames with A records to ingress IP)
  secondary_publisher_backend_address = "staging.publisher-k8s.mystira.app"
  secondary_chain_backend_address     = "staging.chain-k8s.mystira.app"

  # Staging custom domains (public hostnames with CNAME to Front Door)
  secondary_custom_domain_publisher = "staging.publisher.mystira.app"
  secondary_custom_domain_chain     = "staging.chain.mystira.app"

  # Staging Admin services
  secondary_admin_api_backend_address = "staging.admin-api-k8s.mystira.app"
  secondary_admin_ui_backend_address  = "staging.admin-k8s.mystira.app"
  secondary_custom_domain_admin_api   = "staging.admin-api.mystira.app"
  secondary_custom_domain_admin_ui    = "staging.admin.mystira.app"

  # Staging Story Generator services
  # API uses K8s backend, SWA uses Azure hostname directly (no K8s)
  secondary_story_generator_api_backend_address = "staging.story-api-k8s.mystira.app"
  secondary_story_generator_swa_backend_address = var.staging_story_generator_swa_backend != "" ? var.staging_story_generator_swa_backend : "staging.story.mystira.app"
  secondary_custom_domain_story_generator_api   = "staging.story-api.mystira.app"
  secondary_custom_domain_story_generator_swa   = "staging.story.mystira.app"

  # Staging Mystira.App services
  # These use App Service/SWA backends directly (not K8s), so no -k8s suffix needed
  secondary_mystira_app_api_backend_address = var.staging_mystira_app_api_backend != "" ? var.staging_mystira_app_api_backend : "staging.api.mystira.app"
  secondary_mystira_app_swa_backend_address = var.staging_mystira_app_swa_backend != "" ? var.staging_mystira_app_swa_backend : "staging.app.mystira.app"
  secondary_custom_domain_mystira_app_api   = "staging.api.mystira.app"
  secondary_custom_domain_mystira_app_swa   = "staging.app.mystira.app"

  # ==========================================================================
  # WAF Configuration - Non-Production Settings
  # ==========================================================================
  enable_waf           = true
  waf_mode             = "Detection" # Use Detection mode for non-prod to avoid blocking test traffic
  rate_limit_threshold = 100         # Lower threshold for non-prod

  # Caching Configuration
  enable_caching         = true
  cache_duration_seconds = 1800 # 30 minutes for non-prod

  # Health Probes
  health_probe_path     = "/health"
  health_probe_interval = 30

  # Session Affinity
  session_affinity_enabled = false # Disabled for stateless apps

  tags = merge(local.common_tags, {
    Component   = "front-door"
    Purpose     = "cdn-waf"
    Environment = "nonprod"
    Handles     = "dev,staging"
  })
}

# =============================================================================
# Dev Environment Outputs
# =============================================================================

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

# Dev Validation token outputs
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
# Staging Environment Outputs (from shared Front Door)
# =============================================================================

output "front_door_staging_publisher_endpoint" {
  description = "Front Door endpoint for Staging Publisher - use this for CNAME"
  value       = module.front_door.secondary_publisher_endpoint_hostname
}

output "front_door_staging_chain_endpoint" {
  description = "Front Door endpoint for Staging Chain - use this for CNAME"
  value       = module.front_door.secondary_chain_endpoint_hostname
}

output "front_door_staging_admin_api_endpoint" {
  description = "Front Door endpoint for Staging Admin API - use this for CNAME"
  value       = module.front_door.secondary_admin_api_endpoint_hostname
}

output "front_door_staging_admin_ui_endpoint" {
  description = "Front Door endpoint for Staging Admin UI - use this for CNAME"
  value       = module.front_door.secondary_admin_ui_endpoint_hostname
}

output "front_door_staging_story_generator_api_endpoint" {
  description = "Front Door endpoint for Staging Story Generator API - use this for CNAME"
  value       = module.front_door.secondary_story_generator_api_endpoint_hostname
}

output "front_door_staging_story_generator_swa_endpoint" {
  description = "Front Door endpoint for Staging Story Generator SWA - use this for CNAME"
  value       = module.front_door.secondary_story_generator_swa_endpoint_hostname
}

output "front_door_staging_mystira_app_api_endpoint" {
  description = "Front Door endpoint for Staging Mystira.App API - use this for CNAME"
  value       = module.front_door.secondary_mystira_app_api_endpoint_hostname
}

output "front_door_staging_mystira_app_swa_endpoint" {
  description = "Front Door endpoint for Staging Mystira.App SWA - use this for CNAME"
  value       = module.front_door.secondary_mystira_app_swa_endpoint_hostname
}

# Staging Validation token outputs
output "front_door_staging_publisher_validation_token" {
  description = "Validation token for Staging Publisher custom domain"
  value       = module.front_door.secondary_publisher_custom_domain_validation_token
  sensitive   = true
}

output "front_door_staging_chain_validation_token" {
  description = "Validation token for Staging Chain custom domain"
  value       = module.front_door.secondary_chain_custom_domain_validation_token
  sensitive   = true
}

output "front_door_staging_admin_api_validation_token" {
  description = "Validation token for Staging Admin API custom domain"
  value       = module.front_door.secondary_admin_api_custom_domain_validation_token
  sensitive   = true
}

output "front_door_staging_admin_ui_validation_token" {
  description = "Validation token for Staging Admin UI custom domain"
  value       = module.front_door.secondary_admin_ui_custom_domain_validation_token
  sensitive   = true
}

output "front_door_staging_story_generator_api_validation_token" {
  description = "Validation token for Staging Story Generator API custom domain"
  value       = module.front_door.secondary_story_generator_api_custom_domain_validation_token
  sensitive   = true
}

output "front_door_staging_story_generator_swa_validation_token" {
  description = "Validation token for Staging Story Generator SWA custom domain"
  value       = module.front_door.secondary_story_generator_swa_custom_domain_validation_token
  sensitive   = true
}

output "front_door_staging_mystira_app_api_validation_token" {
  description = "Validation token for Staging Mystira.App API custom domain"
  value       = module.front_door.secondary_mystira_app_api_custom_domain_validation_token
  sensitive   = true
}

output "front_door_staging_mystira_app_swa_validation_token" {
  description = "Validation token for Staging Mystira.App SWA custom domain"
  value       = module.front_door.secondary_mystira_app_swa_custom_domain_validation_token
  sensitive   = true
}

# =============================================================================
# DNS Configuration Requirements for Custom Domains
# =============================================================================
#
# IMPORTANT: Custom domains require proper DNS configuration before SSL certificates
# can be provisioned by Front Door. Without this, you'll see ERR_CERT_COMMON_NAME_INVALID.
#
# For each custom domain, you need:
# 1. CNAME record pointing to the Front Door endpoint
# 2. TXT record for domain validation (_dnsauth.<subdomain>)
#
# Dev Environment DNS Records:
# ----------------------------
# | Type  | Name                    | Value                                    |
# |-------|-------------------------|------------------------------------------|
# | CNAME | dev                     | mystira-nonprod-app-swa.azurefd.net      |
# | TXT   | _dnsauth.dev            | <validation_token from terraform output> |
# | CNAME | dev.api                 | mystira-nonprod-app-api.azurefd.net      |
# | TXT   | _dnsauth.dev.api        | <validation_token from terraform output> |
# | CNAME | dev.publisher           | mystira-nonprod-publisher.azurefd.net    |
# | TXT   | _dnsauth.dev.publisher  | <validation_token from terraform output> |
# | CNAME | dev.chain               | mystira-nonprod-chain.azurefd.net        |
# | TXT   | _dnsauth.dev.chain      | <validation_token from terraform output> |
#
# Staging Environment DNS Records:
# --------------------------------
# | Type  | Name                        | Value                                        |
# |-------|-----------------------------|----------------------------------------------|
# | CNAME | staging.app                 | mystira-staging-app-swa.azurefd.net          |
# | TXT   | _dnsauth.staging.app        | <validation_token from terraform output>     |
# | CNAME | staging.api                 | mystira-staging-app-api.azurefd.net          |
# | TXT   | _dnsauth.staging.api        | <validation_token from terraform output>     |
# | CNAME | staging.publisher           | mystira-staging-publisher.azurefd.net        |
# | TXT   | _dnsauth.staging.publisher  | <validation_token from terraform output>     |
# | CNAME | staging.chain               | mystira-staging-chain.azurefd.net            |
# | TXT   | _dnsauth.staging.chain      | <validation_token from terraform output>     |
#
# After deploying, run: terraform output -json | jq '.front_door_mystira_app_swa_custom_domain_validation_token'
# to get the validation tokens.
#
# =============================================================================
