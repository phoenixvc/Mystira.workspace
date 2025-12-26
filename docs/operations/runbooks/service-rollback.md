# Service Rollback Procedures

**Last Updated**: December 2025

## Quick Reference

| Service | Rollback Method | Recovery Time |
|---------|----------------|---------------|
| Mystira.App | Azure App Service slot swap | < 1 min |
| Mystira.Admin.Api | Azure App Service slot swap | < 1 min |
| Mystira.StoryGenerator | Container rollback | < 2 min |
| Mystira.Publisher | Container rollback | < 2 min |
| Mystira.Chain | Container rollback | < 2 min |
| Mystira.Admin.UI | SWA revert | < 1 min |

---

## General Rollback Process

### 1. Identify the Issue

```bash
# Check recent deployments
az webapp deployment list --name <app-name> --resource-group <rg>

# Check logs
az webapp log tail --name <app-name> --resource-group <rg>
```

### 2. Decide: Rollback vs Fix Forward

| Situation | Action |
|-----------|--------|
| Critical outage | Rollback immediately |
| Data corruption | Rollback + investigate |
| Minor bug | Fix forward if < 30 min |
| Performance degradation | Rollback if > 20% impact |

---

## Azure App Service (Mystira.App, Admin.Api)

### Slot Swap Rollback

```bash
# Swap staging back to production
az webapp deployment slot swap \
  --name mystira-app \
  --resource-group rg-mystira-prod \
  --slot staging \
  --target-slot production

# Verify health
curl https://api.mystira.app/health
```

### Container Image Rollback

```bash
# List available images
az acr repository show-tags --name mystiraacr --repository mystira-app

# Set specific version
az webapp config container set \
  --name mystira-app \
  --resource-group rg-mystira-prod \
  --docker-custom-image-name mystiraacr.azurecr.io/mystira-app:v1.2.3
```

---

## Kubernetes Services (StoryGenerator, Publisher, Chain)

### Quick Rollback

```bash
# Rollback to previous revision
kubectl rollout undo deployment/mystira-storygenerator -n mystira

# Rollback to specific revision
kubectl rollout undo deployment/mystira-storygenerator -n mystira --to-revision=3

# Check status
kubectl rollout status deployment/mystira-storygenerator -n mystira
```

### Helm Rollback

```bash
# List releases
helm history mystira-storygenerator -n mystira

# Rollback to previous
helm rollback mystira-storygenerator -n mystira

# Rollback to specific revision
helm rollback mystira-storygenerator 3 -n mystira
```

---

## Static Web App (Admin.UI)

### SWA Rollback

```bash
# List deployments
az staticwebapp environment list --name mystira-admin-ui

# Delete current and redeploy previous
az staticwebapp environment delete --name mystira-admin-ui --environment-name default

# Or: Redeploy from Git
git revert HEAD
git push origin main
```

---

## Database Rollback

### Cosmos DB

**Point-in-time restore** (if continuous backup enabled):

```bash
az cosmosdb sql container restore \
  --account-name mystira-cosmos \
  --database-name mystira \
  --name scenarios \
  --restore-timestamp "2025-12-26T10:00:00Z"
```

### PostgreSQL

**Point-in-time restore**:

```bash
az postgres flexible-server restore \
  --resource-group rg-mystira-prod \
  --name mystira-postgres-restored \
  --source-server mystira-postgres \
  --restore-time "2025-12-26T10:00:00Z"
```

---

## Rollback Checklist

### Before Rollback

- [ ] Confirm the issue requires rollback
- [ ] Notify team (`jurie@phoenixvc.tech`)
- [ ] Document current state
- [ ] Check for data migrations that need reverting

### During Rollback

- [ ] Execute rollback command
- [ ] Monitor health endpoints
- [ ] Check logs for errors
- [ ] Verify functionality

### After Rollback

- [ ] Confirm service is healthy
- [ ] Update status page (if applicable)
- [ ] Create incident report
- [ ] Schedule post-mortem

---

## Emergency Contacts

| Role | Contact |
|------|---------|
| Technical Lead | `jurie@phoenixvc.tech` |
| Business/Escalation | `eben@phoenixvc.tech` |
