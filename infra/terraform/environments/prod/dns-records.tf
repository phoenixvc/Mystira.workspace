# =============================================================================
# DNS Records for Production Environment
# References existing DNS zone in shared terraform RG (managed by CI/CD bootstrap)
# =============================================================================

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
# Mystira.App SWA DNS Records (apex domain: mystira.app)
# =============================================================================

# ALIAS A record for mystira.app (apex) -> SWA
# Azure DNS supports alias records to point apex to Azure resources
resource "azurerm_dns_a_record" "prod_app_swa" {
  name                = "@"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  target_resource_id = module.mystira_app.static_web_app_id

  tags = local.common_tags
}

# Custom domain binding for SWA (only when bind_custom_domains=true)
resource "azurerm_static_web_app_custom_domain" "prod_app" {
  count = var.bind_custom_domains ? 1 : 0

  static_web_app_id = module.mystira_app.static_web_app_id
  domain_name       = "mystira.app"
  validation_type   = "dns-txt-token"

  depends_on = [azurerm_dns_a_record.prod_app_swa]
}

# =============================================================================
# Mystira.App API DNS Records (api.mystira.app)
# =============================================================================

# CNAME record for api.mystira.app -> App Service
resource "azurerm_dns_cname_record" "prod_api" {
  name                = "api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.mystira_app.app_service_default_hostname

  tags = local.common_tags
}

# TXT record for App Service domain verification
resource "azurerm_dns_txt_record" "prod_api_verification" {
  name                = "asuid.api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300

  record {
    value = module.mystira_app.app_service_custom_domain_verification_id
  }

  tags = local.common_tags
}

# Custom hostname binding for App Service (only when bind_custom_domains=true)
resource "azurerm_app_service_custom_hostname_binding" "prod_api" {
  count = var.bind_custom_domains ? 1 : 0

  hostname            = "api.mystira.app"
  app_service_name    = module.mystira_app.app_service_name
  resource_group_name = azurerm_resource_group.app.name

  depends_on = [
    azurerm_dns_cname_record.prod_api,
    azurerm_dns_txt_record.prod_api_verification
  ]
}

# Free managed SSL certificate for App Service API
resource "azurerm_app_service_managed_certificate" "prod_api" {
  count = var.bind_custom_domains ? 1 : 0

  custom_hostname_binding_id = azurerm_app_service_custom_hostname_binding.prod_api[0].id
}

# Bind the certificate to the hostname
resource "azurerm_app_service_certificate_binding" "prod_api" {
  count = var.bind_custom_domains ? 1 : 0

  hostname_binding_id = azurerm_app_service_custom_hostname_binding.prod_api[0].id
  certificate_id      = azurerm_app_service_managed_certificate.prod_api[0].id
  ssl_state           = "SniEnabled"
}

# =============================================================================
# Admin Services DNS Records
# =============================================================================
# NOTE: Admin services now route through Front Door. The CNAME records pointing
# to Front Door are in the "Front Door CNAME Records" section below.
# The backend A records (*-k8s) for K8s ingress are in the K8s section above.

# =============================================================================
# Story Generator DNS Records
# =============================================================================
# NOTE: Story Generator services now route through Front Door.
# - story.mystira.app and story-api.mystira.app CNAME records are in the
#   "Front Door CNAME Records" section below
# - The backend A record (story-api-k8s) for K8s ingress is in the K8s section above
# - Story SWA backend uses the Azure Static Web App hostname directly (no -k8s)

# =============================================================================
# Kubernetes Services DNS Records
#
# ARCHITECTURE:
# - Backend A records (*-k8s.mystira.app) -> K8s ingress IP (for Front Door origins)
# - Public CNAME records (*.mystira.app) -> Front Door endpoint (for custom domains)
#
# This separation is required because:
# - Front Door custom domains need CNAME -> Front Door endpoint
# - Front Door backends need to reach the actual K8s services (A record -> IP)
# - You cannot have both A and CNAME for the same hostname
# =============================================================================

# -----------------------------------------------------------------------------
# BACKEND A RECORDS (for Front Door to reach K8s)
# -----------------------------------------------------------------------------

# Backend A record for Publisher service
resource "azurerm_dns_a_record" "prod_publisher_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "publisher-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# Backend A record for Chain service
resource "azurerm_dns_a_record" "prod_chain_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "chain-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# Backend A record for Admin API service
resource "azurerm_dns_a_record" "prod_admin_api_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "admin-api-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# Backend A record for Admin UI service
resource "azurerm_dns_a_record" "prod_admin_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "admin-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# Backend A record for Story API service
resource "azurerm_dns_a_record" "prod_story_api_k8s" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "story-api-k8s"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# -----------------------------------------------------------------------------
# FRONT DOOR CNAME RECORDS (for custom domain validation)
# These point public hostnames to Front Door endpoints
# -----------------------------------------------------------------------------

# CNAME for publisher.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "prod_publisher_fd" {
  count = length(try(module.front_door.publisher_endpoint_hostname, "")) > 0 ? 1 : 0

  name                = "publisher"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.front_door.publisher_endpoint_hostname

  tags = local.common_tags
}

