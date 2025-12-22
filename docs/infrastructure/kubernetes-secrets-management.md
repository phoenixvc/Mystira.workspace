# Kubernetes Secrets Management Guide

This guide explains how to create and manage Kubernetes secrets for Mystira services.

## Overview

Kubernetes secrets store sensitive configuration data like connection strings, API keys, and certificates. For Mystira services, secrets are typically stored in Azure Key Vault and synced to Kubernetes.

## Secret Creation Methods

### Method 1: Manual Secret Creation from Key Vault

After Terraform deployment, create Kubernetes secrets from Key Vault:

```bash
# Get connection strings from Key Vault
POSTGRES_CONN=$(az keyvault secret show \
  --vault-name "mystira-sg-dev-kv"  # Legacy name, kept as-is per ADR-0008 (new would be mys-dev-mystira-sg-kv-euw) \
  --name "postgres-connection-string" \
  --query value -o tsv)

REDIS_CONN=$(az keyvault secret show \
  --vault-name "mystira-sg-dev-kv"  # Legacy name, kept as-is per ADR-0008 (new would be mys-dev-mystira-sg-kv-euw) \
  --name "redis-connection-string" \
  --query value -o tsv)

APPINSIGHTS_CONN=$(az keyvault secret show \
  --vault-name "mystira-sg-dev-kv"  # Legacy name, kept as-is per ADR-0008 (new would be mys-dev-mystira-sg-kv-euw) \
  --name "appinsights-connection-string" \
  --query value -o tsv)

# Get API keys from Key Vault or environment
ANTHROPIC_KEY=$(az keyvault secret show \
  --vault-name "mystira-sg-dev-kv"  # Legacy name, kept as-is per ADR-0008 (new would be mys-dev-mystira-sg-kv-euw) \
  --name "anthropic-api-key" \
  --query value -o tsv)

OPENAI_KEY=$(az keyvault secret show \
  --vault-name "mystira-sg-dev-kv"  # Legacy name, kept as-is per ADR-0008 (new would be mys-dev-mystira-sg-kv-euw) \
  --name "openai-api-key" \
  --query value -o tsv)

# Create Kubernetes secret
kubectl create secret generic mystira-story-generator-secrets \
  --from-literal=postgres-connection-string="$POSTGRES_CONN" \
  --from-literal=redis-connection-string="$REDIS_CONN" \
  --from-literal=appinsights-connection="$APPINSIGHTS_CONN" \
  --from-literal=anthropic_api_key="$ANTHROPIC_KEY" \
  --from-literal=openai_api_key="$OPENAI_KEY" \
  --namespace mystira-dev
```

### Method 2: Using Terraform Outputs

When using shared resources, construct connection strings from Terraform outputs:

```bash
# Get outputs from Terraform
terraform output -json > outputs.json

# Parse and create secrets
POSTGRES_CONN=$(jq -r '.shared_postgresql_connection_string_storygenerator.value' outputs.json | \
  sed "s/<REDACTED>/$(az keyvault secret show --vault-name mystira-dev-kv --name postgres-admin-password --query value -o tsv)/")

kubectl create secret generic mystira-story-generator-secrets \
  --from-literal=postgres-connection-string="$POSTGRES_CONN" \
  --namespace mystira-dev
```

### Method 3: Azure Key Vault CSI Driver (Recommended for Production)

Install the Azure Key Vault Provider for Secrets Store CSI Driver:

```bash
helm repo add csi-secrets-store-provider-azure https://azure.github.io/secrets-store-csi-driver-provider-azure/charts
helm install csi-secrets-store-provider-azure csi-secrets-store-provider-azure/csi-secrets-store-provider-azure \
  --namespace kube-system
```

Create a SecretProviderClass:

