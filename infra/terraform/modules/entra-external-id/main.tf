# Microsoft Entra External ID Consumer Authentication Module
# Terraform module for managing External ID app registrations
#
# NOTE: Microsoft Entra External ID tenant creation must be done manually
# or via Azure CLI. This module manages app registrations within an existing
# external tenant.

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.47"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"
    }
  }
}

# Local variables
locals {
  # External ID uses api:// scheme for app ID URIs
  api_identifier_uri = "api://${var.tenant_name}-api"

  common_tags = [var.environment, "external-id", "mystira", "consumer"]

  # API scopes for consumer operations
  api_scopes = {
    "API.Access" = {
      description                = "Access the Mystira API"
      admin_consent_description  = "Allows the app to access the Mystira API on behalf of the signed-in user"
      admin_consent_display_name = "Access Mystira API"
      enabled                    = true
      type                       = "User"
      value                      = "API.Access"
    }
    "Stories.Read" = {
      description                = "Read user stories"
      admin_consent_description  = "Allows the app to read the user's stories"
      admin_consent_display_name = "Read stories"
      enabled                    = true
      type                       = "User"
      value                      = "Stories.Read"
    }
    "Stories.Write" = {
      description                = "Create and edit stories"
      admin_consent_description  = "Allows the app to create and edit stories on behalf of the user"
      admin_consent_display_name = "Write stories"
      enabled                    = true
      type                       = "User"
      value                      = "Stories.Write"
    }
    "Profile.Read" = {
      description                = "Read user profile"
      admin_consent_description  = "Allows the app to read the user's profile"
      admin_consent_display_name = "Read profile"
      enabled                    = true
      type                       = "User"
      value                      = "Profile.Read"
    }
  }
}

# =============================================================================
# Public API App Registration (External ID)
# =============================================================================

resource "azuread_application" "public_api" {
  display_name     = "Mystira Public API (${var.environment})"
  identifier_uris  = [local.api_identifier_uri]
  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  # Expose API scopes
  api {
    mapped_claims_enabled          = true
    requested_access_token_version = 2

    dynamic "oauth2_permission_scope" {
      for_each = local.api_scopes
      content {
        admin_consent_description  = oauth2_permission_scope.value.admin_consent_description
        admin_consent_display_name = oauth2_permission_scope.value.admin_consent_display_name
        enabled                    = oauth2_permission_scope.value.enabled
        id                         = random_uuid.scope_ids[oauth2_permission_scope.key].result
        type                       = oauth2_permission_scope.value.type
        value                      = oauth2_permission_scope.value.value
      }
    }
  }

  # Optional claims for tokens
  optional_claims {
    access_token {
      name = "email"
    }
    access_token {
      name = "family_name"
    }
    access_token {
      name = "given_name"
    }
    id_token {
      name = "email"
    }
    id_token {
      name = "family_name"
    }
    id_token {
      name = "given_name"
    }
  }

  tags = local.common_tags
}

# Service principal for Public API
resource "azuread_service_principal" "public_api" {
  client_id                    = azuread_application.public_api.client_id
  app_role_assignment_required = false

  tags = local.common_tags
}

# =============================================================================
# PWA/SPA App Registration (External ID)
# =============================================================================

resource "azuread_application" "pwa" {
  display_name     = "Mystira PWA (${var.environment})"
  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  # Single-page application (SPA) configuration
  single_page_application {
    redirect_uris = var.pwa_redirect_uris
  }

  # Required resource access (to Public API)
  required_resource_access {
    resource_app_id = azuread_application.public_api.client_id

    dynamic "resource_access" {
      for_each = local.api_scopes
      content {
        id   = random_uuid.scope_ids[resource_access.key].result
        type = "Scope"
      }
    }
  }

  # Optional claims
  optional_claims {
    access_token {
      name = "email"
    }
    id_token {
      name = "email"
    }
    id_token {
      name = "family_name"
    }
    id_token {
      name = "given_name"
    }
  }

  tags = local.common_tags
}

# Service principal for PWA
resource "azuread_service_principal" "pwa" {
  client_id                    = azuread_application.pwa.client_id
  app_role_assignment_required = false

  tags = local.common_tags
}

# =============================================================================
# Mobile App Registration (External ID)
# =============================================================================

resource "azuread_application" "mobile" {
  count            = length(var.mobile_redirect_uris) > 0 ? 1 : 0
  display_name     = "Mystira Mobile App (${var.environment})"
  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  # Public client configuration for mobile apps
  public_client {
    redirect_uris = var.mobile_redirect_uris
  }

  # Required resource access (to Public API)
  required_resource_access {
    resource_app_id = azuread_application.public_api.client_id

    dynamic "resource_access" {
      for_each = local.api_scopes
      content {
        id   = random_uuid.scope_ids[resource_access.key].result
        type = "Scope"
      }
    }
  }

  # Optional claims
  optional_claims {
    access_token {
      name = "email"
    }
    id_token {
      name = "email"
    }
    id_token {
      name = "family_name"
    }
    id_token {
      name = "given_name"
    }
  }

  tags = local.common_tags
}

# Service principal for Mobile App
resource "azuread_service_principal" "mobile" {
  count                        = length(var.mobile_redirect_uris) > 0 ? 1 : 0
  client_id                    = azuread_application.mobile[0].client_id
  app_role_assignment_required = false

  tags = local.common_tags
}

# =============================================================================
# Random UUIDs for scopes
# =============================================================================

resource "random_uuid" "scope_ids" {
  for_each = local.api_scopes
}
