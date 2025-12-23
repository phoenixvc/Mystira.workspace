# Microsoft Entra External ID Module Outputs

# Public API outputs
output "public_api_client_id" {
  description = "Public API application (client) ID"
  value       = azuread_application.public_api.client_id
}

output "public_api_object_id" {
  description = "Public API application object ID"
  value       = azuread_application.public_api.object_id
}

output "public_api_identifier_uri" {
  description = "Public API identifier URI"
  value       = local.api_identifier_uri
}

# PWA outputs
output "pwa_client_id" {
  description = "PWA application (client) ID"
  value       = azuread_application.pwa.client_id
}

output "pwa_object_id" {
  description = "PWA application object ID"
  value       = azuread_application.pwa.object_id
}

# API Scopes
output "api_scopes" {
  description = "Map of API scope names to their IDs and URIs"
  value = {
    for k, v in local.api_scopes : k => {
      id    = random_uuid.scope_ids[k].result
      value = v.value
      uri   = "${local.api_identifier_uri}/${v.value}"
    }
  }
}

# Configuration for Public API
output "public_api_config" {
  description = "Configuration values for Public API (appsettings.json)"
  value = {
    MicrosoftEntraExternalId = {
      Instance  = "https://${var.tenant_name}.ciamlogin.com"
      Domain    = "${var.tenant_name}.onmicrosoft.com"
      TenantId  = var.tenant_id
      ClientId  = azuread_application.public_api.client_id
    }
  }
}

# Configuration for PWA
output "pwa_config" {
  description = "Configuration values for PWA (environment variables)"
  value = {
    VITE_ENTRA_CLIENT_ID     = azuread_application.pwa.client_id
    VITE_ENTRA_TENANT_ID     = var.tenant_id
    VITE_ENTRA_AUTHORITY     = "https://${var.tenant_name}.ciamlogin.com/${var.tenant_id}/v2.0"
    VITE_ENTRA_API_SCOPE     = "${local.api_identifier_uri}/API.Access"
    VITE_ENTRA_REDIRECT_URI  = length(var.pwa_redirect_uris) > 0 ? var.pwa_redirect_uris[0] : ""
  }
}

# Configuration for Mobile App
output "mobile_config" {
  description = "Configuration values for Mobile App (React Native / Expo)"
  value = length(azuread_application.mobile) > 0 ? {
    EXPO_PUBLIC_ENTRA_CLIENT_ID   = azuread_application.mobile[0].client_id
    EXPO_PUBLIC_ENTRA_TENANT_ID   = var.tenant_id
    EXPO_PUBLIC_ENTRA_AUTHORITY   = "https://${var.tenant_name}.ciamlogin.com/${var.tenant_id}/v2.0"
    EXPO_PUBLIC_ENTRA_API_SCOPE   = "${local.api_identifier_uri}/API.Access"
    EXPO_PUBLIC_ENTRA_REDIRECT_URI = length(var.mobile_redirect_uris) > 0 ? var.mobile_redirect_uris[0] : ""
  } : {}
}

# Authentication endpoints
output "auth_endpoints" {
  description = "Microsoft Entra External ID authentication endpoints"
  value = {
    authority          = "https://${var.tenant_name}.ciamlogin.com/${var.tenant_id}/v2.0"
    token_endpoint     = "https://${var.tenant_name}.ciamlogin.com/${var.tenant_id}/oauth2/v2.0/token"
    authorize_endpoint = "https://${var.tenant_name}.ciamlogin.com/${var.tenant_id}/oauth2/v2.0/authorize"
    logout_endpoint    = "https://${var.tenant_name}.ciamlogin.com/${var.tenant_id}/oauth2/v2.0/logout"
  }
}
