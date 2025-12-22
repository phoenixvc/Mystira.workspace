# Runbook: Disaster Recovery

**Last Updated**: 2025-12-22
**Owner**: Platform Engineering Team
**Approval Required**: Engineering Manager (required before execution)
**Estimated Time**: 30-120 minutes (depending on scenario)

## Purpose

This runbook provides procedures for recovering the Mystira platform from catastrophic failures, including complete region failures, data center outages, or major data corruption events.

## Recovery Objectives

| Metric | Target | Definition |
|--------|--------|------------|
| **RTO** (Recovery Time Objective) | 4 hours | Maximum acceptable downtime |
| **RPO** (Recovery Point Objective) | 1 hour | Maximum acceptable data loss |
| **MTTR** (Mean Time to Recovery) | 2 hours | Target average recovery time |

## Disaster Scenarios

| Scenario | Severity | RTO | RPO | Primary Recovery Method |
|----------|----------|-----|-----|-------------------------|
| Single service failure | Low | 15 min | 0 | Rollback/Restart |
| AKS cluster failure | Medium | 1 hour | 0 | Cluster recreation |
| Database failure | High | 2 hours | 1 hour | Point-in-time restore |
| Complete region failure | Critical | 4 hours | 1 hour | Cross-region failover |
| Data corruption | Critical | 4 hours | Varies | Backup restore |
| Ransomware/Security breach | Critical | 4+ hours | Varies | Clean restore |

---

## Prerequisites

- [ ] Azure CLI installed and authenticated with Owner role
- [ ] kubectl configured for cluster access
- [ ] Access to backup storage account
- [ ] Access to Terraform state storage
- [ ] Engineering Manager approval obtained
- [ ] Incident bridge established

## Pre-Disaster Checklist

Ensure these are in place BEFORE a disaster:

- [ ] Database backups running daily (verified weekly)
- [ ] Point-in-time restore enabled on PostgreSQL
- [ ] Blob storage soft delete enabled (30 days)
- [ ] Key Vault soft delete enabled
- [ ] Terraform state backed up
- [ ] DR procedures tested quarterly

---

## Scenario 1: AKS Cluster Failure

**Symptoms**: All pods unschedulable, nodes not ready, control plane unreachable

**Estimated Recovery Time**: 30-60 minutes

### Step 1: Confirm Cluster Status

```bash
# Check cluster health
az aks show \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-core-aks-san \
  --query "provisioningState" \
  -o tsv

# Check node status
kubectl get nodes
```

### Step 2: Attempt Cluster Repair

```bash
# Attempt to reconcile the cluster
az aks update \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-core-aks-san

# If nodes are unhealthy, try node pool upgrade
az aks nodepool upgrade \
  --resource-group mys-prod-core-rg-san \
  --cluster-name mys-prod-core-aks-san \
  --name nodepool1 \
  --kubernetes-version $(az aks show -g mys-prod-core-rg-san -n mys-prod-core-aks-san --query kubernetesVersion -o tsv)
```

### Step 3: If Repair Fails - Recreate Cluster

```bash
# ⚠️ WARNING: This will recreate the cluster
# Ensure you have application manifests and secrets backed up

cd infra/terraform/environments/prod

# Taint the AKS resource to force recreation
terraform taint module.aks.azurerm_kubernetes_cluster.main

# Recreate
terraform plan -out=tfplan
terraform apply tfplan
```

### Step 4: Redeploy Applications

```bash
# Get new cluster credentials
az aks get-credentials \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-core-aks-san \
  --overwrite-existing

# Apply all Kubernetes manifests
kubectl apply -k infra/kubernetes/overlays/prod

# Wait for deployments
kubectl rollout status deployment -n mys-prod --timeout=600s
```

---

## Scenario 2: PostgreSQL Database Failure

**Symptoms**: Database connection errors, data unavailable

**Estimated Recovery Time**: 1-2 hours

### Step 1: Assess Database Status

