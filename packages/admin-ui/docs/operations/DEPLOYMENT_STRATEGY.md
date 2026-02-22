# Deployment Strategy - Mystira Admin UI

**Status**: Draft
**Version**: 1.0
**Last Updated**: 2024-12-22
**Author**: Development Team

## Overview

This document outlines the deployment strategy for the Mystira Admin UI application, covering CI/CD pipelines, environment configurations, and deployment procedures.

## Cross-Repository Deployment Matrix

The Mystira platform uses a distributed deployment strategy across repositories:

| Repository | Dev Deploy | Staging/Prod Deploy | Notes |
|------------|------------|---------------------|-------|
| **Mystira.Admin.UI** | ✅ This repo | Via workspace | React frontend for admin portal |
| **Mystira.Admin.Api** | Via workspace | Via workspace | .NET 9 API, CI in own repo |
| **Mystira.App** | Own repo | Own repo | Full CI/CD for API + PWA |
| **mystira.workspace** | N/A | ✅ Central | Infrastructure & coordinated deploys |

### Deployment Responsibility

```
┌─────────────────────────────────────────────────────────────────────┐
│                        DEVELOPMENT                                  │
├─────────────────────────────────────────────────────────────────────┤
│  Admin.UI      ──► deploy-dev.yml ──► Build artifact ──► k8s (via  │
│                                        workspace)                   │
│  Mystira.App   ──► app-ci.yml     ──► Own deployment pipeline      │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                   STAGING / PRODUCTION                              │
├─────────────────────────────────────────────────────────────────────┤
│  mystira.workspace (Central Orchestrator)                           │
│  ├── staging-deploy.yml  ──► Coordinates all staging deploys       │
│  ├── prod-deploy.yml     ──► Coordinates all production deploys    │
│  ├── chain-*.yml         ──► Blockchain infrastructure             │
│  └── publisher-*.yml     ──► Content publishing services           │
└─────────────────────────────────────────────────────────────────────┘
```

### Why This Strategy?

1. **Fast dev iterations**: Dev deploys directly from feature repos for quick feedback
2. **Coordinated releases**: Staging/prod via workspace ensures version compatibility
3. **Single source of truth**: Workspace manages environment configs centrally
4. **Reduced complexity**: Each repo only needs CI + dev deploy workflows

## Deployment Architecture

### Target Deployment

The Admin UI is a static SPA that can be deployed to:

1. **Kubernetes (AKS)** - Dev/Staging/Prod via mystira.workspace
2. **Azure Blob Storage + CDN**
3. **Any static file hosting**

### Architecture Diagram

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   User Browser  │────▶│  AKS Ingress     │────▶│  Static Files   │
└─────────────────┘     └──────────────────┘     │  (nginx/dist)   │
                                                  └─────────────────┘
                                                          │
                                                          │ API Calls
                                                          ▼
                                                  ┌─────────────────┐
                                                  │  Admin API      │
                                                  │  (Mystira.      │
                                                  │   Admin.Api)    │
                                                  └─────────────────┘
```

## Environments

### Development

- **URL**: http://localhost:5173
- **API**: Local or dev API
- **Purpose**: Local development

### Staging

- **URL**: https://staging-admin.mystira.app
- **API**: https://staging-api.admin.mystira.app
- **Purpose**: Pre-production testing
- **Deploy Trigger**: Merge to `dev` branch

### Production

- **URL**: https://admin.mystira.app
- **API**: https://api.admin.mystira.app
- **Purpose**: Live user environment
- **Deploy Trigger**: Release tag or merge to `main`

## CI/CD Pipeline

### GitHub Actions Workflow

Create `.github/workflows/deploy.yml`:

```yaml
name: Build and Deploy

on:
  push:
    branches: [dev, main]
  pull_request:
    branches: [dev, main]

