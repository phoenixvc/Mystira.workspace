# Azure OIDC Authentication Migration Guide

This document describes the migration from Azure service principal credentials (`MYSTIRA_AZURE_CREDENTIALS`) to OIDC (Workload Identity Federation) authentication.

## Overview

OIDC (OpenID Connect) authentication, also known as Workload Identity Federation, is the recommended approach for authenticating GitHub Actions to Azure. It eliminates the need for long-lived secrets.

### Benefits of OIDC

| Aspect | Service Principal (Legacy) | OIDC (Recommended) |
|--------|---------------------------|-------------------|
| Secret Rotation | Manual, every 1-2 years | Not required |
| Credential Exposure | Risk if secret leaks | No secrets to leak |
| Secret Management | Complex JSON credential | Three simple IDs |
| Token Lifetime | Long-lived (configurable) | Short-lived (auto-expires) |
| Audit Trail | Limited | Full Azure AD audit logs |

## Migration Status

All production workflows have been migrated to OIDC authentication. The reusable workflow `_azure-login.yml` supports both methods for backwards compatibility during transition.

### Migrated Workflows

- `production-release.yml`
- `staging-release.yml`
- `infra-deploy.yml`
- `infra-terragrunt-deploy.yml`
- `infra-terragrunt-validate.yml`
- `infra-drift-detection.yml`
- `bind-custom-domains.yml`
- `deploy-admin-ui-swa.yml`
- `mystira-app-api-cicd-prod.yml`
- `mystira-app-api-rollback.yml`
- `submodule-deploy-dev.yml`
- `submodule-deploy-dev-appservice.yml`

## Prerequisites

### 1. Azure AD App Registration

Ensure you have an Azure AD App Registration with Federated Credentials configured for GitHub.

```bash
# Get your App Registration Client ID
az ad app list --display-name "Mystira GitHub Actions" --query "[].appId" -o tsv
```

### 2. Configure Federated Credentials

In Azure Portal:
1. Navigate to **Azure Active Directory** > **App Registrations**
2. Select your app registration
3. Go to **Certificates & secrets** > **Federated credentials**
4. Add credential for your repository:
   - Organization: `phoenixvc`
   - Repository: `Mystira.workspace`
   - Entity type: `Environment` (for environment-scoped) or `Branch` (for branch-scoped)
   - Subject identifier: Based on your selection

Alternatively, use Azure CLI:

```bash
# Create federated credential for production environment
az ad app federated-credential create \
  --id <app-object-id> \
  --parameters '{
    "name": "github-mystira-production",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:phoenixvc/Mystira.workspace:environment:production",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# Create federated credential for main branch
az ad app federated-credential create \
  --id <app-object-id> \
  --parameters '{
    "name": "github-mystira-main",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:phoenixvc/Mystira.workspace:ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'
```

### 3. GitHub Repository Secrets

Configure these secrets in your GitHub repository:

| Secret | Description | Example |
|--------|-------------|---------|
| `AZURE_CLIENT_ID` | Azure AD App Registration Client ID | `12345678-1234-1234-1234-123456789abc` |
| `AZURE_TENANT_ID` | Azure AD Tenant ID | `87654321-4321-4321-4321-cba987654321` |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID | `aaaabbbb-cccc-dddd-eeee-ffff00001111` |

## Usage in Workflows

### Direct OIDC Login

```yaml
jobs:
  deploy:
    runs-on: ubuntu-latest
    permissions:
      id-token: write  # Required for OIDC
      contents: read
    steps:
      - name: Azure Login (OIDC)
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

### Using Reusable Workflow

```yaml
jobs:
  login:
    uses: ./.github/workflows/_azure-login.yml
    with:
      environment: production
      set_terraform_env: true
    secrets:
      AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
      AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
      AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

### Terraform with OIDC

When using Terraform with OIDC, set these environment variables:

```yaml
- name: Set Terraform environment variables
  run: |
    {
      echo "ARM_CLIENT_ID=${{ secrets.AZURE_CLIENT_ID }}"
      echo "ARM_SUBSCRIPTION_ID=${{ secrets.AZURE_SUBSCRIPTION_ID }}"
      echo "ARM_TENANT_ID=${{ secrets.AZURE_TENANT_ID }}"
      echo "ARM_USE_OIDC=true"
    } >> "$GITHUB_ENV"
```

**Important**: Use `ARM_USE_OIDC=true` instead of `ARM_CLIENT_SECRET` to enable OIDC authentication for the AzureRM provider.

## Permissions Required

Jobs using OIDC must have the `id-token: write` permission:

```yaml
jobs:
  my-job:
    runs-on: ubuntu-latest
    permissions:
      id-token: write   # Required for OIDC
      contents: read    # Typically needed for checkout
```

If your workflow has a top-level `permissions` block, ensure it includes `id-token: write`, or add it at the job level.

## Troubleshooting

### Error: "AADSTS70021: No matching federated identity record found"

This error occurs when the federated credential doesn't match the token claims.

**Solutions:**
1. Verify the subject identifier matches your trigger context
2. Check if you're using environment-scoped vs branch-scoped credentials
3. Ensure the repository name is correct (case-sensitive)

### Error: "The template is not valid"

Ensure the `permissions` block is correctly indented:

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    permissions:      # At job level
      id-token: write
      contents: read
```

### Error: "Audience validation failed"

Ensure the audience in your federated credential is set to `api://AzureADTokenExchange`.

## Deprecation of Service Principal

The `MYSTIRA_AZURE_CREDENTIALS` secret is deprecated. The reusable `_azure-login.yml` workflow will emit warnings when legacy credentials are used.

### Timeline

1. **Current**: Both methods supported, warnings for legacy usage
2. **Future**: Legacy support removed, OIDC required

## Best Practices

1. **Use Environment-Scoped Credentials**: Create separate federated credentials for `dev`, `staging`, and `production` environments
2. **Principle of Least Privilege**: Grant only necessary Azure RBAC roles
3. **Monitor Usage**: Review Azure AD sign-in logs for the service principal
4. **Regular Audits**: Run the secret rotation check workflow to verify configuration

## Related Documentation

- [Azure OIDC Authentication](https://learn.microsoft.com/en-us/azure/active-directory/workload-identities/workload-identity-federation)
- [GitHub OIDC with Azure](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-azure)
- [azure/login Action](https://github.com/Azure/login)
- [Terraform AzureRM OIDC](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/guides/service_principal_oidc)
