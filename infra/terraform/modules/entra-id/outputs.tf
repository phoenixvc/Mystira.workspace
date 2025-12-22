# Entra ID Module Outputs

# Admin API outputs
output "admin_api_client_id" {
  description = "Admin API application (client) ID"
  value       = azuread_application.admin_api.client_id
}

output "admin_api_object_id" {
  description = "Admin API application object ID"
  value       = azuread_application.admin_api.object_id
}

output "admin_api_identifier_uri" {
  description = "Admin API identifier URI (audience)"
  value       = local.admin_api_identifier_uri
}

output "admin_api_service_principal_id" {
  description = "Admin API service principal object ID"
  value       = azuread_service_principal.admin_api.object_id
}

# Admin UI outputs
output "admin_ui_client_id" {
  description = "Admin UI application (client) ID"
  value       = azuread_application.admin_ui.client_id
}

output "admin_ui_object_id" {
  description = "Admin UI application object ID"
  value       = azuread_application.admin_ui.object_id
}

output "admin_ui_service_principal_id" {
  description = "Admin UI service principal object ID"
  value       = azuread_service_principal.admin_ui.object_id
}

# Tenant information
output "tenant_id" {
  description = "Azure AD tenant ID"
  value       = data.azuread_client_config.current.tenant_id
}

# API Scopes
output "api_scopes" {
  description = "Map of API scope names to their IDs"
  value = {
    for k, v in local.api_scopes : k => {
      id    = random_uuid.scope_ids[k].result
      value = v.value
      uri   = "${local.admin_api_identifier_uri}/${v.value}"
    }
  }
}

# App Roles
output "app_roles" {
  description = "Map of app role names to their IDs"
  value = {
    for k, v in local.app_roles : k => {
      id    = random_uuid.role_ids[k].result
      value = v.value
    }
  }
}

# Configuration for applications
output "admin_api_config" {
  description = "Configuration values for Admin API (appsettings.json)"
  value = {
    AzureAd = {
      Instance = "https://login.microsoftonline.com/"
      TenantId = data.azuread_client_config.current.tenant_id
      ClientId = azuread_application.admin_api.client_id
      Audience = local.admin_api_identifier_uri
    }
  }
}

output "admin_ui_config" {
  description = "Configuration values for Admin UI (environment variables)"
  value = {
    VITE_AZURE_CLIENT_ID = azuread_application.admin_ui.client_id
    VITE_AZURE_TENANT_ID = data.azuread_client_config.current.tenant_id
    VITE_AZURE_AUTHORITY = "https://login.microsoftonline.com/${data.azuread_client_config.current.tenant_id}"
    VITE_API_SCOPE       = "${local.admin_api_identifier_uri}/Admin.Read"
  }
}
