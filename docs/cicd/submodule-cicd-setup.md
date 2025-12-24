# Submodule CI/CD Setup Guide

**For**: Mystira submodule repositories (Admin.Api, Admin.UI, Publisher, Chain, StoryGenerator, App)
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

### Azure OIDC Credentials (Terraform-Managed)

Azure authentication credentials are managed via Terraform in `infra/terraform/modules/github-oidc`. After running `terraform apply`, get the values:

```bash
cd infra/terraform/environments/dev
terraform output github_oidc_secrets
```

Add these secrets to **each submodule repository**:

| Secret | Source | Purpose |
|--------|--------|---------|
| `AZURE_CLIENT_ID` | `terraform output github_oidc_client_id` | Azure OIDC authentication |
| `AZURE_TENANT_ID` | `terraform output github_oidc_tenant_id` | Azure OIDC authentication |
| `AZURE_SUBSCRIPTION_ID` | `terraform output github_oidc_subscription_id` | Azure OIDC authentication |

> **Note**: Federated credentials for each repo/branch are automatically created by Terraform. No manual Azure AD configuration needed.

### Other Secrets

| Secret | Value | Purpose |
|--------|-------|---------|
| `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` | GitHub PAT with `repo` scope | Trigger workspace deployments |
| `GH_PACKAGES_TOKEN` | GitHub PAT with `read:packages` | NuGet package restore |

**Workspace-level secrets** (configured in `Mystira.workspace`):

| Secret | Value | Purpose |
|--------|-------|---------|
| `MYSTIRA_AZURE_CREDENTIALS` | Azure service principal JSON | Azure login for deployments |
| `MS_TEAMS_WEBHOOK_URL` | Teams incoming webhook URL | Deployment notifications (optional) |

> **Note**: `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` is the standard PAT used across all Mystira repositories. It must have `repo` scope to trigger `repository_dispatch` events in the workspace.

### Adding a New Submodule Repository

When adding a new submodule that needs CI/CD:

1. Add the repository to `infra/terraform/environments/dev/main.tf`:
   ```hcl
   module "github_oidc" {
     # ... existing config ...
     repositories = {
       # ... existing repos ...
       "new-service" = {
         name     = "Mystira.NewService"
         branches = ["dev", "main"]
       }
     }
   }
   ```

2. Run `terraform apply` to create the federated credentials

3. Copy the OIDC secrets to the new repository's GitHub secrets

---

## Event Type Mapping

Each submodule uses a specific event type:

| Repository | Event Type | Target | Infra |
|------------|------------|--------|-------|
| Mystira.Admin.Api | `admin-api-deploy` | `mys-admin-api` | Kubernetes |
| Mystira.Admin.UI | `admin-ui-deploy` | `mys-admin-ui` | Kubernetes |
| Mystira.StoryGenerator (API) | `story-generator-deploy` | `mys-story-generator` | Kubernetes |
| Mystira.Publisher | `publisher-deploy` | `mys-publisher` | Kubernetes |
| Mystira.Chain | `chain-deploy` | `mys-chain` | Kubernetes |
| Mystira.App (API) | `app-deploy` | `mys-dev-app-api-san` | App Service |
| Mystira.App (SWA) | `app-swa-deploy` | `mys-dev-mystira-swa-eus2` | Static Web App |
| Mystira.DevHub | `devhub-deploy` | `mys-dev-devhub-san` | App Service |

> **Note**: `Mystira.StoryGenerator` follows the same API/Web pattern as `Mystira.App`:
> - **API** (`Mystira.StoryGenerator.Api`) → Kubernetes via `story-generator-deploy`
> - **Web** (`Mystira.StoryGenerator.Web`, Blazor WASM) → Static Web App (if needed, similar to `app-swa-deploy`)

---

## Workflow Architecture Options

### Option A: Separate Jobs (Recommended)

Faster PR feedback - lint and test run in parallel, fail fast.

```
PR opened → lint ─┬─→ (all pass) → PR ready
                  │
           test ──┘

Push to dev → lint ─┬─→ build-and-push → trigger-deploy
                    │
             test ──┘
```

### Option B: Combined Job

Simpler, single workflow - but slower PR feedback.

```
PR opened → build-and-push (push=false)
Push to dev → build-and-push (push=true) → trigger-deploy
```

