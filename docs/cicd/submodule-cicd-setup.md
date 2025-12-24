# Submodule CI/CD Setup Guide

**For**: Mystira submodule repositories (Admin.Api, Admin.UI, Publisher, Chain, StoryGenerator)
**Last Updated**: 2024-12-24

This guide provides complete instructions for setting up CI/CD in Mystira submodule repositories to enable automated dev deployments via the workspace.

---

## Overview

```
┌─────────────────────────┐     ┌─────────────────────────────┐
│   Submodule Repository  │     │      Mystira.workspace      │
├─────────────────────────┤     ├─────────────────────────────┤
│ 1. PR created           │     │                             │
│    → CI runs (validate) │     │                             │
│         ↓               │     │                             │
│ 2. PR merged to dev     │     │                             │
│    → push event fires   │     │                             │
│         ↓               │     │                             │
│ 3. Build Docker image   │     │                             │
│ 4. Push to ACR          │────►│ 5. repository_dispatch      │
│         ↓               │     │    event received           │
│ 5. Trigger dispatch     │     │         ↓                   │
│                         │     │ 6. Update K8s deployment    │
│                         │     │ 7. Rollout to dev AKS       │
└─────────────────────────┘     └─────────────────────────────┘
```

**Important**: This deploys to **dev environment only**. Staging and production deployments are managed through the workspace release workflows.

---

## CI/CD Trigger Behavior

| Event | CI (lint/test/build) | Deploy to Dev |
|-------|---------------------|---------------|
| PR opened/updated | ✅ | ❌ |
| Draft PR | ❌ skip | ❌ |
| Push/merge to dev | ✅ | ✅ |
| Push to main | ✅ | ❌ |
| Manual dispatch | ✅ | ✅ |

### Design Rationale

| Decision | Why |
|----------|-----|
| Skip draft PRs | Save CI minutes; drafts are WIP |
| CI on PRs | Validate before merge; catch issues early |
| Deploy only on push to dev | Single dev environment; avoid conflicts |
| No deploy on PR open | Unreviewed code shouldn't run in dev |
| No `pull_request.closed` trigger | Avoid double-trigger (push already covers merges) |

---

## Required Secrets

Add these secrets to your submodule repository:

| Secret | Value | Purpose |
|--------|-------|---------|
| `WORKSPACE_DISPATCH_TOKEN` | GitHub PAT with `repo` scope | Trigger workspace deployments |
| `AZURE_CLIENT_ID` | From Azure service principal | Azure authentication |
| `AZURE_TENANT_ID` | From Azure service principal | Azure authentication |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription | Azure authentication |
| `GH_PACKAGES_TOKEN` | GitHub PAT with `read:packages` | NuGet package restore |

### Getting the WORKSPACE_DISPATCH_TOKEN

Use the existing PAT `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` which already has `repo` scope for the workspace. Add it to your submodule repository as `WORKSPACE_DISPATCH_TOKEN`.

---

## Event Type Mapping

Each submodule uses a specific event type:

| Repository | Event Type | Target Deployment |
|------------|------------|-------------------|
| Mystira.Admin.Api | `admin-api-deploy` | `mys-admin-api` |
| Mystira.Admin.UI | `admin-ui-deploy` | `mys-admin-ui` |
| Mystira.StoryGenerator | `story-generator-deploy` | `mys-story-generator` |
| Mystira.Publisher | `publisher-deploy` | `mys-publisher` |
| Mystira.Chain | `chain-deploy` | `mys-chain` |

---

## Complete Workflow Template

Copy and adapt this workflow for your submodule:

