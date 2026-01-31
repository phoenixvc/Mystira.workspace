# Mystira DNS Infrastructure Module - Azure
# Terraform module for managing DNS zones and records for Mystira services
#
# Related files:
#   - variables.tf: Input variable definitions
#   - outputs.tf: Output value definitions

terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"  # 4.x required for .NET 9.0 support
    }
  }
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
  mystira_app_api_subdomain = var.environment == "prod" ? "api" : "${var.environment}.api"
  # Admin subdomains (dev.admin.mystira.app for dev, admin.mystira.app for prod)
  admin_api_subdomain = var.environment == "prod" ? "admin-api" : "${var.environment}.admin-api"
  admin_ui_subdomain  = var.environment == "prod" ? "admin" : "${var.environment}.admin"
  # Story Generator subdomains (dev.story.mystira.app for dev, story.mystira.app for prod)
  story_api_subdomain = var.environment == "prod" ? "story-api" : "${var.environment}.story-api"
  story_swa_subdomain = var.environment == "prod" ? "story" : "${var.environment}.story"
}

# DNS Zone for mystira.app
resource "azurerm_dns_zone" "main" {
  name                = var.domain_name
  resource_group_name = var.resource_group_name

  tags = local.common_tags

  lifecycle {
    prevent_destroy = true
  }
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

# =============================================================================
# Admin Services DNS Records
# =============================================================================

# CNAME Record for Admin API (when using Front Door)
resource "azurerm_dns_cname_record" "admin_api_fd" {
  count               = var.use_front_door && var.front_door_admin_api_endpoint != "" ? 1 : 0
  name                = local.admin_api_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_admin_api_endpoint

  tags = local.common_tags
}

# CNAME Record for Admin UI (when using Front Door)
resource "azurerm_dns_cname_record" "admin_ui_fd" {
  count               = var.use_front_door && var.front_door_admin_ui_endpoint != "" ? 1 : 0
  name                = local.admin_ui_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_admin_ui_endpoint

  tags = local.common_tags
}

# TXT Record for Front Door Admin API domain validation
resource "azurerm_dns_txt_record" "admin_api_fd_validation" {
  count               = var.front_door_admin_api_validation_token != "" ? 1 : 0
  name                = "_dnsauth.${local.admin_api_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_admin_api_validation_token
  }

  tags = local.common_tags
}

# TXT Record for Front Door Admin UI domain validation
resource "azurerm_dns_txt_record" "admin_ui_fd_validation" {
  count               = var.front_door_admin_ui_validation_token != "" ? 1 : 0
  name                = "_dnsauth.${local.admin_ui_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_admin_ui_validation_token
  }

  tags = local.common_tags
}

# =============================================================================
# Story Generator DNS Records
# =============================================================================

# CNAME Record for Story Generator API (when using Front Door)
resource "azurerm_dns_cname_record" "story_api_fd" {
  count               = var.use_front_door && var.front_door_story_api_endpoint != "" ? 1 : 0
  name                = local.story_api_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_story_api_endpoint

  tags = local.common_tags
}

# CNAME Record for Story Generator SWA (when using Front Door)
resource "azurerm_dns_cname_record" "story_swa_fd" {
  count               = var.use_front_door && var.front_door_story_swa_endpoint != "" ? 1 : 0
  name                = local.story_swa_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_story_swa_endpoint

  tags = local.common_tags
}

# TXT Record for Front Door Story Generator API domain validation
resource "azurerm_dns_txt_record" "story_api_fd_validation" {
  count               = var.front_door_story_api_validation_token != "" ? 1 : 0
  name                = "_dnsauth.${local.story_api_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_story_api_validation_token
  }

  tags = local.common_tags
}

# TXT Record for Front Door Story Generator SWA domain validation
resource "azurerm_dns_txt_record" "story_swa_fd_validation" {
  count               = var.front_door_story_swa_validation_token != "" ? 1 : 0
  name                = "_dnsauth.${local.story_swa_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_story_swa_validation_token
  }

  tags = local.common_tags
}