**Choose Option A** if you want faster CI feedback on PRs.

---

## Complete Workflow Template (Option A - Separate Jobs)

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
  AZURE_CONTAINER_REGISTRY: myssharedacr
  IMAGE_NAME: admin-api  # Change to your image name

permissions:
  id-token: write
  contents: read
  packages: read

jobs:
  # ============================================
  # CI Jobs (run in parallel for fast feedback)
  # ============================================
  lint:
    name: Lint
    runs-on: ubuntu-latest
    if: github.event_name != 'pull_request' || !github.event.pull_request.draft
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore

      - name: Format check
        run: dotnet format --verify-no-changes --verbosity diagnostic

  test:
    name: Test
    runs-on: ubuntu-latest
    if: github.event_name != 'pull_request' || !github.event.pull_request.draft
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore
        env:
          NUGET_AUTH_TOKEN: ${{ secrets.GH_PACKAGES_TOKEN }}

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal

  # ============================================
  # Build and Push Docker Image
  # ============================================
  build-and-push:
    name: Build & Push Image
    runs-on: ubuntu-latest
    needs: [lint, test]
    # Only build/push on push to branch (not PRs)
    if: github.event_name != 'pull_request'
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
          push: true
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
          token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
          repository: phoenixvc/Mystira.workspace
          event-type: admin-api-deploy  # Change to your event type
          client-payload: |
            {
              "environment": "dev",
              "ref": "${{ github.sha }}",
              "triggered_by": "${{ github.actor }}",
              "run_id": "${{ github.run_id }}",
              "repository": "${{ github.repository }}",
              "image_tag": "dev-${{ github.sha }}",
              "pr_number": ""
            }

  # ============================================
  # Deployment Summary
  # ============================================
  notify:
    name: Deployment Summary
    runs-on: ubuntu-latest
    needs: [lint, test, build-and-push, trigger-workspace-deploy]
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
          echo "| Lint | ${{ needs.lint.result }} |" >> $GITHUB_STEP_SUMMARY
          echo "| Test | ${{ needs.test.result }} |" >> $GITHUB_STEP_SUMMARY
          echo "| Build | ${{ needs.build-and-push.result }} |" >> $GITHUB_STEP_SUMMARY
          echo "| Deploy | ${{ needs.trigger-workspace-deploy.result }} |" >> $GITHUB_STEP_SUMMARY
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

## Mystira.App Special Case

Mystira.App uses **Azure App Service** (API) and **Static Web App** (Blazor WASM frontend), not Kubernetes.

### Event Types

| Component | Event Type | Target | Notes |
|-----------|------------|--------|-------|
| API | `app-deploy` | `mys-dev-app-api-san` | Builds and deploys to App Service |
| SWA | `app-swa-deploy` | `mys-dev-mystira-swa-eus2` | SWA deploys from App repo; triggers submodule ref update |

### Deployment Pattern

| Environment | API Workflow | SWA Workflow |
|-------------|--------------|--------------|
| Dev | `submodule-deploy-dev-appservice.yml` | Same (via `app-swa-deploy` event) |
| Production | `mystira-app-api-cicd-prod.yml` | Separate SWA workflow (blue-green) |

**Note**: The Static Web App deployment happens directly in the App repository via `Azure/static-web-apps-deploy`. After successful SWA deployment, the App repo triggers `app-swa-deploy` to update the submodule reference in the workspace.

---

## NuGet Package Publishing (Optional)

If your submodule has shared contracts (e.g., `Mystira.App.Contracts`), add a NuGet publish job:

```yaml
trigger-nuget-publish:
  name: Trigger NuGet Publish
  runs-on: ubuntu-latest
  needs: build-and-push
  if: |
    needs.build-and-push.result == 'success' &&
    (github.ref == 'refs/heads/dev' || github.ref == 'refs/heads/main')
  steps:
    - name: Trigger NuGet publish
      uses: peter-evans/repository-dispatch@v3
      with:
        token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
        repository: phoenixvc/Mystira.workspace
        event-type: nuget-publish
        client-payload: |
          {
            "package": "app-contracts",
            "ref": "${{ github.sha }}",
            "triggered_by": "${{ github.actor }}",
            "run_id": "${{ github.run_id }}",
            "version_suffix": "${{ github.ref == 'refs/heads/dev' && format('dev.{0}', github.run_number) || '' }}",
            "is_prerelease": "${{ github.ref == 'refs/heads/dev' }}"
          }
```