```bash
# Check database server status
az postgres flexible-server show \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-core-psql-san \
  --query "state" \
  -o tsv

# Check replication status (if using replicas)
az postgres flexible-server replica list \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-core-psql-san
```

### Step 2: Point-in-Time Restore

```bash
# ⚠️ WARNING: Creates a new database server
# Determine restore point (within last 35 days)
RESTORE_TIME=$(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%SZ)

# Create restored server
az postgres flexible-server restore \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-core-psql-san-restored \
  --source-server mys-prod-core-psql-san \
  --restore-time "$RESTORE_TIME"
```

### Step 3: Update Application Connection Strings

```bash
# Get new server FQDN
NEW_FQDN=$(az postgres flexible-server show \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-core-psql-san-restored \
  --query "fullyQualifiedDomainName" \
  -o tsv)

# Update Key Vault secret
az keyvault secret set \
  --vault-name mys-prod-app-kv-san \
  --name "PostgreSQLConnectionString" \
  --value "Host=${NEW_FQDN};Database=mystira_app;Username=mystiraadmin;Password=<password>"

# Restart applications to pick up new connection string
kubectl rollout restart deployment -n mys-prod
```

### Step 4: Verify Data Integrity

```bash
# Connect to new database and verify
psql "host=${NEW_FQDN} dbname=mystira_app user=mystiraadmin" << 'EOF'
-- Check record counts
SELECT 'accounts' as table_name, COUNT(*) as count FROM accounts
UNION ALL
SELECT 'game_sessions', COUNT(*) FROM game_sessions
UNION ALL
SELECT 'scenarios', COUNT(*) FROM scenarios;

-- Check latest data timestamp
SELECT MAX(updated_at) as latest_update FROM accounts;
EOF
```

---

## Scenario 3: Complete Region Failure

**Symptoms**: All Azure resources in South Africa North unavailable

**Estimated Recovery Time**: 2-4 hours

### Step 1: Confirm Region Outage

```bash
# Check Azure status
curl -s https://status.azure.com/en-us/status | grep "South Africa"

# Or check via Azure CLI
az account list-locations -o table | grep -i africa
```

### Step 2: Deploy to Backup Region

```bash
# ⚠️ MAJOR OPERATION: Requires significant infrastructure changes

# Option A: If using Azure Front Door with backup origin
# Front Door should automatically route to healthy origin

# Option B: Deploy new infrastructure to backup region
cd infra/terraform/environments/prod-dr

terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

### Step 3: Restore Data from Geo-Redundant Backups

```bash
# Restore PostgreSQL from geo-redundant backup
az postgres flexible-server geo-restore \
  --resource-group mys-prod-dr-rg-weu \
  --name mys-prod-dr-psql-weu \
  --source-server "/subscriptions/<sub>/resourceGroups/mys-prod-core-rg-san/providers/Microsoft.DBforPostgreSQL/flexibleServers/mys-prod-core-psql-san" \
  --location westeurope

# Restore blob storage from geo-redundant backup
az storage account failover \
  --resource-group mys-prod-dr-rg-weu \
  --name mysprodcorestoragedr
```

### Step 4: Update DNS

```bash
# Update Azure DNS to point to DR region
az network dns record-set a update \
  --resource-group mys-prod-dns-rg \
  --zone-name mystira.app \
  --name api \
  --set aRecords[0].ipv4Address="<DR_IP_ADDRESS>"

# Update TTL for faster failover
az network dns record-set a update \
  --resource-group mys-prod-dns-rg \
  --zone-name mystira.app \
  --name api \
  --set ttl=60
```

---

## Scenario 4: Data Corruption

**Symptoms**: Inconsistent data, failed integrity checks, user reports of missing/incorrect data

**Estimated Recovery Time**: 2-4 hours

### Step 1: Stop Writes Immediately

```bash
# Scale down write-capable deployments
kubectl scale deployment mys-app-api -n mys-prod --replicas=0

