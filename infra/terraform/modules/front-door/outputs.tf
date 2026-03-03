output "front_door_id" {
  description = "ID of the Front Door profile"
  value       = azurerm_cdn_frontdoor_profile.main.id
}

output "front_door_name" {
  description = "Name of the Front Door profile"
  value       = azurerm_cdn_frontdoor_profile.main.name
}

# =============================================================================
# Consolidated Primary Endpoint
# =============================================================================
# All primary environment services share this single endpoint.
# Traffic is routed to the correct backend based on custom domain.

output "primary_endpoint_hostname" {
  description = "Hostname of the consolidated primary endpoint (all services share this)"
  value       = azurerm_cdn_frontdoor_endpoint.primary.host_name
}

output "primary_endpoint_id" {
  description = "ID of the consolidated primary endpoint"
  value       = azurerm_cdn_frontdoor_endpoint.primary.id
}

# Legacy aliases for backward compatibility - all point to consolidated endpoint
output "publisher_endpoint_hostname" {
  description = "Hostname for Publisher service (uses consolidated primary endpoint)"
  value       = azurerm_cdn_frontdoor_endpoint.primary.host_name
}

output "chain_endpoint_hostname" {
  description = "Hostname for Chain service (uses consolidated primary endpoint)"
  value       = azurerm_cdn_frontdoor_endpoint.primary.host_name
}

output "publisher_endpoint_id" {
  description = "ID of the Publisher endpoint (consolidated primary endpoint)"
  value       = azurerm_cdn_frontdoor_endpoint.primary.id
}

output "chain_endpoint_id" {
  description = "ID of the Chain endpoint (consolidated primary endpoint)"
  value       = azurerm_cdn_frontdoor_endpoint.primary.id
}

# =============================================================================
# Custom Domain Validation Tokens
# =============================================================================

output "publisher_custom_domain_validation_token" {
  description = "Validation token for Publisher custom domain"
  value       = azurerm_cdn_frontdoor_custom_domain.publisher.validation_token
  sensitive   = true
}

output "chain_custom_domain_validation_token" {
  description = "Validation token for Chain custom domain"
  value       = azurerm_cdn_frontdoor_custom_domain.chain.validation_token
  sensitive   = true
}

# =============================================================================
# WAF Policy
# =============================================================================

output "waf_policy_id" {
  description = "ID of the WAF policy (if enabled)"
  value       = var.enable_waf ? azurerm_cdn_frontdoor_firewall_policy.main[0].id : null
}

output "waf_policy_name" {
  description = "Name of the WAF policy (if enabled)"
  value       = var.enable_waf ? azurerm_cdn_frontdoor_firewall_policy.main[0].name : null
}

# =============================================================================
# DNS Configuration
# =============================================================================

output "dns_cname_targets" {
  description = "CNAME targets for DNS configuration (all services use consolidated endpoint)"
  value = {
    publisher = azurerm_cdn_frontdoor_endpoint.primary.host_name
    chain     = azurerm_cdn_frontdoor_endpoint.primary.host_name
  }
}

output "custom_domain_verification" {
  description = "Instructions for custom domain verification"
  value = {
    publisher = {
      domain = var.custom_domain_publisher
      cname  = azurerm_cdn_frontdoor_endpoint.primary.host_name
    }
    chain = {
      domain = var.custom_domain_chain
      cname  = azurerm_cdn_frontdoor_endpoint.primary.host_name
    }
  }
}

# =============================================================================
# Admin Services Outputs (conditional)
# =============================================================================

output "admin_api_endpoint_hostname" {
  description = "Hostname for Admin API service (uses consolidated primary endpoint)"
  value       = var.enable_admin_services ? azurerm_cdn_frontdoor_endpoint.primary.host_name : null
}

output "admin_ui_endpoint_hostname" {
  description = "Hostname for Admin UI service (uses consolidated primary endpoint)"
  value       = var.enable_admin_services ? azurerm_cdn_frontdoor_endpoint.primary.host_name : null
}

output "admin_api_custom_domain_validation_token" {
  description = "Validation token for Admin API custom domain"
  value       = var.enable_admin_services ? azurerm_cdn_frontdoor_custom_domain.admin_api[0].validation_token : null
  sensitive   = true
}

output "admin_ui_custom_domain_validation_token" {
  description = "Validation token for Admin UI custom domain"
  value       = var.enable_admin_services ? azurerm_cdn_frontdoor_custom_domain.admin_ui[0].validation_token : null
  sensitive   = true
}

# =============================================================================
# Story Generator Outputs (conditional)
# =============================================================================

output "story_generator_api_endpoint_hostname" {
  description = "Hostname for Story Generator API (uses consolidated primary endpoint)"
  value       = var.enable_story_generator ? azurerm_cdn_frontdoor_endpoint.primary.host_name : null
}

output "story_generator_swa_endpoint_hostname" {
  description = "Hostname for Story Generator SWA (uses consolidated primary endpoint)"
  value       = var.enable_story_generator ? azurerm_cdn_frontdoor_endpoint.primary.host_name : null
}

output "story_generator_api_custom_domain_validation_token" {
  description = "Validation token for Story Generator API custom domain"
  value       = var.enable_story_generator ? azurerm_cdn_frontdoor_custom_domain.story_generator_api[0].validation_token : null
  sensitive   = true
}

output "story_generator_swa_custom_domain_validation_token" {
  description = "Validation token for Story Generator SWA custom domain"
  value       = var.enable_story_generator ? azurerm_cdn_frontdoor_custom_domain.story_generator_swa[0].validation_token : null
  sensitive   = true
}

# =============================================================================
# Mystira.App Outputs (conditional)
# =============================================================================