```yaml
apiVersion: secrets-store.csi.x-k8s.io/v1
kind: SecretProviderClass
metadata:
  name: mystira-story-generator-secrets
  namespace: mystira-dev
spec:
  provider: azure
  parameters:
    usePodIdentity: "false"
    useVMManagedIdentity: "true"
    userAssignedIdentityID: "<managed-identity-client-id>"
    keyvaultName: "mystira-sg-dev-kv" # Legacy name, kept as-is per ADR-0008
    tenantId: "<azure-tenant-id>"
    objects: |
      array:
        - |
          objectName: postgres-connection-string
          objectType: secret
          objectVersion: ""
        - |
          objectName: redis-connection-string
          objectType: secret
          objectVersion: ""
        - |
          objectName: appinsights-connection-string
          objectType: secret
          objectVersion: ""
        - |
          objectName: anthropic-api-key
          objectType: secret
          objectVersion: ""
        - |
          objectName: openai-api-key
          objectType: secret
          objectVersion: ""
  secretObjects:
    - secretName: mystira-story-generator-secrets
      type: Opaque
      data:
        - objectName: postgres-connection-string
          key: postgres_connection_string
        - objectName: redis-connection-string
          key: redis_connection_string
        - objectName: appinsights-connection-string
          key: appinsights_connection
        - objectName: anthropic-api-key
          key: anthropic_api_key
        - objectName: openai-api-key
          key: openai_api_key
```

Update the Deployment to mount the secret:

```yaml
spec:
  template:
    spec:
      volumes:
        - name: secrets-store
          csi:
            driver: secrets-store.csi.k8s.io
            readOnly: true
            volumeAttributes:
              secretProviderClass: "mystira-story-generator-secrets"
      containers:
        - name: story-generator
          volumeMounts:
            - name: secrets-store
              mountPath: /mnt/secrets-store
              readOnly: true
```

## Required Secrets for Story-Generator

| Secret Key                   | Source    | Description                            |
| ---------------------------- | --------- | -------------------------------------- |
| `postgres_connection_string` | Key Vault | PostgreSQL connection string           |
| `redis_connection_string`    | Key Vault | Redis connection string                |
| `appinsights_connection`     | Key Vault | Application Insights connection string |
| `anthropic_api_key`          | Key Vault | Anthropic API key for Claude           |
| `openai_api_key`             | Key Vault | OpenAI API key for GPT models          |

## Required Secrets for Authentication

### Admin API (Entra ID)

| Secret Key                   | Source    | Description                            |
| ---------------------------- | --------- | -------------------------------------- |
| `azure-ad-tenant-id`         | Key Vault | Microsoft Entra ID tenant ID           |
| `azure-ad-client-id`         | Key Vault | Admin API app registration client ID   |
| `azure-ad-client-secret`     | Key Vault | Admin API app registration secret      |

### Public API (Microsoft Entra External ID)

| Secret Key                   | Source    | Description                            |
| ---------------------------- | --------- | -------------------------------------- |
| `azure-b2c-tenant-id`        | Key Vault | External ID tenant ID                  |
| `azure-b2c-client-id`        | Key Vault | Public API External ID app registration ID |
| `azure-b2c-client-secret`    | Key Vault | External ID client secret (if confidential) |

### Social Identity Provider Secrets

These secrets are stored in Azure Key Vault and referenced by External ID user flows:

| Secret Key                   | Source    | Description                            |
| ---------------------------- | --------- | -------------------------------------- |
| `google-oauth-client-id`     | Key Vault | Google OAuth 2.0 client ID             |
| `google-oauth-client-secret` | Key Vault | Google OAuth 2.0 client secret         |
| `discord-oauth-client-id`    | Key Vault | Discord OAuth client ID                |
| `discord-oauth-client-secret`| Key Vault | Discord OAuth client secret            |

### Creating Authentication Secrets in Kubernetes

```bash
# Get authentication secrets from Key Vault
AZURE_AD_TENANT_ID=$(az keyvault secret show \
  --vault-name "mystira-admin-kv" \
  --name "azure-ad-tenant-id" \
  --query value -o tsv)

AZURE_AD_CLIENT_ID=$(az keyvault secret show \
  --vault-name "mystira-admin-kv" \
  --name "azure-ad-client-id" \
  --query value -o tsv)

AZURE_AD_CLIENT_SECRET=$(az keyvault secret show \
  --vault-name "mystira-admin-kv" \
  --name "azure-ad-client-secret" \
  --query value -o tsv)

# Create Kubernetes secret for Admin API
kubectl create secret generic mystira-admin-api-auth \
  --from-literal=azure-ad-tenant-id="$AZURE_AD_TENANT_ID" \
  --from-literal=azure-ad-client-id="$AZURE_AD_CLIENT_ID" \
  --from-literal=azure-ad-client-secret="$AZURE_AD_CLIENT_SECRET" \
  --namespace mystira-dev
```

