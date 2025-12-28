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

# =============================================================================
# CONSOLIDATED ENDPOINT ARCHITECTURE
# =============================================================================
# Instead of separate endpoints per service (which hit Azure quota limits),
# we use a single consolidated endpoint per environment. Custom domains and
# routes handle routing to different origin groups based on the hostname.
#
# This reduces endpoints from 16 (with secondary environment) to just 2:
# - Primary endpoint: handles all primary environment traffic
# - Secondary endpoint: handles all secondary environment traffic (if enabled)
# =============================================================================

# Primary Consolidated Endpoint - handles all primary environment services
resource "azurerm_cdn_frontdoor_endpoint" "primary" {
  name                     = "${var.project_name}-${var.environment}"
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

# Publisher Route - uses consolidated primary endpoint
resource "azurerm_cdn_frontdoor_route" "publisher" {
  name                            = "publisher-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.primary.id
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

# Chain Route - uses consolidated primary endpoint
resource "azurerm_cdn_frontdoor_route" "chain" {
  name                            = "chain-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.primary.id
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
        # Conditionally include Mystira.App domains when enabled
        dynamic "domain" {
          for_each = var.enable_mystira_app ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.mystira_app_api[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_mystira_app ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.mystira_app_swa[0].id
          }
        }
        # Secondary environment domains (when shared non-prod Front Door is enabled)
        dynamic "domain" {
          for_each = var.enable_secondary_environment ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.secondary_publisher[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_secondary_environment ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.secondary_chain[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_secondary_environment && var.enable_admin_services ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.secondary_admin_api[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_secondary_environment && var.enable_admin_services ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.secondary_admin_ui[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_secondary_environment && var.enable_story_generator ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.secondary_story_generator_api[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_secondary_environment && var.enable_story_generator ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.secondary_story_generator_swa[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_secondary_environment && var.enable_mystira_app ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.secondary_mystira_app_api[0].id
          }
        }
        dynamic "domain" {
          for_each = var.enable_secondary_environment && var.enable_mystira_app ? [1] : []
          content {
            cdn_frontdoor_domain_id = azurerm_cdn_frontdoor_custom_domain.secondary_mystira_app_swa[0].id
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
# Note: Admin services use the consolidated primary endpoint instead of
# individual endpoints to stay within Azure Front Door quota limits.

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

# Admin API Route - uses consolidated primary endpoint
resource "azurerm_cdn_frontdoor_route" "admin_api" {
  count                           = var.enable_admin_services ? 1 : 0
  name                            = "admin-api-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.primary.id
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

# Admin UI Route - uses consolidated primary endpoint
resource "azurerm_cdn_frontdoor_route" "admin_ui" {
  count                           = var.enable_admin_services ? 1 : 0
  name                            = "admin-ui-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.primary.id
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
# Note: Story Generator services use the consolidated primary endpoint instead
# of individual endpoints to stay within Azure Front Door quota limits.

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

# Story Generator API Route - uses consolidated primary endpoint
resource "azurerm_cdn_frontdoor_route" "story_generator_api" {
  count                           = var.enable_story_generator ? 1 : 0
  name                            = "story-generator-api-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.primary.id
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

# Story Generator SWA Route - uses consolidated primary endpoint
resource "azurerm_cdn_frontdoor_route" "story_generator_swa" {
  count                           = var.enable_story_generator ? 1 : 0
  name                            = "story-generator-swa-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.primary.id
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

# =============================================================================
# Mystira.App Services (Optional)
# =============================================================================
# Note: Mystira.App services use the consolidated primary endpoint instead
# of individual endpoints to stay within Azure Front Door quota limits.

# Mystira.App API Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "mystira_app_api" {
  count                    = var.enable_mystira_app ? 1 : 0
  name                     = "mystira-app-api-origin-group"
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

# Mystira.App SWA Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "mystira_app_swa" {
  count                    = var.enable_mystira_app ? 1 : 0
  name                     = "mystira-app-swa-origin-group"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  session_affinity_enabled = false  # SWA/PWA is stateless

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

# Mystira.App API Origin
resource "azurerm_cdn_frontdoor_origin" "mystira_app_api" {
  count                         = var.enable_mystira_app ? 1 : 0
  name                          = "mystira-app-api-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.mystira_app_api[0].id

  enabled                        = true
  host_name                      = var.mystira_app_api_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.mystira_app_api_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Mystira.App SWA Origin
resource "azurerm_cdn_frontdoor_origin" "mystira_app_swa" {
  count                         = var.enable_mystira_app ? 1 : 0
  name                          = "mystira-app-swa-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.mystira_app_swa[0].id

  enabled                        = true
  host_name                      = var.mystira_app_swa_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.mystira_app_swa_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Mystira.App API Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "mystira_app_api" {
  count                    = var.enable_mystira_app ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-app-api-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.custom_domain_mystira_app_api

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Mystira.App SWA Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "mystira_app_swa" {
  count                    = var.enable_mystira_app ? 1 : 0
  name                     = "${var.project_name}-${var.environment}-app-swa-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.custom_domain_mystira_app_swa

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Mystira.App API Route - uses consolidated primary endpoint
resource "azurerm_cdn_frontdoor_route" "mystira_app_api" {
  count                           = var.enable_mystira_app ? 1 : 0
  name                            = "mystira-app-api-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.primary.id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.mystira_app_api[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.mystira_app_api[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.mystira_app_api[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  # No caching for API endpoints - important for SignalR WebSocket connections
  cache {
    query_string_caching_behavior = "UseQueryString"
    compression_enabled           = true
    content_types_to_compress = [
      "application/json",
      "text/plain",
    ]
  }
}

# Mystira.App SWA Route - uses consolidated primary endpoint
resource "azurerm_cdn_frontdoor_route" "mystira_app_swa" {
  count                           = var.enable_mystira_app ? 1 : 0
  name                            = "mystira-app-swa-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.primary.id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.mystira_app_swa[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.mystira_app_swa[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.mystira_app_swa[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  # Cache static assets from PWA/SWA
  dynamic "cache" {
    for_each = var.enable_caching ? [1] : []
    content {
      query_string_caching_behavior = "IgnoreQueryString"
      query_strings                 = []
      compression_enabled           = true
      content_types_to_compress = [
        "application/javascript",
        "application/json",
        "image/svg+xml",
        "text/css",
        "text/html",
        "text/javascript",
        "text/plain",
      ]
    }
  }
}

# =============================================================================
# Secondary Environment Resources (for shared non-prod Front Door)
# =============================================================================
# These resources are only created when enable_secondary_environment is true
# They handle domains/backends for the secondary environment (e.g., staging)
#
# Like the primary environment, secondary uses a consolidated endpoint to stay
# within Azure Front Door quota limits.

# Secondary Consolidated Endpoint - handles all secondary environment services
resource "azurerm_cdn_frontdoor_endpoint" "secondary" {
  count                    = var.enable_secondary_environment ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id

  tags = merge(local.common_tags, { SecondaryEnvironment = var.secondary_environment })
}

# Secondary Publisher Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "secondary_publisher" {
  count                    = var.enable_secondary_environment ? 1 : 0
  name                     = "secondary-publisher-origin-group"
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

# Secondary Chain Origin Group
resource "azurerm_cdn_frontdoor_origin_group" "secondary_chain" {
  count                    = var.enable_secondary_environment ? 1 : 0
  name                     = "secondary-chain-origin-group"
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

# Secondary Publisher Origin
resource "azurerm_cdn_frontdoor_origin" "secondary_publisher" {
  count                         = var.enable_secondary_environment ? 1 : 0
  name                          = "secondary-publisher-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.secondary_publisher[0].id

  enabled                        = true
  host_name                      = var.secondary_publisher_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.secondary_publisher_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Secondary Chain Origin
resource "azurerm_cdn_frontdoor_origin" "secondary_chain" {
  count                         = var.enable_secondary_environment ? 1 : 0
  name                          = "secondary-chain-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.secondary_chain[0].id

  enabled                        = true
  host_name                      = var.secondary_chain_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.secondary_chain_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

# Secondary Publisher Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "secondary_publisher" {
  count                    = var.enable_secondary_environment ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}-publisher-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.secondary_custom_domain_publisher

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Secondary Chain Custom Domain
resource "azurerm_cdn_frontdoor_custom_domain" "secondary_chain" {
  count                    = var.enable_secondary_environment ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}-chain-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.secondary_custom_domain_chain

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Secondary Publisher Route - uses consolidated secondary endpoint
resource "azurerm_cdn_frontdoor_route" "secondary_publisher" {
  count                           = var.enable_secondary_environment ? 1 : 0
  name                            = "secondary-publisher-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.secondary[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.secondary_publisher[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.secondary_publisher[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.secondary_publisher[0].id]

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

# Secondary Chain Route - uses consolidated secondary endpoint
resource "azurerm_cdn_frontdoor_route" "secondary_chain" {
  count                           = var.enable_secondary_environment ? 1 : 0
  name                            = "secondary-chain-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.secondary[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.secondary_chain[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.secondary_chain[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.secondary_chain[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  cache {
    query_string_caching_behavior = "IgnoreQueryString"
    compression_enabled           = false
  }
}

# =============================================================================
# Secondary Admin Services
# =============================================================================
# Note: Secondary admin services use the consolidated secondary endpoint.

resource "azurerm_cdn_frontdoor_origin_group" "secondary_admin_api" {
  count                    = var.enable_secondary_environment && var.enable_admin_services ? 1 : 0
  name                     = "secondary-admin-api-origin-group"
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

resource "azurerm_cdn_frontdoor_origin_group" "secondary_admin_ui" {
  count                    = var.enable_secondary_environment && var.enable_admin_services ? 1 : 0
  name                     = "secondary-admin-ui-origin-group"
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

resource "azurerm_cdn_frontdoor_origin" "secondary_admin_api" {
  count                         = var.enable_secondary_environment && var.enable_admin_services ? 1 : 0
  name                          = "secondary-admin-api-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.secondary_admin_api[0].id

  enabled                        = true
  host_name                      = var.secondary_admin_api_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.secondary_admin_api_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_origin" "secondary_admin_ui" {
  count                         = var.enable_secondary_environment && var.enable_admin_services ? 1 : 0
  name                          = "secondary-admin-ui-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.secondary_admin_ui[0].id

  enabled                        = true
  host_name                      = var.secondary_admin_ui_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.secondary_admin_ui_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_custom_domain" "secondary_admin_api" {
  count                    = var.enable_secondary_environment && var.enable_admin_services ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}-admin-api-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.secondary_custom_domain_admin_api

  tls {
    certificate_type = "ManagedCertificate"
  }
}

resource "azurerm_cdn_frontdoor_custom_domain" "secondary_admin_ui" {
  count                    = var.enable_secondary_environment && var.enable_admin_services ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}-admin-ui-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.secondary_custom_domain_admin_ui

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Secondary Admin API Route - uses consolidated secondary endpoint
resource "azurerm_cdn_frontdoor_route" "secondary_admin_api" {
  count                           = var.enable_secondary_environment && var.enable_admin_services ? 1 : 0
  name                            = "secondary-admin-api-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.secondary[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.secondary_admin_api[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.secondary_admin_api[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.secondary_admin_api[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  cache {
    query_string_caching_behavior = "UseQueryString"
    compression_enabled           = true
    content_types_to_compress = [
      "application/json",
      "text/plain",
    ]
  }
}

# Secondary Admin UI Route - uses consolidated secondary endpoint
resource "azurerm_cdn_frontdoor_route" "secondary_admin_ui" {
  count                           = var.enable_secondary_environment && var.enable_admin_services ? 1 : 0
  name                            = "secondary-admin-ui-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.secondary[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.secondary_admin_ui[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.secondary_admin_ui[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.secondary_admin_ui[0].id]

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

# =============================================================================
# Secondary Story Generator Services
# =============================================================================
# Note: Secondary story generator services use the consolidated secondary endpoint.

resource "azurerm_cdn_frontdoor_origin_group" "secondary_story_generator_api" {
  count                    = var.enable_secondary_environment && var.enable_story_generator ? 1 : 0
  name                     = "secondary-story-generator-api-origin-group"
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

resource "azurerm_cdn_frontdoor_origin_group" "secondary_story_generator_swa" {
  count                    = var.enable_secondary_environment && var.enable_story_generator ? 1 : 0
  name                     = "secondary-story-generator-swa-origin-group"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  session_affinity_enabled = false

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

resource "azurerm_cdn_frontdoor_origin" "secondary_story_generator_api" {
  count                         = var.enable_secondary_environment && var.enable_story_generator ? 1 : 0
  name                          = "secondary-story-generator-api-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.secondary_story_generator_api[0].id

  enabled                        = true
  host_name                      = var.secondary_story_generator_api_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.secondary_story_generator_api_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_origin" "secondary_story_generator_swa" {
  count                         = var.enable_secondary_environment && var.enable_story_generator ? 1 : 0
  name                          = "secondary-story-generator-swa-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.secondary_story_generator_swa[0].id

  enabled                        = true
  host_name                      = var.secondary_story_generator_swa_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.secondary_story_generator_swa_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_custom_domain" "secondary_story_generator_api" {
  count                    = var.enable_secondary_environment && var.enable_story_generator ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}-story-api-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.secondary_custom_domain_story_generator_api

  tls {
    certificate_type = "ManagedCertificate"
  }
}

resource "azurerm_cdn_frontdoor_custom_domain" "secondary_story_generator_swa" {
  count                    = var.enable_secondary_environment && var.enable_story_generator ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}-story-swa-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.secondary_custom_domain_story_generator_swa

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Secondary Story Generator API Route - uses consolidated secondary endpoint
resource "azurerm_cdn_frontdoor_route" "secondary_story_generator_api" {
  count                           = var.enable_secondary_environment && var.enable_story_generator ? 1 : 0
  name                            = "secondary-story-generator-api-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.secondary[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.secondary_story_generator_api[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.secondary_story_generator_api[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.secondary_story_generator_api[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  cache {
    query_string_caching_behavior = "UseQueryString"
    compression_enabled           = true
    content_types_to_compress = [
      "application/json",
      "text/plain",
    ]
  }
}

# Secondary Story Generator SWA Route - uses consolidated secondary endpoint
resource "azurerm_cdn_frontdoor_route" "secondary_story_generator_swa" {
  count                           = var.enable_secondary_environment && var.enable_story_generator ? 1 : 0
  name                            = "secondary-story-generator-swa-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.secondary[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.secondary_story_generator_swa[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.secondary_story_generator_swa[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.secondary_story_generator_swa[0].id]

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

# =============================================================================
# Secondary Mystira.App Services
# =============================================================================
# Note: Secondary Mystira.App services use the consolidated secondary endpoint.

resource "azurerm_cdn_frontdoor_origin_group" "secondary_mystira_app_api" {
  count                    = var.enable_secondary_environment && var.enable_mystira_app ? 1 : 0
  name                     = "secondary-mystira-app-api-origin-group"
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

resource "azurerm_cdn_frontdoor_origin_group" "secondary_mystira_app_swa" {
  count                    = var.enable_secondary_environment && var.enable_mystira_app ? 1 : 0
  name                     = "secondary-mystira-app-swa-origin-group"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  session_affinity_enabled = false

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

resource "azurerm_cdn_frontdoor_origin" "secondary_mystira_app_api" {
  count                         = var.enable_secondary_environment && var.enable_mystira_app ? 1 : 0
  name                          = "secondary-mystira-app-api-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.secondary_mystira_app_api[0].id

  enabled                        = true
  host_name                      = var.secondary_mystira_app_api_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.secondary_mystira_app_api_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_origin" "secondary_mystira_app_swa" {
  count                         = var.enable_secondary_environment && var.enable_mystira_app ? 1 : 0
  name                          = "secondary-mystira-app-swa-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.secondary_mystira_app_swa[0].id

  enabled                        = true
  host_name                      = var.secondary_mystira_app_swa_backend_address
  http_port                      = 80
  https_port                     = 443
  origin_host_header             = var.secondary_mystira_app_swa_backend_address
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_custom_domain" "secondary_mystira_app_api" {
  count                    = var.enable_secondary_environment && var.enable_mystira_app ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}-app-api-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.secondary_custom_domain_mystira_app_api

  tls {
    certificate_type = "ManagedCertificate"
  }
}

resource "azurerm_cdn_frontdoor_custom_domain" "secondary_mystira_app_swa" {
  count                    = var.enable_secondary_environment && var.enable_mystira_app ? 1 : 0
  name                     = "${var.project_name}-${var.secondary_environment}-app-swa-domain"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.main.id
  dns_zone_id              = null
  host_name                = var.secondary_custom_domain_mystira_app_swa

  tls {
    certificate_type = "ManagedCertificate"
  }
}

# Secondary Mystira.App API Route - uses consolidated secondary endpoint
resource "azurerm_cdn_frontdoor_route" "secondary_mystira_app_api" {
  count                           = var.enable_secondary_environment && var.enable_mystira_app ? 1 : 0
  name                            = "secondary-mystira-app-api-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.secondary[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.secondary_mystira_app_api[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.secondary_mystira_app_api[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.secondary_mystira_app_api[0].id]

  supported_protocols    = ["Http", "Https"]
  patterns_to_match      = ["/*"]
  forwarding_protocol    = "HttpsOnly"
  https_redirect_enabled = true
  link_to_default_domain = true

  cache {
    query_string_caching_behavior = "UseQueryString"
    compression_enabled           = true
    content_types_to_compress = [
      "application/json",
      "text/plain",
    ]
  }
}

# Secondary Mystira.App SWA Route - uses consolidated secondary endpoint
resource "azurerm_cdn_frontdoor_route" "secondary_mystira_app_swa" {
  count                           = var.enable_secondary_environment && var.enable_mystira_app ? 1 : 0
  name                            = "secondary-mystira-app-swa-route"
  cdn_frontdoor_endpoint_id       = azurerm_cdn_frontdoor_endpoint.secondary[0].id
  cdn_frontdoor_origin_group_id   = azurerm_cdn_frontdoor_origin_group.secondary_mystira_app_swa[0].id
  cdn_frontdoor_origin_ids        = [azurerm_cdn_frontdoor_origin.secondary_mystira_app_swa[0].id]
  cdn_frontdoor_custom_domain_ids = [azurerm_cdn_frontdoor_custom_domain.secondary_mystira_app_swa[0].id]

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
        "image/svg+xml",
        "text/css",
        "text/html",
        "text/javascript",
        "text/plain",
      ]
    }
  }
}
