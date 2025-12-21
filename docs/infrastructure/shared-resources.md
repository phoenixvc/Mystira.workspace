# Shared Resources Usage Guide

This guide explains how to use shared infrastructure resources (PostgreSQL, Redis, Monitoring) in the Mystira workspace.

## Overview

Shared resources provide:

- **Cost efficiency**: Single database/cache instance shared across services
- **Centralized management**: Unified backup, monitoring, and maintenance
- **Simplified networking**: Single VNet integration point

## Shared PostgreSQL

### Configuration

The shared PostgreSQL module creates a flexible server with multiple databases:

```hcl
module "shared_postgresql" {
  source = "../../modules/shared/postgresql"

  environment         = "dev"
  location           = "eastus"
  resource_group_name = "mys-dev-core-rg-eus"
  vnet_id            = azurerm_virtual_network.main.id
  subnet_id          = azurerm_subnet.postgresql.id

  databases = [
    "storygenerator",
    "publisher"
  ]
}
```

### Connection Strings

#### For .NET Applications (Npgsql)

The shared PostgreSQL module outputs connection strings in the correct format:

```hcl
# Access connection strings from module output
output "postgresql_connection_strings" {
  value = module.shared_postgresql.connection_strings
  sensitive = true
}
```

Format: `Host={fqdn};Port=5432;Username={admin_login};Password={password};Database={database_name};SSL Mode=Require;Trust Server Certificate=true`

#### For Python Applications

Python applications using `psycopg2` should use:

```python
import psycopg2

conn_string = f"host={fqdn} port=5432 dbname={database_name} user={admin_login} password={password} sslmode=require"
```

#### For TypeScript/Node.js Applications

Node.js applications using `pg` should use:

```typescript
const connectionString = `postgresql://${adminLogin}:${password}@${fqdn}:5432/${databaseName}?sslmode=require`;
```

### Storing Connection Strings in Key Vault

When using shared PostgreSQL, you need to manually create Key Vault secrets or use Terraform outputs:

```hcl
# Option 1: Use module output directly in environment
resource "azurerm_key_vault_secret" "story_generator_postgres" {
  name         = "postgres-connection-string"
  value        = module.shared_postgresql.connection_strings["storygenerator"]
  key_vault_id = module.story_generator.key_vault_id
}

# Option 2: Construct from outputs
resource "azurerm_key_vault_secret" "story_generator_postgres" {
  name  = "postgres-connection-string"
  value = "Host=${module.shared_postgresql.server_fqdn};Port=5432;Username=${module.shared_postgresql.admin_login};Password=${module.shared_postgresql.admin_password};Database=storygenerator;SSL Mode=Require;Trust Server Certificate=true"
  key_vault_id = module.story_generator.key_vault_id
}
```

### Accessing from Kubernetes

In Kubernetes, reference the secret:

```yaml
env:
  - name: ConnectionStrings__PostgreSQL
    valueFrom:
      secretKeyRef:
        name: mystira-story-generator-secrets
        key: postgres-connection-string
```

## Shared Redis

### Configuration

The shared Redis module creates an Azure Cache for Redis:

```hcl
module "shared_redis" {
  source = "../../modules/shared/redis"

  environment         = "dev"
  location           = "eastus"
  resource_group_name = "mys-dev-core-rg-eus"
  subnet_id          = azurerm_subnet.redis.id

  capacity = 1
  family   = "C"
  sku_name = "Standard"
}
```

### Connection Strings

#### For .NET Applications (StackExchange.Redis)

The shared Redis module outputs connection strings:

```hcl
output "redis_connection_string" {
  value = module.shared_redis.primary_connection_string
  sensitive = true
}
```

Format: Azure Redis provides connection strings in the format: `{cache_name}.redis.cache.windows.net:6380,password={access_key},ssl=True,abortConnect=False`

#### For Python Applications

Python applications using `redis-py`:

```python
import redis

# Parse connection string
r = redis.from_url(connection_string, ssl_cert_reqs=None)
```

#### For TypeScript/Node.js Applications

Node.js applications using `ioredis`:

```typescript
import Redis from "ioredis";

const redis = new Redis(connectionString, {
  tls: {
    rejectUnauthorized: false,
  },
});
```

### Storing Connection Strings in Key Vault

```hcl
resource "azurerm_key_vault_secret" "story_generator_redis" {
  name         = "redis-connection-string"
  value        = module.shared_redis.primary_connection_string
  key_vault_id = module.story_generator.key_vault_id
}
```

## Shared Monitoring (Log Analytics)

### Configuration

The shared monitoring module creates Log Analytics workspace and Application Insights:

```hcl
module "shared_monitoring" {
  source = "../../modules/shared/monitoring"

  environment         = "dev"
  location           = "eastus"
  resource_group_name = "mys-dev-core-rg-eus"

  retention_in_days = 30
}
```

### Integration

#### For .NET Applications

Reference Application Insights connection string:

```hcl
# Store in Key Vault
resource "azurerm_key_vault_secret" "appinsights_connection" {
  name         = "appinsights-connection-string"
  value        = module.shared_monitoring.application_insights_connection_string
  key_vault_id = module.story_generator.key_vault_id
}
```

In application configuration:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=..."
  }
}
```

