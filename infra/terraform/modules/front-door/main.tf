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
    certificate_type = "ManagedCertificate"
  }
}

# Chain Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "chain" {
  name                     = "${var.project_name}-${var.environment}-chain-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null # Will be managed externally via DNS module
  host_name                = var.custom_domain_chain

  tls {
    certificate_type = "ManagedCertificate"
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
# so we need a single security policy covering all domains (including admin if enabled)
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
        # Conditionally include admin domains when admin services are enabled
        dynamic "domain" {
          for_each = var.enable_admin_services ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.admin_api[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_admin_services ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.admin_ui[0].id
          }
        }
        # Conditionally include story-generator domains when enabled
        dynamic "domain" {
          for_each = var.enable_story_generator ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.story_generator_api[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_story_generator ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.story_generator_swa[0].id
          }
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

# =============================================================================
# Admin Services (Optional)
# =============================================================================

# Admin API Endpoint
resource "azurerm_cdn_frontdoor_endpoint" "admin_api" {
  count                    = var.enable_admin_services ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-admin-api"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  tags = local.common_tags
}

# Admin UI Endpoint
resource "azurerm_cdn_frontdoor_endpoint" "admin_ui" {
  count                    = var.enable_admin_services ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-admin-ui"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  tags = local.common_tags
}

# Admin API Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "admin_api" {
  count                    = var.enable_admin_services ? 1 : 0
  name                     = "admin-api-origin-group"
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

# Admin UI Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "admin_ui" {
  count                    = var.enable_admin_services ? 1 : 0
  name                     = "admin-ui-origin-group"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  session_affinity_enabled = var.session_affinity_enabled

  load_balancing {
    sample_size                        = 4
    successful_samples_required        = 3
    additional_latency_in_milliseconds = 50
  }

  health_probe {
    path                = "/"
    request_type        = "HEAD"
    protocol            = "Https"
    interval_in_seconds = var.health_probe_interval
  }
}

# Admin API Origin
resource "azurerm_cdn_frontdoor_origin" "admin_api" {
  count                         = var.enable_admin_services ? 1 : 0
  name                          = "admin-api-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.admin_api[0].id

  enabled                        = true
  host_name                      = var.admin_api_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.admin_api_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Admin UI Origin
resource "azurerm_cdn_frontdoor_origin" "admin_ui" {
  count                         = var.enable_admin_services ? 1 : 0
  name                          = "admin-ui-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.admin_ui[0].id

  enabled                        = true
  host_name                      = var.admin_ui_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.admin_ui_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Admin API Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "admin_api" {
  count                    = var.enable_admin_services ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-admin-api-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.custom_domain_admin_api

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Admin UI Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "admin_ui" {
  count                    = var.enable_admin_services ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-admin-ui-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.custom_domain_admin_ui

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Admin API Route
resource "azurerm_cdn_frontdoor_route" "admin_api" {
  count                           = var.enable_admin_services ? 1 : 0
  name                            = "admin-api-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.admin_api[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.admin_api[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.admin_api[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.admin_api[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  # No caching for API endpoints
  cache {
    query_string_caching_behavior = "UseQueryString"
    compression_enabled           = true
    content_types_to_compress = [
      "application/json",
      "text/plain",
    ]
  }
}

# Admin UI Route
resource "azurerm_cdn_frontdoor_route" "admin_ui" {
  count                           = var.enable_admin_services ? 1 : 0
  name                            = "admin-ui-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.admin_ui[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.admin_ui[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.admin_ui[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.admin_ui[0].id]

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
        "text/css",
        "text/html",
        "text/javascript",
        "text/plain",
      ]
    }
  }
}

# NOTE: Admin domains WAF protection is handled by the main security policy above
# via dynamic blocks. A separate admin security policy was removed because Azure
# only allows one WAF firewall policy attachment per Front Door profile.

# =============================================================================
# Story Generator Services (Optional)
# =============================================================================

# Story Generator API Endpoint
resource "azurerm_cdn_frontdoor_endpoint" "story_generator_api" {
  count                    = var.enable_story_generator ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-story-api"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  tags = local.common_tags
}

# Story Generator SWA Endpoint
resource "azurerm_cdn_frontdoor_endpoint" "story_generator_swa" {
  count                    = var.enable_story_generator ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-story-swa"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  tags = local.common_tags
}

# Story Generator API Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "story_generator_api" {
  count                    = var.enable_story_generator ? 1 : 0
  name                     = "story-generator-api-origin-group"
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

# Story Generator SWA Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "story_generator_swa" {
  count                    = var.enable_story_generator ? 1 : 0
  name                     = "story-generator-swa-origin-group"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  session_affinity_enabled = false  # SWA is stateless

  load_balancing {
    sample_size                        = 4
    successful_samples_required        = 3
    additional_latency_in_milliseconds = 50
  }

  health_probe {
    path                = "/"  # SWA health check at root
    request_type        = "HEAD"
    protocol            = "Https"
    interval_in_seconds = var.health_probe_interval
  }
}

# Story Generator API Origin
resource "azurerm_cdn_frontdoor_origin" "story_generator_api" {
  count                         = var.enable_story_generator ? 1 : 0
  name                          = "story-generator-api-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.story_generator_api[0].id

  enabled                        = true
  host_name                      = var.story_generator_api_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.story_generator_api_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Story Generator SWA Origin
resource "azurerm_cdn_frontdoor_origin" "story_generator_swa" {
  count                         = var.enable_story_generator ? 1 : 0
  name                          = "story-generator-swa-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.story_generator_swa[0].id

  enabled                        = true
  host_name                      = var.story_generator_swa_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.story_generator_swa_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Story Generator API Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "story_generator_api" {
  count                    = var.enable_story_generator ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-story-api-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.custom_domain_story_generator_api

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Story Generator SWA Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "story_generator_swa" {
  count                    = var.enable_story_generator ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-story-swa-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.custom_domain_story_generator_swa

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Story Generator API Route
resource "azurerm_cdn_frontdoor_route" "story_generator_api" {
  count                           = var.enable_story_generator ? 1 : 0
  name                            = "story-generator-api-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.story_generator_api[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.story_generator_api[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.story_generator_api[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.story_generator_api[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  # No caching for API endpoints
  cache {
    query_string_caching_behavior = "UseQueryString"
    compression_enabled           = true
    content_types_to_compress = [
      "application/json",
      "text/plain",
    ]
  }
}

# Story Generator SWA Route
resource "azurerm_cdn_frontdoor_route" "story_generator_swa" {
  count                           = var.enable_story_generator ? 1 : 0
  name                            = "story-generator-swa-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.story_generator_swa[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.story_generator_swa[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.story_generator_swa[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.story_generator_swa[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  # Cache static assets from SWA
  dynamic "cache" {
    for_each = var.enable_caching ? [1] : []
    content {
      query_string_caching_behavior = "IgnoreQueryString"
      query_strings                 = []
      compression_enabled           = true
      content_types_to_compress = [
        "application/javascript",
        "application/json",
        "text/css",
        "text/html",
        "text/javascript",
        "text/plain",
      ]
    }
  }
}
