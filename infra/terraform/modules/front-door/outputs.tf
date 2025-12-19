output "front_door_id" {
  description = "ID of the Front Door profile"
  value       = azurerm_cdn_frontdoor_profile.main.id
}

output "front_door_name" {
  description = "Name of the Front Door profile"
  value       = azurerm_cdn_frontdoor_profile.main.name
}

output "publisher_endpoint_hostname" {
  description = "Hostname of the Publisher endpoint"
  value       = azurerm_cdn_frontdoor_endpoint.publisher.host_name
}

output "chain_endpoint_hostname" {
  description = "Hostname of the Chain endpoint"
  value       = azurerm_cdn_frontdoor_endpoint.chain.host_name
}

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

output "waf_policy_id" {
  description = "ID of the WAF policy (if enabled)"
  value       = var.enable_waf ? azurerm_cdn_frontdoor_firewall_policy.main[0].id : null
}

output "waf_policy_name" {
  description = "Name of the WAF policy (if enabled)"
  value       = var.enable_waf ? azurerm_cdn_frontdoor_firewall_policy.main[0].name : null
}

output "publisher_endpoint_id" {
  description = "ID of the Publisher endpoint"
  value       = azurerm_cdn_frontdoor_endpoint.publisher.id
}

output "chain_endpoint_id" {
  description = "ID of the Chain endpoint"
  value       = azurerm_cdn_frontdoor_endpoint.chain.id
}

output "dns_cname_targets" {
  description = "CNAME targets for DNS configuration"
  value = {
    publisher = azurerm_cdn_frontdoor_endpoint.publisher.host_name
    chain     = azurerm_cdn_frontdoor_endpoint.chain.host_name
  }
}

output "custom_domain_verification" {
  description = "Instructions for custom domain verification"
  value = {
    publisher = {
      domain = var.custom_domain_publisher
      cname  = azurerm_cdn_frontdoor_endpoint.publisher.host_name
    }
    chain = {
      domain = var.custom_domain_chain
      cname  = azurerm_cdn_frontdoor_endpoint.chain.host_name
    }
  }
}
