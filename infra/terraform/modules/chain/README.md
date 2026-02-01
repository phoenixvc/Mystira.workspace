# Chain Infrastructure Module

Terraform module for deploying Mystira Chain blockchain infrastructure on Azure.

## Overview

Mystira Chain is a private blockchain network used for content authenticity and provenance tracking. This module provisions the supporting Azure infrastructure for running chain nodes in AKS.

## Resources Created

| Resource                              | Purpose                                       |
| ------------------------------------- | --------------------------------------------- |
| User Assigned Managed Identity        | Workload identity for AKS, Key Vault access   |
| Network Security Group                | Network rules for P2P, RPC, and WebSocket traffic |
| Storage Account (Premium FileStorage) | Persistent storage for chain data             |
| Azure File Shares                     | Per-node file shares for chain state          |
| Application Insights                  | Monitoring and telemetry                      |
| Key Vault                             | Secure storage for chain secrets              |

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

| Rule           | Port  | Protocol | Purpose                            |
| -------------- | ----- | -------- | ---------------------------------- |
| AllowChainP2P  | 30303 | TCP/UDP  | P2P communication between nodes    |
| AllowRPC       | 8545  | TCP      | JSON-RPC endpoint (internal only)  |
| AllowWebSocket | 8546  | TCP      | WebSocket endpoint (internal only) |

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

| Output                                   | Description                                          |
| ---------------------------------------- | ---------------------------------------------------- |
| `nsg_id`                                 | Network Security Group ID                            |
| `identity_id`                            | Managed Identity resource ID                         |
| `identity_principal_id`                  | Managed Identity principal ID (for RBAC)             |
| `identity_client_id`                     | Managed Identity client ID (for workload identity)   |
| `storage_account_name`                   | Storage account name for chain data                  |
| `application_insights_id`                | Application Insights resource ID                     |
| `application_insights_connection_string` | App Insights connection string (sensitive)           |
| `key_vault_id`                           | Key Vault resource ID                                |
| `key_vault_uri`                          | Key Vault URI for accessing secrets                  |

## Rollback Procedures

### Terraform State Rollback

If a deployment fails or causes issues, you can rollback using the following procedures:

1. **Identify the previous state version**:
   ```bash
   az storage blob list \
     --account-name mystfstate \
     --container-name tfstate \
     --prefix "chain/" \
     --query "[].{name:name, lastModified:properties.lastModified}" \
     --output table
   ```

2. **Download the previous state file**:
   ```bash
   az storage blob download \
     --account-name mystfstate \
     --container-name tfstate \
     --name "chain/<environment>.tfstate" \
     --file previous.tfstate \
     --snapshot "<snapshot-datetime>"
   ```

3. **Apply the previous state** (caution - this may cause drift):
   ```bash
   terraform state push previous.tfstate
   terraform apply -refresh-only
   ```

### Kubernetes Rollback

For chain StatefulSet rollback:

```bash
# View rollout history
kubectl rollout history statefulset/mys-chain -n mys-<environment>

# Rollback to previous revision
kubectl rollout undo statefulset/mys-chain -n mys-<environment>

# Rollback to specific revision
kubectl rollout undo statefulset/mys-chain -n mys-<environment> --to-revision=<revision>

# Verify rollback status
kubectl rollout status statefulset/mys-chain -n mys-<environment>
```

### Emergency Procedures

In case of chain node corruption or consensus failure:

1. **Scale down to prevent further damage**:
   ```bash
   kubectl scale statefulset/mys-chain -n mys-<environment> --replicas=0
   ```

2. **Backup current chain data** (from Azure File Share):
   ```bash
   az storage file download-batch \
     --source chain-data-0 \
     --destination ./backup \
     --account-name <storage-account>
   ```

3. **Restore from known-good snapshot** and scale back up.

## Security Considerations

### Current Security Measures

1. **External TLS**: All external traffic is encrypted via TLS at the Ingress level (cert-manager with Let's Encrypt)
2. **Network Policies**: Kubernetes NetworkPolicies restrict which pods can access chain RPC endpoints
3. **Network Security Groups**: Azure NSGs limit RPC/WS access to VirtualNetwork sources only
4. **Private Storage**: Chain storage account has `public_network_access_enabled = false`
5. **Workload Identity**: Pods use Azure Managed Identity (no credentials in code)

### Internal RPC Security (mTLS Roadmap)

The chain RPC currently uses HTTP for internal cluster communication. While protected by:
- Kubernetes NetworkPolicies (only authorized pods can connect)
- ClusterIP services (not exposed outside the cluster)
- Azure NSG rules (VNet-only access)

For enhanced security, consider implementing mTLS for internal communication:

#### Option 1: Service Mesh (Recommended for production)

```bash
# Install Istio with strict mTLS
istioctl install --set profile=default
kubectl label namespace mystira istio-injection=enabled

# Enable strict mTLS for the namespace
kubectl apply -f - <<EOF
apiVersion: security.istio.io/v1beta1
kind: PeerAuthentication
metadata:
  name: default
  namespace: mystira
spec:
  mtls:
    mode: STRICT
EOF
```

#### Option 2: Application-Level TLS

Configure the chain application to use TLS with certificates from Key Vault:

```yaml
# In chain ConfigMap
tls_enabled: "true"
tls_cert_secret: "chain-tls-cert"
tls_key_secret: "chain-tls-key"
```

#### Option 3: Linkerd (Lightweight alternative)

```bash
# Install Linkerd
linkerd install | kubectl apply -f -
linkerd inject deployment.yaml | kubectl apply -f -
```

### Recommendations by Environment

| Environment | Security Level | Recommendation                              |
| ----------- | -------------- | ------------------------------------------- |
| dev         | Basic          | NetworkPolicies + external TLS              |
| staging     | Enhanced       | NetworkPolicies + service mesh (permissive) |
| prod        | Maximum        | Service mesh with strict mTLS               |
