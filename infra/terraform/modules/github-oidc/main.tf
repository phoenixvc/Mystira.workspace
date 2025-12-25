# GitHub Actions OIDC Module
# Manages federated identity credentials for GitHub Actions CI/CD
#
# This module creates:
# - A dedicated Azure AD app for CI/CD operations
# - A service principal with necessary Azure role assignments
# - Federated credentials for GitHub Actions OIDC authentication

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

data "azuread_client_config" "current" {}
data "azurerm_subscription" "current" {}

# =============================================================================
# CI/CD App Registration
# =============================================================================

resource "azuread_application" "cicd" {
  display_name = "Mystira CI/CD (${var.environment})"

  tags = [var.environment, "cicd", "github-actions", "mystira"]
}

resource "azuread_service_principal" "cicd" {
  client_id                    = azuread_application.cicd.client_id
  app_role_assignment_required = false

  tags = [var.environment, "cicd", "github-actions", "mystira"]
}

# =============================================================================
# Azure Role Assignments for CI/CD
# =============================================================================

# Contributor on subscription (for deployments)
resource "azurerm_role_assignment" "cicd_contributor" {
  count                = var.enable_subscription_contributor ? 1 : 0
  scope                = data.azurerm_subscription.current.id
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.cicd.object_id
  description          = "Allow GitHub Actions CI/CD to deploy resources"
}

# AcrPush for container registry
resource "azurerm_role_assignment" "cicd_acr_push" {
  count                = var.acr_id != "" ? 1 : 0
  scope                = var.acr_id
  role_definition_name = "AcrPush"
  principal_id         = azuread_service_principal.cicd.object_id
  description          = "Allow GitHub Actions CI/CD to push images to ACR"
}

# AKS Cluster Admin for kubectl operations
resource "azurerm_role_assignment" "cicd_aks_admin" {
  count                = var.aks_id != "" ? 1 : 0
  scope                = var.aks_id
  role_definition_name = "Azure Kubernetes Service Cluster Admin Role"
  principal_id         = azuread_service_principal.cicd.object_id
  description          = "Allow GitHub Actions CI/CD to manage AKS cluster"
}

# =============================================================================
# GitHub Actions Federated Identity Credentials
# =============================================================================

resource "azuread_application_federated_identity_credential" "github_actions" {
  for_each = { for cred in local.all_credentials : "${cred.repo}-${cred.ref_type}-${cred.ref_name}" => cred }

  application_id = azuread_application.cicd.id
  display_name   = each.value.display_name
  description    = "GitHub Actions OIDC for ${each.value.repo} (${each.value.ref_type}: ${each.value.ref_name})"
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = each.value.subject
}

locals {
  # Flatten the repository configurations into individual credentials
  all_credentials = flatten([
    for repo_key, repo in var.repositories : concat(
      # Branch-based credentials
      [for branch in repo.branches : {
        repo         = repo_key
        ref_type     = "branch"
        ref_name     = branch
        display_name = "github-${repo_key}-${branch}"
        subject      = "repo:${var.github_org}/${repo.name}:ref:refs/heads/${branch}"
      }],
      # Tag-based credentials (for releases)
      repo.enable_tags ? [{
        repo         = repo_key
        ref_type     = "tag"
        ref_name     = "releases"
        display_name = "github-${repo_key}-tags"
        subject      = "repo:${var.github_org}/${repo.name}:ref:refs/tags/*"
      }] : [],
      # Environment-based credentials
      [for env in repo.environments : {
        repo         = repo_key
        ref_type     = "environment"
        ref_name     = env
        display_name = "github-${repo_key}-env-${env}"
        subject      = "repo:${var.github_org}/${repo.name}:environment:${env}"
      }],
      # Pull request credentials (for PR deployments)
      repo.enable_pull_requests ? [{
        repo         = repo_key
        ref_type     = "pull_request"
        ref_name     = "pr"
        display_name = "github-${repo_key}-pr"
        subject      = "repo:${var.github_org}/${repo.name}:pull_request"
      }] : []
    )
  ])
}
