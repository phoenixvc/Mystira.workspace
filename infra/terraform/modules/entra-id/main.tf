# Entra ID (Azure AD) Authentication Module
# Terraform module for managing Mystira app registrations and service principals
#
# This module creates:
# - Admin API app registration with exposed API scopes
# - Admin UI app registration with API permissions
# - App roles for authorization
# - Service principal for the applications

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.47"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"
    }
  }
}

# Data sources
data "azuread_client_config" "current" {}

data "azurerm_client_config" "current" {}

# Local variables
locals {
  admin_api_identifier_uri = "api://mystira-admin-api-${var.environment}"

  common_tags = merge(var.tags, {
    Component   = "entra-id"
    Environment = var.environment
    ManagedBy   = "terraform"
    Project     = "Mystira"
  })

  # Admin API scopes
  api_scopes = {
    "Admin.Read" = {
      description                = "Read admin data"
      admin_consent_description  = "Allows the app to read admin data"
      admin_consent_display_name = "Read admin data"
      enabled                    = true
      type                       = "Admin"
      value                      = "Admin.Read"
    }
    "Admin.Write" = {
      description                = "Modify admin data"
      admin_consent_description  = "Allows the app to modify admin data"
      admin_consent_display_name = "Modify admin data"
      enabled                    = true
      type                       = "Admin"
      value                      = "Admin.Write"
    }
    "Users.Manage" = {
      description                = "Manage platform users"
      admin_consent_description  = "Allows the app to manage platform users"
      admin_consent_display_name = "Manage users"
      enabled                    = true
      type                       = "Admin"
      value                      = "Users.Manage"
    }
    "Content.Moderate" = {
      description                = "Moderate content"
      admin_consent_description  = "Allows the app to moderate content"
      admin_consent_display_name = "Moderate content"
      enabled                    = true
      type                       = "Admin"
      value                      = "Content.Moderate"
    }
  }

  # App roles
  app_roles = {
    "Admin" = {
      description  = "Full admin access to the platform"
      display_name = "Admin"
      enabled      = true
      value        = "Admin"
    }
    "SuperAdmin" = {
      description  = "System-level administrative access"
      display_name = "Super Admin"
      enabled      = true
      value        = "SuperAdmin"
    }
    "Moderator" = {
      description  = "Content moderation access"
      display_name = "Moderator"
      enabled      = true
      value        = "Moderator"
    }
    "Viewer" = {
      description  = "Read-only access to admin data"
      display_name = "Viewer"
      enabled      = true
      value        = "Viewer"
    }
  }
}

# =============================================================================
# Admin API App Registration
# =============================================================================

resource "azuread_application" "admin_api" {
  display_name     = "Mystira Admin API (${var.environment})"
  identifier_uris  = [local.admin_api_identifier_uri]
  sign_in_audience = "AzureADMyOrg" # Single tenant

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

  # App roles for authorization
  dynamic "app_role" {
    for_each = local.app_roles
    content {
      allowed_member_types = ["User", "Application"]
      description          = app_role.value.description
      display_name         = app_role.value.display_name
      enabled              = app_role.value.enabled
      id                   = random_uuid.role_ids[app_role.key].result
      value                = app_role.value.value
    }
  }

  # Optional claims
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

  # Web configuration (for API)
  web {
    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = false
    }
  }

  tags = [var.environment, "api", "mystira"]
}

# Service principal for Admin API
resource "azuread_service_principal" "admin_api" {
  client_id                    = azuread_application.admin_api.client_id
  app_role_assignment_required = false

  tags = [var.environment, "api", "mystira"]
}

# =============================================================================
# Admin UI App Registration
# =============================================================================

resource "azuread_application" "admin_ui" {
  display_name     = "Mystira Admin UI (${var.environment})"
  sign_in_audience = "AzureADMyOrg" # Single tenant

  # Single-page application (SPA) configuration
  single_page_application {
    redirect_uris = var.admin_ui_redirect_uris
  }

  # Required resource access (to Admin API)
  required_resource_access {
    resource_app_id = azuread_application.admin_api.client_id

    dynamic "resource_access" {
      for_each = local.api_scopes
      content {
        id   = random_uuid.scope_ids[resource_access.key].result
        type = "Scope"
      }
    }
  }

  # Microsoft Graph permissions (for user info)
  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" # User.Read
      type = "Scope"
    }
    resource_access {
      id   = "64a6cdd6-aab1-4aaf-94b8-3cc8405e90d0" # email
      type = "Scope"
    }
    resource_access {
      id   = "14dad69e-099b-42c9-810b-d002981feec1" # profile
      type = "Scope"
    }
    resource_access {
      id   = "7427e0e9-2fba-42fe-b0c0-848c9e6a8182" # offline_access
      type = "Scope"
    }
    resource_access {
      id   = "37f7f235-527c-4136-accd-4a02d197296e" # openid
      type = "Scope"
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

  tags = [var.environment, "ui", "mystira", "spa"]
}

# Service principal for Admin UI
resource "azuread_service_principal" "admin_ui" {
  client_id                    = azuread_application.admin_ui.client_id
  app_role_assignment_required = false

  tags = [var.environment, "ui", "mystira"]
}

# =============================================================================
# Random UUIDs for scopes and roles
# =============================================================================

resource "random_uuid" "scope_ids" {
  for_each = local.api_scopes
}

resource "random_uuid" "role_ids" {
  for_each = local.app_roles
}

# =============================================================================
# Admin consent grant for UI to access API
# =============================================================================

resource "azuread_service_principal_delegated_permission_grant" "admin_ui_to_api" {
  service_principal_object_id          = azuread_service_principal.admin_ui.object_id
  resource_service_principal_object_id = azuread_service_principal.admin_api.object_id
  claim_values                         = [for k, v in local.api_scopes : v.value]
}
