# Rollback Procedure - Azure v2.2 Naming Convention Migration

**Status**: Draft
**Version**: 1.0
**Last Updated**: 2025-12-21
**Author**: DevOps Team

## Overview

This document provides detailed rollback procedures for the Azure v2.2 naming convention migration. Rollback procedures vary based on deployment phase and time elapsed since cutover.

## Rollback Decision Matrix

| Phase | Time Since Cutover | Rollback Approach | Data Loss Risk |
|-------|-------------------|-------------------|----------------|
| Pre-deployment | N/A | Abort deployment | None |
| Deployment (before cutover) | N/A | Destroy new infra | None |
| Immediate post-cutover | 0-1 hour | Quick rollback | None |
| Short-term post-cutover | 1-24 hours | Evaluated rollback | Low |
| Long-term post-cutover | 24-48 hours | Fix forward | Medium |
| Old infra destroyed | > 48 hours | Emergency recovery | High |

## Rollback Criteria

### Automatic Rollback (Critical Issues)

Immediately initiate rollback if ANY of these occur:

- [ ] HTTP 5xx error rate > 10% for more than 5 minutes
- [ ] Complete application outage
- [ ] Data corruption detected
- [ ] Database connectivity loss
- [ ] Security breach related to migration
- [ ] Cascading failures across services

### Evaluated Rollback (Serious Issues)

Evaluate rollback decision if:

- [ ] HTTP 5xx error rate > 5% for more than 15 minutes
- [ ] Performance degradation > 50%
- [ ] Critical functionality broken (not all features)
- [ ] Intermittent database connectivity issues
- [ ] Significant user complaints

### Fix Forward (Minor Issues)

Do NOT rollback for:

- [ ] Minor bugs that don't affect critical paths
- [ ] Performance degradation < 25%
- [ ] Non-critical functionality broken
- [ ] Cosmetic issues
- [ ] Individual user issues

## Pre-Deployment Phase Rollback

**Scenario**: Issues found before `terraform apply`

**Steps**:
1. Do not proceed with deployment
2. Review and fix issues
3. Re-validate terraform plans
4. Reschedule deployment

**No rollback needed** - deployment never started

## Deployment Phase Rollback (Before Cutover)

**Scenario**: Issues found after `terraform apply` but before traffic cutover

**Risk Level**: Low
**Data Loss**: None
**Downtime**: None

### Procedure

```bash
#!/bin/bash
# rollback-deployment-phase.sh

set -euo pipefail

ENVIRONMENT=$1  # dev, staging, or prod

echo "=== Rolling back ${ENVIRONMENT} deployment (pre-cutover) ==="

# 1. Do NOT cutover traffic - old infrastructure still serving users
echo "✓ Traffic still on old infrastructure, no user impact"

# 2. Destroy new infrastructure
cd "infra/terraform/environments/${ENVIRONMENT}"

echo "Destroying new infrastructure..."
terraform destroy -auto-approve

# 3. Verify destruction
az group show --name "mys-${ENVIRONMENT}-core-rg-san" \
  && echo "✗ Resource group still exists" \
  || echo "✓ Resource group destroyed"

echo "=== Rollback complete ==="
echo "Old infrastructure still serving traffic"
echo "Review issues, fix, and reschedule deployment"
```

## Quick Rollback (0-1 Hour Post-Cutover)

**Scenario**: Critical issues detected within 1 hour of cutover
**Risk Level**: Low
**Data Loss**: Minimal (1 hour of new data)
**Downtime**: 5-10 minutes

### Prerequisites

- [ ] Old infrastructure still running
- [ ] Old databases still accessible
- [ ] Decision approved by incident commander

### Procedure

#### Step 1: Immediate Actions

```bash
# INCIDENT COMMANDER DECLARATION
echo "ROLLBACK INITIATED at $(date)"
echo "Incident: _____________________"
echo "Commander: ____________________"
```

#### Step 2: Switch Traffic Back