**Note**: For production, use Azure Key Vault CSI Driver with Managed Identity to automatically sync secrets. See [ADR-0011: Entra ID Integration](../architecture/adr/0011-entra-id-authentication-integration.md) for details.

## Secret Storage in Key Vault

### For Dedicated Resources

When Story-Generator uses dedicated PostgreSQL/Redis, Terraform automatically stores connection strings in Key Vault.

### For Shared Resources

When using shared resources, connection strings must be manually stored:

1. **Get connection strings from Terraform outputs**:

   ```bash
   terraform output shared_postgresql_connection_string_storygenerator
   terraform output shared_redis_connection_string
   ```

2. **Store in Key Vault**:

   ```bash
   az keyvault secret set \
     --vault-name "mystira-sg-dev-kv"  # Legacy name, kept as-is per ADR-0008 (new would be mys-dev-mystira-sg-kv-euw) \
     --name "postgres-connection-string" \
     --value "<connection-string>"

   az keyvault secret set \
     --vault-name "mystira-sg-dev-kv"  # Legacy name, kept as-is per ADR-0008 (new would be mys-dev-mystira-sg-kv-euw) \
     --name "redis-connection-string" \
     --value "<connection-string>"
   ```

3. **Store Application Insights connection string**:
   ```bash
   az keyvault secret set \
     --vault-name "mystira-sg-dev-kv"  # Legacy name, kept as-is per ADR-0008 (new would be mys-dev-mystira-sg-kv-euw) \
     --name "appinsights-connection-string" \
     --value "$(terraform output -raw shared_monitoring_appinsights_connection_string)"
   ```

## Environment-Specific Secrets

### Development

```bash
kubectl create secret generic mystira-story-generator-secrets \
  --from-literal=postgres-connection-string="..." \
  --from-literal=redis-connection-string="..." \
  --from-literal=appinsights-connection="..." \
  --from-literal=anthropic_api_key="..." \
  --from-literal=openai_api_key="..." \
  --namespace mystira-dev
```

### Staging

```bash
kubectl create secret generic mystira-story-generator-secrets \
  --from-literal=postgres-connection-string="..." \
  --from-literal=redis-connection-string="..." \
  --from-literal=appinsights-connection="..." \
  --from-literal=anthropic_api_key="..." \
  --from-literal=openai_api_key="..." \
  --namespace mystira-staging
```

### Production

Use Azure Key Vault CSI Driver for automatic secret sync and rotation.

## Secret Updates

### Manual Update

```bash
kubectl create secret generic mystira-story-generator-secrets \
  --from-literal=postgres-connection-string="<new-value>" \
  --dry-run=client -o yaml | kubectl apply -f -
```

### With Key Vault CSI Driver

Secrets are automatically updated when Key Vault values change. Pods need to be restarted to pick up new values.

## Secret Rotation

### Manual Rotation

1. Update secret in Key Vault
2. Update Kubernetes secret (Method 1 or 2)
3. Restart pods to pick up new secret values

### Automated Rotation (Future)

Implement secret rotation using:

- Azure Key Vault automatic rotation
- Kubernetes CronJob to sync secrets
- External Secrets Operator

## Troubleshooting

### Pod Can't Read Secret

```bash
# Check if secret exists
kubectl get secret mystira-story-generator-secrets -n mystira-dev

# Check secret contents (values are base64 encoded)
kubectl get secret mystira-story-generator-secrets -n mystira-dev -o yaml

# Decode a specific key
kubectl get secret mystira-story-generator-secrets -n mystira-dev \
  -o jsonpath='{.data.postgres-connection-string}' | base64 -d
```

### Secret Not Found Error

- Verify secret exists: `kubectl get secrets -n <namespace>`
- Check secret name matches deployment reference
- Verify namespace is correct

### Key Vault Access Issues

- Verify managed identity has Key Vault access policies
- Check Key Vault firewall rules allow AKS access
- Verify service account workload identity configuration

## Related Documentation

- [Shared Resources Usage Guide](./shared-resources.md)
- [Infrastructure Guide](./infrastructure.md)
- [Azure Key Vault CSI Driver](https://learn.microsoft.com/en-us/azure/aks/csi-secrets-store-driver)
- [Kubernetes Secrets](https://kubernetes.io/docs/concepts/configuration/secret/)
