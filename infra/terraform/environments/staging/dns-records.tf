# =============================================================================
# DNS Records for Staging Environment
# References existing DNS zone in shared terraform RG (managed by CI/CD bootstrap)
# =============================================================================
# NOTE: Front Door CNAME and TXT validation records are managed in the dev
# environment terraform since the shared non-prod Front Door is defined there.

variable "bind_custom_domains" {
  description = "Set to true to bind custom domains (run after DNS propagates)"
  type        = bool
  default     = false
}

variable "k8s_ingress_ip" {
  description = "Kubernetes NGINX ingress controller external IP (get with: kubectl get svc -n ingress-nginx)"
  type        = string
  default     = ""  # Set after AKS is deployed
}

# Reference existing DNS Zone (created by CI/CD bootstrap in shared terraform RG)
data "azurerm_dns_zone" "mystira" {
  name                = "mystira.app"
  resource_group_name = "mys-shared-terraform-rg-san"
}

# =============================================================================
# Mystira.App SWA DNS Records (staging.mystira.app)
# =============================================================================

# CNAME record for staging.mystira.app -> SWA
resource "azurerm_dns_cname_record" "staging_app_swa" {
  name                = "staging"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.mystira_app.static_web_app_default_hostname

  tags = local.common_tags
}

# Custom domain binding for SWA (only when bind_custom_domains=true)
resource "azurerm_static_web_app_custom_domain" "staging_app" {
  count = var.bind_custom_domains ? 1 : 0

  static_web_app_id = module.mystira_app.static_web_app_id
  domain_name       = "staging.mystira.app"
  validation_type   = "cname-delegation"

  depends_on = [azurerm_dns_cname_record.staging_app_swa]
}

# =============================================================================
# Mystira.App API DNS Records (staging.api.mystira.app)
# =============================================================================

# CNAME record for staging.api.mystira.app -> App Service
resource "azurerm_dns_cname_record" "staging_api" {
  name                = "staging.api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.mystira_app.app_service_default_hostname

  tags = local.common_tags
}

# TXT record for App Service domain verification
resource "azurerm_dns_txt_record" "staging_api_verification" {
  name                = "asuid.staging.api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300

  record {
    value = module.mystira_app.app_service_custom_domain_verification_id
  }

  tags = local.common_tags
}

# Custom hostname binding for App Service (only when bind_custom_domains=true)
resource "azurerm_app_service_custom_hostname_binding" "staging_api" {
  count = var.bind_custom_domains ? 1 : 0

  hostname            = "staging.api.mystira.app"
  app_service_name    = module.mystira_app.app_service_name
  resource_group_name = azurerm_resource_group.app.name

  depends_on = [
    azurerm_dns_cname_record.staging_api,
    azurerm_dns_txt_record.staging_api_verification
  ]
}

# Free managed SSL certificate for App Service API
resource "azurerm_app_service_managed_certificate" "staging_api" {
  count = var.bind_custom_domains ? 1 : 0

  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.staging_api[0].id
}

# Bind the certificate to the hostname
resource "azurerm_app_service_certificate_binding" "staging_api" {
  count = var.bind_custom_domains ? 1 : 0

  hostname_binding_id = azurerm_app_service_custom_hostname_binding.staging_api[0].id
  certificate_id      = azurerm_app_service_managed_certificate.staging_api[0].id
  ssl_state           = "SniEnabled"
}

# =============================================================================
# Admin Services DNS Records
# =============================================================================
# NOTE: Admin services now route through the shared non-prod Front Door.
# The CNAME record is created in the K8s DNS section below.
# Direct App Service verification not needed when using Front Door.

# =============================================================================
# Story Generator DNS Records
# =============================================================================

# CNAME record for staging.story.mystira.app -> Story Generator SWA
resource "azurerm_dns_cname_record" "staging_story_swa" {
  name                = "staging.story"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.story_generator.static_web_app_default_hostname

  tags = local.common_tags
}

# Custom domain binding for Story Generator SWA
resource "azurerm_static_web_app_custom_domain" "staging_story" {
  count = var.bind_custom_domains ? 1 : 0

  static_web_app_id = module.story_generator.static_web_app_id
  domain_name       = "staging.story.mystira.app"
  validation_type   = "cname-delegation"

  depends_on = [azurerm_dns_cname_record.staging_story_swa]
}

