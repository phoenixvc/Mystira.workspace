# Secrets Management Guide

This guide documents how secrets are managed across Mystira's services, including which secrets are auto-populated by Terraform and which require manual configuration.

## Overview

Per [ADR-0017](../architecture/adr/0017-resource-group-organization-strategy.md), each service has its own Key Vault in its service-specific resource group. Secrets are managed in two ways:

1. **Auto-populated by Terraform** - Connection strings from shared infrastructure
2. **Manual via CI/CD** - API keys and external credentials synced from GitHub Secrets

## Key Vault Naming

| Service | Key Vault Name | Resource Group |
|---------|----------------|----------------|
| Story-Generator | `mys-{env}-story-kv-san` | `mys-{env}-story-rg-san` |
| Admin-API | `mys-{env}-adm-kv-san` | `mys-{env}-admin-rg-san` |
| Publisher | `mys-{env}-pub-kv-san` | `mys-{env}-publisher-rg-san` |
| Chain | `mys-{env}-chain-kv-san` | `mys-{env}-chain-rg-san` |

## Secrets by Service

### Story-Generator

| Secret Name | Source | Auto-Populated | Required |
|-------------|--------|----------------|----------|
| `postgres-connection-string` | Shared PostgreSQL | ✅ Yes | Yes |
| `redis-connection-string` | Shared Redis | ✅ Yes | Yes |
| `appinsights-connection-string` | Shared Monitoring | ✅ Yes | Yes |
| `anthropic-api-key` | GitHub Secrets | ❌ No | Yes |
| `openai-api-key` | GitHub Secrets | ❌ No | Yes |

**GitHub Secrets Required:**
- `ANTHROPIC_API_KEY_DEV`, `ANTHROPIC_API_KEY_STAGING`, `ANTHROPIC_API_KEY_PROD`
- `OPENAI_API_KEY_DEV`, `OPENAI_API_KEY_STAGING`, `OPENAI_API_KEY_PROD`

### Admin-API

| Secret Name | Source | Auto-Populated | Required |
|-------------|--------|----------------|----------|
| `postgres-connection-string` | Shared PostgreSQL | ✅ Yes | Yes |
| `redis-connection-string` | Shared Redis | ✅ Yes | Yes |
| `appinsights-connection-string` | Shared Monitoring | ✅ Yes | Yes |
| `azure-ad-tenant-id` | Entra ID Module | ✅ Yes | Yes |
| `azure-ad-client-id` | Entra ID Module | ✅ Yes | Yes |
| `admin-ui-client-id` | Entra ID Module | ✅ Yes | Yes |

**GitHub Secrets Required:** None - all secrets are auto-populated from Terraform modules.

### Publisher

| Secret Name | Source | Auto-Populated | Required |
|-------------|--------|----------------|----------|
| `servicebus-connection-string` | Shared Service Bus | ✅ Yes | Yes |
| `appinsights-connection-string` | Shared Monitoring | ✅ Yes | Yes |
| `chain-rpc-endpoint` | Terraform variable | ✅ Yes | Yes |

**GitHub Secrets Required:** None - all secrets are auto-populated.

### Chain

| Secret Name | Source | Auto-Populated | Required |
|-------------|--------|----------------|----------|
| `appinsights-connection-string` | Shared Monitoring | ✅ Yes | Yes |

**GitHub Secrets Required:** None - all secrets are auto-populated.

## Setting Up Secrets

### Step 1: Deploy Infrastructure

Run Terraform to create the infrastructure. Auto-populated secrets are created during this step:

```bash
cd infra/terraform/environments/dev
terraform init
terraform apply
```

This automatically creates:
- All Key Vaults in service-specific RGs
- Connection strings from shared PostgreSQL, Redis, Service Bus, Monitoring

### Step 2: Configure GitHub Secrets

Add the required secrets to your GitHub repository:

1. Go to **Settings > Secrets and variables > Actions**
2. Add environment-specific secrets:

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `ANTHROPIC_API_KEY_DEV` | Anthropic API key for dev | `sk-ant-...` |
| `OPENAI_API_KEY_DEV` | OpenAI API key for dev | `sk-...` |

**Note:** Entra ID credentials are auto-populated by Terraform from the `entra_id` module.

### Step 3: Sync Secrets to Key Vault

Run the GitHub Actions workflow to sync secrets:

1. Go to **Actions > Key Vault Secrets**
2. Click **Run workflow**
3. Select:
   - Environment: `dev`
   - Service: `all` (or specific service)
   - Action: `sync-manual-secrets`

### Step 4: Verify Secrets

Run the verification action:

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
        key: postgres-connection-string
  - name: Ai__Anthropic__ApiKey
    valueFrom:
      secretKeyRef:
        name: mys-story-generator-secrets
        key: anthropic-api-key
```

### Creating Kubernetes Secrets from Key Vault

```bash
# Get secrets from Key Vault
POSTGRES_CONN=$(az keyvault secret show --vault-name "mys-dev-story-kv-san" --name "postgres-connection-string" --query value -o tsv)
ANTHROPIC_KEY=$(az keyvault secret show --vault-name "mys-dev-story-kv-san" --name "anthropic-api-key" --query value -o tsv)

# Create Kubernetes secret
kubectl create secret generic mys-story-generator-secrets \
  --from-literal=postgres-connection-string="$POSTGRES_CONN" \
  --from-literal=anthropic-api-key="$ANTHROPIC_KEY" \
  -n mystira
```

## Secret Rotation

### Rotating Auto-Populated Secrets

1. Update the source resource (e.g., regenerate PostgreSQL password)
2. Run `terraform apply` to update Key Vault secrets
3. Restart affected pods to pick up new secrets

### Rotating Manual Secrets

1. Update the secret in GitHub Secrets
2. Run the `sync-manual-secrets` workflow
3. Restart affected pods

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

### GitHub Secrets Not Syncing

1. Check workflow run logs in GitHub Actions
2. Verify GitHub Secrets are set with correct environment suffix
3. Ensure Azure credentials have Key Vault access

### Pod Cannot Access Secrets

1. Verify Kubernetes secret exists:
   ```bash
   kubectl get secret mys-story-generator-secrets -n mystira
   ```

2. Check workload identity is configured:
   ```bash
   kubectl describe pod -n mystira | grep -A5 "azure.workload.identity"
   ```

## Complete GitHub Secrets Reference

### Per-Environment Secrets

| Secret Pattern | Services | Description |
|----------------|----------|-------------|
| `ANTHROPIC_API_KEY_{ENV}` | Story-Generator | Anthropic Claude API key |
| `OPENAI_API_KEY_{ENV}` | Story-Generator | OpenAI API key |

**Note:** Admin-API, Publisher, and Chain services have all secrets auto-populated by Terraform - no GitHub Secrets required.

### Global Secrets (all environments)

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