output "mystira_app_api_endpoint_hostname" {
  description = "Hostname for Mystira.App API (uses consolidated primary endpoint)"
  value       = var.enable_mystira_app ? azurerm_cdn_frontdoor_endpoint.primary.host_name : null
}

output "mystira_app_swa_endpoint_hostname" {
  description = "Hostname for Mystira.App SWA (uses consolidated primary endpoint)"
  value       = var.enable_mystira_app ? azurerm_cdn_frontdoor_endpoint.primary.host_name : null
}

output "mystira_app_api_custom_domain_validation_token" {
  description = "Validation token for Mystira.App API custom domain"
  value       = var.enable_mystira_app ? azurerm_cdn_frontdoor_custom_domain.mystira_app_api[0].validation_token : null
  sensitive   = true
}

output "mystira_app_swa_custom_domain_validation_token" {
  description = "Validation token for Mystira.App SWA custom domain"
  value       = var.enable_mystira_app ? azurerm_cdn_frontdoor_custom_domain.mystira_app_swa[0].validation_token : null
  sensitive   = true
}

# =============================================================================
# Secondary Environment Outputs (for shared non-prod Front Door)
# =============================================================================
# All secondary environment services share the consolidated secondary endpoint.

output "secondary_endpoint_hostname" {
  description = "Hostname of the consolidated secondary endpoint (all secondary services share this)"
  value       = var.enable_secondary_environment ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_endpoint_id" {
  description = "ID of the consolidated secondary endpoint"
  value       = var.enable_secondary_environment ? azurerm_cdn_frontdoor_endpoint.secondary[0].id : null
}

# Legacy aliases for backward compatibility - all point to consolidated secondary endpoint
output "secondary_publisher_endpoint_hostname" {
  description = "Hostname for secondary Publisher (uses consolidated secondary endpoint)"
  value       = var.enable_secondary_environment ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_chain_endpoint_hostname" {
  description = "Hostname for secondary Chain (uses consolidated secondary endpoint)"
  value       = var.enable_secondary_environment ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_publisher_custom_domain_validation_token" {
  description = "Validation token for secondary Publisher custom domain"
  value       = var.enable_secondary_environment ? azurerm_cdn_frontdoor_custom_domain.secondary_publisher[0].validation_token : null
  sensitive   = true
}

output "secondary_chain_custom_domain_validation_token" {
  description = "Validation token for secondary Chain custom domain"
  value       = var.enable_secondary_environment ? azurerm_cdn_frontdoor_custom_domain.secondary_chain[0].validation_token : null
  sensitive   = true
}

output "secondary_admin_api_endpoint_hostname" {
  description = "Hostname for secondary Admin API (uses consolidated secondary endpoint)"
  value       = var.enable_secondary_environment && var.enable_admin_services ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_admin_ui_endpoint_hostname" {
  description = "Hostname for secondary Admin UI (uses consolidated secondary endpoint)"
  value       = var.enable_secondary_environment && var.enable_admin_services ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_admin_api_custom_domain_validation_token" {
  description = "Validation token for secondary Admin API custom domain"
  value       = var.enable_secondary_environment && var.enable_admin_services ? azurerm_cdn_frontdoor_custom_domain.secondary_admin_api[0].validation_token : null
  sensitive   = true
}

output "secondary_admin_ui_custom_domain_validation_token" {
  description = "Validation token for secondary Admin UI custom domain"
  value       = var.enable_secondary_environment && var.enable_admin_services ? azurerm_cdn_frontdoor_custom_domain.secondary_admin_ui[0].validation_token : null
  sensitive   = true
}

output "secondary_story_generator_api_endpoint_hostname" {
  description = "Hostname for secondary Story Generator API (uses consolidated secondary endpoint)"
  value       = var.enable_secondary_environment && var.enable_story_generator ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_story_generator_swa_endpoint_hostname" {
  description = "Hostname for secondary Story Generator SWA (uses consolidated secondary endpoint)"
  value       = var.enable_secondary_environment && var.enable_story_generator ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_story_generator_api_custom_domain_validation_token" {
  description = "Validation token for secondary Story Generator API custom domain"
  value       = var.enable_secondary_environment && var.enable_story_generator ? azurerm_cdn_frontdoor_custom_domain.secondary_story_generator_api[0].validation_token : null
  sensitive   = true
}

output "secondary_story_generator_swa_custom_domain_validation_token" {
  description = "Validation token for secondary Story Generator SWA custom domain"
  value       = var.enable_secondary_environment && var.enable_story_generator ? azurerm_cdn_frontdoor_custom_domain.secondary_story_generator_swa[0].validation_token : null
  sensitive   = true
}

output "secondary_mystira_app_api_endpoint_hostname" {
  description = "Hostname for secondary Mystira.App API (uses consolidated secondary endpoint)"
  value       = var.enable_secondary_environment && var.enable_mystira_app ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_mystira_app_swa_endpoint_hostname" {
  description = "Hostname for secondary Mystira.App SWA (uses consolidated secondary endpoint)"
  value       = var.enable_secondary_environment && var.enable_mystira_app ? azurerm_cdn_frontdoor_endpoint.secondary[0].host_name : null
}

output "secondary_mystira_app_api_custom_domain_validation_token" {
  description = "Validation token for secondary Mystira.App API custom domain"
  value       = var.enable_secondary_environment && var.enable_mystira_app ? azurerm_cdn_frontdoor_custom_domain.secondary_mystira_app_api[0].validation_token : null
  sensitive   = true
}

output "secondary_mystira_app_swa_custom_domain_validation_token" {
  description = "Validation token for secondary Mystira.App SWA custom domain"
  value       = var.enable_secondary_environment && var.enable_mystira_app ? azurerm_cdn_frontdoor_custom_domain.secondary_mystira_app_swa[0].validation_token : null
  sensitive   = true
}