# NOTE: staging.story-api CNAME now points to Front Door (defined in K8s DNS section below)
# Direct App Service verification not needed when using Front Door.

# =============================================================================
# Kubernetes Services DNS Records
#
# ARCHITECTURE:
# - Backend A records (*-k8s.mystira.app) -> K8s ingress IP (for Front Door origins)
# - Public CNAME records (*.mystira.app) -> Front Door endpoint (for custom domains)
#
# The shared non-prod Front Door (managed in dev environment) routes traffic.
# =============================================================================

# -----------------------------------------------------------------------------
# BACKEND A RECORDS (for Front Door to reach K8s)
# -----------------------------------------------------------------------------

# Backend A record for Publisher service
resource "azurerm_dns_a_record" "staging_publisher_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "staging.publisher-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# Backend A record for Chain service
resource "azurerm_dns_a_record" "staging_chain_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "staging.chain-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# Backend A record for Admin API service
resource "azurerm_dns_a_record" "staging_admin_api_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "staging.admin-api-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# Backend A record for Admin UI service
resource "azurerm_dns_a_record" "staging_admin_ui_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "staging.admin-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# Backend A record for Story API service
resource "azurerm_dns_a_record" "staging_story_api_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "staging.story-api-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# =============================================================================
# Front Door CNAME Records
# =============================================================================
# Staging uses the shared non-prod Front Door deployed from dev environment.
# These CNAMEs point staging hostnames to the dev Front Door endpoint.

# Reference the non-prod Front Door (must exist before staging can use it)
data "azurerm_cdn_frontdoor_profile" "nonprod" {
  name                = "mystira-nonprod-fd"
  resource_group_name = "mys-dev-core-rg-san"
}

data "azurerm_cdn_frontdoor_endpoint" "nonprod_primary" {
  name                     = "mystira-nonprod"
  profile_name             = data.azurerm_cdn_frontdoor_profile.nonprod.name
  resource_group_name      = "mys-dev-core-rg-san"
}

# CNAME for staging.publisher.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "staging_publisher_fd" {
  name                = "staging.publisher"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = data.azurerm_cdn_frontdoor_endpoint.nonprod_primary.host_name

  tags = local.common_tags
}

# CNAME for staging.chain.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "staging_chain_fd" {
  name                = "staging.chain"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = data.azurerm_cdn_frontdoor_endpoint.nonprod_primary.host_name

  tags = local.common_tags
}

# CNAME for staging.admin-api.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "staging_admin_api_fd" {
  name                = "staging.admin-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = data.azurerm_cdn_frontdoor_endpoint.nonprod_primary.host_name

  tags = local.common_tags
}

# CNAME for staging.admin.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "staging_admin_ui_fd" {
  name                = "staging.admin"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = data.azurerm_cdn_frontdoor_endpoint.nonprod_primary.host_name

  tags = local.common_tags
}

# CNAME for staging.story-api.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "staging_story_api_fd" {
  name                = "staging.story-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = data.azurerm_cdn_frontdoor_endpoint.nonprod_primary.host_name

  tags = local.common_tags
}

# =============================================================================
# Outputs
# =============================================================================

output "dns_zone_name_servers" {
  description = "Name servers for mystira.app DNS zone"
  value       = data.azurerm_dns_zone.mystira.name_servers
}

output "staging_dns_records_created" {
  description = "List of DNS records created for staging"
  value = {
    app         = "staging.mystira.app"
    api         = "staging.api.mystira.app"
    admin       = "staging.admin.mystira.app"
    story       = "staging.story.mystira.app"
    story_api   = "staging.story-api.mystira.app"
    publisher   = var.k8s_ingress_ip != "" ? "staging.publisher.mystira.app" : "Not created (no k8s_ingress_ip)"
    chain       = var.k8s_ingress_ip != "" ? "staging.chain.mystira.app" : "Not created (no k8s_ingress_ip)"
  }
}

output "custom_domains_binding_status" {
  description = "Custom domain binding status"
  value       = var.bind_custom_domains ? "Custom domains bound!" : "Run: terraform apply -var='bind_custom_domains=true'"
}
