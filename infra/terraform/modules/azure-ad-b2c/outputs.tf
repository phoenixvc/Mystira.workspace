# Azure AD B2C Module Outputs

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

# Mobile App outputs
output "mobile_client_id" {
  description = "Mobile application (client) ID"
  value       = length(azuread_application.mobile) > 0 ? azuread_application.mobile[0].client_id : ""
}

output "mobile_object_id" {
  description = "Mobile application object ID"
  value       = length(azuread_application.mobile) > 0 ? azuread_application.mobile[0].object_id : ""
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

# B2C Configuration for Public API
output "public_api_config" {
  description = "Configuration values for Public API (appsettings.json)"
  value = {
    AzureAdB2C = {
      Instance          = "https://${var.b2c_tenant_name}.b2clogin.com"
      Domain            = "${var.b2c_tenant_name}.onmicrosoft.com"
      ClientId          = azuread_application.public_api.client_id
      SignUpSignInPolicy = var.sign_up_sign_in_policy
    }
  }
}

# B2C Configuration for PWA
output "pwa_config" {
  description = "Configuration values for PWA (environment variables)"
  value = {
    VITE_B2C_CLIENT_ID     = azuread_application.pwa.client_id
    VITE_B2C_TENANT_NAME   = var.b2c_tenant_name
    VITE_B2C_AUTHORITY     = "https://${var.b2c_tenant_name}.b2clogin.com/${var.b2c_tenant_name}.onmicrosoft.com/${var.sign_up_sign_in_policy}"
    VITE_B2C_KNOWN_AUTHORITY = "https://${var.b2c_tenant_name}.b2clogin.com"
    VITE_B2C_API_SCOPE     = "${local.api_identifier_uri}/API.Access"
    VITE_B2C_REDIRECT_URI  = length(var.pwa_redirect_uris) > 0 ? var.pwa_redirect_uris[0] : ""
  }
}

# B2C Configuration for Mobile App
output "mobile_config" {
  description = "Configuration values for Mobile App (React Native / Expo)"
  value = length(azuread_application.mobile) > 0 ? {
    EXPO_PUBLIC_B2C_CLIENT_ID     = azuread_application.mobile[0].client_id
    EXPO_PUBLIC_B2C_TENANT_NAME   = var.b2c_tenant_name
    EXPO_PUBLIC_B2C_AUTHORITY     = "https://${var.b2c_tenant_name}.b2clogin.com/${var.b2c_tenant_name}.onmicrosoft.com/${var.sign_up_sign_in_policy}"
    EXPO_PUBLIC_B2C_KNOWN_AUTHORITY = "https://${var.b2c_tenant_name}.b2clogin.com"
    EXPO_PUBLIC_B2C_API_SCOPE     = "${local.api_identifier_uri}/API.Access"
    EXPO_PUBLIC_B2C_REDIRECT_URI  = length(var.mobile_redirect_uris) > 0 ? var.mobile_redirect_uris[0] : ""
  } : {}
}

# User flow URLs
output "user_flow_urls" {
  description = "B2C user flow URLs"
  value = {
    sign_up_sign_in = "https://${var.b2c_tenant_name}.b2clogin.com/${var.b2c_tenant_name}.onmicrosoft.com/${var.sign_up_sign_in_policy}/oauth2/v2.0/authorize"
    password_reset  = "https://${var.b2c_tenant_name}.b2clogin.com/${var.b2c_tenant_name}.onmicrosoft.com/${var.password_reset_policy}/oauth2/v2.0/authorize"
    profile_edit    = "https://${var.b2c_tenant_name}.b2clogin.com/${var.b2c_tenant_name}.onmicrosoft.com/${var.profile_edit_policy}/oauth2/v2.0/authorize"
  }
}