# Or put service in maintenance mode
kubectl set env deployment/mys-app-api -n mys-prod MAINTENANCE_MODE=true
kubectl rollout restart deployment/mys-app-api -n mys-prod
```

### Step 2: Identify Corruption Scope

```bash
# Determine when corruption started
psql "host=mys-prod-core-psql-san.postgres.database.azure.com dbname=mystira_app" << 'EOF'
-- Find anomalies in recent data
SELECT
  date_trunc('hour', updated_at) as hour,
  COUNT(*) as updates
FROM accounts
WHERE updated_at > NOW() - INTERVAL '24 hours'
GROUP BY 1
ORDER BY 1;

-- Check for invalid data patterns
SELECT * FROM accounts
WHERE email IS NULL OR email = ''
LIMIT 10;
EOF
```

### Step 3: Restore from Point-in-Time

```bash
# Identify last known good timestamp
LAST_GOOD_TIME="2025-12-22T10:00:00Z"  # Adjust based on investigation

# Restore to new server
az postgres flexible-server restore \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-core-psql-san-clean \
  --source-server mys-prod-core-psql-san \
  --restore-time "$LAST_GOOD_TIME"
```

### Step 4: Merge Good Data (If Partial Corruption)

```bash
# This requires careful SQL to merge clean data with any valid new data
# Consult with DBA before executing

# Example: Export good data from restore, import to corrupted tables
pg_dump -h mys-prod-core-psql-san-clean.postgres.database.azure.com \
  -U mystiraadmin \
  -d mystira_app \
  -t accounts \
  --data-only \
  -f accounts_clean.sql

# Review and apply carefully
```

---

## Communication Templates

### Initial Incident Notification

```
🚨 INCIDENT: [Severity] - [Brief Description]

Status: Investigating / Identified / Recovering
Impact: [User impact description]
Start Time: [UTC timestamp]

Current Actions:
- [Action 1]
- [Action 2]

Next Update: [Time]

Incident Commander: @[name]
```

### Recovery Update

```
🔄 INCIDENT UPDATE: [Brief Description]

Status: Recovering
Progress: [X]% complete
ETA to Recovery: [Time estimate]

Completed:
- ✅ [Completed step]

In Progress:
- 🔄 [Current step]

Next Update: [Time]
```

### Resolution Notification

```
✅ RESOLVED: [Brief Description]

Duration: [X hours Y minutes]
Root Cause: [Brief description]
Resolution: [What was done]

Post-mortem scheduled for: [Date/Time]

Thank you for your patience.
```

---

## Verification Checklist

After any disaster recovery:

- [ ] All services responding to health checks
- [ ] Database connectivity verified
- [ ] Data integrity verified (record counts, checksums)
- [ ] User authentication working
- [ ] Critical user flows tested
- [ ] Monitoring and alerting operational
- [ ] Logs flowing to Application Insights
- [ ] Error rates back to normal levels

## Post-Recovery Actions

- [ ] Create detailed incident timeline
- [ ] Preserve all logs and metrics from incident period
- [ ] Update affected runbooks with lessons learned
- [ ] Schedule post-mortem within 48 hours
- [ ] Review and update DR procedures if gaps found
- [ ] Test DR procedures in staging (if not already current)

---

## Backup Inventory

| Resource | Backup Location | Retention | Recovery Method |
|----------|-----------------|-----------|-----------------|
| PostgreSQL | Azure-managed | 35 days | Point-in-time restore |
| Blob Storage | GRS replication | Continuous | Failover / Copy |
| Key Vault | Soft delete | 90 days | Recover deleted items |
| Terraform State | Azure Storage | 30 versions | Version restore |
| AKS Secrets | Key Vault refs | N/A | Redeploy from KV |

## Emergency Contacts

| Role | Name | Phone | Email |
|------|------|-------|-------|
| Engineering Manager | [Name] | [Phone] | [Email] |
| DBA Lead | [Name] | [Phone] | [Email] |
| Security Lead | [Name] | [Phone] | [Email] |
| Azure Support | - | - | [Support ticket] |

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-22 | Platform Team | Initial runbook |
