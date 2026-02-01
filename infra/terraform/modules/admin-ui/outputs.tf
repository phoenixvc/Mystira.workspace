# Mystira Admin-UI Infrastructure Module - Outputs
# Module: infra/terraform/modules/admin-ui
# See main.tf for resource definitions and variables.tf for inputs

output "static_web_app_id" {
  description = "Static Web App ID"
  value       = azurerm_static_web_app.admin_ui.id
}

output "static_web_app_name" {
  description = "Static Web App name"
  value       = azurerm_static_web_app.admin_ui.name
}

output "static_web_app_default_hostname" {
  description = "Static Web App default hostname (use this for Front Door backend)"
  value       = azurerm_static_web_app.admin_ui.default_host_name
}

output "static_web_app_url" {
  description = "Static Web App URL"
  value       = "https://${azurerm_static_web_app.admin_ui.default_host_name}"
}

output "static_web_app_api_key" {
  description = "Static Web App API key for deployments (use in GitHub Actions)"
  value       = azurerm_static_web_app.admin_ui.api_key
  sensitive   = true
}

output "custom_domain" {
  description = "Static Web App custom domain (if enabled)"
  value       = var.enable_custom_domain && var.custom_domain != "" ? var.custom_domain : null
}
