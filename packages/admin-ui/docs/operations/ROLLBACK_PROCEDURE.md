# Rollback Procedure - Mystira Admin UI

**Status**: Draft
**Version**: 1.0
**Last Updated**: 2024-12-22
**Author**: Development Team

## Overview

This document provides rollback procedures for the Mystira Admin UI application. Since the Admin UI is a static SPA, rollback is straightforward and low-risk.

## Rollback Decision Matrix

| Issue Type | Severity | Action |
|------------|----------|--------|
| UI bug (minor) | Low | Fix forward |
| UI bug (major) | Medium | Evaluate rollback |
| API integration broken | High | Rollback immediately |
| Security vulnerability | Critical | Rollback immediately |
| Complete app failure | Critical | Rollback immediately |

## Rollback Criteria

### Rollback Immediately

- [ ] Application fails to load
- [ ] Authentication completely broken
- [ ] Major security vulnerability discovered
- [ ] Data corruption risk
- [ ] API integration completely broken

### Evaluate Rollback

- [ ] Significant functionality broken (some features work)
- [ ] Performance severely degraded
- [ ] High error rates in monitoring

### Fix Forward (No Rollback)

- [ ] Minor UI glitches
- [ ] Non-critical feature broken
- [ ] Styling issues
- [ ] Low-impact bugs

## Rollback Methods

### Method 1: GitHub Actions Re-Deploy (Recommended)

**Time to Rollback**: 3-5 minutes

1. Go to GitHub Repository
2. Navigate to **Actions** tab
3. Filter to successful production deployments
4. Find the deployment before the problematic one
5. Click **Re-run all jobs**

```
GitHub → Actions → [Previous Successful Workflow] → Re-run all jobs
```

### Method 2: Azure Static Web Apps Portal

**Time to Rollback**: 2-3 minutes

1. Go to Azure Portal
2. Navigate to your Static Web App resource
3. Click **Deployment history** in left menu
4. Find the previous working deployment
5. Click the three dots (...) menu
6. Select **Activate**

### Method 3: Revert Git Commit

**Time to Rollback**: 5-10 minutes

```bash
# 1. Identify the problematic commit
git log --oneline -10

# 2. Revert the commit
git revert <commit-hash>

# 3. Push the revert
git push origin main
```

This triggers a new deployment with the reverted code.

### Method 4: Force Deploy Previous Version

**Time to Rollback**: 10-15 minutes

```bash
# 1. Checkout the last known good commit
git checkout <good-commit-hash>

# 2. Create a rollback branch
git checkout -b rollback/emergency

# 3. Build the application
npm run build

# 4. Deploy manually using Azure CLI
az staticwebapp deploy \
  --app-name <your-swa-name> \
  --environment production \
  --app-artifact-location ./dist
```

## Rollback Procedure

### Step 1: Identify the Issue

```markdown
- [ ] Issue identified at: ___:___ (time)
- [ ] Issue description: _________________
- [ ] Severity: Critical / High / Medium
- [ ] Users impacted: All / Some / Few
```

### Step 2: Decide on Rollback

```markdown
- [ ] Rollback decision made by: _______________
- [ ] Decision time: ___:___
- [ ] Rollback method chosen: GitHub Actions / Azure Portal / Git Revert
```

### Step 3: Execute Rollback

**For GitHub Actions method**:
1. Open GitHub Actions
2. Select previous successful workflow
3. Click "Re-run all jobs"
4. Wait for deployment (2-3 minutes)

### Step 4: Verify Rollback

- [ ] Application loads correctly
- [ ] Login works
- [ ] Dashboard loads
- [ ] Navigation works
- [ ] API calls succeed
- [ ] No console errors

### Step 5: Notify Stakeholders

```markdown
Rollback Notification

Subject: [Mystira Admin UI] Production Rollback Completed

Details:
- Issue: _______________
- Rollback time: ___:___
- Previous version restored
- Current status: Stable

Next steps:
- Root cause analysis
- Fix implementation
- Re-deployment plan
```

## Post-Rollback Actions

### Immediate (0-1 hour)

- [ ] Verify application stable
- [ ] Check error monitoring
- [ ] Confirm user reports resolved
- [ ] Document incident timeline

### Short-term (1-24 hours)

- [ ] Root cause analysis
- [ ] Create fix in development
- [ ] Test fix thoroughly
- [ ] Plan re-deployment

### Medium-term (1-7 days)

- [ ] Implement preventive measures
- [ ] Update testing checklist
- [ ] Post-mortem review
- [ ] Documentation updates

## Special Considerations

### API Changes

If the API was also updated and the UI rollback creates incompatibility:

1. Coordinate with Admin API team
2. May need to rollback API as well
3. Or deploy UI version compatible with current API

### Cache Invalidation

After rollback, users may need to clear browser cache:

- Instruct users to hard refresh (Ctrl+Shift+R)
- Or add cache-busting via deployment

### Feature Flags

If using feature flags, consider:
- Disabling problematic features via flags
- Instead of full rollback

## Rollback Verification Checklist

After any rollback, verify:

- [ ] Application loads (no white screen)
- [ ] Login page accessible
- [ ] Can log in with valid credentials
- [ ] Dashboard data loads
- [ ] Navigation works (all menu items)
- [ ] At least one CRUD operation works
- [ ] No JavaScript errors in console
- [ ] Network requests succeed (no CORS errors)
- [ ] Performance acceptable

## Lessons Learned Template

```markdown
# Rollback Post-Mortem - Mystira Admin UI

**Date**: ___________
**Rollback Method Used**: ___________
**Time to Rollback**: ___ minutes

## Timeline

| Time | Event |
|------|-------|
| __:__ | Issue detected |
| __:__ | Rollback decision made |
| __:__ | Rollback initiated |
| __:__ | Rollback completed |
| __:__ | Verification complete |

## Root Cause

[Description of what caused the issue]

## Impact

- **Users Affected**: ___
- **Duration**: ___ minutes
- **Features Affected**: ___

## What Went Well

-
-

## What Could Be Improved

-
-

## Action Items

| Action | Owner | Due Date |
|--------|-------|----------|
|        |       |          |

## Prevention

How to prevent this in the future:
-
-

**Completed By**: _______________
**Date**: _______________
```

## Emergency Contacts

- **On-Call Engineer**: _______________
- **DevOps Lead**: _______________
- **Product Owner**: _______________
- **API Team Lead**: _______________

## References

- [Deployment Strategy](./DEPLOYMENT_STRATEGY.md)
- [Testing Checklist](./TESTING_CHECKLIST.md)
- [Azure SWA Rollback Docs](https://learn.microsoft.com/azure/static-web-apps/)
