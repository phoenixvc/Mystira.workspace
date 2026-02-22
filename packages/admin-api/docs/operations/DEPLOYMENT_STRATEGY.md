# Deployment Strategy - Azure v2.2 Naming Convention Migration

**Status**: Draft
**Version**: 1.0
**Last Updated**: 2025-12-21
**Author**: DevOps Team

## Executive Summary

This document outlines the complete deployment strategy for migrating from the old Azure naming convention to v2.2 (`[org]-[env]-[project]-[type]-[region]`). The migration affects all environments (dev, staging, production) and requires careful coordination to ensure zero data loss and minimal service disruption.

## Migration Scope

### Infrastructure Changes

| Resource Type | Old Name Pattern | New Name Pattern | Recreate Required |
|---------------|------------------|------------------|-------------------|
| Resource Groups | `mys-{env}-mystira-rg-eus` | `mys-{env}-core-rg-san` | No (can rename) |
| AKS Clusters | `mys-{env}-mystira-aks-eus` | `mys-{env}-core-aks-san` | Yes |
| PostgreSQL | `mys-{env}-db-psql-eus` | `mys-{env}-core-psql-san` | Yes |
| Redis Cache | `mys-{env}-redis-eus` | `mys-{env}-core-redis-san` | Yes |
| VNets | `mys-{env}-mystira-vnet-eus` | `mys-{env}-core-vnet-san` | No (can rename) |
| Storage Accounts | `mys{env}storageeus` | `mys{env}corestoragesa` | Yes |
| Key Vaults | `mys{env}kv` | `mys-{env}-{service}-kv-san` | Yes |
| Log Analytics | Multiple per service | 3 total (1 per env) | Consolidation |
| ACR | `mysprodacr` | `myssharedacr` | No (keep old) |

### Region Migration

All resources moving from **East US** → **South Africa North**

**Impact:**
- Physical data transfer required
- Network latency changes
- Compliance considerations
- Cost implications

## Deployment Strategies Comparison

### Option A: Fresh Deployment (Green Field)

**Approach:** Deploy new infrastructure, migrate data, cutover

**Workflow:**
```
Old Infra (running) → Deploy New Infra → Migrate Data → Test → Cutover → Destroy Old
```

**Pros:**
- Clean slate, no legacy issues
- Validates Terraform from scratch
- Clear separation between old and new
- Easy rollback (keep old running)

**Cons:**
- Requires double resources temporarily
- More complex data migration
- Higher cost during transition
- Coordination overhead

**Best For:** Staging and Production

**Cost Impact:** 2x infrastructure cost for 24-48 hours

### Option B: In-Place Update (Not Viable)

**Approach:** Rename resources and update Terraform state

**Why Not Viable:**
- PostgreSQL servers cannot be renamed
- Redis cache cannot be renamed
- Storage accounts cannot be renamed
- Key Vaults cannot be renamed
- AKS clusters cannot be renamed
- Region change requires recreation

**Verdict:** ❌ Not possible for this migration

### Option C: Terraform Import with Partial Recreation

**Approach:** Import renameable resources, recreate others

**Workflow:**
```
Rename RG/VNet → Import to Terraform → Recreate databases/AKS → Update apps
```

**Pros:**
- Reuse some existing resources
- Lower cost than full fresh deployment

**Cons:**
- Complex state management
- High risk of state corruption
- Still requires data migration for databases
- No rollback for imported resources

**Best For:** None - too risky

**Verdict:** ❌ Not recommended

### Recommended Strategy: Blue-Green with Phased Rollout

**Approach:** Deploy new infrastructure alongside old, migrate per service, gradual cutover

**Phases:**
1. Deploy new infrastructure (green)
2. Migrate databases (backup/restore)
3. Deploy applications to new AKS
4. Test with synthetic traffic
5. Switch DNS/ingress (cutover)
6. Monitor for 24-48 hours
7. Destroy old infrastructure (blue)