# CNAME for chain.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "prod_chain_fd" {
  count = length(try(module.front_door.chain_endpoint_hostname, "")) > 0 ? 1 : 0

  name                = "chain"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.front_door.chain_endpoint_hostname

  tags = local.common_tags
}

# CNAME for admin-api.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "prod_admin_api_fd" {
  count = length(try(module.front_door.admin_api_endpoint_hostname, "")) > 0 ? 1 : 0

  name                = "admin-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.front_door.admin_api_endpoint_hostname

  tags = local.common_tags
}

# CNAME for admin.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "prod_admin_fd" {
  count = length(try(module.front_door.admin_ui_endpoint_hostname, "")) > 0 ? 1 : 0

  name                = "admin"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.front_door.admin_ui_endpoint_hostname

  tags = local.common_tags
}

# CNAME for story.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "prod_story_fd" {
  count = length(try(module.front_door.story_generator_swa_endpoint_hostname, "")) > 0 ? 1 : 0

  name                = "story"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.front_door.story_generator_swa_endpoint_hostname

  tags = local.common_tags
}

# CNAME for story-api.mystira.app -> Front Door
resource "azurerm_dns_cname_record" "prod_story_api_fd" {
  count = length(try(module.front_door.story_generator_api_endpoint_hostname, "")) > 0 ? 1 : 0

  name                = "story-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.front_door.story_generator_api_endpoint_hostname

  tags = local.common_tags
}

# =============================================================================
# Front Door TXT Validation Records (when using Front Door)
# =============================================================================

# TXT validation for mystira.app (apex - Front Door)
resource "azurerm_dns_txt_record" "fd_prod_app" {
  count = length(try(module.front_door.mystira_app_swa_custom_domain_validation_token, "")) > 0 ? 1 : 0

  name                = "_dnsauth"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.mystira_app_swa_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for api.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_prod_api" {
  count = length(try(module.front_door.mystira_app_api_custom_domain_validation_token, "")) > 0 ? 1 : 0

  name                = "_dnsauth.api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.mystira_app_api_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for admin.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_prod_admin" {
  count = length(try(module.front_door.admin_ui_custom_domain_validation_token, "")) > 0 ? 1 : 0

  name                = "_dnsauth.admin"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.admin_ui_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for admin-api.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_prod_admin_api" {
  count = length(try(module.front_door.admin_api_custom_domain_validation_token, "")) > 0 ? 1 : 0

  name                = "_dnsauth.admin-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.admin_api_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for story.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_prod_story_swa" {
  count = length(try(module.front_door.story_generator_swa_custom_domain_validation_token, "")) > 0 ? 1 : 0

  name                = "_dnsauth.story"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.story_generator_swa_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for story-api.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_prod_story_api" {
  count = length(try(module.front_door.story_generator_api_custom_domain_validation_token, "")) > 0 ? 1 : 0

  name                = "_dnsauth.story-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.story_generator_api_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for publisher.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_prod_publisher" {
  count = length(try(module.front_door.publisher_custom_domain_validation_token, "")) > 0 ? 1 : 0

  name                = "_dnsauth.publisher"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 3600

  record {
    value = module.front_door.publisher_custom_domain_validation_token
  }

  tags = local.common_tags
}

# TXT validation for chain.mystira.app (Front Door)
resource "azurerm_dns_txt_record" "fd_prod_chain" {
  count = length(try(module.front_door.chain_custom_domain_validation_token, "")) > 0 ? 1 : 0

  name                = "_dnsauth.chain"
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
  description = "Name servers for mystira.app DNS zone"
  value       = data.azurerm_dns_zone.mystira.name_servers
}

output "prod_dns_records_created" {
  description = "List of DNS records created for prod"
  value = {
    apex_swa       = "mystira.app"
    api            = "api.mystira.app"
    admin          = "admin.mystira.app (via Front Door)"
    admin_api      = "admin-api.mystira.app (via Front Door)"
    story          = "story.mystira.app (via Front Door)"
    story_api      = "story-api.mystira.app (via Front Door)"
    publisher      = "publisher.mystira.app (via Front Door)"
    chain          = "chain.mystira.app (via Front Door)"
    publisher_k8s  = var.k8s_ingress_ip != "" ? "publisher-k8s.mystira.app" : "Not created (no k8s_ingress_ip)"
    chain_k8s      = var.k8s_ingress_ip != "" ? "chain-k8s.mystira.app" : "Not created (no k8s_ingress_ip)"
    admin_api_k8s  = var.k8s_ingress_ip != "" ? "admin-api-k8s.mystira.app" : "Not created (no k8s_ingress_ip)"
    admin_k8s      = var.k8s_ingress_ip != "" ? "admin-k8s.mystira.app" : "Not created (no k8s_ingress_ip)"
    story_api_k8s  = var.k8s_ingress_ip != "" ? "story-api-k8s.mystira.app" : "Not created (no k8s_ingress_ip)"
  }
}

output "custom_domains_binding_status" {
  description = "Custom domain binding status"
  value       = var.bind_custom_domains ? "Custom domains bound!" : "Run: terraform apply -var='bind_custom_domains=true'"
}
