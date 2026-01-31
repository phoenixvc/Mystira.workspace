# Chain Infrastructure Module

Terraform module for deploying Mystira Chain blockchain infrastructure on Azure.

## Overview

Mystira Chain is a private blockchain network used for content authenticity and provenance tracking. This module provisions the supporting Azure infrastructure for running chain nodes in AKS.

## Resources Created

| Resource | Purpose |
|----------|---------|
| User Assigned Managed Identity | Workload identity for AKS, Key Vault access |
| Network Security Group | Network rules for P2P, RPC, and WebSocket traffic |
| Storage Account (Premium FileStorage) | Persistent storage for chain data |
| Azure File Shares | Per-node file shares for chain state |
| Application Insights | Monitoring and telemetry |
| Key Vault | Secure storage for chain secrets |

## Usage

```hcl
module "chain" {
  source = "../../modules/chain"

  environment                       = "dev"
  location                          = var.location
  region_code                       = "san"
  resource_group_name               = azurerm_resource_group.chain.name
  vnet_id                           = azurerm_virtual_network.main.id
  subnet_id                         = azurerm_subnet.chain.id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id

  chain_node_count      = 3
  chain_storage_size_gb = 100  # Minimum 100 GB for Premium

  tags = {
    CostCenter = "blockchain"
  }
}
```

## Network Security Rules

| Rule | Port | Protocol | Purpose |
|------|------|----------|---------|
| AllowChainP2P | 30303 | TCP/UDP | P2P communication between nodes |
| AllowRPC | 8545 | TCP | JSON-RPC endpoint (internal only) |
| AllowWebSocket | 8546 | TCP | WebSocket endpoint (internal only) |

## Storage Configuration

The module uses Azure Premium FileStorage for chain data persistence:

- **Account tier**: Premium (required for file shares)
- **Replication**: ZRS for production, LRS for non-production
- **Minimum quota**: 100 GB per file share

File shares are created using the Azure CLI data plane API to work around ARM API limitations.

## Workload Identity Setup

To enable Azure Workload Identity for chain nodes in AKS:

```hcl
workload_identities = {
  "chain" = {
    identity_id         = module.chain.identity_id
    aks_oidc_issuer_url = azurerm_kubernetes_cluster.main.oidc_issuer_url
    namespace           = "mystira"
    service_account     = "chain-sa"
  }
}
```

## Outputs

| Output | Description |
|--------|-------------|
| `nsg_id` | Network Security Group ID |
| `identity_id` | Managed Identity resource ID |
| `identity_principal_id` | Managed Identity principal ID (for RBAC) |
| `storage_account_name` | Storage account name for chain data |
| `application_insights_id` | Application Insights resource ID |
| `application_insights_connection_string` | App Insights connection string (sensitive) |
| `key_vault_id` | Key Vault resource ID |