**Workflow:**
```
┌─────────────┐
│  Old Infra  │ ← Users (100% traffic)
│   (Blue)    │
└─────────────┘
      ↓
┌─────────────┐   ┌─────────────┐
│  Old Infra  │   │  New Infra  │
│   (Blue)    │   │  (Green)    │ ← Deploy & Test
└─────────────┘   └─────────────┘
      ↓                    ↓
      ├────── Migrate Data ─────┤
      ↓                    ↓
┌─────────────┐   ┌─────────────┐
│  Old Infra  │   │  New Infra  │ ← Users (100% traffic)
│   (Blue)    │   │  (Green)    │
└─────────────┘   └─────────────┘
                         ↓
                  ┌─────────────┐
                  │  New Infra  │ ← Users (100% traffic)
                  │  (Green)    │
                  └─────────────┘
                         ↓
                  Destroy Old (Blue)
```

**Pros:**
- Zero downtime (or minimal)
- Easy rollback (switch back to blue)
- Production-safe
- Test thoroughly before cutover

**Cons:**
- Requires double resources
- More complex orchestration
- Higher temporary cost

**Cost Impact:** 2x for 24-48 hours (~$200-400 depending on environment)

## Environment-Specific Strategies

### Dev Environment

**Strategy:** Fresh Deployment (simplified)

**Rationale:**
- Non-critical data
- Can afford downtime
- Cost-sensitive (avoid double resources)
- Faster migration

**Estimated Downtime:** 1-2 hours

**Procedure:**
1. Notify dev team of maintenance window
2. Optional: Backup databases for reference
3. Deploy new infrastructure
4. Reseed databases with test data
5. Deploy applications
6. Verify functionality
7. Destroy old infrastructure

**When:** Next available maintenance window (weekday afternoon)

### Staging Environment

**Strategy:** Blue-Green Deployment

**Rationale:**
- Important test data
- Practice for production
- Minimal downtime requirement
- Validate migration process

**Estimated Downtime:** 5-10 minutes (DNS cutover)

**Procedure:**
1. Deploy new infrastructure
2. Backup and restore databases
3. Verify data integrity
4. Deploy applications to new AKS
5. Run automated test suite
6. Cutover DNS/ingress
7. Monitor for 24 hours
8. Destroy old infrastructure

**When:** Week before production (Tuesday or Wednesday)

### Production Environment

**Strategy:** Blue-Green Deployment with Phased Cutover

**Rationale:**
- Zero data loss requirement
- Minimal downtime requirement
- Risk mitigation critical
- User impact must be minimized

**Estimated Downtime:** <5 minutes (DNS propagation)

**Procedure:**
1. **Pre-deployment (1 week before):**
   - Final review of migration plan
   - Stakeholder communication
   - Maintenance window approval
   - Backup verification procedures

2. **Deployment Day - Phase 1 (T-0):**
   - Deploy new infrastructure
   - Verify all resources created
   - Initial smoke tests

3. **Deployment Day - Phase 2 (T+1h):**
   - Backup production databases
   - Upload backups to Azure Storage
   - Verify backup integrity
   - Test restore on temporary server

4. **Deployment Day - Phase 3 (T+2h):**
   - Restore databases to new PostgreSQL
   - Verify data integrity (checksums, row counts)
   - Migrate storage account data (AzCopy)
   - Migrate Key Vault secrets

5. **Deployment Day - Phase 4 (T+3h):**
   - Deploy applications to new AKS
   - Run health checks
   - Synthetic transaction testing
   - Load testing (if applicable)

6. **Deployment Day - Phase 5 (T+4h - CUTOVER):**
   - Update ingress to point to new AKS
   - Monitor error rates
   - Verify user traffic flowing
   - Watch dashboards closely

7. **Post-Deployment (T+24h):**
   - Review metrics and logs
   - User acceptance testing
   - Performance comparison
   - Incident review (if any)

8. **Cleanup (T+48h):**
   - Final approval to destroy old infrastructure
   - Terraform destroy old resources
   - Archive backups
   - Update documentation

**When:** Saturday 6am-10am UTC (low traffic period)

## Detailed Deployment Steps

### Step 1: Pre-Deployment Validation

