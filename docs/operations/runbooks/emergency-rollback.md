# Runbook: Emergency Rollback

**Last Updated**: 2025-12-22
**Owner**: Platform Engineering Team
**Approval Required**: On-call Lead (verbal approval acceptable during incident)
**Estimated Time**: 5-15 minutes

## Purpose

This runbook provides step-by-step procedures for performing an emergency rollback of production services when critical issues are detected. The rollback uses Azure App Service slot swapping to restore the previous known-good deployment.

## When to Use This Runbook

Use this runbook when ANY of the following conditions are met:

| Condition | Threshold | Action |
|-----------|-----------|--------|
| HTTP 5xx error rate | > 10% for 5+ minutes | Immediate rollback |
| Complete service outage | Any duration | Immediate rollback |
| Data corruption detected | Any severity | Immediate rollback |
| Security vulnerability exploited | Any severity | Immediate rollback |
| P95 latency | > 3x normal for 10+ minutes | Evaluate for rollback |

## Prerequisites

- [ ] Azure CLI installed and authenticated
- [ ] GitHub CLI (`gh`) installed and authenticated
- [ ] Access to Azure subscription (Contributor role or higher)
- [ ] Access to GitHub repository (write access)
- [ ] On-call lead approval (can be verbal during incident)

## Decision Flow

```
┌────────────────────────────────────────┐
│        Issue Detected                   │
└────────────────┬───────────────────────┘
                 │
                 ▼
┌────────────────────────────────────────┐
│  Is error rate > 10% or service down?  │
└────────────────┬───────────────────────┘
                 │
        ┌────────┴────────┐
        │ YES             │ NO
        ▼                 ▼
┌───────────────┐  ┌─────────────────────┐
│  ROLLBACK     │  │  Investigate first  │
│  IMMEDIATELY  │  │  (15 min max)       │
└───────┬───────┘  └──────────┬──────────┘
        │                     │
        │              Still broken?
        │                     │
        │          ┌──────────┴──────────┐
        │          │ YES                 │ NO
        │          ▼                     ▼
        │   ┌───────────────┐    ┌───────────────┐
        │   │  ROLLBACK     │    │  Fix forward  │
        │   └───────────────┘    └───────────────┘
        │
        ▼
┌────────────────────────────────────────┐
│         Follow Rollback Steps          │
└────────────────────────────────────────┘
```

---

## Procedure

### Step 1: Confirm the Issue (2 minutes)

Quickly verify the issue before initiating rollback.

```bash
# Check current production health
curl -s -o /dev/null -w "HTTP Status: %{http_code}\nTime: %{time_total}s\n" \
  https://mystira.app/api/health

# Check error rate in last 5 minutes (requires Azure CLI)
az monitor metrics list \
  --resource "/subscriptions/<sub>/resourceGroups/mys-prod-core-rg-san/providers/Microsoft.Web/sites/mys-prod-app-api-san" \
  --metric "Http5xx" \
  --interval PT1M \
  --start-time $(date -u -d '5 minutes ago' +%Y-%m-%dT%H:%M:%SZ) \
  --query "value[0].timeseries[0].data[].total" \
  -o tsv
```

**Expected Output (Healthy):**
```
HTTP Status: 200
Time: 0.150s
```

**If Unhealthy:** Proceed to Step 2.

### Step 2: Notify On-Call Lead (1 minute)

```
@oncall-lead Production issue detected. Initiating emergency rollback.

Error rate: [X]%
Duration: [X] minutes
Affected endpoint: [endpoint]

Proceeding with rollback unless you advise otherwise.
```

### Step 3: Execute Rollback via GitHub Workflow (Recommended)

**Option A: Using GitHub CLI (Preferred)**

```bash
# Trigger the rollback workflow
gh workflow run mystira-app-api-rollback.yml \
  --repo phoenixvc/Mystira.workspace \
  -f confirm="ROLLBACK PRODUCTION" \
  -f reason="[Brief description of the issue]"

# Monitor the workflow
gh run watch --repo phoenixvc/Mystira.workspace
```

**Option B: Using GitHub Web UI**

1. Go to: https://github.com/phoenixvc/Mystira.workspace/actions/workflows/mystira-app-api-rollback.yml
2. Click "Run workflow"
3. Enter `ROLLBACK PRODUCTION` in the confirmation field
4. Enter reason for rollback
5. Click "Run workflow"

### Step 4: Execute Rollback via Azure CLI (Alternative)

