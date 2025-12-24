# Data Migration Plan - Azure v2.2 Naming Convention

**Status**: Draft
**Version**: 1.0
**Last Updated**: 2025-12-21
**Author**: DevOps Team

## Overview

This document outlines the data migration strategy for transitioning from the old Azure naming convention to v2.2. The migration must ensure zero data loss and minimal downtime for production environments.

## Data at Risk

### PostgreSQL Databases

Each environment has a PostgreSQL Flexible Server with application databases:

**Dev Environment:**

- Server: `mys-dev-db-psql-eus` → `mys-dev-core-psql-san`
- Databases: `chain_db`, `publisher_db`, `story_generator_db`
- Size: ~5-10 GB (estimated)
- Criticality: Low (can be recreated)

**Staging Environment:**

- Server: `mys-staging-db-psql-eus` → `mys-staging-core-psql-san`
- Databases: `chain_db`, `publisher_db`, `story_generator_db`
- Size: ~10-20 GB (estimated)
- Criticality: Medium (test data, but valuable)

**Production Environment:**

- Server: `mys-prod-db-psql-eus` → `mys-prod-core-psql-san`
- Databases: `chain_db`, `publisher_db`, `story_generator_db`
- Size: ~50-100 GB (estimated)
- Criticality: **CRITICAL** (cannot lose data)

### Redis Cache

- Dev: `mys-dev-redis-eus` → `mys-dev-core-redis-san`
- Staging: `mys-staging-redis-eus` → `mys-staging-core-redis-san`
- Production: `mys-prod-redis-eus` → `mys-prod-core-redis-san`
- Criticality: Low (cache data, can be regenerated)

### Azure Storage Accounts

- Dev: `mysdevstorageeus` → `mysdevcorestoragesa`
- Staging: `mysstagingstorageeus` → `mysstagcorestoragesa`
- Production: `mysprodstorageeus` → `mysprodcorestoragesa`
- Contains: Application files, logs, backups
- Criticality: Medium to High

## Migration Strategies

### Strategy 1: Fresh Deployment (Recommended for Dev)

**Approach**: Deploy new infrastructure, recreate/reseed data

**Pros:**

- Clean slate
- No migration complexity
- Validates terraform from scratch

**Cons:**

- Lose all dev data

**Best For:** Dev environment

**Steps:**

1. Backup existing dev data (optional, for reference)
2. Deploy new infrastructure with `terraform apply`
3. Reseed databases with test data
4. Validate application functionality
5. Delete old infrastructure with `terraform destroy` (old state)

### Strategy 2: Blue-Green Deployment (Recommended for Staging/Production)

**Approach**: Deploy new infrastructure alongside old, migrate data, switch traffic

**Pros:**

- Zero data loss
- Easy rollback
- No downtime (or minimal)
- Production-safe

**Cons:**

- Requires double resources temporarily
- More complex coordination
- Higher cost during migration

**Best For:** Staging and Production environments

**Steps:**

1. Deploy new infrastructure (blue-green)
2. Backup all databases
3. Restore backups to new PostgreSQL servers
4. Verify data integrity
5. Update DNS/connection strings to point to new resources
6. Monitor for issues
7. After validation period (24-48 hours), destroy old infrastructure

### Strategy 3: In-Place Migration with Terraform Import (Not Recommended)

**Approach**: Rename resources and import into new terraform state

**Cons:**

- Cannot rename PostgreSQL servers (requires recreation)
- Cannot rename Redis (requires recreation)
- Cannot rename storage accounts (requires recreation)
- High risk of terraform state corruption
- Difficult rollback

**Verdict:** Not viable for this migration due to Azure resource naming restrictions.

## Recommended Approach per Environment

| Environment | Strategy         | Downtime   | Risk Level |
| ----------- | ---------------- | ---------- | ---------- |
| Dev         | Fresh Deployment | 1-2 hours  | Low        |
| Staging     | Blue-Green       | ~5 minutes | Low        |
| Production  | Blue-Green       | ~5 minutes | Medium     |

## Detailed Migration Procedures

### Dev Environment - Fresh Deployment

#### Pre-Migration

```bash
# 1. Optional: Backup existing dev data for reference
export OLD_PG_SERVER="mys-dev-db-psql-eus"
export OLD_PG_ADMIN="psqladmin"
export BACKUP_DATE=$(date +%Y%m%d_%H%M%S)

# Backup each database
for db in chain_db publisher_db story_generator_db; do
  az postgres flexible-server db export \
    --resource-group mys-dev-mystira-rg-eus \
    --server-name $OLD_PG_SERVER \
    --database-name $db \
    --backup-file "backup_${db}_${BACKUP_DATE}.sql"
done
```

#### Deployment

