# GitHub OIDC Module Outputs

output "cicd_client_id" {
  description = "CI/CD application client ID (for AZURE_CLIENT_ID secret)"
  value       = azuread_application.cicd.client_id
}

output "cicd_object_id" {
  description = "CI/CD application object ID"
  value       = azuread_application.cicd.object_id
}

output "cicd_service_principal_id" {
  description = "CI/CD service principal object ID"
  value       = azuread_service_principal.cicd.object_id
}

output "tenant_id" {
  description = "Azure AD tenant ID (for AZURE_TENANT_ID secret)"
  value       = data.azuread_client_config.current.tenant_id
}

output "subscription_id" {
  description = "Azure subscription ID (for AZURE_SUBSCRIPTION_ID secret)"
  value       = data.azurerm_subscription.current.subscription_id
}

output "federated_credentials" {
  description = "Map of created federated credentials"
  value = {
    for key, cred in azuread_application_federated_identity_credential.github_actions : key => {
      id           = cred.id
      display_name = cred.display_name
      subject      = cred.subject
    }
  }
}

output "credential_count" {
  description = "Number of federated credentials created"
  value       = length(azuread_application_federated_identity_credential.github_actions)
}

# GitHub Secrets Configuration (copy these to each submodule repo)
output "github_secrets" {
  description = "Values to configure as GitHub repository secrets"
  value = {
    AZURE_CLIENT_ID       = azuread_application.cicd.client_id
    AZURE_TENANT_ID       = data.azuread_client_config.current.tenant_id
    AZURE_SUBSCRIPTION_ID = data.azurerm_subscription.current.subscription_id
  }
}
