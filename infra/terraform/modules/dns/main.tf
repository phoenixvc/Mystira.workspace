# Mystira DNS Infrastructure Module - Azure
# Terraform module for managing DNS zones and records for Mystira services

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "domain_name" {
  description = "Base domain name (e.g., mystira.app)"
  type        = string
  default     = "mystira.app"
}

variable "publisher_ip" {
  description = "IP address for publisher service ingress"
  type        = string
  default     = ""
}

variable "chain_ip" {
  description = "IP address for chain service ingress"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

locals {
  common_tags = merge(var.tags, {
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
    Component   = "dns"
  })
}

# DNS Zone for mystira.app
resource "azurerm_dns_zone" "main" {
  name                = var.domain_name
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# A Record for publisher
resource "azurerm_dns_a_record" "publisher" {
  count               = var.publisher_ip != "" ? 1 : 0
  name                = "publisher"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  records             = [var.publisher_ip]

  tags = local.common_tags
}

# A Record for chain
resource "azurerm_dns_a_record" "chain" {
  count               = var.chain_ip != "" ? 1 : 0
  name                = "chain"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  records             = [var.chain_ip]

  tags = local.common_tags
}

# TXT Record for domain verification
resource "azurerm_dns_txt_record" "verification" {
  name                = "@"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = "mystira-domain-verification=${var.environment}"
  }

  tags = local.common_tags
}

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
  value       = "publisher.${var.domain_name}"
}

output "chain_fqdn" {
  description = "Fully qualified domain name for chain service"
  value       = "chain.${var.domain_name}"
}
