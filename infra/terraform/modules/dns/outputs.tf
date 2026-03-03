# Mystira DNS Infrastructure Module - Outputs
# Module: infra/terraform/modules/dns
# This file contains all output definitions for the DNS module.

output "dns_zone_id" {
  description = "DNS Zone ID"
  value       = azurerm_dns_zone.main.id
}

output "dns_zone_name" {
  description = "DNS Zone name"
  value       = azurerm_dns_zone.main.name
}

output "name_servers" {
  description = "Name servers for the DNS zone"
  value       = azurerm_dns_zone.main.name_servers
}

output "publisher_fqdn" {
  description = "Fully qualified domain name for publisher service"
  value       = "${local.publisher_subdomain}.${var.domain_name}"
}

output "chain_fqdn" {
  description = "Fully qualified domain name for chain service"
  value       = "${local.chain_subdomain}.${var.domain_name}"
}

output "mystira_app_swa_fqdn" {
  description = "Fully qualified domain name for Mystira.App SWA/PWA"
  value       = var.environment == "prod" ? var.domain_name : "${local.mystira_app_swa_subdomain}.${var.domain_name}"
}

output "mystira_app_api_fqdn" {
  description = "Fully qualified domain name for Mystira.App API"
  value       = "${local.mystira_app_api_subdomain}.${var.domain_name}"
}

output "admin_api_fqdn" {
  description = "Fully qualified domain name for Admin API"
  value       = "${local.admin_api_subdomain}.${var.domain_name}"
}

output "admin_ui_fqdn" {
  description = "Fully qualified domain name for Admin UI"
  value       = "${local.admin_ui_subdomain}.${var.domain_name}"
}

output "story_api_fqdn" {
  description = "Fully qualified domain name for Story Generator API"
  value       = "${local.story_api_subdomain}.${var.domain_name}"
}

output "story_swa_fqdn" {
  description = "Fully qualified domain name for Story Generator SWA"
  value       = "${local.story_swa_subdomain}.${var.domain_name}"
}