**Checklist:**
- [ ] All code changes reviewed and merged to branch `claude/standardize-dev-resources-cT39Z`
- [ ] PR analysis reviewed, critical issues addressed
- [ ] Terraform plan reviewed for all environments
- [ ] Database backup strategy validated
- [ ] Rollback procedure documented and understood
- [ ] Monitoring dashboards updated
- [ ] Stakeholders notified
- [ ] Maintenance window approved
- [ ] On-call engineers briefed

**Commands:**
```bash
# Verify terraform plans
for env in dev staging prod; do
  cd infra/terraform/environments/$env
  terraform init
  terraform plan -out=tfplan-$env
  terraform show tfplan-$env > tfplan-$env.txt
done

# Review plans
less infra/terraform/environments/*/tfplan-*.txt
```

### Step 2: Deploy New Infrastructure

**Per Environment:**

```bash
#!/bin/bash
# deploy-infrastructure.sh

set -euo pipefail

ENVIRONMENT=$1  # dev, staging, or prod

echo "=== Deploying ${ENVIRONMENT} infrastructure ==="

# Navigate to environment directory
cd "infra/terraform/environments/${ENVIRONMENT}"

# Initialize Terraform
terraform init

# Create execution plan
terraform plan -out=tfplan

# Apply plan
terraform apply tfplan

# Verify outputs
terraform output

echo "=== Infrastructure deployed successfully ==="

# Get AKS credentials
az aks get-credentials \
  --resource-group "mys-${ENVIRONMENT}-core-rg-san" \
  --name "mys-${ENVIRONMENT}-core-aks-san" \
  --overwrite-existing

# Verify cluster access
kubectl cluster-info
kubectl get nodes

echo "=== Deployment complete for ${ENVIRONMENT} ==="
```

### Step 3: Migrate Data

See [DATA_MIGRATION_PLAN.md](./DATA_MIGRATION_PLAN.md) for detailed procedures.

**Summary:**
```bash
# 1. Backup old databases
./scripts/backup-postgres.sh ${ENVIRONMENT}

# 2. Restore to new servers
./scripts/restore-postgres.sh ${ENVIRONMENT}

# 3. Verify data integrity
./scripts/verify-data.sh ${ENVIRONMENT}

# 4. Migrate storage accounts
azcopy copy \
  "https://mys${ENVIRONMENT}storageeus.blob.core.windows.net/*?${OLD_SAS}" \
  "https://mys${ENVIRONMENT}corestoragesa.blob.core.windows.net/?${NEW_SAS}" \
  --recursive

# 5. Migrate Key Vault secrets
./scripts/migrate-keyvault.sh ${ENVIRONMENT}
```

### Step 4: Deploy Applications

```bash
#!/bin/bash
# deploy-applications.sh

set -euo pipefail

ENVIRONMENT=$1

echo "=== Deploying applications to ${ENVIRONMENT} ==="

# Set kubeconfig context
kubectl config use-context "mys-${ENVIRONMENT}-core-aks-san"

# Create namespace
kubectl create namespace "mys-${ENVIRONMENT}" --dry-run=client -o yaml | kubectl apply -f -

# Deploy applications using Kustomize
kubectl apply -k "infra/kubernetes/overlays/${ENVIRONMENT}/"

# Wait for rollout
kubectl rollout status deployment -n "mys-${ENVIRONMENT}" -l app.kubernetes.io/part-of=mys

# Verify pods
kubectl get pods -n "mys-${ENVIRONMENT}"

echo "=== Applications deployed successfully ==="
```

### Step 5: Cutover

**DNS/Ingress Update:**

```bash
# Ingress will automatically get new IP
kubectl get ingress -n mys-${ENVIRONMENT}

# Verify certificates
kubectl get certificates -n mys-${ENVIRONMENT}

# Test endpoints
for service in publisher chain story-generator; do
  echo "Testing ${service}..."
  curl -f "https://${ENVIRONMENT}.${service}.mystira.app/health" || echo "FAILED"
done
```

**For production, gradual traffic shift:**

```bash
# Option: Use Azure Traffic Manager or Application Gateway for gradual shift
# 1. Route 10% traffic to new infrastructure
# 2. Monitor for 1 hour
# 3. Increase to 50%
# 4. Monitor for 1 hour
# 5. Increase to 100%
# 6. Decommission old infrastructure
```