```bash
# 2. Deploy new infrastructure
cd infra/terraform/environments/dev
terraform init
terraform plan -out=tfplan
terraform apply tfplan

# 3. Get new connection details
export NEW_PG_SERVER="mys-dev-core-psql-san"
export NEW_PG_ADMIN="psqladmin"

# 4. Optional: Restore backups if needed
for db in chain_db publisher_db story_generator_db; do
  az postgres flexible-server db import \
    --resource-group mys-dev-core-rg-san \
    --server-name $NEW_PG_SERVER \
    --database-name $db \
    --backup-file "backup_${db}_${BACKUP_DATE}.sql"
done

# 5. Redeploy applications
kubectl config use-context mys-dev-core-aks-san
kubectl apply -k infra/kubernetes/overlays/dev/

# 6. Verify applications
kubectl get pods -n mys-dev
kubectl logs -n mys-dev -l app.kubernetes.io/name=mys-publisher --tail=50

# 7. Test endpoints
curl https://dev.publisher.mystira.app/health
curl https://dev.chain.mystira.app/health
curl https://dev.story-api.mystira.app/health
```

#### Post-Migration

```bash
# 8. After validation (24 hours), destroy old infrastructure
# Update terraform to point to old state
cd infra/terraform/environments/dev
terraform destroy -target=module.database -target=module.redis
```

### Staging/Production - Blue-Green Deployment

#### Phase 1: Backup (Critical)

```bash
#!/bin/bash
# backup-postgres.sh - Run this BEFORE deploying new infrastructure

set -euo pipefail

ENVIRONMENT="staging"  # or "prod"
BACKUP_DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="./backups/${ENVIRONMENT}_${BACKUP_DATE}"

mkdir -p "$BACKUP_DIR"

# Old server details
OLD_RG="mys-${ENVIRONMENT}-mystira-rg-eus"
OLD_SERVER="mys-${ENVIRONMENT}-db-psql-eus"
OLD_ADMIN="psqladmin"

echo "=== Starting backup of ${ENVIRONMENT} PostgreSQL ==="

# Get password from Azure Key Vault
OLD_KV="mys${ENVIRONMENT}kv"
PG_PASSWORD=$(az keyvault secret show \
  --vault-name "$OLD_KV" \
  --name "postgres-admin-password" \
  --query value -o tsv)

# Backup each database using pg_dump
for db in chain_db publisher_db story_generator_db; do
  echo "Backing up ${db}..."

  PGPASSWORD="$PG_PASSWORD" pg_dump \
    -h "${OLD_SERVER}.postgres.database.azure.com" \
    -U "$OLD_ADMIN" \
    -d "$db" \
    -F c \
    -f "${BACKUP_DIR}/${db}.backup"

  # Also create SQL format for easy inspection
  PGPASSWORD="$PG_PASSWORD" pg_dump \
    -h "${OLD_SERVER}.postgres.database.azure.com" \
    -U "$OLD_ADMIN" \
    -d "$db" \
    -F p \
    -f "${BACKUP_DIR}/${db}.sql"

  # Verify backup
  if [ -f "${BACKUP_DIR}/${db}.backup" ]; then
    SIZE=$(du -h "${BACKUP_DIR}/${db}.backup" | cut -f1)
    echo "✓ ${db} backed up successfully (${SIZE})"
  else
    echo "✗ FAILED to backup ${db}"
    exit 1
  fi
done

# Backup to Azure Storage for redundancy
STORAGE_ACCOUNT="mys${ENVIRONMENT}storageeus"
az storage blob upload-batch \
  --account-name "$STORAGE_ACCOUNT" \
  --destination "database-backups/${BACKUP_DATE}" \
  --source "$BACKUP_DIR"

echo "=== Backup completed successfully ==="
echo "Local: $BACKUP_DIR"
echo "Azure: ${STORAGE_ACCOUNT}/database-backups/${BACKUP_DATE}"
echo ""
echo "IMPORTANT: Verify backups before proceeding!"
echo "ls -lh $BACKUP_DIR"
```

#### Phase 2: Deploy New Infrastructure

```bash
#!/bin/bash
# deploy-new-infra.sh

set -euo pipefail

ENVIRONMENT="staging"  # or "prod"

echo "=== Deploying new ${ENVIRONMENT} infrastructure ==="

cd "infra/terraform/environments/${ENVIRONMENT}"

# Initialize with new backend
terraform init

# Plan deployment
terraform plan -out=tfplan

# Review plan carefully!
echo "Review the plan above. Press Enter to continue or Ctrl+C to abort..."
read

# Apply
terraform apply tfplan

echo "=== New infrastructure deployed ==="
echo "Next: Restore data to new PostgreSQL server"
```

#### Phase 3: Restore Data

