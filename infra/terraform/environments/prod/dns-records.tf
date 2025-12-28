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

# CNAME record for admin.mystira.app -> Admin API App Service
resource "azurerm_dns_cname_record" "prod_admin" {
  name                = "admin"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.admin_api.app_service_default_hostname

  tags = local.common_tags
}

# TXT record for Admin App Service domain verification
resource "azurerm_dns_txt_record" "prod_admin_verification" {
  name                = "asuid.admin"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300

  record {
    value = module.admin_api.app_service_custom_domain_verification_id
  }

  tags = local.common_tags
}

# =============================================================================
# Story Generator DNS Records
# =============================================================================

# CNAME record for story.mystira.app -> Story Generator SWA
resource "azurerm_dns_cname_record" "prod_story_swa" {
  name                = "story"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.story_generator.static_web_app_default_hostname

  tags = local.common_tags
}

# Custom domain binding for Story Generator SWA
resource "azurerm_static_web_app_custom_domain" "prod_story" {
  count = var.bind_custom_domains ? 1 : 0

  static_web_app_id = module.story_generator.static_web_app_id
  domain_name       = "story.mystira.app"
  validation_type   = "cname-delegation"

  depends_on = [azurerm_dns_cname_record.prod_story_swa]
}

# CNAME record for story-api.mystira.app -> Story Generator API
resource "azurerm_dns_cname_record" "prod_story_api" {
  name                = "story-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  record              = module.story_generator.app_service_default_hostname

  tags = local.common_tags
}

# TXT record for Story API App Service domain verification
resource "azurerm_dns_txt_record" "prod_story_api_verification" {
  name                = "asuid.story-api"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300

  record {
    value = module.story_generator.app_service_custom_domain_verification_id
  }

  tags = local.common_tags
}

# =============================================================================
# Kubernetes Services DNS Records (A records to ingress IP)
# =============================================================================

# A record for publisher.mystira.app -> K8s ingress
resource "azurerm_dns_a_record" "prod_publisher" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "publisher"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

  tags = local.common_tags
}

# A record for chain.mystira.app -> K8s ingress
resource "azurerm_dns_a_record" "prod_chain" {
  count = var.k8s_ingress_ip != "" ? 1 : 0

  name                = "chain"
  zone_name           = data.azurerm_dns_zone.mystira.name
  resource_group_name = data.azurerm_dns_zone.mystira.resource_group_name
  ttl                 = 300
  records             = [var.k8s_ingress_ip]

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
    apex_swa    = "mystira.app"
    api         = "api.mystira.app"
    admin       = "admin.mystira.app"
    story       = "story.mystira.app"
    story_api   = "story-api.mystira.app"
    publisher   = var.k8s_ingress_ip != "" ? "publisher.mystira.app" : "Not created (no k8s_ingress_ip)"
    chain       = var.k8s_ingress_ip != "" ? "chain.mystira.app" : "Not created (no k8s_ingress_ip)"
  }
}

output "custom_domains_binding_status" {
  description = "Custom domain binding status"
  value       = var.bind_custom_domains ? "Custom domains bound!" : "Run: terraform apply -var='bind_custom_domains=true'"
}