#### For Python Applications

Use Azure Monitor Python SDK:

```python
from opencensus.ext.azure.log_exporter import AzureLogHandler

logger.addHandler(AzureLogHandler(
    connection_string=connection_string
))
```

#### For TypeScript/Node.js Applications

Use Application Insights SDK:

```typescript
import appInsights from "applicationinsights";

appInsights.setup(connectionString).setAutoDependencyCorrelation(true).start();
```

## Service-Specific Integration

### Story-Generator

Story-Generator uses shared resources by default:

```hcl
module "story_generator" {
  source = "../../modules/story-generator"

  # Use shared resources
  shared_postgresql_server_id     = module.shared_postgresql.server_id
  shared_redis_cache_id           = module.shared_redis.cache_id
  shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
}
```

**Note**: Connection strings must be manually stored in Key Vault when using shared resources. See environment outputs for connection string templates.

### Publisher

Publisher currently uses dedicated resources but can be migrated to use shared Redis:

```hcl
# Future: Use shared Redis
shared_redis_cache_id = module.shared_redis.cache_id
```

### Chain

Chain service doesn't currently use shared resources but could benefit from shared monitoring:

```hcl
shared_log_analytics_workspace_id = module.shared_monitoring.log_analytics_workspace_id
```

## Key Vault Secret Management

### Creating Secrets Manually

When using shared resources, create secrets in the service's Key Vault:

```bash
# Get connection strings from Terraform outputs
az keyvault secret set \
  --vault-name "mystira-sg-dev-kv" \
  --name "postgres-connection-string" \
  --value "$(terraform output -raw shared_postgresql_connection_string_storygenerator)"

az keyvault secret set \
  --vault-name "mystira-sg-dev-kv" \
  --name "redis-connection-string" \
  --value "$(terraform output -raw shared_redis_connection_string)"
```

### Using Terraform

For dedicated resources, Terraform automatically stores secrets. For shared resources, use environment-level secrets:

```hcl
# In environment main.tf
resource "azurerm_key_vault_secret" "story_generator_postgres_shared" {
  name         = "postgres-connection-string"
  value        = "Host=${module.shared_postgresql.server_fqdn};Port=5432;Username=${module.shared_postgresql.admin_login};Password=${module.shared_postgresql.admin_password};Database=storygenerator;SSL Mode=Require;Trust Server Certificate=true"
  key_vault_id = module.story_generator.key_vault_id
}
```

## Kubernetes Secret Sync

### Option 1: Manual Secret Creation

Create Kubernetes secrets from Key Vault:

```bash
# Get secret from Key Vault
POSTGRES_CONN=$(az keyvault secret show \
  --vault-name "mystira-sg-dev-kv" \
  --name "postgres-connection-string" \
  --query value -o tsv)

# Create Kubernetes secret
kubectl create secret generic mystira-story-generator-secrets \
  --from-literal=postgres-connection-string="$POSTGRES_CONN"
```

### Option 2: Azure Key Vault CSI Driver (Future)

Install Azure Key Vault CSI driver for automatic secret sync:

```yaml
apiVersion: v1
kind: Pod
spec:
  volumes:
    - name: secrets-store
      csi:
        driver: secrets-store.csi.k8s.io
        readOnly: true
        volumeAttributes:
          secretProviderClass: "azure-keyvault"
```

## Best Practices

1. **Use shared resources for cost efficiency**: Multiple services can share PostgreSQL/Redis
2. **Dedicated databases per service**: Use separate databases on shared server for isolation
3. **Connection pooling**: Configure appropriate connection pool sizes per service
4. **Secret rotation**: Implement secret rotation policies
5. **Monitoring**: Use shared monitoring workspace for unified observability
6. **Network security**: All resources are in private subnets with VNet integration

## Troubleshooting

### Connection Issues

**Problem**: Cannot connect to shared PostgreSQL  
**Solutions**:

- Verify VNet/subnet configuration
- Check NSG rules allow traffic
- Verify database exists: `SELECT datname FROM pg_database;`
- Check connection string format matches .NET Npgsql requirements

**Problem**: Cannot connect to shared Redis  
**Solutions**:

- Verify VNet/subnet configuration
- Check firewall rules (if Premium tier)
- Verify connection string format
- Check TLS/SSL settings match

### Secret Access Issues

**Problem**: Kubernetes pods can't read secrets  
**Solutions**:

- Verify secrets exist: `kubectl get secrets`
- Check Key Vault access policies
- Verify service account permissions
- Check workload identity configuration

## Related Documentation

- [Infrastructure Guide](./infrastructure.md)
- [ADR-0001: Infrastructure Organization](./architecture/adr/0001-infrastructure-organization-hybrid-approach.md)
- [Azure PostgreSQL Flexible Server Documentation](https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/)
- [Azure Cache for Redis Documentation](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/)