```bash
#!/bin/bash
# restore-postgres.sh

set -euo pipefail

ENVIRONMENT="staging"  # or "prod"
BACKUP_DATE="20231221_143000"  # Use actual backup date
BACKUP_DIR="./backups/${ENVIRONMENT}_${BACKUP_DATE}"

# New server details
NEW_RG="mys-${ENVIRONMENT}-core-rg-san"
NEW_SERVER="mys-${ENVIRONMENT}-core-psql-san"
NEW_ADMIN="psqladmin"

echo "=== Restoring ${ENVIRONMENT} PostgreSQL from backup ==="

# Get password from new Key Vault
NEW_KV="mys-${ENVIRONMENT}-shared-kv-san"
PG_PASSWORD=$(az keyvault secret show \
  --vault-name "$NEW_KV" \
  --name "postgres-admin-password" \
  --query value -o tsv)

# Restore each database
for db in chain_db publisher_db story_generator_db; do
  echo "Restoring ${db}..."

  # Drop and recreate database
  PGPASSWORD="$PG_PASSWORD" psql \
    -h "${NEW_SERVER}.postgres.database.azure.com" \
    -U "$NEW_ADMIN" \
    -d postgres \
    -c "DROP DATABASE IF EXISTS ${db};"

  PGPASSWORD="$PG_PASSWORD" psql \
    -h "${NEW_SERVER}.postgres.database.azure.com" \
    -U "$NEW_ADMIN" \
    -d postgres \
    -c "CREATE DATABASE ${db};"

  # Restore from backup
  PGPASSWORD="$PG_PASSWORD" pg_restore \
    -h "${NEW_SERVER}.postgres.database.azure.com" \
    -U "$NEW_ADMIN" \
    -d "$db" \
    -v \
    "${BACKUP_DIR}/${db}.backup"

  echo "✓ ${db} restored successfully"
done

echo "=== Restore completed ==="
echo "Next: Verify data integrity"
```

#### Phase 4: Data Verification

```bash
#!/bin/bash
# verify-data.sh

set -euo pipefail

ENVIRONMENT="staging"  # or "prod"

OLD_SERVER="mys-${ENVIRONMENT}-db-psql-eus"
NEW_SERVER="mys-${ENVIRONMENT}-core-psql-san"
ADMIN="psqladmin"

# Get passwords
OLD_KV="mys${ENVIRONMENT}kv"
NEW_KV="mys-${ENVIRONMENT}-shared-kv-san"

OLD_PASSWORD=$(az keyvault secret show --vault-name "$OLD_KV" --name "postgres-admin-password" -o tsv --query value)
NEW_PASSWORD=$(az keyvault secret show --vault-name "$NEW_KV" --name "postgres-admin-password" -o tsv --query value)

echo "=== Verifying data integrity ==="

for db in chain_db publisher_db story_generator_db; do
  echo "Checking ${db}..."

  # Count rows in old database
  OLD_COUNT=$(PGPASSWORD="$OLD_PASSWORD" psql \
    -h "${OLD_SERVER}.postgres.database.azure.com" \
    -U "$ADMIN" \
    -d "$db" \
    -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';")

  # Count rows in new database
  NEW_COUNT=$(PGPASSWORD="$NEW_PASSWORD" psql \
    -h "${NEW_SERVER}.postgres.database.azure.com" \
    -U "$ADMIN" \
    -d "$db" \
    -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';")

  if [ "$OLD_COUNT" -eq "$NEW_COUNT" ]; then
    echo "✓ ${db}: Table count matches (${OLD_COUNT} tables)"
  else
    echo "✗ ${db}: Table count MISMATCH (old: ${OLD_COUNT}, new: ${NEW_COUNT})"
    exit 1
  fi
done

echo "=== Verification successful ==="
```

#### Phase 5: Switch Traffic

```bash
# 1. Update Kubernetes secrets with new connection strings
kubectl create secret generic mys-publisher-db-secret \
  --from-literal=connection-string="postgresql://psqladmin:${PASSWORD}@mys-${ENV}-core-psql-san.postgres.database.azure.com/publisher_db" \
  --dry-run=client -o yaml | kubectl apply -f -

# 2. Restart deployments to pick up new secrets
kubectl rollout restart deployment -n mys-${ENV} -l app.kubernetes.io/part-of=mys

# 3. Monitor rollout
kubectl rollout status deployment -n mys-${ENV} -l app.kubernetes.io/part-of=mys

# 4. Verify health
kubectl get pods -n mys-${ENV}
```

#### Phase 6: Monitor

```bash
# Monitor application logs
kubectl logs -n mys-${ENV} -l app.kubernetes.io/name=mys-publisher --tail=100 -f

# Monitor database connections
az postgres flexible-server show \
  --resource-group mys-${ENV}-core-rg-san \
  --name mys-${ENV}-core-psql-san \
  --query "state"

# Test endpoints
for service in publisher chain story-generator; do
  curl -f https://${ENV}.${service}.mystira.app/health || echo "FAILED: ${service}"
done
```