```bash
#!/bin/bash
# quick-rollback.sh

set -euo pipefail

ENVIRONMENT=$1

echo "=== QUICK ROLLBACK: Switching traffic back to old infrastructure ==="

# 1. Get old AKS credentials
az aks get-credentials \
  --resource-group "mys-${ENVIRONMENT}-mystira-rg-eus" \
  --name "mys-${ENVIRONMENT}-mystira-aks-eus" \
  --overwrite-existing

# 2. Verify old infrastructure healthy
kubectl get pods -n "mys-${ENVIRONMENT}"
kubectl get ingress -n "mys-${ENVIRONMENT}"

# 3. Get old ingress IP
OLD_IP=$(kubectl get ingress -n "mys-${ENVIRONMENT}" -o jsonpath='{.items[0].status.loadBalancer.ingress[0].ip}')
echo "Old ingress IP: $OLD_IP"

# 4. Update DNS records to point back to old infrastructure
# (If using Azure DNS)
for service in publisher chain story-generator; do
  az network dns record-set a update \
    --resource-group "dns-zone-rg" \
    --zone-name "mystira.app" \
    --name "${ENVIRONMENT}.${service}" \
    --set "aRecords[0].ipv4Address=${OLD_IP}"
done

echo "=== Traffic switched back to old infrastructure ==="
echo "Monitor for 5 minutes to confirm traffic flow"
```

#### Step 3: Verify Rollback

```bash
# Verify DNS propagation
for service in publisher chain story-generator; do
  nslookup "${ENVIRONMENT}.${service}.mystira.app"
done

# Verify endpoints responding
for service in publisher chain story-generator; do
  curl -f "https://${ENVIRONMENT}.${service}.mystira.app/health" \
    && echo "✓ ${service} responding" \
    || echo "✗ ${service} not responding"
done

# Monitor application logs
kubectl logs -n "mys-${ENVIRONMENT}" -l app.kubernetes.io/name=mys-publisher --tail=50
```

#### Step 4: Handle Data Loss (1 Hour Window)

**Option A: Accept Data Loss**
- Simplest approach
- Lose 0-1 hour of new data
- Communicate to users

**Option B: Manual Data Recovery**

```bash
#!/bin/bash
# recover-recent-data.sh

# 1. Dump data created in new database during the 1 hour window
NEW_KV="mys-${ENVIRONMENT}-shared-kv-san"
NEW_SERVER="mys-${ENVIRONMENT}-core-psql-san"
NEW_PASSWORD=$(az keyvault secret show --vault-name "$NEW_KV" --name "postgres-admin-password" -o tsv --query value)

CUTOVER_TIME="2025-12-21 10:00:00"  # Actual cutover timestamp

for db in chain_db publisher_db story_generator_db; do
  PGPASSWORD="$NEW_PASSWORD" psql \
    -h "${NEW_SERVER}.postgres.database.azure.com" \
    -U "psqladmin" \
    -d "$db" \
    -c "COPY (SELECT * FROM users WHERE created_at > '${CUTOVER_TIME}') TO STDOUT CSV HEADER" \
    > "recovery_${db}_users.csv"

  # Repeat for other tables as needed
done

# 2. Import recovered data into old database
OLD_KV="mys${ENVIRONMENT}kv"
OLD_SERVER="mys-${ENVIRONMENT}-db-psql-eus"
OLD_PASSWORD=$(az keyvault secret show --vault-name "$OLD_KV" --name "postgres-admin-password" -o tsv --query value)

for db in chain_db publisher_db story_generator_db; do
  PGPASSWORD="$OLD_PASSWORD" psql \
    -h "${OLD_SERVER}.postgres.database.azure.com" \
    -U "psqladmin" \
    -d "$db" \
    -c "\COPY users FROM 'recovery_${db}_users.csv' CSV HEADER"
done
```

#### Step 5: Cleanup New Infrastructure

```bash
# After confirming old infrastructure is stable (1-2 hours)
cd "infra/terraform/environments/${ENVIRONMENT}"
terraform destroy -auto-approve
```

#### Step 6: Post-Incident

- [ ] Document incident in post-mortem
- [ ] Identify root cause
- [ ] Update migration plan
- [ ] Reschedule deployment after fixes

**Estimated Time**: 15-30 minutes
**User Impact**: 5-10 minutes downtime during DNS cutover

## Evaluated Rollback (1-24 Hours Post-Cutover)

**Scenario**: Serious issues detected 1-24 hours after cutover
**Risk Level**: Medium
**Data Loss**: Up to 24 hours of data
**Downtime**: 15-30 minutes

### Decision Process

**Rollback Committee** (required for approval):
- Incident Commander
- Engineering Lead
- Product Owner
- On-call Engineer

**Considerations**:
1. Severity of issue
2. User impact
3. Data loss implications (1-24 hours of new data)
4. Ability to fix forward
5. Business criticality

### Procedure

Similar to Quick Rollback, but with more comprehensive data recovery:

#### Data Recovery Strategy