| Package | `package` Value |
|---------|-----------------|
| Mystira.App.Contracts | `app-contracts` |
| Mystira.StoryGenerator.Contracts | `story-generator-contracts` |

**Version strategy:**
- `dev` branch: Pre-release (`1.0.0-dev.123`) → GitHub Packages only
- `main` branch: Stable (`1.0.0`) → GitHub Packages + NuGet.org

---

## Deployment Features

The workspace deployment workflows include several reliability and observability features:

### Image Verification

Before deploying, the workflow verifies the image exists in ACR:

```yaml
- name: Verify image exists in ACR
  run: |
    if ! az acr repository show-tags \
      --name myssharedacr \
      --repository "$IMAGE" \
      --query "[?@=='$TAG']" \
      --output tsv | grep -q .; then
      echo "::error::Image not found in ACR"
      exit 1
    fi
```

### Automatic Rollback

If a Kubernetes deployment fails, the workflow automatically rolls back:

```yaml
- name: Rollback on failure
  if: failure() && steps.rollout.outcome == 'failure'
  run: |
    kubectl rollout undo deployment/$DEPLOYMENT -n $NAMESPACE
    kubectl rollout status deployment/$DEPLOYMENT -n $NAMESPACE --timeout=120s
```

### Deployment Annotations

Successful deployments are annotated with tracking metadata:

```yaml
kubectl annotate deployment/$DEPLOYMENT -n $NAMESPACE --overwrite \
  mystira.app/deployed-by="$ACTOR" \
  mystira.app/deployed-at="$(date -u +"%Y-%m-%dT%H:%M:%SZ")" \
  mystira.app/source-commit="$REF" \
  mystira.app/image-tag="$IMAGE_TAG"
```

Query annotations with:
```bash
kubectl get deployment mys-admin-api -n mys-dev -o jsonpath='{.metadata.annotations}'
```

### Teams Notifications

If `MS_TEAMS_WEBHOOK_URL` is configured, the workflow sends success/failure notifications to Microsoft Teams.

---

## Setting Up Teams Notifications

To receive deployment notifications in Microsoft Teams:

### 1. Create an Incoming Webhook

1. Open Microsoft Teams and navigate to the channel where you want notifications
2. Click the **...** (More options) next to the channel name
3. Select **Connectors** (or **Manage channel** → **Connectors**)
4. Find **Incoming Webhook** and click **Configure**
5. Give the webhook a name (e.g., "Mystira Dev Deployments")
6. Optionally upload a custom icon
7. Click **Create**
8. **Copy the webhook URL** - you'll need this for the secret

### 2. Add the Secret

Add the webhook URL as a repository secret in `Mystira.workspace`:

1. Go to https://github.com/phoenixvc/Mystira.workspace/settings/secrets/actions
2. Click **New repository secret**
3. Name: `MS_TEAMS_WEBHOOK_URL`
4. Value: Paste the webhook URL from step 1
5. Click **Add secret**

### 3. Verify It Works

Trigger a deployment and check your Teams channel for the notification card.

> **Note**: Teams notifications are optional. If the secret is not configured, the notification steps are skipped gracefully.

---

## What Happens After Deployment

After a successful dev deployment, the workspace:

1. **Deploys** to K8s/App Service/SWA
2. **Updates submodule reference** to the deployed commit
3. **Commits and pushes** to workspace `dev` branch

This keeps the workspace in sync with deployed versions. You don't need to manually update submodule refs.

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

1. **PR opens → lint + test run in parallel**
2. **Draft PR → all jobs skip**
3. **Merge to dev → lint + test + build + deploy**
4. **Push to main → lint + test + build (no deploy)**

### Check deployment status

```bash
# View workspace workflow runs
gh run list --repo phoenixvc/Mystira.workspace --workflow=submodule-deploy-dev.yml

# Check pod in dev cluster (Kubernetes services)
kubectl get pods -n mys-dev -l app=mys-admin-api

# Check App Service (Mystira.App)
az webapp show --name mys-dev-app-api-san --resource-group mys-dev-core-rg-san --query state
```

---

## Troubleshooting

### Dispatch not triggering

1. Verify `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` has `repo` scope
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