### Step 6: Post-Deployment Validation

See [TESTING_CHECKLIST.md](./TESTING_CHECKLIST.md) for comprehensive testing procedures.

**Quick Validation:**
```bash
#!/bin/bash
# validate-deployment.sh

set -euo pipefail

ENVIRONMENT=$1

echo "=== Validating ${ENVIRONMENT} deployment ==="

# 1. Check all pods are running
POD_STATUS=$(kubectl get pods -n "mys-${ENVIRONMENT}" --field-selector=status.phase!=Running -o json | jq '.items | length')
if [ "$POD_STATUS" -eq 0 ]; then
  echo "✓ All pods running"
else
  echo "✗ ${POD_STATUS} pods not running"
  kubectl get pods -n "mys-${ENVIRONMENT}"
  exit 1
fi

# 2. Check ingress has external IP
INGRESS_IP=$(kubectl get ingress -n "mys-${ENVIRONMENT}" -o jsonpath='{.items[0].status.loadBalancer.ingress[0].ip}')
if [ -n "$INGRESS_IP" ]; then
  echo "✓ Ingress has external IP: ${INGRESS_IP}"
else
  echo "✗ Ingress has no external IP"
  exit 1
fi

# 3. Check certificates are ready
CERT_COUNT=$(kubectl get certificates -n "mys-${ENVIRONMENT}" -o json | jq '[.items[] | select(.status.conditions[] | select(.type=="Ready" and .status=="True"))] | length')
echo "✓ ${CERT_COUNT} certificates ready"

# 4. Test HTTPS endpoints
for service in publisher chain story-generator; do
  URL="https://${ENVIRONMENT}.${service}.mystira.app/health"
  if curl -sf "$URL" > /dev/null; then
    echo "✓ ${service} responding"
  else
    echo "✗ ${service} not responding at $URL"
  fi
done

# 5. Check database connectivity
for pod in $(kubectl get pods -n "mys-${ENVIRONMENT}" -l app.kubernetes.io/part-of=mys -o name); do
  echo "Checking database connectivity for $pod..."
  kubectl exec -n "mys-${ENVIRONMENT}" "$pod" -- sh -c 'echo "SELECT 1" | psql $DATABASE_URL' || echo "✗ DB connection failed"
done

echo "=== Validation complete ==="
```

### Step 7: Monitor

**Metrics to Watch:**
- Pod restart counts
- HTTP error rates (4xx, 5xx)
- Response times (p50, p95, p99)
- Database connection pool usage
- Memory and CPU usage
- Certificate expiration dates

**Commands:**
```bash
# Watch pods
kubectl get pods -n mys-${ENVIRONMENT} -w

# Stream logs
kubectl logs -n mys-${ENVIRONMENT} -l app.kubernetes.io/name=mys-publisher -f --tail=100

# Check events
kubectl get events -n mys-${ENVIRONMENT} --sort-by='.lastTimestamp'

# Monitor ingress
kubectl describe ingress -n mys-${ENVIRONMENT}
```

### Step 8: Destroy Old Infrastructure

**Only after 24-48 hour validation period!**

```bash
#!/bin/bash
# destroy-old-infrastructure.sh

set -euo pipefail

ENVIRONMENT=$1

echo "WARNING: This will destroy old ${ENVIRONMENT} infrastructure"
echo "Press Enter to continue or Ctrl+C to abort..."
read

# Delete old resource group (if all resources migrated)
az group delete \
  --name "mys-${ENVIRONMENT}-mystira-rg-eus" \
  --yes \
  --no-wait

echo "Old infrastructure deletion initiated"
echo "Check status: az group show --name mys-${ENVIRONMENT}-mystira-rg-eus"
```

## Rollback Strategy

See [ROLLBACK_PROCEDURE.md](./ROLLBACK_PROCEDURE.md) for detailed rollback steps.

**Quick Rollback Decision Tree:**