**Option A: Partial Data Recovery**

```bash
#!/bin/bash
# partial-data-recovery.sh

# Export all data modified since cutover
CUTOVER_TIME="2025-12-21 10:00:00"

# 1. Identify tables with timestamp columns
for db in chain_db publisher_db story_generator_db; do
  echo "Analyzing $db..."

  # Export new/modified records
  TABLES=$(PGPASSWORD="$NEW_PASSWORD" psql \
    -h "${NEW_SERVER}.postgres.database.azure.com" \
    -U "psqladmin" \
    -d "$db" \
    -t -c "SELECT tablename FROM pg_tables WHERE schemaname='public'")

  for table in $TABLES; do
    # Check if table has created_at or updated_at column
    HAS_TIMESTAMP=$(PGPASSWORD="$NEW_PASSWORD" psql \
      -h "${NEW_SERVER}.postgres.database.azure.com" \
      -U "psqladmin" \
      -d "$db" \
      -t -c "SELECT EXISTS (SELECT FROM information_schema.columns WHERE table_name='$table' AND column_name IN ('created_at', 'updated_at'))")

    if [ "$HAS_TIMESTAMP" = " t" ]; then
      echo "Exporting new records from $table..."

      PGPASSWORD="$NEW_PASSWORD" psql \
        -h "${NEW_SERVER}.postgres.database.azure.com" \
        -U "psqladmin" \
        -d "$db" \
        -c "COPY (SELECT * FROM $table WHERE created_at > '${CUTOVER_TIME}' OR updated_at > '${CUTOVER_TIME}') TO STDOUT CSV HEADER" \
        > "recovery_${db}_${table}.csv"
    fi
  done
done

# 2. Manual review and import
echo "Review CSV files and manually import critical data"
echo "Coordinate with data team for complex merges"
```

**Option B: Full Database Sync**

```bash
#!/bin/bash
# full-database-sync.sh

# Use logical replication to sync changes
# WARNING: Complex, requires DBA expertise

# 1. Set up logical replication from new to old
# 2. Let it sync for 1-2 hours
# 3. Cutover back to old database (now up-to-date)
# 4. Destroy new infrastructure

# This is advanced - consult DBA team
```

**Option C: Accept Data Loss**

- Document lost data range
- Communicate to affected users
- Provide customer support

### Rollback Execution

```bash
# Execute quick rollback procedure
./scripts/quick-rollback.sh ${ENVIRONMENT}

# Execute data recovery (if chosen)
./scripts/partial-data-recovery.sh ${ENVIRONMENT}

# Verify and monitor
./scripts/validate-deployment.sh ${ENVIRONMENT}
```

**Estimated Time**: 1-3 hours
**User Impact**: 15-30 minutes downtime

## Long-Term Rollback (24-48 Hours Post-Cutover)

**Scenario**: Issues detected 24-48 hours after cutover
**Risk Level**: High
**Data Loss**: 24-48 hours of data (significant)
**Recommendation**: **Fix Forward**

### Why Fix Forward is Preferred

At this point:
- 24-48 hours of new data in new databases
- Old infrastructure may be partially destroyed
- Data recovery extremely complex
- User impact of rollback > impact of fixing issues

### If Rollback is Absolutely Necessary

**Approval Required From**:
- CTO/Engineering VP
- Product VP
- Legal (data loss implications)
- Customer Success (user communication)

**Procedure**:
1. Full database export from new infrastructure
2. Complex data merge/reconciliation
3. Restore old infrastructure from backups
4. Import merged data
5. Extensive validation

**Estimated Time**: 6-12 hours
**User Impact**: 1-4 hours downtime
**Cost**: High (engineering hours, potential data loss)

**Recommendation**: Only in catastrophic scenarios

## Emergency Recovery (Old Infrastructure Destroyed)

**Scenario**: Critical issue after old infrastructure destroyed
**Risk Level**: Critical
**Data Loss**: Potentially severe if backups fail
**Downtime**: Hours to days

### Procedure

