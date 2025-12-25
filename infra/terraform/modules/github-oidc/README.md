# GitHub Actions OIDC Module

This module manages federated identity credentials for GitHub Actions CI/CD pipelines. It creates a dedicated Azure AD application and service principal with the necessary permissions, then configures OIDC federation for specified GitHub repositories.

## Features

- Creates a dedicated CI/CD Azure AD app registration per environment
- Configures federated credentials for multiple repositories
- Supports branch, tag, environment, and pull request triggers
- Assigns necessary Azure roles (Contributor, AcrPush, AKS Admin)
- Outputs all values needed for GitHub repository secrets

## Usage

```hcl
module "github_oidc" {
  source = "../../modules/github-oidc"

  environment = "dev"
  github_org  = "phoenixvc"

  # ACR and AKS for role assignments
  acr_id = module.shared_acr.acr_id
  aks_id = azurerm_kubernetes_cluster.main.id

  repositories = {
    # Kubernetes-deployed services
    "admin-api" = {
      name     = "Mystira.Admin.Api"
      branches = ["dev", "main"]
    }
    "chain" = {
      name     = "Mystira.Chain"
      branches = ["dev", "main"]
    }
    "publisher" = {
      name     = "Mystira.Publisher"
      branches = ["dev", "main"]
    }
    "story-generator" = {
      name     = "Mystira.StoryGenerator"
      branches = ["dev", "main"]
    }

    # App Service / SWA services
    "app" = {
      name     = "Mystira.App"
      branches = ["dev", "main"]
    }
    "devhub" = {
      name     = "Mystira.DevHub"
      branches = ["dev", "main"]
    }

    # Workspace (for infra deployments)
    "workspace" = {
      name         = "Mystira.workspace"
      branches     = ["dev", "main"]
      enable_tags  = true  # For release deployments
    }
  }
}
```

## After Applying

After running `terraform apply`, update GitHub repository secrets:

```bash
# Get the output values
terraform output github_oidc_secrets

# For each submodule repository, set these secrets:
# - AZURE_CLIENT_ID
# - AZURE_TENANT_ID
# - AZURE_SUBSCRIPTION_ID
```

## Inputs

| Name | Description | Type | Default |
|------|-------------|------|---------|
| environment | Deployment environment (dev, staging, prod) | string | - |
| github_org | GitHub organization name | string | "phoenixvc" |
| repositories | Map of repositories to configure | map(object) | {} |
| enable_subscription_contributor | Grant Contributor role | bool | true |
| acr_id | ACR resource ID for AcrPush role | string | "" |
| aks_id | AKS resource ID for Cluster Admin role | string | "" |

## Outputs

| Name | Description |
|------|-------------|
| cicd_client_id | CI/CD app client ID (for AZURE_CLIENT_ID) |
| tenant_id | Azure AD tenant ID (for AZURE_TENANT_ID) |
| subscription_id | Azure subscription ID (for AZURE_SUBSCRIPTION_ID) |
| github_secrets | All values needed for GitHub secrets |
| federated_credentials | Map of created federated credentials |

## How OIDC Works

1. GitHub Actions workflow requests an OIDC token from GitHub
2. Token contains claims about the repo, branch, and workflow
3. Azure AD validates the token against configured federated credentials
4. If valid, Azure AD issues an access token for the CI/CD service principal
5. Workflow uses access token to deploy to Azure

No secrets are stored in GitHub - authentication is based on trust between GitHub and Azure AD.