```
Issue Detected?
  ├─ Before Cutover → Abort deployment, keep old infrastructure
  ├─ During Cutover (0-1 hour) → Quick rollback (switch ingress back)
  ├─ After Cutover (1-24 hours) → Evaluate severity
  │   ├─ Critical → Rollback
  │   └─ Non-critical → Fix forward
  └─ After 24 hours → Fix forward only (old infrastructure destroyed)
```

## Communication Plan

### Stakeholders

- **Users**: End users of the platform
- **Development Team**: Application developers
- **DevOps Team**: Infrastructure engineers
- **Management**: Product and engineering leadership

### Pre-Deployment Communication

**1 Week Before:**
- Email to all stakeholders with maintenance window
- Post in Slack #engineering channel
- Update status page

**24 Hours Before:**
- Reminder email
- Reminder in Slack
- Pre-deployment briefing with on-call team

**1 Hour Before:**
- Final reminder in Slack
- Set status page to "Scheduled Maintenance"

### During Deployment

- Real-time updates in Slack #incidents channel
- Status page updates at each phase
- Escalation to management if issues arise

### Post-Deployment

- Success email to stakeholders
- Post-mortem if any incidents
- Update status page to "Operational"

## Timeline

### Dev Environment

| Day | Activity | Duration |
|-----|----------|----------|
| Week 1 | Code review and final testing | 2 days |
| Week 1 | Deploy new infrastructure | 30 min |
| Week 1 | Deploy applications | 30 min |
| Week 1 | Validation | 1 day |
| Week 1 | Destroy old infra | 15 min |

**Total: 3 days**

### Staging Environment

| Day | Activity | Duration |
|-----|----------|----------|
| Week 2 | Deploy new infrastructure | 30 min |
| Week 2 | Backup databases | 15 min |
| Week 2 | Restore databases | 30 min |
| Week 2 | Verify data integrity | 30 min |
| Week 2 | Deploy applications | 30 min |
| Week 2 | Automated testing | 1 hour |
| Week 2 | Cutover | 10 min |
| Week 2 | Monitoring period | 24 hours |
| Week 2 | Destroy old infra | 15 min |

**Total: 2 days**

### Production Environment

| Day | Activity | Duration |
|-----|----------|----------|
| Week 3 | Final stakeholder approval | 1 day |
| Week 3 | Deploy new infrastructure | 30 min |
| Week 3 | Backup databases | 30 min |
| Week 3 | Restore databases | 1 hour |
| Week 3 | Verify data integrity | 1 hour |
| Week 3 | Deploy applications | 30 min |
| Week 3 | Smoke testing | 30 min |
| Week 3 | Cutover | 10 min |
| Week 3 | Intensive monitoring | 4 hours |
| Week 3 | Monitoring period | 48 hours |
| Week 3 | Destroy old infra | 15 min |

**Total: 4 days (including validation periods)**

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Data loss during migration | Low | Critical | Multiple backups, verification scripts |
| Extended downtime | Medium | High | Blue-green deployment, tested rollback |
| Application errors post-migration | Medium | High | Comprehensive testing, gradual rollout |
| Cost overrun | Low | Medium | Temporary double resources budgeted |
| DNS propagation delays | Low | Low | Use Azure DNS with low TTL |
| Certificate issues | Medium | Medium | Pre-provision certificates, staging issuer |
| Terraform state corruption | Low | High | State backups, careful import procedures |
| Network connectivity issues | Low | Medium | VNet peering, firewall rules validated |

## Success Criteria

- [ ] All environments migrated to new naming convention
- [ ] Zero data loss
- [ ] Production downtime <5 minutes
- [ ] No critical incidents during migration
- [ ] All applications passing health checks
- [ ] Monitoring dashboards show normal metrics
- [ ] User acceptance testing passed
- [ ] Old infrastructure destroyed
- [ ] Documentation updated
- [ ] Post-mortem completed

## References

- [Data Migration Plan](./DATA_MIGRATION_PLAN.md)
- [Testing Checklist](./TESTING_CHECKLIST.md)
- [Rollback Procedure](./ROLLBACK_PROCEDURE.md)
- [Azure Resource Naming Convention ADR](../architecture/adr/0008-azure-resource-naming-conventions.md)
