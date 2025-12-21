terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
}

locals {
  common_tags = merge(var.tags, {
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = var.project_name
    Component   = "front-door"
  })

  front_door_name = "${var.project_name}-${var.environment}-fd"
  waf_policy_name = "${var.project_name}${var.environment}waf"
}

# Azure Front Door Standard/Premium
resource "azurerm_cdn_frontdoor_profile" "main" {
  name                = local.front_door_name
  resource_group_name = var.resource_group_name
  sku_name            = "Standard_AzureFrontDoor"

  tags = local.common_tags
}

# Publisher Endpoint
resource "azurerm_cdn_frontdoor_endpoint" "publisher" {
  name                     = "${var.project_name}-${var.environment}-publisher"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  tags = local.common_tags
}

# Chain Endpoint
resource "azurerm_cdn_frontdoor_endpoint" "chain" {
  name                     = "${var.project_name}-${var.environment}-chain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  tags = local.common_tags
}

# Publisher Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "publisher" {
  name                     = "publisher-origin-group"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  session_affinity_enabled = var.session_affinity_enabled

  load_balancing {
    sample_size                        = 4
    successful_samples_required        = 3
    additional_latency_in_milliseconds = 50
  }

  health_probe {
    path                = var.health_probe_path
    request_type        = "HEAD"
    protocol            = "Https"
    interval_in_seconds = var.health_probe_interval
  }
}

# Chain Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "chain" {
  name                     = "chain-origin-group"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  session_affinity_enabled = var.session_affinity_enabled

  load_balancing {
    sample_size                        = 4
    successful_samples_required        = 3
    additional_latency_in_milliseconds = 50
  }

  health_probe {
    path                = var.health_probe_path
    request_type        = "HEAD"
    protocol            = "Https"
    interval_in_seconds = var.health_probe_interval
  }
}

# Publisher Origin
resource "azurerm_cdn_frontdoor_origin" "publisher" {
  name                          = "publisher-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.publisher.id

  enabled                        = true
  host_name                      = var.publisher_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.publisher_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Chain Origin
resource "azurerm_cdn_frontdoor_origin" "chain" {
  name                          = "chain-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.chain.id

  enabled                        = true
  host_name                      = var.chain_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.chain_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Publisher Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "publisher" {
  name                     = "${var.project_name}-${var.environment}-publisher-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null # Will be managed externally via DNS module
  host_name                = var.custom_domain_publisher

  tls {
    certificate_type    = "ManagedCertificate"
    minimum_tls_version = "TLS12"
  }
}

# Chain Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "chain" {
  name                     = "${var.project_name}-${var.environment}-chain-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null # Will be managed externally via DNS module
  host_name                = var.custom_domain_chain

  tls {
    certificate_type    = "ManagedCertificate"
    minimum_tls_version = "TLS12"
  }
}

# Publisher Route
resource "azurerm_cdn_frontdoor_route" "publisher" {
  name                            = "publisher-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.publisher.id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.publisher.id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.publisher.id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.publisher.id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  dynamic "cache" {
    for_each = var.enable_caching ? [1] : []
    content {
      query_string_caching_behavior = "IgnoreQueryString"
      query_strings                 = []
      compression_enabled           = true
      content_types_to_compress = [
        "application/javascript",
        "application/json",
        "application/xml",
        "text/css",
        "text/html",
        "text/javascript",
        "text/plain",
      ]
    }
  }
}

# Chain Route
resource "azurerm_cdn_frontdoor_route" "chain" {
  name                            = "chain-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.chain.id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.chain.id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.chain.id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.chain.id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  # No caching for Chain service (blockchain RPC)
  cache {
    query_string_caching_behavior = "IgnoreQueryString"
    compression_enabled           = false
  }
}

# WAF Policy (if enabled)
resource "azurerm_cdn_frontdoor_firewall_policy" "main" {
  count               = var.enable_waf ? 1 : 0
  name                = local.waf_policy_name
  resource_group_name = var.resource_group_name
  sku_name            = azurerm_cdn_frontdoor_profile.main.sku_name
  enabled             = true
  mode                = var.waf_mode

  # Note: managed_rule blocks require Premium_AzureFrontDoor SKU
  # Managed rules are only available with Premium tier
  # For Standard tier, only custom rules are supported

  # Custom rule - Rate limiting
  custom_rule {
    name                           = "RateLimitRule"
    enabled                        = true
    priority                       = 1
    rate_limit_duration_in_minutes = 1
    rate_limit_threshold           = var.rate_limit_threshold
    type                           = "RateLimitRule"
    action                         = "Block"

    match_condition {
      match_variable     = "RemoteAddr"
      operator           = "IPMatch"
      negation_condition = false
      match_values       = ["0.0.0.0/0"]
    }
  }

  # Custom rule - Block known bad user agents
  custom_rule {
    name     = "BlockBadBots"
    enabled  = true
    priority = 2
    type     = "MatchRule"
    action   = "Block"

    match_condition {
      match_variable     = "RequestHeader"
      selector           = "User-Agent"
      operator           = "Contains"
      negation_condition = false
      match_values = [
        "sqlmap",
        "nikto",
        "scanner",
        "masscan"
      ]
      transforms = ["Lowercase"]
    }
  }

  # Custom rule - Allow only standard HTTP methods
  custom_rule {
    name     = "AllowedMethods"
    enabled  = true
    priority = 3
    type     = "MatchRule"
    action   = "Block"

    match_condition {
      match_variable     = "RequestMethod"
      operator           = "Equal"
      negation_condition = true
      match_values = [
        "GET",
        "POST",
        "PUT",
        "DELETE",
        "PATCH",
        "HEAD",
        "OPTIONS"
      ]
    }
  }

  tags = local.common_tags
}

# Associate WAF policy with all custom domains
# Note: A WAF policy can only be attached once per Front Door profile,
# so we need a single security policy covering all domains
resource "azurerm_cdn_frontdoor_security_policy" "main" {
  count                    = var.enable_waf ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-waf-security-policy"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  security_policies {
    firewall {
      cdn_frontdoor_firewall_policy_id = azurerm_cdn_frontdoor_firewall_policy.main[0].id

      association {
        domain {
          cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.publisher.id
        }
        domain {
          cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.chain.id
        }
        patterns_to_match = ["/*"]
      }
    }
  }

  lifecycle {
    create_before_destroy = false
    # Ignore changes to association since Azure may modify the order
    ignore_changes = [
      security_policies[0].firewall[0].association[0].domain
    ]
  }

  # Ensure the WAF policy and profile are fully created before attaching
  depends_on = [
    azurerm_cdn_frontdoor_firewall_policy.main,
    azurerm_cdn_frontdoor_custom_domain.publisher,
    azurerm_cdn_frontdoor_custom_domain.chain
  ]
}
