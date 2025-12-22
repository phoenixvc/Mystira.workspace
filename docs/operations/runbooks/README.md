# Operations Runbooks Index

**Last Updated**: 2025-12-22
**Owner**: Platform Engineering Team
**On-Call Escalation**: See [Escalation Matrix](#escalation-matrix)

## Overview

This directory contains operational runbooks for the Mystira platform. Each runbook provides step-by-step procedures for handling specific operational scenarios.

## Quick Reference

### Emergency Procedures

| Scenario | Runbook | Estimated Time | Approval Required |
|----------|---------|----------------|-------------------|
| Production rollback | [emergency-rollback.md](./emergency-rollback.md) | 5-15 minutes | On-call lead |
| Disaster recovery | [disaster-recovery.md](./disaster-recovery.md) | 30-120 minutes | Engineering Manager |
| Database failover | [database-failover.md](./database-failover.md) | 15-30 minutes | DBA + On-call lead |
| Security incident | [security-incident.md](./security-incident.md) | Varies | Security Team |

### Routine Operations

| Scenario | Runbook | Estimated Time | Approval Required |
|----------|---------|----------------|-------------------|
| Planned maintenance | [planned-maintenance.md](./planned-maintenance.md) | Varies | Change Advisory Board |
| Certificate renewal | [certificate-renewal.md](./certificate-renewal.md) | 15-30 minutes | None |
| Scaling operations | [scaling-operations.md](./scaling-operations.md) | 10-20 minutes | On-call lead |
| Log rotation | [log-management.md](./log-management.md) | 5-10 minutes | None |

### Infrastructure Maintenance

| Scenario | Runbook | Estimated Time | Approval Required |
|----------|---------|----------------|-------------------|
| Terraform state cleanup | [terraform-state-cleanup.md](./terraform-state-cleanup.md) | 30-60 minutes | Engineering Lead |
| Fix workflow naming | [workflow-naming-fix.md](./workflow-naming-fix.md) | 5 min/submodule | None |

### Troubleshooting

| Scenario | Runbook | Priority |
|----------|---------|----------|
| High latency investigation | [troubleshooting-latency.md](./troubleshooting-latency.md) | P1/P2 |
| Error rate spike | [troubleshooting-errors.md](./troubleshooting-errors.md) | P1/P2 |
| Memory issues | [troubleshooting-memory.md](./troubleshooting-memory.md) | P2/P3 |
| Connection issues | [troubleshooting-connections.md](./troubleshooting-connections.md) | P1/P2 |

---

## Runbook Categories

### 1. Emergency Runbooks

Critical procedures for immediate incident response. These runbooks are designed for speed and should be executable within minutes.

- **[Emergency Rollback](./emergency-rollback.md)**: Quickly revert production to a known-good state
- **[Disaster Recovery](./disaster-recovery.md)**: Full recovery procedures for catastrophic failures
- **Database Failover**: Switch to replica database in case of primary failure
- **Security Incident Response**: Initial containment and escalation procedures

### 2. Deployment Runbooks

Procedures for deploying and managing application releases.

- **Blue-Green Deployment**: Standard production deployment procedure
- **Canary Release**: Gradual rollout with monitoring
- **Hotfix Deployment**: Emergency fix deployment process
- **Configuration Updates**: Updating environment variables and secrets

### 3. Infrastructure Runbooks

Procedures for managing infrastructure components.

- **AKS Node Maintenance**: Draining and updating cluster nodes
- **Database Maintenance**: Backup, restore, and optimization procedures
- **Storage Management**: Blob storage operations and cleanup
- **Network Troubleshooting**: DNS, firewall, and connectivity issues

### 4. Monitoring Runbooks

Procedures for investigating and resolving monitoring alerts.

- **High Error Rate Investigation**: Steps to diagnose elevated error rates
- **Latency Investigation**: Diagnosing performance degradation
- **Resource Exhaustion**: Handling CPU, memory, or disk pressure
- **Alert Tuning**: Adjusting alert thresholds

---

## Escalation Matrix

### Priority Definitions

| Priority | Response Time | Example Scenarios |
|----------|---------------|-------------------|
| **P0** | 15 minutes | Complete service outage, data loss |
| **P1** | 30 minutes | Partial outage, major feature broken |
| **P2** | 2 hours | Performance degradation, non-critical feature broken |
| **P3** | 8 hours | Minor issues, cosmetic bugs |

### Escalation Path

```
┌─────────────────────────────────────────────────────────────────┐
│                         P0 Escalation                            │
├─────────────────────────────────────────────────────────────────┤
│  0-15 min    │  On-call Engineer                                │
│  15-30 min   │  On-call Lead + Team Lead                        │
│  30-60 min   │  Engineering Manager                             │
│  60+ min     │  VP Engineering + Exec Team                      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                         P1 Escalation                            │
├─────────────────────────────────────────────────────────────────┤
│  0-30 min    │  On-call Engineer                                │
│  30-60 min   │  On-call Lead                                    │
│  60-120 min  │  Team Lead                                       │
│  120+ min    │  Engineering Manager                             │
└─────────────────────────────────────────────────────────────────┘
```

### Contact Information

| Role | Primary | Backup | Method |
|------|---------|--------|--------|
| On-call Engineer | PagerDuty rotation | - | PagerDuty |
| On-call Lead | PagerDuty rotation | - | PagerDuty |
| Engineering Manager | @eng-manager | @eng-director | Slack + Phone |
| DBA | @dba-team | @platform-team | Slack |
| Security | @security-team | security@mystira.app | Slack + Email |

---

## Runbook Standards

### Required Sections

Every runbook must include:

1. **Purpose**: What the runbook is for
2. **Prerequisites**: Required access, tools, or approvals
3. **Procedure**: Step-by-step instructions
4. **Verification**: How to confirm success
5. **Rollback**: How to undo changes if needed
6. **Post-Incident**: Follow-up actions

### Formatting Guidelines

- Use numbered steps for sequential procedures
- Use code blocks for commands
- Include expected output examples
- Mark dangerous commands with ⚠️ WARNING
- Include time estimates for each section

### Template

```markdown
# Runbook: [Title]

**Last Updated**: YYYY-MM-DD
**Owner**: [Team/Individual]
**Approval Required**: [Yes/No - Who]
**Estimated Time**: [X minutes/hours]

## Purpose

[Brief description of what this runbook accomplishes]

## Prerequisites

- [ ] Required access/permissions
- [ ] Required tools installed
- [ ] Required approvals obtained

## Procedure

### Step 1: [Step Title]

[Description]

\`\`\`bash
# Command to execute
\`\`\`

**Expected Output:**
\`\`\`
[Example output]
\`\`\`

### Step 2: [Step Title]
...

## Verification

- [ ] Verification step 1
- [ ] Verification step 2

## Rollback

[Steps to undo changes if needed]

## Post-Incident

- [ ] Update incident ticket
- [ ] Notify stakeholders
- [ ] Schedule post-mortem if applicable
```

---

## Quick Commands Reference

### Azure CLI

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "Mystira Production"

# Get AKS credentials
az aks get-credentials --resource-group mys-prod-core-rg-san --name mys-prod-core-aks-san
```

### Kubernetes

```bash
# View pods
kubectl get pods -n mys-prod

# View logs
kubectl logs -n mys-prod deployment/mys-app-api -f --tail=100

# Restart deployment
kubectl rollout restart deployment/mys-app-api -n mys-prod

# Check deployment status
kubectl rollout status deployment/mys-app-api -n mys-prod
```

### GitHub Workflows

```bash
# Trigger rollback workflow
gh workflow run mystira-app-api-rollback.yml \
  -f confirm="ROLLBACK PRODUCTION" \
  -f reason="[Your reason here]"

# View workflow runs
gh run list --workflow=mystira-app-api-cicd-prod.yml
```

---

## Related Documentation

- [SLO Definitions](../slo-definitions.md)
- [Deployment Strategy](../DEPLOYMENT_STRATEGY.md)
- [Rollback Procedure](../ROLLBACK_PROCEDURE.md)
- [Testing Checklist](../TESTING_CHECKLIST.md)
- [Data Migration Plan](../DATA_MIGRATION_PLAN.md)

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-22 | Platform Team | Initial runbook index |