```bash
#!/bin/bash
# emergency-recovery.sh

# 1. Restore from latest backup
LATEST_BACKUP=$(az storage blob list \
  --account-name "mysprodbackupstorage" \
  --container-name "database-backups" \
  --query "sort_by([?properties.createdOn], &properties.createdOn)[-1].name" \
  -o tsv)

echo "Latest backup: $LATEST_BACKUP"

# 2. Download backup
az storage blob download \
  --account-name "mysprodbackupstorage" \
  --container-name "database-backups" \
  --name "$LATEST_BACKUP" \
  --file "./emergency-backup.tar.gz"

# 3. Extract and restore
tar -xzf emergency-backup.tar.gz

for db in chain_db publisher_db story_generator_db; do
  PGPASSWORD="$PASSWORD" pg_restore \
    -h "${SERVER}.postgres.database.azure.com" \
    -U "psqladmin" \
    -d "$db" \
    -v \
    "${db}.backup"
done

# 4. Verify data integrity
# 5. Restart applications
# 6. Extensive validation
```

**This is a disaster recovery scenario - follow your organization's DR plan**

## Rollback Communication Plan

### During Rollback

**Immediate** (within 5 minutes of decision):
- [ ] Post in #incidents Slack channel
- [ ] Update status page: "Investigating issue"
- [ ] Page on-call team

**After Rollback Initiated** (within 15 minutes):
- [ ] Email to stakeholders: "Rollback in progress"
- [ ] Update status page: "Service degradation - rollback initiated"
- [ ] Post update in #incidents every 15 minutes

**After Rollback Complete**:
- [ ] Email to stakeholders: "Rollback complete"
- [ ] Update status page: "Operational"
- [ ] Post-incident update in #engineering

### Post-Rollback

- [ ] Schedule post-mortem within 24 hours
- [ ] Document incident timeline
- [ ] Identify root cause
- [ ] Create action items
- [ ] Update deployment plan

## Rollback Validation Checklist

After any rollback, verify:

- [ ] All pods running in old AKS cluster
- [ ] All health endpoints responding
- [ ] Database connectivity working
- [ ] Redis connectivity working
- [ ] No errors in application logs
- [ ] Response times normal
- [ ] Error rates normal
- [ ] Users able to complete critical workflows
- [ ] Monitoring dashboards showing healthy state
- [ ] On-call team briefed on status

## Rollback Testing

**Recommended**: Test rollback procedure in dev/staging

```bash
# 1. Deploy to dev
./scripts/deploy-infrastructure.sh dev

# 2. Cutover
./scripts/deploy-applications.sh dev

# 3. Wait 15 minutes

# 4. Practice rollback
./scripts/quick-rollback.sh dev

# 5. Verify rollback successful
./scripts/validate-deployment.sh dev

# 6. Document lessons learned
```

## Post-Rollback Recovery

After a rollback:

1. **Immediate** (0-1 hour):
   - Verify old infrastructure stable
   - Monitor for any issues
   - Communicate status to stakeholders

2. **Short-term** (1-24 hours):
   - Conduct preliminary post-mortem
   - Identify root cause
   - Develop fix

3. **Medium-term** (1-7 days):
   - Fix identified issues
   - Update deployment plan
   - Re-validate in dev/staging
   - Schedule new deployment

4. **Long-term** (1-4 weeks):
   - Complete full post-mortem
   - Process improvements
   - Documentation updates
   - Knowledge sharing

## Lessons Learned Template

```markdown
# Rollback Post-Mortem - Azure v2.2 Migration

**Date**: ___________
**Environment**: ___________
**Rollback Type**: Quick / Evaluated / Emergency

## Timeline

| Time | Event |
|------|-------|
|      | Issue detected |
|      | Rollback decision |
|      | Rollback initiated |
|      | Rollback complete |
|      | Service restored |

## Root Cause

[Detailed analysis]

## Impact

- **Users Affected**: _____
- **Downtime**: _____ minutes
- **Data Loss**: Yes / No (details)
- **Financial Impact**: $ _____

## What Went Well

-
-

## What Went Wrong

-
-

## Action Items

| Action | Owner | Due Date | Priority |
|--------|-------|----------|----------|
|        |       |          |          |

## Process Improvements

-
-

**Incident Commander**: _________________
**Date**: _________________
```

## References

- [Deployment Strategy](./DEPLOYMENT_STRATEGY.md)
- [Data Migration Plan](./DATA_MIGRATION_PLAN.md)
- [Testing Checklist](./TESTING_CHECKLIST.md)
- [Incident Response Plan](./INCIDENT_RESPONSE.md) (if exists)

## Emergency Contacts

- **Incident Commander**: _________________
- **Engineering Lead**: _________________
- **DevOps Lead**: _________________
- **On-Call Engineer**: _________________
- **Database Administrator**: _________________
- **Product Owner**: _________________

---

**Remember**: The goal is to minimize user impact. Sometimes fixing forward is better than rolling back.
