# Shared Identity and RBAC Module

This Terraform module manages Azure RBAC role assignments and workload identity federation for the Mystira platform.

## Overview

This module provides centralized management of:

- **AKS to ACR Access**: Allows Kubernetes clusters to pull container images
- **Key Vault Access**: Grants services access to secrets
- **PostgreSQL Access**: Configures Azure AD authentication for database access
- **Redis Cache Access**: Enables managed identity authentication
- **Log Analytics Access**: Allows services to write logs and metrics
- **Storage Account Access**: Configures blob storage access
- **Workload Identity**: Federated credentials for AKS pods

## Usage

```hcl
module "identity" {
  source = "../../modules/shared/identity"

  resource_group_name = azurerm_resource_group.main.name

  # AKS to ACR access
  aks_principal_id = azurerm_kubernetes_cluster.main.identity[0].principal_id
  acr_id           = azurerm_container_registry.shared.id

  # Service identity configurations
  service_identities = {
    "story-generator" = {
      principal_id               = module.story_generator.identity_principal_id
      key_vault_id               = module.story_generator.key_vault_id
      postgres_server_id         = module.shared_postgresql.server_id
      redis_cache_id             = module.shared_redis.cache_id
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
    "publisher" = {
      principal_id               = module.publisher.identity_principal_id
      key_vault_id               = module.publisher.key_vault_id
      log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
    }
  }

  # Workload identity for AKS pods
  workload_identities = {
    "story-generator" = {
      identity_id         = module.story_generator.identity_id
      aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
      namespace           = "mystira"
      service_account     = "story-generator-sa"
    }
  }
}
```

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| resource_group_name | Resource group name | `string` | n/a | yes |
| aks_principal_id | AKS cluster managed identity principal ID | `string` | `""` | no |
| acr_id | Container registry resource ID | `string` | `""` | no |
| storage_role | Storage account role assignment | `string` | `"Storage Blob Data Contributor"` | no |
| service_identities | Map of service identity configurations | `map(object)` | `{}` | no |
| workload_identities | Map of workload identity configurations | `map(object)` | `{}` | no |
| tags | Resource tags | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| aks_acr_role_assignment_id | Role assignment ID for AKS to ACR |
| service_key_vault_role_assignments | Map of Key Vault role assignments |
| service_postgres_role_assignments | Map of PostgreSQL role assignments |
| service_redis_role_assignments | Map of Redis role assignments |
| workload_identity_federation_ids | Map of workload identity credentials |

## Role Definitions

| Resource | Role | Purpose |
|----------|------|---------|
| ACR | AcrPull | Pull container images |
| Key Vault | Key Vault Secrets User | Read secrets |
| PostgreSQL | Reader | Database access (use Azure AD auth) |
| Redis | Redis Cache Contributor | Cache operations |
| Log Analytics | Log Analytics Contributor | Write logs/metrics |
| Storage | Storage Blob Data Contributor | Blob operations |

## Security Best Practices

1. **Principle of Least Privilege**: Only grant necessary permissions
2. **Use Managed Identities**: Avoid storing credentials in code
3. **Enable Workload Identity**: Use federated credentials for AKS
4. **Regular Audits**: Review role assignments periodically
5. **Separate Identities**: Each service should have its own identity

## Workload Identity Setup

For AKS workload identity to work:

1. Enable OIDC issuer on AKS cluster:
   ```hcl
   oidc_issuer_enabled = true
   ```

2. Create ServiceAccount in Kubernetes with annotation:
   ```yaml
   apiVersion: v1
   kind: ServiceAccount
   metadata:
     name: story-generator-sa
     namespace: mystira
     annotations:
       azure.workload.identity/client-id: <managed-identity-client-id>
   ```

3. Add label to pods:
   ```yaml
   labels:
     azure.workload.identity/use: "true"
   ```

## Related Documentation

- [Azure Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/)
- [AKS Workload Identity](https://docs.microsoft.com/en-us/azure/aks/workload-identity-overview)
- [Azure RBAC Roles](https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles)
