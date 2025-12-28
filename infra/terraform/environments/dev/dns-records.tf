# =============================================================================
# DNS Records for Dev Environment
# References existing DNS zone in shared terraform RG (managed by CI/CD bootstrap)
#
# Two-step deployment:
#   1. terraform apply                                    (creates DNS records)
#   2. terraform apply -var="bind_custom_domains=true"    (binds custom domains)
# =============================================================================

# =============================================================================
# Import blocks for existing DNS records
# These records were created by previous CI/CD runs and need to be imported
# =============================================================================

import {
  to = azurerm_dns_cname_record.dev_app_swa
  id = "/subscriptions/22f9eb18-6553-4b7d-9451-47d0195085fe/resourceGroups/mys-shared-terraform-rg-san/providers/Microsoft.Network/dnsZones/mystira.app/CNAME/dev"
}

import {
  to = azurerm_dns_cname_record.dev_api
  id = "/subscriptions/22f9eb18-6553-4b7d-9451-47d0195085fe/resourceGroups/mys-shared-terraform-rg-san/providers/Microsoft.Network/dnsZones/mystira.app/CNAME/dev.api"
}

import {
  to = azurerm_dns_cname_record.dev_story_swa
  id = "/subscriptions/22f9eb18-6553-4b7d-9451-47d0195085fe/resourceGroups/mys-shared-terraform-rg-san/providers/Microsoft.Network/dnsZones/mystira.app/CNAME/dev.story"
}

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
# Kubernetes Services DNS Records (A records to ingress IP)
# Cert-manager auto-provisions Let's Encrypt certs via HTTP-01 challenge
# =============================================================================

# A record for dev.publisher.mystira.app -> K8s ingress
resource "azurerm_dns_a_record" "dev_publisher" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "dev.publisher"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# A record for dev.chain.mystira.app -> K8s ingress
resource "azurerm_dns_a_record" "dev_chain" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "dev.chain"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# A record for dev.story-api.mystira.app -> K8s ingress
resource "azurerm_dns_a_record" "dev_story_api" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "dev.story-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# A record for dev.admin-api.mystira.app -> K8s ingress
resource "azurerm_dns_a_record" "dev_admin_api" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "dev.admin-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# A record for dev.admin.mystira.app -> K8s ingress
resource "azurerm_dns_a_record" "dev_admin_ui" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "dev.admin"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# =============================================================================
# Story Generator SWA DNS Records
# =============================================================================

# CNAME record for dev.story.mystira.app -> Story Generator SWA
resource "azurerm_dns_cname_record" "dev_story_swa" {
  name                = "dev.story"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.story_generator.static_web_app_default_hostname

  tags = local.common_tags
}

# Custom domain binding for Story Generator SWA
resource "azurerm_static_web_app_custom_domain" "dev_story" {
  count = var.bind_custom_domains ? 1 : 0

  static_web_app_id = module.story_generator.static_web_app_id
  domain_name       = "dev.story.mystira.app"
  validation_type   = "cname-delegation"

  depends_on = [azurerm_dns_cname_record.dev_story_swa]
}

# =============================================================================
# Front Door TXT Validation Records
# These records validate domain ownership for Azure Front Door custom domains
# =============================================================================

# TXT validation for dev.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_dev_app" {
  name                = "_dnsauth.dev"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.mystira_app_swa_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for dev.api.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_dev_api" {
  name                = "_dnsauth.dev.api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.mystira_app_api_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for dev.admin-api.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_dev_admin_api" {
  name                = "_dnsauth.dev.admin-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.admin_api_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for dev.admin.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_dev_admin_ui" {
  name                = "_dnsauth.dev.admin"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.admin_ui_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for dev.story.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_dev_story_swa" {
  name                = "_dnsauth.dev.story"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.story_generator_swa_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for dev.story-api.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_dev_story_api" {
  name                = "_dnsauth.dev.story-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.story_generator_api_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for dev.publisher.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_dev_publisher" {
  name                = "_dnsauth.dev.publisher"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.publisher_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for dev.chain.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_dev_chain" {
  name                = "_dnsauth.dev.chain"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.chain_custom_domain_validation_token
  }

  tags = local.common_tags
}

# =============================================================================
# Outputs
# =============================================================================

output "dns_zone_name_servers" {
  description = "Name servers for mystira.app DNS zone - update your domain registrar"
  value       = data.azurerm_dns_zone.mystira.name_servers
}

output "next_step" {
  description = "Instructions for binding custom domains"
  value       = var.bind_custom_domains ? "Custom domains bound!" : "Run: terraform apply -var='bind_custom_domains=true'"
}

output "k8s_dns_records_note" {
  description = "K8s DNS records status"
  value       = var.k8s_ingress_ip != "" ? "K8s A records created for ingress IP ${var.k8s_ingress_ip}" : "Set k8s_ingress_ip to create K8s DNS records: kubectl get svc -n ingress-nginx -o jsonpath='{.items[0].status.loadBalancer.ingress[0].ip}'"
}