#### Phase 7: Cleanup (After 24-48 hour validation period)

```bash
# Only after confirming everything works!

# 1. Destroy old infrastructure
cd infra/terraform/environments/${ENVIRONMENT}

# Point to old state
terraform destroy \
  -target=module.database \
  -target=module.redis \
  -target=module.storage

# 2. Delete old resource group (after all resources removed)
az group delete --name mys-${ENVIRONMENT}-mystira-rg-eus --yes
```

## Terraform State Protection

Add lifecycle rules to prevent accidental deletion:

```hcl
# infra/terraform/modules/database/main.tf

resource "azurerm_postgresql_flexible_server" "this" {
  # ... other config ...

  lifecycle {
    prevent_destroy = true
    ignore_changes  = [tags]
  }
}
```

## Migration Checklist

### Pre-Migration

- [ ] Review this migration plan
- [ ] Communicate maintenance window to stakeholders
- [ ] Backup all PostgreSQL databases
- [ ] Verify backups are valid (test restore to temp server)
- [ ] Upload backups to Azure Storage for redundancy
- [ ] Document current connection strings
- [ ] Take screenshots of monitoring dashboards
- [ ] Ensure rollback plan is ready

### During Migration

- [ ] Deploy new infrastructure with Terraform
- [ ] Verify new resources are created correctly
- [ ] Restore databases to new PostgreSQL servers
- [ ] Verify data integrity (row counts, checksums)
- [ ] Update Kubernetes secrets with new connection strings
- [ ] Deploy applications to new AKS cluster
- [ ] Test all endpoints for functionality
- [ ] Monitor error rates and performance

### Post-Migration

- [ ] Applications running successfully for 24 hours
- [ ] No errors in application logs
- [ ] Database connections stable
- [ ] Performance metrics normal
- [ ] User acceptance testing passed
- [ ] Update documentation with new resource names
- [ ] Destroy old infrastructure
- [ ] Archive backups for compliance

## Rollback Procedures

See [ROLLBACK_PROCEDURE.md](./ROLLBACK_PROCEDURE.md) for detailed rollback steps.

**Quick Rollback (if issues detected immediately):**

```bash
# 1. Switch Kubernetes back to old resources
kubectl create secret generic mys-publisher-db-secret \
  --from-literal=connection-string="postgresql://psqladmin:${PASSWORD}@mys-${ENV}-db-psql-eus.postgres.database.azure.com/publisher_db" \
  --dry-run=client -o yaml | kubectl apply -f -

# 2. Restart deployments
kubectl rollout restart deployment -n mys-${ENV} -l app.kubernetes.io/part-of=mys

# 3. Destroy new infrastructure
cd infra/terraform/environments/${ENVIRONMENT}
terraform destroy
```

## Special Considerations

### Storage Account Migration

Storage accounts cannot be renamed. Data must be copied:

```bash
# Use AzCopy for efficient transfer
azcopy copy \
  "https://mys${ENV}storageeus.blob.core.windows.net/*?${OLD_SAS}" \
  "https://mys${ENV}corestoragesa.blob.core.windows.net/?${NEW_SAS}" \
  --recursive
```

### Key Vault Migration

Secrets cannot be moved between vaults. Must be copied:

```bash
# Export secrets from old vault
for secret in $(az keyvault secret list --vault-name mys${ENV}kv --query "[].name" -o tsv); do
  value=$(az keyvault secret show --vault-name mys${ENV}kv --name $secret --query value -o tsv)
  az keyvault secret set --vault-name mys-${ENV}-shared-kv-san --name $secret --value "$value"
done
```

### Redis Cache

No data migration needed (cache data). Just update connection strings.

## Estimated Timeline

| Environment | Backup | Deploy | Restore | Verify | Total  |
| ----------- | ------ | ------ | ------- | ------ | ------ |
| Dev         | -      | 30m    | -       | 15m    | 45m    |
| Staging     | 15m    | 30m    | 30m     | 30m    | 1h 45m |
| Production  | 30m    | 30m    | 1h      | 1h     | 3h     |

## Contacts

- **Lead**: DevOps Team Lead
- **DBA**: Database Administrator
- **On-Call**: Platform Engineering

## References

- [Deployment Strategy](./DEPLOYMENT_STRATEGY.md)
- [Testing Checklist](./TESTING_CHECKLIST.md)
- [Rollback Procedure](./ROLLBACK_PROCEDURE.md)
- [Azure PostgreSQL Backup Documentation](https://learn.microsoft.com/en-us/azure/postgresql/)
