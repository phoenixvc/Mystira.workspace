# Shared Identity and RBAC Module
# Configures managed identity role assignments for Azure services

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
}

data "azurerm_client_config" "current" {}

# =============================================================================
# AKS to ACR Role Assignment (AcrPull)
# Allows AKS to pull images from the shared container registry
# =============================================================================

resource "azurerm_role_assignment" "aks_acr_pull" {
  count                = var.aks_principal_id != "" && var.acr_id != "" ? 1 : 0
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = var.aks_principal_id
  description          = "Allow AKS cluster to pull images from ACR"
}

# =============================================================================
# CI/CD to ACR Role Assignment (AcrPush)
# Allows CI/CD pipelines to push images to the container registry
# =============================================================================

resource "azurerm_role_assignment" "cicd_acr_push" {
  count                = var.cicd_principal_id != "" && var.acr_id != "" ? 1 : 0
  scope                = var.acr_id
  role_definition_name = "AcrPush"
  principal_id         = var.cicd_principal_id
  description          = "Allow CI/CD pipeline to push images to ACR"
}

# =============================================================================
# Service Managed Identities to Key Vault
# Grants read access to Key Vault secrets for service identities
# =============================================================================

resource "azurerm_role_assignment" "service_key_vault_reader" {
  for_each = { for k, v in var.service_identities : k => v if v.key_vault_id != "" }

  scope                = each.value.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = each.value.principal_id
  description          = "Allow ${each.key} service to read Key Vault secrets"
}

# =============================================================================
# Service Managed Identities to Shared PostgreSQL
# Allows services to connect to PostgreSQL using Azure AD authentication
# =============================================================================

resource "azurerm_role_assignment" "service_postgres_reader" {
  for_each = { for k, v in var.service_identities : k => v if v.postgres_server_id != "" && v.postgres_role == "reader" }

  scope                = each.value.postgres_server_id
  role_definition_name = "Reader"
  principal_id         = each.value.principal_id
  description          = "Allow ${each.key} service reader access to PostgreSQL"
}

# =============================================================================
# Service Managed Identities to Shared Redis
# Allows services to connect to Redis using managed identity
# =============================================================================

resource "azurerm_role_assignment" "service_redis_contributor" {
  for_each = { for k, v in var.service_identities : k => v if v.redis_cache_id != "" }

  scope                = each.value.redis_cache_id
  role_definition_name = "Redis Cache Contributor"
  principal_id         = each.value.principal_id
  description          = "Allow ${each.key} service to access Redis cache"
}

# =============================================================================
# Log Analytics Workspace Contributor
# Allows services to write logs and metrics
# =============================================================================

resource "azurerm_role_assignment" "service_log_analytics" {
  for_each = { for k, v in var.service_identities : k => v if v.log_analytics_workspace_id != "" }

  scope                = each.value.log_analytics_workspace_id
  role_definition_name = "Log Analytics Contributor"
  principal_id         = each.value.principal_id
  description          = "Allow ${each.key} service to write to Log Analytics"
}

# =============================================================================
# Storage Account Access
# For services that need blob storage access
# =============================================================================

resource "azurerm_role_assignment" "service_storage_blob" {
  for_each = { for k, v in var.service_identities : k => v if v.storage_account_id != "" }

  scope                = each.value.storage_account_id
  role_definition_name = var.storage_role
  principal_id         = each.value.principal_id
  description          = "Allow ${each.key} service ${var.storage_role} access to storage"
}

# =============================================================================
# AKS Workload Identity Federation
# For services running in AKS that need Azure AD authentication
# =============================================================================

resource "azurerm_federated_identity_credential" "workload_identity" {
  for_each = { for k, v in var.workload_identities : k => v }

  name                = "${each.key}-federated-credential"
  resource_group_name = var.resource_group_name
  parent_id           = each.value.identity_id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = each.value.aks_oidc_issuer_url
  subject             = "system:serviceaccount:${each.value.namespace}:${each.value.service_account}"
}