env:
  NODE_VERSION: '20'

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'

      - name: Install dependencies
        run: npm ci

      - name: Run linting
        run: npm run lint

      - name: Run type check
        run: npx tsc --noEmit

      - name: Run tests
        run: npm run test -- --run

      - name: Build application
        run: npm run build
        env:
          VITE_API_BASE_URL: ${{ vars.API_BASE_URL }}

      - name: Upload build artifact
        uses: actions/upload-artifact@v4
        with:
          name: dist
          path: dist

  deploy-staging:
    needs: build
    if: github.ref == 'refs/heads/dev'
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - name: Download build artifact
        uses: actions/download-artifact@v4
        with:
          name: dist
          path: dist

      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_SWA_TOKEN }}
          action: 'upload'
          app_location: 'dist'
          skip_app_build: true

  deploy-production:
    needs: build
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production
    steps:
      - name: Download build artifact
        uses: actions/download-artifact@v4
        with:
          name: dist
          path: dist

      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_SWA_TOKEN_PROD }}
          action: 'upload'
          app_location: 'dist'
          skip_app_build: true
```

### Pipeline Stages

```
┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────────┐
│ Install │───▶│  Lint   │───▶│  Test   │───▶│    Build    │
└─────────┘    └─────────┘    └─────────┘    └─────────────┘
                                                     │
                                    ┌────────────────┴────────────────┐
                                    ▼                                 ▼
                            ┌──────────────┐                 ┌──────────────┐
                            │Deploy Staging│                 │Deploy Prod   │
                            │  (dev branch)│                 │ (main branch)│
                            └──────────────┘                 └──────────────┘
```

## Deployment Procedures

### Manual Deployment (Dev)

```bash
# 1. Ensure clean working directory
git status

# 2. Pull latest changes
git pull origin dev

# 3. Install dependencies
npm ci

# 4. Run tests
npm run test -- --run

# 5. Build for production
npm run build

# 6. Preview build locally
npm run preview
```

### Staging Deployment

**Trigger**: Push or merge to `dev` branch

**Automatic Steps**:
1. CI builds and tests
2. Artifact uploaded
3. Deployed to staging environment

**Manual Verification**:
1. Check deployment completed in GitHub Actions
2. Verify staging URL accessible
3. Run smoke tests
4. Check browser console for errors

### Production Deployment

**Trigger**: Push or merge to `main` branch

**Pre-Deployment Checklist**:
- [ ] All staging tests passed
- [ ] QA sign-off obtained
- [ ] Rollback plan ready
- [ ] Stakeholders notified

**Deployment Steps**:
1. Create PR from `dev` to `main`
2. Review and approve PR
3. Merge PR (triggers deployment)
4. Monitor deployment in GitHub Actions
5. Verify production URL
6. Run production smoke tests

**Post-Deployment**:
- [ ] Verify all pages load
- [ ] Verify API connectivity
- [ ] Check error monitoring
- [ ] Notify stakeholders of completion

## Environment Variables

### Required Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `VITE_API_BASE_URL` | Admin API URL | `https://api.admin.mystira.app` |

### GitHub Secrets

| Secret | Environment | Description |
|--------|-------------|-------------|
| `AZURE_SWA_TOKEN` | Staging | Azure SWA deployment token |
| `AZURE_SWA_TOKEN_PROD` | Production | Azure SWA deployment token |

## Rollback Procedure

See [ROLLBACK_PROCEDURE.md](./ROLLBACK_PROCEDURE.md) for detailed rollback steps.

**Quick Rollback** (via GitHub Actions):
1. Go to Actions tab
2. Find last successful production deployment
3. Click "Re-run all jobs"

**Or via Azure Portal**:
1. Open Azure Static Web Apps
2. Navigate to Deployment History
3. Select previous deployment
4. Click "Activate"

## Monitoring

### Health Checks

- **Uptime Monitoring**: Configure uptime monitoring for production URL
- **Error Tracking**: Integrate error tracking (e.g., Sentry)
- **Performance**: Monitor Core Web Vitals

### Alerts

Set up alerts for:
- Deployment failures
- High error rates
- Availability drops

## Deployment Timeline

| Environment | Deployment Window | Frequency |
|-------------|-------------------|-----------|
| Staging | Anytime | On every `dev` push |
| Production | Business hours | On `main` merge |

## Contacts

- **DevOps Lead**: _________________
- **On-Call Engineer**: _________________
- **Product Owner**: _________________

## References

- [Testing Checklist](./TESTING_CHECKLIST.md)
- [Rollback Procedure](./ROLLBACK_PROCEDURE.md)
- [Azure Kubernetes Service Docs](https://learn.microsoft.com/azure/aks/)
- [mystira.workspace Repository](https://github.com/phoenixvc/Mystira.workspace)
