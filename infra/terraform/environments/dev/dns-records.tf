# =============================================================================
# DNS Records for Dev Environment
# References existing DNS zone (created by prod) and adds dev-specific records
# =============================================================================

# Reference the existing DNS zone (managed by prod terraform)
data "azurerm_dns_zone" "mystira" {
  name                = "mystira.app"
  resource_group_name = "mys-prod-core-rg-san"  # DNS zone is in prod
}

# =============================================================================
# Mystira.App SWA DNS Records
# =============================================================================

# CNAME record for dev.mystira.app -> SWA
resource "azurerm_dns_cname_record" "dev_app_swa" {
  name                = "dev"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.mystira_app.static_web_app_default_hostname

  tags = local.common_tags
}

# Custom domain binding for SWA (depends on DNS record)
resource "azurerm_static_web_app_custom_domain" "dev_app" {
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
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.mystira_app.app_service_default_hostname

  tags = local.common_tags
}

# TXT record for App Service domain verification
resource "azurerm_dns_txt_record" "dev_api_verification" {
  name                = "asuid.dev.api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300

  record {
    value = module.mystira_app.app_service_custom_domain_verification_id
  }

  tags = local.common_tags
}

# Custom hostname binding for App Service (depends on DNS records)
resource "azurerm_app_service_custom_hostname_binding" "dev_api" {
  hostname            = "dev.api.mystira.app"
  app_service_name    = module.mystira_app.app_service_name
  resource_group_name = azurerm_resource_group.app.name

  depends_on = [
    azurerm_dns_cname_record.dev_api,
    azurerm_dns_txt_record.dev_api_verification
  ]
}
