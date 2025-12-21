# Comprehensive Setup Guide

Complete setup guide for the Mystira workspace covering all projects, CI/CD workflows, secrets, environments, and platforms.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Initial Workspace Setup](#initial-workspace-setup)
3. [GitHub Secrets Configuration](#github-secrets-configuration)
4. [Azure Infrastructure Setup](#azure-infrastructure-setup)
5. [Project-Specific Setup](#project-specific-setup)
6. [CI/CD Configuration](#cicd-configuration)
7. [Environment Setup](#environment-setup)
8. [Deployment](#deployment)
9. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Tools

- **Node.js**: v18.x or higher
- **pnpm**: v8.10.0 or higher
- **Git**: Latest version with submodule support
- **Docker**: Latest version (for local development)
- **Azure CLI**: Latest version (for infrastructure deployment)
- **Terraform**: v1.5.0 or higher (for infrastructure)
- **kubectl**: Latest version (for Kubernetes management)

### Install Prerequisites

```bash
# Install Node.js (use nvm or download from nodejs.org)
nvm install 18
nvm use 18

# Install pnpm globally
npm install -g pnpm@8.10.0

# Install Azure CLI
# Windows (PowerShell as Admin):
Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi; Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet'; rm .\AzureCLI.msi

# macOS:
brew install azure-cli

# Linux:
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Install Terraform
# Windows (choco):
choco install terraform

# macOS:
brew install terraform

# Linux:
curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -
sudo apt-add-repository "deb [arch=$(dpkg --print-architecture)] https://apt.releases.hashicorp.com $(lsb_release -cs) main"
sudo apt install terraform

# Install kubectl
# Windows (choco):
choco install kubernetes-cli

# macOS:
brew install kubectl

# Linux:
curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl
```

### Azure Account Setup

1. **Create Azure Account**: Sign up at https://azure.microsoft.com/
2. **Create Service Principal** (for CI/CD):

   ```bash
   az login
   az account set --subscription "Your Subscription Name"

   # Create service principal with Contributor role
   az ad sp create-for-rbac --name "MystiraGitHubActions" \
     --role contributor \
     --scopes /subscriptions/{subscription-id} \
     --sdk-auth
   ```

   Save the JSON output - you'll need it for GitHub secrets.

3. **Create Azure DevOps Organization** (for NuGet feed):
   - Go to https://dev.azure.com/
   - Create or select an organization
   - Create a project (e.g., "Mystira")
   - Create an Artifacts feed named "Mystira-Internal"
   - Generate a Personal Access Token (PAT) with **Packaging (Read & Write)** scope

---

## Initial Workspace Setup

### 1. Clone Repository with Submodules

```bash
# Clone with all submodules
git clone --recurse-submodules https://github.com/phoenixvc/Mystira.workspace.git
cd Mystira.workspace
```

If you already cloned without submodules:

```bash
git submodule update --init --recursive
```

### 2. Verify Submodules

```bash
# List all submodules
git submodule status

# Update all submodules to latest
git submodule update --remote --recursive
```

Expected submodules:

- `infra/` → `Mystira.Infra`
- `packages/app/` → `Mystira.App`
- `packages/chain/` → `Mystira.Chain`
- `packages/publisher/` → `Mystira.Publisher`
- `packages/story-generator/` → `Mystira.StoryGenerator`
- `packages/devhub/` → `Mystira.DevHub`
- `packages/admin-api/` → `Mystira.Admin.Api`

### 3. Install Dependencies

```bash
# Install all workspace dependencies
pnpm install --frozen-lockfile
```

### 4. Set Up Environment Variables

See [Environment Variables](./environment-variables.md) for detailed environment variable configuration.

Create `.env.local` files in:

- Root: `.env.local` (workspace config)
- `packages/chain/.env.local` (blockchain config)
- `packages/story-generator/.env.local` (AI services)

---

## GitHub Secrets Configuration

All secrets must be configured in GitHub repository settings: `Settings → Secrets and variables → Actions`

### Required Secrets for Mystira.workspace

#### Submodule Access

| Secret Name                             | Description                                                            | How to Create                                                                                                                     |
| --------------------------------------- | ---------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` | Personal Access Token with `repo` scope for cloning private submodules | GitHub → Settings → Developer settings → Personal access tokens → Generate new token (classic) → Select `repo` scope → Copy token |

#### Azure Infrastructure

| Secret Name         | Description                                            | How to Create                                                                   |
| ------------------- | ------------------------------------------------------ | ------------------------------------------------------------------------------- |
| `AZURE_CREDENTIALS` | Service Principal credentials for Azure authentication | Use the JSON output from `az ad sp create-for-rbac` command (see Prerequisites) |

**Format** (JSON):

```json
{
  "clientId": "xxx",
  "clientSecret": "xxx",
  "subscriptionId": "xxx",
  "tenantId": "xxx"
}
```

#### NPM Package Publishing (Optional)

| Secret Name | Description                              | How to Create                                                                          |
| ----------- | ---------------------------------------- | -------------------------------------------------------------------------------------- |
| `NPM_TOKEN` | NPM access token for publishing packages | npm → Account Settings → Access Tokens → Generate New Token → Select "Automation" type |

### Required Secrets for Mystira.Admin.Api

| Secret Name                    | Description                                  | How to Create                                                                                 |
| ------------------------------ | -------------------------------------------- | --------------------------------------------------------------------------------------------- |
| `MYSTIRA_DEVOPS_AZURE_ORG`     | Azure DevOps organization name               | Your Azure DevOps org name (e.g., "phoenixvc")                                                |
| `MYSTIRA_DEVOPS_AZURE_PROJECT` | Azure DevOps project name                    | Your project name (e.g., "Mystira")                                                           |
| `MYSTIRA_DEVOPS_AZURE_PAT`     | Azure DevOps PAT with Packaging (Read) scope | Azure DevOps → User Settings → Personal access tokens → New Token → Select "Packaging (Read)" |
| `MYSTIRA_DEVOPS_NUGET_FEED`    | NuGet feed name                              | Your feed name (e.g., "Mystira-Internal")                                                     |

### Creating GitHub Personal Access Token (PAT)

1. Go to GitHub → Your Profile → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Name: `Mystira Submodule Access`
4. Expiration: Set appropriate expiration (90 days recommended)
5. Scopes: Check `repo` (Full control of private repositories)
6. Click "Generate token"
7. Copy the token immediately (you won't see it again)
8. Add as secret `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` in repository settings

---

## Azure Infrastructure Setup

### 1. Deploy Dev Environment (Required First)

The dev environment creates the shared Azure Container Registry (`mystiraacr`) that all environments use.

```bash
cd infra/terraform/environments/dev

# Initialize Terraform
terraform init

# Review plan
terraform plan

# Apply infrastructure
terraform apply
```

**What gets created** (per [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md)):

- Resource Group: `mys-dev-core-rg-eus`
- Azure Container Registry: `myssharedacr` (shared across all environments)
- Virtual Network: `mys-dev-core-vnet-eus`
- Key Vaults: `mys-dev-mystira-{component}-kv-eus`
- Shared PostgreSQL (if configured)
- Shared Redis (if configured)
- AKS cluster: `mys-dev-core-aks-eus`

**Important**: The ACR `myssharedacr` is created in the dev environment but is used by all environments. This is intentional - we use a shared ACR with environment-specific tags (dev, staging, prod).

### 2. Configure ACR Access

After dev environment is deployed, grant access to service principals:

```bash
# Get ACR resource ID
ACR_ID=$(az acr show --name myssharedacr --resource-group mys-dev-core-rg-eus --query id --output tsv)

# Grant pull permissions to staging/prod service principals (when created)
# az role assignment create --assignee <service-principal-id> --role AcrPull --scope $ACR_ID
```

### 3. Update Infra Submodule for Shared ACR

The infra submodule has been updated to use shared ACR with environment tags. Ensure you have the latest:

```bash
# In workspace root
cd infra
git pull origin main
cd ..

# Update submodule reference
git add infra
git commit -m "chore: update infra submodule"
git push origin dev
```

### 4. Deploy Staging Environment (Optional)

```bash
cd infra/terraform/environments/staging
terraform init
terraform plan
terraform apply
```

### 5. Deploy Production Environment (Optional)

```bash
cd infra/terraform/environments/prod
terraform init
terraform plan
terraform apply
```

**⚠️ Production Warning**: Production deployment should be carefully planned and tested in staging first.

---

## Authentication Setup (Entra ID & B2C)

The Mystira platform uses Microsoft Entra ID for admin authentication and Azure AD B2C for consumer authentication with social login support. For complete details, see [ADR-0011: Entra ID Integration](./architecture/adr/0011-entra-id-authentication-integration.md).

### 1. Microsoft Entra ID Setup (Admin)

#### Create App Registration for Admin API

1. Go to Azure Portal → Microsoft Entra ID → App registrations
2. Click "New registration"
3. Configure:
   - Name: `mystira-admin-api`
   - Supported account types: Single tenant
   - Redirect URI: (none for API)
4. After creation, note the **Application (client) ID**
5. Go to "Expose an API" → Set Application ID URI: `api://mystira-admin-api`
6. Add scopes: `Admin.Read`, `Admin.Write`, `Users.Manage`, `Content.Moderate`

#### Create App Registration for Admin UI

1. Go to Azure Portal → Microsoft Entra ID → App registrations
2. Click "New registration"
3. Configure:
   - Name: `mystira-admin-ui`
   - Supported account types: Single tenant
   - Redirect URI: `https://admin.mystira.app/auth/callback` (Web)
4. Add localhost for development: `http://localhost:5173/auth/callback`
5. Enable "Access tokens" and "ID tokens" in Authentication

#### Configure App Roles

1. Go to Admin API app registration → App roles
2. Create roles:
   - Admin (users/groups)
   - SuperAdmin (users)
   - Moderator (users/groups)
   - Viewer (users/groups)

#### Set Up Conditional Access (MFA)

1. Go to Azure Portal → Microsoft Entra ID → Security → Conditional Access
2. Create policy:
   - Name: "Require MFA for Mystira Admin"
   - Users: Assign to admin groups
   - Cloud apps: Select `mystira-admin-api` and `mystira-admin-ui`
   - Grant: Require MFA

### 2. Azure AD B2C Setup (Consumer)

#### Create B2C Tenant

```bash
# Via Azure CLI (or use Azure Portal)
az ad b2c tenant create \
  --display-name "Mystira B2C" \
  --domain-name "mystirab2c.onmicrosoft.com" \
  --country-code "US"
```

#### Configure Identity Providers

**Google**:
1. Create OAuth credentials at [Google Cloud Console](https://console.cloud.google.com/)
2. In B2C tenant → Identity providers → Add Google
3. Enter Client ID and Client Secret from Google
4. Redirect URI: `https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/oauth2/authresp`

**Discord**:
1. Create application at [Discord Developer Portal](https://discord.com/developers/applications)
2. In B2C tenant → Identity providers → Add OpenID Connect (custom)
3. Configure:
   - Name: Discord
   - Metadata URL: `https://discord.com/.well-known/openid-configuration`
   - Client ID/Secret from Discord
   - Scope: `identify email`

#### Create User Flows

1. Go to B2C tenant → User flows
2. Create **Sign up and sign in** flow (`B2C_1_SignUpSignIn`):
   - Identity providers: Email, Google, Discord
   - User attributes: Email Address, Display Name
   - Application claims: Object ID, Email, Display Name, Identity Provider
3. Create **Password reset** flow (`B2C_1_PasswordReset`)
4. Create **Profile editing** flow (`B2C_1_ProfileEdit`)

#### Create App Registration for Public API

1. In B2C tenant → App registrations
2. Create `mystira-public-api`:
   - Supported account types: B2C tenant accounts
   - Application ID URI: `https://mystirab2c.onmicrosoft.com/mystira-api`
   - Add scope: `API.Access`

### 3. Environment Configuration

After setup, configure environment variables as documented in [Environment Variables](./environment-variables.md#authentication-variables).

**Required Secrets in Azure Key Vault**:

| Secret Name | Description |
|-------------|-------------|
| `azure-ad-client-secret` | Admin API client secret |
| `azure-b2c-client-secret` | B2C API client secret (if confidential) |
| `google-client-secret` | Google OAuth client secret |
| `discord-client-secret` | Discord OAuth client secret |

### 4. Verify Setup

```bash
# Test Entra ID token acquisition (Admin)
az account get-access-token --resource api://mystira-admin-api

# Verify B2C metadata endpoint
curl https://mystirab2c.b2clogin.com/mystirab2c.onmicrosoft.com/B2C_1_SignUpSignIn/v2.0/.well-known/openid-configuration
```

---

## Project-Specific Setup

### Mystira.Chain (Python gRPC Service)

```bash
cd packages/chain

# Install Python dependencies
pip install -r requirements.txt  # If requirements.txt exists

# Set up environment variables
cp .env.example .env.local
# Edit .env.local with your values

# Test locally
python server.py
```

### Mystira.Publisher (TypeScript/React)

```bash
cd packages/publisher

# Dependencies are installed at workspace root
# Run development server
pnpm dev

# Build
pnpm build
```

### Mystira.StoryGenerator (.NET)

```bash
cd packages/story-generator

# Restore NuGet packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/StoryGenerator
```

### Mystira.App (.NET + Blazor)

```bash
cd packages/app

# Restore NuGet packages
dotnet restore

# Build
dotnet build

# Run specific projects
dotnet run --project src/Mystira.App.Api
dotnet run --project src/Mystira.App.PWA
```

### Mystira.Admin.Api (.NET)

```bash
cd packages/admin-api

# Configure NuGet.config with Azure DevOps feed
# Edit NuGet.config and update feed URL/token placeholders

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/Mystira.App.Admin.Api
```

**NuGet.config Setup**:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="Mystira-Internal" value="https://pkgs.dev.azure.com/{ORG}/{PROJECT}/_packaging/{FEED}/nuget/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <Mystira-Internal>
      <add key="Username" value="{ORG}" />
      <add key="ClearTextPassword" value="{PAT}" />
    </Mystira-Internal>
  </packageSourceCredentials>
</configuration>
```

---

## CI/CD Configuration

### Workflow Overview

The workspace uses GitHub Actions for CI/CD with the following workflows:

| Workflow                 | Triggers                                  | Purpose                                               |
| ------------------------ | ----------------------------------------- | ----------------------------------------------------- |
| `ci.yml`                 | Push/PR to `dev`/`main`                   | Lint, test, build workspace packages                  |
| `chain-ci.yml`           | Push/PR to `dev`/`main` (chain files)     | Chain service CI: lint, test, build, Docker build     |
| `publisher-ci.yml`       | Push/PR to `dev`/`main` (publisher files) | Publisher service CI: lint, test, build, Docker build |
| `staging-release.yml`    | Push to `main`                            | Automatic staging deployment                          |
| `production-release.yml` | Manual workflow dispatch                  | Production deployment (with approval)                 |
| `release.yml`            | Push to `main`                            | NPM package releases via Changesets                   |

### Branch Strategy

- **`dev`**: Development branch - all feature work integrated here
  - Requires PR (0 approvals for fast iteration)
  - CI checks required
  - Docker images tagged with `dev`
- **`main`**: Production-ready code
  - Requires PR with 1 approval
  - Code owner review required
  - Conversation resolution required
  - Docker images tagged with `staging`
  - Automatic staging deployment on merge

### Image Tagging Strategy

The CI/CD workflows use a shared ACR (`myssharedacr`) with environment-specific tags:

- **`dev` branch pushes**: Images tagged with `dev`, `latest`, and branch name
- **`main` branch pushes**: Images tagged with `staging`, `latest`, and branch name
- **Production deployments**: Use `prod` tag (manually tagged or via production workflow)

Kubernetes overlays map base images to environment tags:

- Dev overlay: `myssharedacr.azurecr.io/chain:dev`
- Staging overlay: `myssharedacr.azurecr.io/chain:staging`
- Prod overlay: `myssharedacr.azurecr.io/chain:prod`

### Configuring Branch Protection

Branch protection is configured via GitHub UI or CLI:

**Via GitHub UI**:

1. Go to Repository → Settings → Branches
2. Click "Add rule" or edit existing rule
3. Configure for `dev` and `main` as per [Branch Protection Guide](./cicd/branch-protection.md)

**Via GitHub CLI**:
See [Branch Protection Guide](./cicd/branch-protection.md) for CLI commands.

### Configuring GitHub Environments

**Staging Environment**:

1. Repository → Settings → Environments
2. Click "New environment"
3. Name: `staging`
4. Deployment branches: Restrict to `main`
5. Protection rules: Optional (no approval required for staging)

**Production Environment**:

1. Repository → Settings → Environments
2. Click "New environment"
3. Name: `production`
4. Deployment branches: Restrict to `main`
5. Protection rules:
   - ✅ Required reviewers: Add at least 1 reviewer
   - Optional: Wait timer (e.g., 5 minutes)

---

## Environment Setup

### Local Development

```bash
# Start shared services (PostgreSQL, Redis)
docker-compose up -d

# Start all services in development mode
pnpm dev

# Or start individual services
pnpm --filter @mystira/chain dev
pnpm --filter mystira-publisher dev
```

### Development Environment (Azure)

- **Resource Group**: `mys-dev-core-rg-eus`
- **ACR**: `myssharedacr` (shared)
- **AKS**: `mys-dev-core-aks-eus`
- **Namespace**: `mystira-dev`

### Staging Environment (Azure)

- **Resource Group**: `mys-staging-core-rg-eus`
- **ACR**: `myssharedacr` (shared, uses `staging` tags)
- **AKS**: `mys-staging-core-aks-eus`
- **Namespace**: `mystira-staging`

### Production Environment (Azure)

- **Resource Group**: `mys-prod-core-rg-eus`
- **ACR**: `myssharedacr` (shared, uses `prod` tags)
- **AKS**: `mys-prod-core-aks-eus`
- **Namespace**: `mystira-prod`

---

## Deployment

### Initial Infrastructure Deployment

1. **Deploy Dev Environment** (creates shared ACR):

   ```bash
   cd infra/terraform/environments/dev
   terraform init
   terraform apply
   ```

2. **Update Infra Submodule** (if changes were made):

   ```bash
   # If you modified infra submodule locally
   cd infra
   git add .
   git commit -m "your changes"
   git push origin main

   # Then update reference in workspace
   cd ..
   git add infra
   git commit -m "chore: update infra submodule"
   git push origin dev
   ```

3. **Verify ACR exists**:
   ```bash
   az acr show --name myssharedacr --resource-group mys-dev-core-rg-eus
   ```

### Deploying Services to Kubernetes

#### Staging Deployment (Automatic)

Staging deployment happens automatically when code is merged to `main`:

1. Create PR: `dev` → `main`
2. CI checks pass
3. PR approved and merged
4. `staging-release.yml` workflow runs automatically
5. Services deployed to staging AKS cluster

#### Production Deployment (Manual)

Production deployment requires manual approval:

1. Go to GitHub Actions → "Production Release"
2. Click "Run workflow"
3. Select branch: `main`
4. In confirmation field, type: `DEPLOY TO PRODUCTION`
5. Click "Run workflow"
6. Environment reviewers receive notification
7. After approval, deployment proceeds

### Manual Kubernetes Deployment

```bash
# Get AKS credentials
az aks get-credentials --resource-group mystira-dev-rg --name mystira-dev-aks

# Deploy using kustomize
kubectl apply -k infra/kubernetes/overlays/dev

# Verify deployment
kubectl get pods -n mystira-dev
kubectl get services -n mystira-dev
```

---

## Troubleshooting

### Submodule Issues

**Problem**: Submodules not found or empty

```bash
# Re-initialize submodules
git submodule deinit -f --all
git submodule update --init --recursive
```

**Problem**: CI fails with "repository not found" for submodules

- Verify `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` secret is configured
- Verify PAT has `repo` scope
- Check that all submodule repositories exist and are accessible

### ACR Login Issues

**Problem**: `az acr login` fails with "resource not found"

- Verify dev environment is deployed (ACR is created in dev)
- Check ACR name: `myssharedacr`
- Verify you're in correct Azure subscription: `az account show`

**Problem**: CI/CD can't push to ACR

- Verify `AZURE_CREDENTIALS` secret is configured correctly
- Verify service principal has `AcrPush` role on ACR:
  ```bash
  az role assignment list --assignee <sp-client-id> --scope /subscriptions/<sub-id>/resourceGroups/mystira-dev-rg/providers/Microsoft.ContainerRegistry/registries/mystiraacr
  ```

### Terraform Issues

**Problem**: Terraform state locked

```bash
# Force unlock (use with caution)
terraform force-unlock <lock-id>
```

**Problem**: Terraform backend authentication fails

- Verify Azure authentication: `az login`
- Check backend configuration in `terraform` block
- Verify service principal has access to storage account

### Kubernetes Deployment Issues

**Problem**: Pods not starting

```bash
# Check pod logs
kubectl logs <pod-name> -n mystira-dev

# Describe pod for events
kubectl describe pod <pod-name> -n mystira-dev

# Check image pull errors
kubectl get events -n mystira-dev --sort-by='.lastTimestamp'
```

**Problem**: Image pull errors

- Verify ACR credentials in Kubernetes: `kubectl get secret -n mystira-dev`
- Create pull secret if missing:
  ```bash
  kubectl create secret docker-registry acr-secret \
    --docker-server=myssharedacr.azurecr.io \
    --docker-username=<sp-client-id> \
    --docker-password=<sp-client-secret> \
    -n mystira-dev
  ```

### CI/CD Workflow Issues

**Problem**: Workflow fails with "Input required and not supplied: token"

- Verify `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` secret exists
- Check workflow file has token validation step
- Verify secret name matches exactly (case-sensitive)

**Problem**: Docker build fails in CI

- Check Dockerfile paths are correct
- Verify build context is set correctly
- Check that required files exist in build context

---

## Next Steps

- Review [Architecture Overview](./architecture.md)
- Read [CI/CD Setup Guide](../cicd/cicd-setup.md)
- Check [Infrastructure Guide](../infrastructure/infrastructure.md)
- See [Shared Resources Guide](../infrastructure/shared-resources.md)
- Review [ACR Strategy](../infrastructure/acr-strategy.md)

## Getting Help

- Check existing GitHub issues
- Review documentation in `docs/` directory
- Check workflow logs in GitHub Actions
- Review service logs in Azure Portal or Kubernetes

---

**Last Updated**: 2025-12-14
