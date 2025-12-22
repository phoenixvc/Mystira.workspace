# Admin API Infrastructure Module

Terraform module for deploying Mystira Admin API infrastructure on Azure.

## Overview

The Admin API is a .NET Web API that provides administrative functionality for Mystira. It uses Microsoft Entra ID for authentication via the Microsoft.Identity.Web library.

## Resources Created

| Resource | Purpose |
|----------|---------|
| User Assigned Managed Identity | Workload identity for AKS, Key Vault access, PostgreSQL auth |
| Network Security Group | Network traffic rules for the service |
| Application Insights | Monitoring and telemetry |
| Key Vault | Secure storage for secrets and configuration |

## Usage

```hcl
module "admin_api" {
  source = "../../modules/admin-api"

  environment                       = "dev"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.main.name
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.admin_api.id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
  shared_postgresql_server_id       = module.shared_postgresql.server_id

  tags = {
    CostCenter = "development"
  }
}
```

## Workload Identity Setup

To enable Azure Workload Identity for the Admin API in AKS:

1. Add to workload_identities in the identity module:

```hcl
workload_identities = {
  "admin-api" = {
    identity_id         = module.admin_api.identity_id
    aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
    namespace           = "mystira"
    service_account     = "admin-api-sa"
  }
}
```

2. Deploy the Kubernetes ServiceAccount with the client ID annotation:

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: admin-api-sa
  namespace: mystira
  annotations:
    azure.workload.identity/client-id: "<identity_client_id from terraform output>"
  labels:
    azure.workload.identity/use: "true"
```

## PostgreSQL Authentication

The Admin API can authenticate to PostgreSQL using Azure AD tokens instead of passwords:

1. Add the managed identity as a PostgreSQL AD administrator (see shared/postgresql module)
2. Configure connection string with `Authentication=Active Directory Default`
3. The Azure SDK automatically obtains tokens via workload identity

Example connection string:
```
Host=<server>.postgres.database.azure.com;Database=admindb;Username=<identity-name>;Authentication=Active Directory Default
```

## Entra ID Integration

The Admin API uses Microsoft.Identity.Web for JWT token validation. Configuration is provided by the entra-id module:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<client-id>",
    "Audience": "api://mystira-admin-api-dev"
  }
}
```

## Outputs

| Output | Description |
|--------|-------------|
| `identity_id` | Managed Identity resource ID |
| `identity_principal_id` | Managed Identity principal ID (for RBAC) |
| `identity_client_id` | Managed Identity client ID (for workload identity) |
| `key_vault_id` | Key Vault resource ID |
| `key_vault_uri` | Key Vault URI |
| `app_insights_connection_string` | Application Insights connection string |
