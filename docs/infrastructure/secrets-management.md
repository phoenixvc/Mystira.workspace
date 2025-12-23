# Secrets Management Guide

This guide documents how secrets are managed across Mystira's services. All secrets are auto-populated by Terraform modules - no manual configuration is required.

## Overview

Per [ADR-0017](../architecture/adr/0017-resource-group-organization-strategy.md), each service has its own Key Vault in its service-specific resource group. **All secrets are auto-populated by Terraform** from shared infrastructure modules.

## Key Vault Naming

| Service | Key Vault Name | Resource Group |
|---------|----------------|----------------|
| Story-Generator | `mys-{env}-story-kv-san` | `mys-{env}-story-rg-san` |
| Admin-API | `mys-{env}-adm-kv-san` | `mys-{env}-admin-rg-san` |
| Publisher | `mys-{env}-pub-kv-san` | `mys-{env}-publisher-rg-san` |
| Chain | `mys-{env}-chain-kv-san` | `mys-{env}-chain-rg-san` |

## Secrets by Service

### Story-Generator

| Secret Name | Source | Description |
|-------------|--------|-------------|
| `postgres-connection-string` | Shared PostgreSQL | Database connection string |
| `redis-connection-string` | Shared Redis | Cache connection string |
| `appinsights-connection-string` | Shared Monitoring | Application Insights connection |
| `azure-ai-endpoint` | Shared Azure AI | Azure AI Foundry endpoint URL |
| `azure-ai-api-key` | Shared Azure AI | Azure AI Foundry API key |

**GitHub Secrets Required:** None - all secrets are auto-populated from Terraform modules.

### Admin-API

| Secret Name | Source | Description |
|-------------|--------|-------------|
| `postgres-connection-string` | Shared PostgreSQL | Database connection string |
| `redis-connection-string` | Shared Redis | Cache connection string |
| `appinsights-connection-string` | Shared Monitoring | Application Insights connection |
| `azure-ad-tenant-id` | Entra ID Module | Azure AD tenant ID |
| `azure-ad-client-id` | Entra ID Module | Admin API client ID |
| `admin-ui-client-id` | Entra ID Module | Admin UI client ID |

**GitHub Secrets Required:** None - all secrets are auto-populated from Terraform modules.

### Publisher

| Secret Name | Source | Description |
|-------------|--------|-------------|
| `servicebus-connection-string` | Shared Service Bus | Message queue connection |
| `appinsights-connection-string` | Shared Monitoring | Application Insights connection |

**GitHub Secrets Required:** None - all secrets are auto-populated.

### Chain

| Secret Name | Source | Description |
|-------------|--------|-------------|
| `appinsights-connection-string` | Shared Monitoring | Application Insights connection |

**GitHub Secrets Required:** None - all secrets are auto-populated.

## Setting Up Secrets

### Step 1: Deploy Infrastructure

Run Terraform to create the infrastructure. All secrets are auto-populated during this step:

```bash
cd infra/terraform/environments/dev
terraform init
terraform apply
```

This automatically creates:
- All Key Vaults in service-specific RGs
- All secrets from shared modules (PostgreSQL, Redis, Service Bus, Monitoring, Azure AI, Entra ID)

### Step 2: Verify Secrets

Run the GitHub Actions workflow to verify all secrets are present:

1. Go to **Actions > Key Vault Secrets**
2. Click **Run workflow**
3. Select:
   - Environment: `dev`
   - Service: `all`
   - Action: `verify-secrets`

The workflow will report any missing secrets.

## Kubernetes Secret Sync

Kubernetes deployments reference secrets from Key Vault. Use the Azure Key Vault CSI Driver for automatic sync.

### Example: Story-Generator Deployment

```yaml
env:
  - name: ConnectionStrings__PostgreSQL
    valueFrom:
      secretKeyRef:
        name: mys-story-generator-secrets
        key: postgres_connection_string
  - name: Ai__AzureOpenAI__Endpoint
    valueFrom:
      secretKeyRef:
        name: mys-story-generator-secrets
        key: azure_ai_endpoint
  - name: Ai__AzureOpenAI__ApiKey
    valueFrom:
      secretKeyRef:
        name: mys-story-generator-secrets
        key: azure_ai_api_key
```

### Creating Kubernetes Secrets from Key Vault

```bash
# Get secrets from Key Vault
POSTGRES_CONN=$(az keyvault secret show --vault-name "mys-dev-story-kv-san" --name "postgres-connection-string" --query value -o tsv)
AZURE_AI_ENDPOINT=$(az keyvault secret show --vault-name "mys-dev-story-kv-san" --name "azure-ai-endpoint" --query value -o tsv)
AZURE_AI_KEY=$(az keyvault secret show --vault-name "mys-dev-story-kv-san" --name "azure-ai-api-key" --query value -o tsv)

# Create Kubernetes secret
kubectl create secret generic mys-story-generator-secrets \
  --from-literal=postgres_connection_string="$POSTGRES_CONN" \
  --from-literal=azure_ai_endpoint="$AZURE_AI_ENDPOINT" \
  --from-literal=azure_ai_api_key="$AZURE_AI_KEY" \
  -n mystira
```

## Secret Rotation

### Rotating Infrastructure Secrets

All secrets are managed by Terraform. To rotate:

1. Update the source resource (e.g., regenerate PostgreSQL password, rotate Azure AI key)
2. Run `terraform apply` to update Key Vault secrets
3. Restart affected pods to pick up new secrets

```bash
cd infra/terraform/environments/dev
terraform apply

# Restart pods
kubectl rollout restart deployment/mys-story-generator -n mystira
```

## Troubleshooting

### Secret Not Found in Key Vault

1. Check if Terraform has been applied:
   ```bash
   cd infra/terraform/environments/dev
   terraform plan
   ```

2. Check if the Key Vault exists:
   ```bash
   az keyvault show --name "mys-dev-story-kv-san"
   ```

3. List secrets in the Key Vault:
   ```bash
   az keyvault secret list --vault-name "mys-dev-story-kv-san" -o table
   ```

### Pod Cannot Access Secrets

1. Verify Kubernetes secret exists:
   ```bash
   kubectl get secret mys-story-generator-secrets -n mystira
   ```

2. Check workload identity is configured:
   ```bash
   kubectl describe pod -n mystira | grep -A5 "azure.workload.identity"
   ```

## Terraform Modules

All secrets are auto-populated from these Terraform modules:

| Module | Secrets Provided |
|--------|------------------|
| `shared/postgresql` | postgres-connection-string |
| `shared/redis` | redis-connection-string |
| `shared/servicebus` | servicebus-connection-string |
| `shared/monitoring` | appinsights-connection-string |
| `shared/azure-ai` | azure-ai-endpoint, azure-ai-api-key |
| `entra-id` | azure-ad-tenant-id, azure-ad-client-id, admin-ui-client-id |

## Global Azure Secrets (CI/CD only)

These secrets are only needed in GitHub Actions for infrastructure deployments:

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Service principal client ID for deployments |
| `AZURE_TENANT_ID` | Azure tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

## Related Documentation

- [Infrastructure Guide](./infrastructure.md)
- [Shared Resources](./shared-resources.md)
- [ADR-0017: Resource Group Organization Strategy](../architecture/adr/0017-resource-group-organization-strategy.md)
- [Kubernetes Secrets Management](./kubernetes-secrets-management.md)
- [Entra ID Best Practices](./entra-id-best-practices.md)
