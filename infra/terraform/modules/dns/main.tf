# Mystira DNS Infrastructure Module - Azure
# Terraform module for managing DNS zones and records for Mystira services

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"  # 4.x required for .NET 9.0 support
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

# Front Door Configuration
variable "use_front_door" {
  description = "Use Azure Front Door (CNAME) instead of direct Load Balancer (A record)"
  type        = bool
  default     = false
}

variable "front_door_publisher_endpoint" {
  description = "Front Door publisher endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_chain_endpoint" {
  description = "Front Door chain endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_publisher_validation_token" {
  description = "Front Door publisher custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

variable "front_door_chain_validation_token" {
  description = "Front Door chain custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

# Mystira.App Front Door Configuration
variable "front_door_mystira_app_swa_endpoint" {
  description = "Front Door Mystira.App SWA endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_mystira_app_api_endpoint" {
  description = "Front Door Mystira.App API endpoint hostname (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_mystira_app_swa_validation_token" {
  description = "Front Door Mystira.App SWA custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
}

variable "front_door_mystira_app_api_validation_token" {
  description = "Front Door Mystira.App API custom domain validation token"
  type        = string
  default     = ""
  sensitive   = true
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
  # Use environment prefix for non-prod environments
  publisher_subdomain = var.environment == "prod" ? "publisher" : "${var.environment}.publisher"
  chain_subdomain     = var.environment == "prod" ? "chain" : "${var.environment}.chain"
  # Mystira.App subdomains (dev.mystira.app for dev, mystira.app for prod)
  mystira_app_swa_subdomain = var.environment == "prod" ? "@" : var.environment
  mystira_app_api_subdomain = var.environment == "prod" ? "api" : "api.${var.environment}"
}

# DNS Zone for mystira.app
resource "azurerm_dns_zone" "main" {
  name                = var.domain_name
  resource_group_name = var.resource_group_name

  tags = local.common_tags
}

# A Record for publisher (when NOT using Front Door)
resource "azurerm_dns_a_record" "publisher" {
  count               = !var.use_front_door && var.publisher_ip != "" ? 1 : 0
  name                = local.publisher_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  records             = [var.publisher_ip]

  tags = local.common_tags
}

# A Record for chain (when NOT using Front Door)
resource "azurerm_dns_a_record" "chain" {
  count               = !var.use_front_door && var.chain_ip != "" ? 1 : 0
  name                = local.chain_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  records             = [var.chain_ip]

  tags = local.common_tags
}

# CNAME Record for publisher (when using Front Door)
resource "azurerm_dns_cname_record" "publisher_fd" {
  count               = var.use_front_door && var.front_door_publisher_endpoint != "" ? 1 : 0
  name                = local.publisher_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_publisher_endpoint

  tags = local.common_tags
}

# CNAME Record for chain (when using Front Door)
resource "azurerm_dns_cname_record" "chain_fd" {
  count               = var.use_front_door && var.front_door_chain_endpoint != "" ? 1 : 0
  name                = local.chain_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_chain_endpoint

  tags = local.common_tags
}

# TXT Record for Front Door publisher domain validation
resource "azurerm_dns_txt_record" "publisher_fd_validation" {
  count               = var.front_door_publisher_validation_token != "" ? 1 : 0
  name                = "_dnsauth.${local.publisher_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_publisher_validation_token
  }

  tags = local.common_tags
}

# TXT Record for Front Door chain domain validation
resource "azurerm_dns_txt_record" "chain_fd_validation" {
  count               = var.front_door_chain_validation_token != "" ? 1 : 0
  name                = "_dnsauth.${local.chain_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_chain_validation_token
  }

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

# =============================================================================
# Mystira.App DNS Records (SWA/PWA and API)
# =============================================================================

# CNAME Record for Mystira.App SWA (when using Front Door)
# For prod: @ record (apex domain) - requires special handling
# For dev/staging: dev.mystira.app, staging.mystira.app
resource "azurerm_dns_cname_record" "mystira_app_swa_fd" {
  count               = var.use_front_door && var.front_door_mystira_app_swa_endpoint != "" && var.environment != "prod" ? 1 : 0
  name                = local.mystira_app_swa_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_mystira_app_swa_endpoint

  tags = local.common_tags
}

# CNAME Record for Mystira.App API (when using Front Door)
resource "azurerm_dns_cname_record" "mystira_app_api_fd" {
  count               = var.use_front_door && var.front_door_mystira_app_api_endpoint != "" ? 1 : 0
  name                = local.mystira_app_api_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_mystira_app_api_endpoint

  tags = local.common_tags
}

# TXT Record for Front Door Mystira.App SWA domain validation
resource "azurerm_dns_txt_record" "mystira_app_swa_fd_validation" {
  count               = var.front_door_mystira_app_swa_validation_token != "" ? 1 : 0
  name                = var.environment == "prod" ? "_dnsauth" : "_dnsauth.${local.mystira_app_swa_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_mystira_app_swa_validation_token
  }

  tags = local.common_tags
}

# TXT Record for Front Door Mystira.App API domain validation
resource "azurerm_dns_txt_record" "mystira_app_api_fd_validation" {
  count               = var.front_door_mystira_app_api_validation_token != "" ? 1 : 0
  name                = "_dnsauth.${local.mystira_app_api_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_mystira_app_api_validation_token
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