```yaml
name: "Service Name: Build & Deploy Dev"

on:
  push:
    branches: [dev, main]
  pull_request:
    branches: [dev, main]
  workflow_dispatch:

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: ${{ github.event_name == 'pull_request' }}

env:
  DOTNET_NOLOGO: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true
  AZURE_CONTAINER_REGISTRY: mysdevacr
  IMAGE_NAME: admin-api  # Change to your image name

permissions:
  id-token: write
  contents: read
  packages: read

jobs:
  # ============================================
  # Build and Push Docker Image
  # ============================================
  build-and-push:
    name: Build & Push Image
    runs-on: ubuntu-latest
    # Skip draft PRs, run on everything else
    if: github.event_name != 'pull_request' || !github.event.pull_request.draft
    outputs:
      image-tag: ${{ steps.meta.outputs.version }}
      image-digest: ${{ steps.build.outputs.digest }}
    steps:
      - uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Azure Login
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZURE_CLIENT_ID }}
          tenant-id: ${{ secrets.AZURE_TENANT_ID }}
          subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

      - name: Login to Azure Container Registry
        run: az acr login --name ${{ env.AZURE_CONTAINER_REGISTRY }}

      - name: Extract metadata for Docker
        id: meta
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.AZURE_CONTAINER_REGISTRY }}.azurecr.io/${{ env.IMAGE_NAME }}
          tags: |
            type=sha,prefix=dev-
            type=raw,value=dev,enable=${{ github.ref == 'refs/heads/dev' }}
            type=raw,value=latest,enable=${{ github.ref == 'refs/heads/main' }}

      - name: Build and push Docker image
        id: build
        uses: docker/build-push-action@v5
        with:
          context: .
          file: ./src/YourProject/Dockerfile  # Update path
          push: ${{ github.event_name != 'pull_request' }}
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max
          build-args: |
            NUGET_AUTH_TOKEN=${{ secrets.GH_PACKAGES_TOKEN }}

  # ============================================
  # Trigger Workspace Deployment (Dev Only)
  # ============================================
  trigger-workspace-deploy:
    name: Trigger Workspace Deployment
    runs-on: ubuntu-latest
    needs: build-and-push
    # Only deploy on push to dev or manual dispatch
    if: |
      needs.build-and-push.result == 'success' &&
      github.ref == 'refs/heads/dev' &&
      (github.event_name == 'push' || github.event_name == 'workflow_dispatch')
    steps:
      - name: Trigger deployment via workspace
        uses: peter-evans/repository-dispatch@v3
        with:
          token: ${{ secrets.WORKSPACE_DISPATCH_TOKEN }}
          repository: phoenixvc/Mystira.workspace
          event-type: admin-api-deploy  # Change to your event type
          client-payload: |
            {
              "environment": "dev",
              "ref": "${{ github.sha }}",
              "triggered_by": "${{ github.actor }}",
              "run_id": "${{ github.run_id }}",
              "image_tag": "dev-${{ github.sha }}",
              "pr_number": ""
            }

  # ============================================
  # Deployment Summary
  # ============================================
  notify:
    name: Deployment Summary
    runs-on: ubuntu-latest
    needs: [build-and-push, trigger-workspace-deploy]
    if: always()
    steps:
      - name: Generate Summary
        run: |
          echo "## Deployment Summary" >> $GITHUB_STEP_SUMMARY
          echo "" >> $GITHUB_STEP_SUMMARY
          echo "| Property | Value |" >> $GITHUB_STEP_SUMMARY
          echo "|----------|-------|" >> $GITHUB_STEP_SUMMARY
          echo "| Branch | ${{ github.ref_name }} |" >> $GITHUB_STEP_SUMMARY
          echo "| Commit | \`${{ github.sha }}\` |" >> $GITHUB_STEP_SUMMARY
          echo "| Actor | ${{ github.actor }} |" >> $GITHUB_STEP_SUMMARY
          echo "| Build Status | ${{ needs.build-and-push.result }} |" >> $GITHUB_STEP_SUMMARY
          echo "| Dispatch Status | ${{ needs.trigger-workspace-deploy.result }} |" >> $GITHUB_STEP_SUMMARY
          echo "" >> $GITHUB_STEP_SUMMARY

          if [ "${{ needs.trigger-workspace-deploy.result }}" == "success" ]; then
            echo "### Dev Deployment Triggered" >> $GITHUB_STEP_SUMMARY
            echo "Image: \`${{ env.AZURE_CONTAINER_REGISTRY }}.azurecr.io/${{ env.IMAGE_NAME }}:dev-${{ github.sha }}\`" >> $GITHUB_STEP_SUMMARY
          elif [ "${{ needs.trigger-workspace-deploy.result }}" == "skipped" ]; then
            echo "### No Deployment" >> $GITHUB_STEP_SUMMARY
            echo "Deployment only runs on push to \`dev\` branch." >> $GITHUB_STEP_SUMMARY
          fi

          echo "" >> $GITHUB_STEP_SUMMARY
          echo "---" >> $GITHUB_STEP_SUMMARY
          echo "_Staging/prod deployments are managed through the workspace._" >> $GITHUB_STEP_SUMMARY
```

---

## Customization Checklist

When adapting the template:

1. **Update workflow name**: `name: "Your Service: Build & Deploy Dev"`

2. **Update environment variables**:
   ```yaml
   env:
     IMAGE_NAME: your-image-name  # e.g., admin-api, admin-ui, publisher
   ```

3. **Update Dockerfile path**:
   ```yaml
   file: ./src/YourProject/Dockerfile
   ```

4. **Update event type** in `trigger-workspace-deploy`:
   ```yaml
   event-type: your-service-deploy  # Must match workspace handler
   ```

5. **Add any service-specific build args** if needed

---

## Validation

After setup, verify:

1. **PR creates CI run** - Open a PR and confirm CI runs
2. **Draft PR skips CI** - Convert to draft and confirm no run
3. **Merge triggers deploy** - Merge PR and confirm:
   - Docker image pushed to ACR with `dev-<sha>` tag
   - Workspace `submodule-deploy-dev` workflow triggered
   - Pod updated in `mys-dev` namespace

### Check deployment status

```bash
# View workspace workflow runs
gh run list --repo phoenixvc/Mystira.workspace --workflow=submodule-deploy-dev.yml

# Check pod in dev cluster
kubectl get pods -n mys-dev -l app=mys-admin-api
```

---

## Troubleshooting

### Dispatch not triggering

1. Verify `WORKSPACE_DISPATCH_TOKEN` has `repo` scope
2. Check event type matches workspace handler
3. Verify push to `dev` branch (not `main` or feature branch)

### Image not found in ACR

1. Confirm Azure login succeeded
2. Check ACR name matches (`mysdevacr`)
3. Verify Dockerfile path is correct

### Deployment failing in workspace

1. Check workspace workflow logs
2. Verify Kubernetes manifests exist in workspace
3. Confirm image tag format matches (`dev-<sha>`)

---

## Related Documentation

- [Publishing Flow](./publishing-flow.md) - Complete deployment documentation
- [Submodule Access](./submodule-access.md) - Token and access troubleshooting
- [Submodules Guide](../guides/submodules.md) - Working with submodules
