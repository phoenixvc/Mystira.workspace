# =============================================================================
# DNS Zone and Records for Dev Environment
# Creates DNS zone in dev core-rg (shared across all environments)
#
# Two-step deployment:
#   1. terraform apply                                    (creates DNS zone + records)
#   2. terraform apply -var="bind_custom_domains=true"    (binds custom domains)
# =============================================================================

variable "bind_custom_domains" {
  description = "Set to true to bind custom domains (run after DNS propagates)"
  type        = bool
  default     = false
}

# DNS Zone for mystira.app (created in dev, shared by all environments)
resource "azurerm_dns_zone" "mystira" {
  name                = "mystira.app"
  resource_group_name = azurerm_resource_group.main.name

  tags = local.common_tags
}

# =============================================================================
# Mystira.App SWA DNS Records
# =============================================================================

# CNAME record for dev.mystira.app -> SWA
resource "azurerm_dns_cname_record" "dev_app_swa" {
  name                = "dev"
  zone_name           = azurerm_dns_zone.mystira.name
  resource_group_name = azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.mystira_app.static_web_app_default_hostname

  tags = local.common_tags
}

# Custom domain binding for SWA (only when bind_custom_domains=true)
resource "azurerm_static_web_app_custom_domain" "dev_app" {
  count = var.bind_custom_domains ? 1 : 0

  static_web_app_id = module.mystira_app.static_web_app_id
  domain_name       = "dev.mystira.app"
  validation_type   = "cname-delegation"

  depends_on = [azurerm_dns_cname_record.dev_app_swa]
}

# =============================================================================
# Mystira.App API DNS Records
# =============================================================================

# CNAME record for dev.api.mystira.app -> App Service
resource "azurerm_dns_cname_record" "dev_api" {
  name                = "dev.api"
  zone_name           = azurerm_dns_zone.mystira.name
  resource_group_name = azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.mystira_app.app_service_default_hostname

  tags = local.common_tags
}

# TXT record for App Service domain verification
resource "azurerm_dns_txt_record" "dev_api_verification" {
  name                = "asuid.dev.api"
  zone_name           = azurerm_dns_zone.mystira.name
  resource_group_name = azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300

  record {
    value = module.mystira_app.app_service_custom_domain_verification_id
  }

  tags = local.common_tags
}

# Custom hostname binding for App Service (only when bind_custom_domains=true)
resource "azurerm_app_service_custom_hostname_binding" "dev_api" {
  count = var.bind_custom_domains ? 1 : 0

  hostname            = "dev.api.mystira.app"
  app_service_name    = module.mystira_app.app_service_name
  resource_group_name = azurerm_resource_group.app.name

  depends_on = [
    azurerm_dns_cname_record.dev_api,
    azurerm_dns_txt_record.dev_api_verification
  ]
}

# Free managed SSL certificate for App Service API
resource "azurerm_app_service_managed_certificate" "dev_api" {
  count = var.bind_custom_domains ? 1 : 0

  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.dev_api[0].id
}

# Bind the certificate to the hostname
resource "azurerm_app_service_certificate_binding" "dev_api" {
  count = var.bind_custom_domains ? 1 : 0

  hostname_binding_id = azurerm_app_service_custom_hostname_binding.dev_api[0].id
  certificate_id      = azurerm_app_service_managed_certificate.dev_api[0].id
  ssl_state           = "SniEnabled"
}

# =============================================================================
# Outputs
# =============================================================================

output "dns_zone_name_servers" {
  description = "Name servers for mystira.app DNS zone - update your domain registrar"
  value       = azurerm_dns_zone.mystira.name_servers
}

output "next_step" {
  description = "Instructions for binding custom domains"
  value       = var.bind_custom_domains ? "Custom domains bound!" : "Run: terraform apply -var='bind_custom_domains=true'"
}
