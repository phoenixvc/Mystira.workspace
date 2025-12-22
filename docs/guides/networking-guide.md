# Networking Guide

This guide covers the networking architecture and configuration for the Mystira platform on Azure.

## Table of Contents

1. [Network Architecture](#network-architecture)
2. [Virtual Network Design](#virtual-network-design)
3. [Subnet Configuration](#subnet-configuration)
4. [Network Security Groups](#network-security-groups)
5. [Azure Front Door](#azure-front-door)
6. [DNS Configuration](#dns-configuration)
7. [Private Endpoints](#private-endpoints)
8. [Quick Reference](#quick-reference)

## Network Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           Mystira Network Architecture                           │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│  Internet                                                                        │
│      │                                                                           │
│      ▼                                                                           │
│  ┌─────────────────────────────────────────────────────────────────────────┐    │
│  │                        Azure Front Door                                  │    │
│  │  • TLS Termination (Managed Certificates)                               │    │
│  │  • WAF Protection (Rate Limiting, Bot Blocking)                         │    │
│  │  • Global Load Balancing                                                │    │
│  │  • Caching for Static Content                                           │    │
│  └──────────────────────────────┬──────────────────────────────────────────┘    │
│                                 │                                                │
│                                 ▼                                                │
│  ┌──────────────────────────────────────────────────────────────────────────┐   │
│  │                      Azure DNS Zone (mystira.app)                         │   │
│  │  • publisher.mystira.app → CNAME → Front Door                            │   │
│  │  • chain.mystira.app → CNAME → Front Door                                │   │
│  │  • admin.mystira.app → CNAME → Front Door                                │   │
│  │  • admin-api.mystira.app → CNAME → Front Door                            │   │
│  └──────────────────────────────────────────────────────────────────────────┘   │
│                                                                                  │
│  ┌──────────────────────────────────────────────────────────────────────────┐   │
│  │              Virtual Network (10.0.0.0/16 - dev)                          │   │
│  │                                                                           │   │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐           │   │
│  │  │ AKS Subnet      │  │ PostgreSQL      │  │ Redis Subnet    │           │   │
│  │  │ 10.0.10.0/22    │  │ 10.0.3.0/24     │  │ 10.0.4.0/24     │           │   │
│  │  │                 │  │ (Delegated)     │  │ (Delegated)     │           │   │
│  │  │ • Admin API     │  │                 │  │                 │           │   │
│  │  │ • Publisher     │  │ Private DNS:    │  │                 │           │   │
│  │  │ • Chain         │  │ .postgres.      │  │                 │           │   │
│  │  │ • Story Gen     │  │ database.azure  │  │                 │           │   │
│  │  └─────────────────┘  └─────────────────┘  └─────────────────┘           │   │
│  │                                                                           │   │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐           │   │
│  │  │ Chain Subnet    │  │ Publisher       │  │ Story Generator │           │   │
│  │  │ 10.0.1.0/24     │  │ 10.0.2.0/24     │  │ 10.0.5.0/24     │           │   │
│  │  │                 │  │                 │  │                 │           │   │
│  │  │ NSG: Allow 8545 │  │ NSG: Allow 80   │  │ NSG: Allow 8080 │           │   │
│  │  │ (RPC), 30303    │  │ 443, 8080       │  │ 8081            │           │   │
│  │  │ (P2P)           │  │                 │  │                 │           │   │
│  │  └─────────────────┘  └─────────────────┘  └─────────────────┘           │   │
│  │                                                                           │   │
│  │  ┌─────────────────┐                                                      │   │
│  │  │ Admin API       │                                                      │   │
│  │  │ 10.0.6.0/24     │                                                      │   │
│  │  │                 │                                                      │   │
│  │  │ NSG: Allow 8080 │                                                      │   │
│  │  │ 8081            │                                                      │   │
│  │  └─────────────────┘                                                      │   │
│  └──────────────────────────────────────────────────────────────────────────┘   │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## Virtual Network Design

### Address Spaces by Environment

| Environment | VNet CIDR | Purpose |
|-------------|-----------|---------|
| Dev | `10.0.0.0/16` | Development and testing |
| Staging | `10.1.0.0/16` | Pre-production validation |
| Prod | `10.2.0.0/16` | Production workloads |

### VNet Configuration

```hcl
# Example from environments/dev/main.tf
resource "azurerm_virtual_network" "main" {
  name                = "mys-dev-core-vnet-san"
  location            = var.location
  resource_group_name = azurerm_resource_group.main.name
  address_space       = ["10.0.0.0/16"]
}
```

**Key Files:**
- Dev VNet: [`environments/dev/main.tf`](../../infra/terraform/environments/dev/main.tf)
- Staging VNet: [`environments/staging/main.tf`](../../infra/terraform/environments/staging/main.tf)
- Prod VNet: [`environments/prod/main.tf`](../../infra/terraform/environments/prod/main.tf)

## Subnet Configuration

### Dev Environment Subnets

| Subnet | CIDR | Purpose | Delegation |
|--------|------|---------|------------|
| `chain-subnet` | `10.0.1.0/24` | Chain blockchain nodes | None |
| `publisher-subnet` | `10.0.2.0/24` | Publisher service | None |
| `postgresql-subnet` | `10.0.3.0/24` | PostgreSQL Flexible Server | `Microsoft.DBforPostgreSQL/flexibleServers` |
| `redis-subnet` | `10.0.4.0/24` | Redis Cache (Premium) | `Microsoft.Cache/redis` |
| `story-generator-subnet` | `10.0.5.0/24` | Story Generator service | None |
| `admin-api-subnet` | `10.0.6.0/24` | Admin API service | None |
| `aks-subnet` | `10.0.10.0/22` | AKS cluster nodes | None |

### Subnet with Delegation Example

```hcl
resource "azurerm_subnet" "postgresql" {
  name                 = "postgresql-subnet"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.3.0/24"]

  delegation {
    name = "postgresql-delegation"
    service_delegation {
      name = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action",
      ]
    }
  }
}
```

## Network Security Groups

Each service has a dedicated NSG with specific inbound rules.

### Chain Service NSG

```hcl
# modules/chain/main.tf
resource "azurerm_network_security_group" "chain" {
  name = "mys-dev-chain-nsg-san"

  # JSON-RPC endpoint
  security_rule {
    name                       = "AllowRPC"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8545"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  # P2P Network
  security_rule {
    name                       = "AllowP2P"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "30303"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }
}
```

### Publisher Service NSG

```hcl
# modules/publisher/main.tf
resource "azurerm_network_security_group" "publisher" {
  name = "mys-dev-pub-nsg-san"

  security_rule {
    name                       = "AllowHTTP"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8080"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "AllowHealthCheck"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8081"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }
}
```

### Admin API NSG

```hcl
# modules/admin-api/main.tf
resource "azurerm_network_security_group" "admin_api" {
  name = "mys-dev-admin-api-nsg-san"

  security_rule {
    name                       = "AllowHTTP"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8080"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }

  security_rule {
    name                       = "AllowHealthCheck"
    priority                   = 110
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "8081"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "*"
  }
}
```

**Key Files:**
- Chain NSG: [`modules/chain/main.tf`](../../infra/terraform/modules/chain/main.tf)
- Publisher NSG: [`modules/publisher/main.tf`](../../infra/terraform/modules/publisher/main.tf)
- Admin API NSG: [`modules/admin-api/main.tf`](../../infra/terraform/modules/admin-api/main.tf)
- Story Generator NSG: [`modules/story-generator/main.tf`](../../infra/terraform/modules/story-generator/main.tf)

## Azure Front Door

Azure Front Door provides global load balancing, TLS termination, WAF protection, and caching.

### Architecture

```
Internet → Front Door → AKS Ingress → Pods
                │
                ├── TLS Termination (Managed Certs)
                ├── WAF (Rate Limiting, Bot Blocking)
                ├── Caching (Static Content)
                └── Health Probes
```

### Configuration

```hcl
# environments/dev/front-door.tf
module "front_door" {
  source = "../../modules/front-door"

  environment         = "dev"
  resource_group_name = azurerm_resource_group.main.name
  project_name        = "mystira"

  # Backend addresses
  publisher_backend_address = "dev.publisher.mystira.app"
  chain_backend_address     = "dev.chain.mystira.app"

  # Custom domains
  custom_domain_publisher = "dev.publisher.mystira.app"
  custom_domain_chain     = "dev.chain.mystira.app"

  # Admin services
  enable_admin_services     = true
  admin_api_backend_address = "dev.admin-api.mystira.app"
  admin_ui_backend_address  = "dev.admin.mystira.app"

  # WAF Configuration
  enable_waf           = true
  waf_mode             = "Prevention"  # or "Detection" for dev
  rate_limit_threshold = 100

  # Caching
  enable_caching         = true
  cache_duration_seconds = 3600

  # Health Probes
  health_probe_path     = "/health"
  health_probe_interval = 30
}
```

### WAF Rules

| Rule | Type | Description |
|------|------|-------------|
| Rate Limiting | RateLimitRule | Block after 100 req/min |
| Bad Bots | MatchRule | Block sqlmap, nikto, scanners |
| Allowed Methods | MatchRule | Only GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS |

### TLS Configuration

- **Certificate Type**: Azure Managed (auto-provisioned and renewed)
- **Minimum TLS Version**: TLS 1.2
- **HTTPS Redirect**: Enabled

**Key Files:**
- Front Door Module: [`modules/front-door/`](../../infra/terraform/modules/front-door/)
- Dev Config: [`environments/dev/front-door.tf`](../../infra/terraform/environments/dev/front-door.tf)

## DNS Configuration

### DNS Zone

```hcl
# modules/dns/main.tf
resource "azurerm_dns_zone" "main" {
  name                = "mystira.app"
  resource_group_name = var.resource_group_name
}
```

### Record Types

| Record Type | Use Case | Example |
|-------------|----------|---------|
| A Record | Direct to Load Balancer IP | `dev.publisher.mystira.app → 10.0.1.50` |
| CNAME | Point to Front Door | `publisher.mystira.app → mystira-prod-publisher.azurefd.net` |
| TXT | Domain validation | `_dnsauth.publisher → <validation-token>` |

### CNAME vs A Records

**Without Front Door (A Record):**
```
dev.publisher.mystira.app → A → <NGINX LB IP>
```

**With Front Door (CNAME):**
```
publisher.mystira.app → CNAME → mystira-prod-publisher.azurefd.net
```

### Environment-Specific Subdomains

| Environment | Pattern | Example |
|-------------|---------|---------|
| Dev | `dev.<service>.mystira.app` | `dev.publisher.mystira.app` |
| Staging | `staging.<service>.mystira.app` | `staging.publisher.mystira.app` |
| Prod | `<service>.mystira.app` | `publisher.mystira.app` |

**Key Files:**
- DNS Module: [`modules/dns/`](../../infra/terraform/modules/dns/)

## Private Endpoints

### PostgreSQL VNet Integration

PostgreSQL uses VNet integration with a private DNS zone for secure, private access.

```hcl
# modules/shared/postgresql/main.tf
resource "azurerm_private_dns_zone" "postgresql" {
  name                = "privatelink.postgres.database.azure.com"
  resource_group_name = var.resource_group_name
}

resource "azurerm_private_dns_zone_virtual_network_link" "postgresql" {
  name                  = "postgresql-vnet-link"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.postgresql.name
  virtual_network_id    = var.vnet_id
}

resource "azurerm_postgresql_flexible_server" "shared" {
  # ...
  delegated_subnet_id = var.enable_vnet_integration ? var.subnet_id : null
  private_dns_zone_id = var.enable_vnet_integration ? azurerm_private_dns_zone.postgresql.id : null
}
```

### Service Communication

All services communicate within the VNet using private DNS:

```
Admin API Pod → mys-dev-core-db.postgres.database.azure.com (Private DNS) → PostgreSQL
```

## Quick Reference

### Common Ports

| Service | Port | Protocol | Purpose |
|---------|------|----------|---------|
| Admin API | 8080 | HTTP | API endpoint |
| Admin API | 8081 | HTTP | Health check |
| Publisher | 8080 | HTTP | API endpoint |
| Story Generator | 8080 | HTTP | API endpoint |
| Chain RPC | 8545 | TCP | JSON-RPC |
| Chain P2P | 30303 | TCP/UDP | Peer discovery |
| PostgreSQL | 5432 | TCP | Database |
| Redis | 6379 | TCP | Cache |

### Terraform Commands

```bash
# View network resources
cd infra/terraform/environments/dev
terraform state list | grep -E "network|subnet|nsg"

# Show VNet details
terraform state show azurerm_virtual_network.main

# Show subnet configuration
terraform state show azurerm_subnet.aks
```

### Azure CLI Commands

```bash
# List VNets
az network vnet list -g mys-dev-core-rg-san -o table

# List subnets
az network vnet subnet list -g mys-dev-core-rg-san --vnet-name mys-dev-core-vnet-san -o table

# List NSGs
az network nsg list -g mys-dev-core-rg-san -o table

# Show NSG rules
az network nsg rule list -g mys-dev-core-rg-san --nsg-name mys-dev-admin-api-nsg-san -o table
```

### Troubleshooting

**Cannot connect to PostgreSQL:**
1. Check VNet integration is enabled
2. Verify private DNS zone is linked to VNet
3. Confirm subnet delegation is correct

**Front Door not routing traffic:**
1. Verify backend health probes are passing
2. Check custom domain validation TXT records
3. Confirm origin host header is correct

**Inter-service communication failing:**
1. Check NSG rules allow traffic between subnets
2. Verify AKS network policy allows egress
3. Confirm DNS resolution works within VNet

## Rollback Procedures

### Rolling Back Terraform Changes

When network changes cause issues, follow these steps to safely rollback:

```bash
# 1. Backup current Terraform state
cd infra/terraform/environments/dev
terraform state pull > state-backup-$(date +%Y%m%d-%H%M%S).json

# 2. Identify the commit to rollback to
git log --oneline -- main.tf modules/

# 3. Checkout the previous working version
git checkout <commit-sha> -- main.tf

# 4. Review changes before applying
terraform plan -out=rollback.tfplan

# 5. Apply the rollback
terraform apply rollback.tfplan

# 6. Verify resources
terraform state list | grep -E "network|subnet|nsg"
```

**Best Practices:**
- Always backup Terraform state before making changes
- Test rollbacks in staging environment first
- Document the reason for rollback in Git commit message
- Verify connectivity after rollback with health checks
- Keep rollback window minimal to reduce impact

**References:**
- [Terraform State Management](https://developer.hashicorp.com/terraform/language/state)
- [Azure Resource Rollback Best Practices](https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/rollback-on-error)

### Rolling Back NSG Rules

Network Security Group rules can be quickly restored:

```bash
# 1. Identify the faulty NSG rule
az network nsg rule list \
  -g mys-dev-core-rg-san \
  --nsg-name mys-dev-admin-api-nsg-san \
  -o table

# 2. Remove the faulty rule
az network nsg rule delete \
  -g mys-dev-core-rg-san \
  --nsg-name mys-dev-admin-api-nsg-san \
  --name <rule-name>

# 3. Reapply NSG via Terraform (recommended)
cd infra/terraform/environments/dev
terraform plan -target=azurerm_network_security_group.admin_api
terraform apply -target=azurerm_network_security_group.admin_api

# 4. Verify critical traffic flows
# Test connectivity to PostgreSQL
kubectl exec -n mystira deploy/mys-admin-api -- nc -zv mys-dev-core-db.postgres.database.azure.com 5432

# Test outbound connectivity
kubectl exec -n mystira deploy/mys-admin-api -- curl -I https://api.github.com
```

**Best Practices:**
- Document all NSG rule changes with comments
- Test rules in non-production environment first
- Use Terraform for NSG management to maintain consistency
- Keep default deny rules as last priority
- Monitor network flow logs after changes

**References:**
- [NSG Diagnostic Logging](https://learn.microsoft.com/en-us/azure/network-watcher/network-watcher-nsg-flow-logging-overview)
- [Network Security Best Practices](https://learn.microsoft.com/en-us/azure/security/fundamentals/network-best-practices)

### Rolling Back Subnet Delegation

Subnet delegation changes require careful coordination:

```bash
# 1. Stop dependent resources (if necessary)
# For PostgreSQL delegation, ensure no active connections
az postgres flexible-server stop \
  -g mys-dev-core-rg-san \
  -n mys-dev-core-db

# 2. Remove or restore delegation via Terraform
cd infra/terraform/environments/dev

# Edit main.tf to remove delegation
# delegated_subnet_id = null  # or restore previous value

# 3. Apply targeted change
terraform plan -target=azurerm_subnet.postgresql
terraform apply -target=azurerm_subnet.postgresql

# 4. Restart services
az postgres flexible-server start \
  -g mys-dev-core-rg-san \
  -n mys-dev-core-db

# 5. Test connectivity
kubectl exec -n mystira deploy/mys-admin-api -- \
  psql -h mys-dev-core-db.postgres.database.azure.com -U adminuser -d adminapi -c "SELECT 1"
```

**Best Practices:**
- Schedule subnet delegation changes during maintenance windows
- Communicate changes to team before executing
- Test in staging environment with same network topology
- Have monitoring alerts ready for connectivity issues
- Keep rollback script ready before making changes

**Important Notes:**
- Some Azure services require subnet delegation (e.g., Azure Database for PostgreSQL Flexible Server)
- Removing delegation may break service connectivity
- Always verify service requirements before removing delegation
- Consult Azure service documentation for delegation requirements

**Escalation Contacts:**
- For production issues: Check team runbooks in `docs/runbooks/`
- Azure Support: Use Azure Portal support request for platform issues
- Network team: Contact designated network administrator

**References:**
- [Subnet Delegation Overview](https://learn.microsoft.com/en-us/azure/virtual-network/subnet-delegation-overview)
- [Azure Database for PostgreSQL Networking](https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-networking)

## Related Documentation

- [ADR-0005: Service Networking and Communication](../architecture/adr/0005-service-networking-and-communication.md)
- [Front Door Module](../../infra/terraform/modules/front-door/README.md)
- [DNS Module](../../infra/terraform/modules/dns/main.tf)
- [PostgreSQL Module](../../infra/terraform/modules/shared/postgresql/README.md)
- [Azure Virtual Network Docs](https://learn.microsoft.com/en-us/azure/virtual-network/)
- [Azure Front Door Docs](https://learn.microsoft.com/en-us/azure/frontdoor/)