If GitHub workflow is unavailable, use Azure CLI directly:

```bash
# ⚠️ WARNING: This bypasses workflow logging and verification
# Only use if GitHub is inaccessible

# Login if needed
az login

# Execute slot swap (rollback)
az webapp deployment slot swap \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-app-api-san \
  --slot staging \
  --target-slot production

# Verify the swap completed
az webapp show \
  --resource-group mys-prod-core-rg-san \
  --name mys-prod-app-api-san \
  --query "state" \
  -o tsv
```

**Expected Output:**
```
Running
```

### Step 5: Verify Rollback Success (2-3 minutes)

```bash
# Wait for swap propagation
sleep 30

# Run multiple health checks
for i in {1..5}; do
  echo "Check $i:"
  curl -s -o /dev/null -w "Status: %{http_code}, Time: %{time_total}s\n" \
    https://mystira.app/api/health
  sleep 5
done
```

**Expected Output (Successful Rollback):**
```
Check 1: Status: 200, Time: 0.145s
Check 2: Status: 200, Time: 0.132s
Check 3: Status: 200, Time: 0.128s
Check 4: Status: 200, Time: 0.135s
Check 5: Status: 200, Time: 0.141s
```

### Step 6: Post-Rollback Verification

```bash
# Check key endpoints
echo "=== Checking Critical Endpoints ==="

echo "1. Health endpoint:"
curl -s https://mystira.app/api/health | jq .

echo "2. Version endpoint:"
curl -s https://mystira.app/api/version | jq .

echo "3. Sample API call:"
curl -s -o /dev/null -w "Status: %{http_code}\n" \
  https://mystira.app/api/scenarios
```

---

## Verification Checklist

After rollback, verify these items:

- [ ] Health endpoint returns 200
- [ ] Error rate has dropped below 1%
- [ ] Response times are within normal range
- [ ] No new errors in Application Insights
- [ ] User-facing functionality confirmed working

## If Rollback Fails

If the rollback doesn't resolve the issue:

1. **Check staging slot** - The problematic code is now in staging:
   ```bash
   curl -s https://mys-prod-app-api-san-staging.azurewebsites.net/health
   ```

2. **Review staging logs**:
   ```bash
   az webapp log tail \
     --resource-group mys-prod-core-rg-san \
     --name mys-prod-app-api-san \
     --slot staging
   ```

3. **If both slots are broken**, escalate immediately to Engineering Manager and consider:
   - Deploying a known-good image from container registry
   - Database restore if data corruption suspected
   - Initiating disaster recovery procedure

## Post-Incident Actions

- [ ] Create incident ticket in tracking system
- [ ] Document timeline of events
- [ ] Preserve logs from staging slot (contains broken deployment)
- [ ] Notify stakeholders of incident and resolution
- [ ] Schedule post-mortem within 48 hours
- [ ] Update this runbook if gaps were identified

## Post-Mortem Template

```markdown
## Incident Post-Mortem: [Date]

### Summary
[One paragraph summary]

### Timeline
| Time (UTC) | Event |
|------------|-------|
| HH:MM | Issue first detected |
| HH:MM | Rollback initiated |
| HH:MM | Rollback completed |
| HH:MM | Service fully restored |

### Impact
- Duration: X minutes
- Users affected: ~N
- Error budget consumed: X minutes

### Root Cause
[Description of root cause]

### Action Items
- [ ] Action 1 - Owner - Due date
- [ ] Action 2 - Owner - Due date

### Lessons Learned
- What went well?
- What could be improved?
```

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────────────┐
│                    EMERGENCY ROLLBACK                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. CONFIRM: curl https://mystira.app/api/health                │
│                                                                  │
│  2. NOTIFY: @oncall-lead in Slack                               │
│                                                                  │
│  3. ROLLBACK:                                                   │
│     gh workflow run mystira-app-api-rollback.yml \              │
│       -f confirm="ROLLBACK PRODUCTION" \                        │
│       -f reason="[reason]"                                      │
│                                                                  │
│  4. VERIFY: curl https://mystira.app/api/health (5 times)       │
│                                                                  │
│  5. DOCUMENT: Create incident ticket                            │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Related Runbooks

- [Disaster Recovery](./disaster-recovery.md)
- [Database Failover](./database-failover.md)
- [Deployment Strategy](../DEPLOYMENT_STRATEGY.md)

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-22 | Platform Team | Initial runbook |
