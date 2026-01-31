# Publisher Infrastructure Module

Terraform module for deploying Mystira Publisher service infrastructure on Azure.

## Overview

Mystira Publisher is a Node.js service responsible for publishing content to the Mystira Chain. It handles content validation, metadata extraction, and blockchain transaction submission.

## Resources Created

| Resource | Purpose |
|----------|---------|
| User Assigned Managed Identity | Workload identity for AKS, Key Vault access |
| Network Security Group | Network rules for HTTP API and health checks |
| Service Bus Namespace (optional) | Event messaging for async processing |
| Service Bus Queue | Publisher events queue |
| Application Insights | Monitoring and telemetry |
| Key Vault | Secure storage for secrets |
| Redis Cache (prod only) | Optional caching layer |

## Usage

### Basic Usage

```hcl
module "publisher" {
  source = "../../modules/publisher"

  environment                       = "dev"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.publisher.name
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.publisher.id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

  chain_rpc_endpoint     = "http://chain-service:8545"
  publisher_replica_count = 2

  tags = {
    CostCenter = "publishing"
  }
}
```

### Using Shared Service Bus

```hcl
module "publisher" {
  source = "../../modules/publisher"
  # ... other variables ...

  use_shared_servicebus         = true
  shared_servicebus_namespace_id = module.shared_servicebus.namespace_id
  shared_servicebus_queue_name   = "publisher-events"
}
```

## Network Security Rules

| Rule | Port | Protocol | Purpose |
|------|------|----------|---------|
| AllowHTTP | 3000 | TCP | HTTP API endpoint |
| AllowHealthCheck | 3001 | TCP | Health check endpoint |

## Key Vault Secrets

The module stores the following secrets in Key Vault:

| Secret | Description |
|--------|-------------|
| `chain-rpc-endpoint` | RPC endpoint for Mystira Chain |

## Workload Identity Setup

To enable Azure Workload Identity for the Publisher in AKS:

```hcl
workload_identities = {
  "publisher" = {
    identity_id         = module.publisher.identity_id
    aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
    namespace           = "mystira"
    service_account     = "publisher-sa"
  }
}
```

## Service Bus Configuration

When not using shared Service Bus, the module creates:
- **Namespace SKU**: Premium for production, Standard otherwise
- **Queue**: `publisher-events` with dead-letter enabled
- **Max delivery count**: 3 attempts before dead-lettering
- **Message TTL**: 1 day

## Troubleshooting

### Key Vault "SoftDeletedVaultDoesNotExist" Error

If you encounter this error during deployment:

1. Check for soft-deleted vaults:
   ```bash
   az keyvault list-deleted --query "[?name=='mys-dev-pub-kv-san']"
   ```

2. If found, purge it:
   ```bash
   az keyvault purge --name mys-dev-pub-kv-san --location <location>
   ```

3. If not found, wait a few minutes and retry (Azure caching issue)

## Outputs

| Output | Description |
|--------|-------------|
| `nsg_id` | Network Security Group ID |
| `identity_id` | Managed Identity resource ID |
| `identity_principal_id` | Managed Identity principal ID (for RBAC) |
| `servicebus_namespace` | Service Bus namespace name (null if using shared) |
| `servicebus_queue_name` | Service Bus queue name |
| `application_insights_id` | Application Insights resource ID |
| `app_insights_connection_string` | App Insights connection string (sensitive) |
| `key_vault_id` | Key Vault resource ID |
