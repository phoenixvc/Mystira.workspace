# Infrastructure Deployment Checklist

## Current Status Summary

The infrastructure is **configured but NOT deployed**. All Terraform modules, Kubernetes manifests, and CI/CD workflows exist, but the actual Azure resources have not been created yet.

---

## Quick Start: Automated Deployment (Recommended)

The entire deployment process is now automated. Just run the GitHub Actions workflow!

### Option 1: GitHub Actions (Fully Automated)

1. Go to **Actions** → **Infrastructure Deploy**
2. Click **Run workflow**
3. Select environment: `dev`, `staging`, or `prod`
4. Click **Run workflow**

The pipeline will automatically:
- ✅ Validate prerequisites
- ✅ Create Terraform backend (if missing)
- ✅ Create Container Registry (if missing)
- ✅ Create DNS Zone (if missing)
- ✅ Run Terraform plan/apply
- ✅ Configure DNS A records
- ✅ Build and push Docker images
- ✅ Deploy to Kubernetes
- ✅ Set up SSL certificates

### Option 2: Local Bootstrap Script

For initial setup or troubleshooting, run locally:

```bash
# Login to Azure first
az login

# Run bootstrap script
./scripts/bootstrap-infra.sh
```

This creates all prerequisites (Terraform backend, ACR, DNS Zone, Service Principal).

---

## Prerequisites Checklist

| Prerequisite | Status | Notes |
|--------------|--------|-------|
| Domain `mystira.app` registered | ✅ Done | |
| Azure subscription | ✅ Done | |
| `AZURE_CREDENTIALS` GitHub secret | ✅ Done | |
| Terraform backend storage | ⏳ Auto-created | Created by pipeline |
| Container Registry | ⏳ Auto-created | Created by pipeline |
| DNS Zone in Azure | ⏳ Auto-created | Created by pipeline |

---

## Domain Naming Convention

**Pattern:** `{env}.{component}.{domain}` (e.g., `dev.publisher.mystira.app`)

| Environment | Publisher | Chain | API |
|-------------|-----------|-------|-----|
| Dev | `dev.publisher.mystira.app` | `dev.chain.mystira.app` | `dev.api.mystira.app` |
| Staging | `staging.publisher.mystira.app` | `staging.chain.mystira.app` | `staging.api.mystira.app` |
| Production | `publisher.mystira.app` | `chain.mystira.app` | `api.mystira.app` |

---

## Phase 1: Prerequisites (Before Any Deployment)

### 1.1 Domain Registration
- [ ] Register `mystira.app` domain (Google Domains, Namecheap, etc.)
- [ ] Verify ownership
- [ ] Note: `.app` domains require HTTPS (HSTS preloaded)

### 1.2 Azure Subscription Setup
- [ ] Create Azure subscription (if not exists)
- [ ] Set spending limits/alerts
- [ ] Enable required resource providers:
  ```bash
  az provider register --namespace Microsoft.ContainerService
  az provider register --namespace Microsoft.ContainerRegistry
  az provider register --namespace Microsoft.Cdn  # For Front Door
  az provider register --namespace Microsoft.Network
  ```

### 1.3 GitHub Secrets Configuration
Configure these secrets in GitHub repository settings (`Settings → Secrets → Actions`):

| Secret | Description | Example |
|--------|-------------|---------|
| `AZURE_CREDENTIALS` | Service principal JSON | `{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}` |
| `ACR_LOGIN_SERVER` | Container registry URL | `mysprodacr.azurecr.io` |
| `ACR_USERNAME` | Registry username | `mysprodacr` |
| `ACR_PASSWORD` | Registry password | (from Azure portal) |

**Create Service Principal:**
```bash
az ad sp create-for-rbac --name "mystira-github-actions" \
  --role contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth
```

### 1.4 Clone Infrastructure Submodule
```bash
# From workspace root
git submodule update --init infra

# Verify it worked
ls infra/terraform/environments/dev/
```

---

## Phase 2: Terraform Backend Setup

### 2.1 Create Terraform State Storage
```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "Your Subscription Name"

# Create resource group for Terraform state
az group create \
  --name mys-prod-terraform-rg-eus \
  --location eastus

# Create storage account
az storage account create \
  --name mysprodterraformstate \
  --resource-group mys-prod-terraform-rg-eus \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2

# Get storage account key
STORAGE_KEY=$(az storage account keys list \
  --resource-group mys-prod-terraform-rg-eus \
  --account-name mysprodterraformstate \
  --query "[0].value" -o tsv)

# Create container for state files
az storage container create \
  --name tfstate \
  --account-name mysprodterraformstate \
  --account-key $STORAGE_KEY
```

---

## Phase 3: Deploy Dev Environment

### 3.1 Initialize Terraform
```bash
cd infra/terraform/environments/dev
terraform init
```

### 3.2 Review Deployment Plan
```bash
terraform plan
```

**Expected resources:**
- Resource Group: `mys-dev-mystira-rg-eus`
- Virtual Network: `mys-dev-mystira-vnet-eus`
- AKS Cluster: `mys-dev-mystira-aks-eus`
- Shared ACR: `mysprodacr` (shared across environments)
- PostgreSQL: `mys-dev-mystira-pg-eus`
- Redis: `mys-dev-mystira-cache-eus`
- Key Vault: `mys-dev-mystira-kv-eus`

### 3.3 Apply Terraform
```bash
terraform apply
# Type 'yes' to confirm
```

**Estimated time:** 15-30 minutes
**Estimated cost:** ~$150-300/month for dev environment

### 3.4 Configure DNS
After AKS is deployed, get the Load Balancer IP:
```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group mys-dev-mystira-rg-eus \
  --name mys-dev-mystira-aks-eus

# Get Ingress external IP
kubectl get svc -n ingress-nginx ingress-nginx-controller -o jsonpath='{.status.loadBalancer.ingress[0].ip}'
```

Create DNS records:
| Type | Name | Value |
|------|------|-------|
| A | dev.publisher | (Load Balancer IP) |
| A | dev.chain | (Load Balancer IP) |
| A | dev.api | (Load Balancer IP) |

### 3.5 Deploy Kubernetes Resources
```bash
# Create namespace
kubectl create namespace mys-dev

# Deploy cert-manager (for SSL)
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml

# Wait for cert-manager to be ready
kubectl wait --for=condition=ready pod -l app.kubernetes.io/instance=cert-manager -n cert-manager --timeout=300s

# Deploy application
kubectl apply -k infra/kubernetes/overlays/dev
```

### 3.6 Build & Push Docker Images
```bash
# Login to ACR
az acr login --name mysprodacr

# Build and push Publisher
docker build -t mysprodacr.azurecr.io/publisher:dev -f infra/docker/publisher/Dockerfile packages/publisher
docker push mysprodacr.azurecr.io/publisher:dev

# Build and push Chain
docker build -t mysprodacr.azurecr.io/chain:dev -f infra/docker/chain/Dockerfile packages/chain
docker push mysprodacr.azurecr.io/chain:dev
```

### 3.7 Verify Deployment
```bash
# Check pods are running
kubectl get pods -n mys-dev

# Check ingress
kubectl get ingress -n mys-dev

# Check certificates
kubectl get certificates -n mys-dev

# Test endpoints
curl https://dev.publisher.mystira.app/health
curl https://dev.chain.mystira.app/health
```

---

## Phase 4: Deploy Staging Environment

Repeat Phase 3 steps with:
- Directory: `infra/terraform/environments/staging`
- Namespace: `mys-staging`
- URLs: `staging.publisher.mystira.app`, etc.

---

## Phase 5: Deploy Production Environment

Repeat Phase 3 steps with:
- Directory: `infra/terraform/environments/prod`
- Namespace: `mys-prod`
- URLs: `publisher.mystira.app`, etc.

**Additional production considerations:**
- [ ] Enable Azure Front Door (WAF, CDN, DDoS protection)
- [ ] Configure horizontal pod autoscaling
- [ ] Set up monitoring alerts
- [ ] Configure backup policies

---

## Quick Reference: What's Missing

### Azure Resources (Not Created)
- [ ] Terraform state storage account
- [ ] Resource Groups (dev, staging, prod)
- [ ] Virtual Networks
- [ ] AKS Clusters
- [ ] Azure Container Registry
- [ ] PostgreSQL servers
- [ ] Redis caches
- [ ] Key Vaults
- [ ] Azure DNS Zone

### Configuration (Not Set)
- [ ] GitHub secrets (AZURE_CREDENTIALS, ACR_*)
- [ ] Domain DNS records (A records pointing to Load Balancers)
- [ ] SSL certificates (auto-generated by cert-manager after DNS is configured)

### Application Deployment
- [ ] Docker images built and pushed to ACR
- [ ] Kubernetes deployments applied
- [ ] Environment variables configured

---

## Estimated Costs

| Environment | Monthly Cost |
|-------------|--------------|
| Dev | $150-300 |
| Staging | $200-400 |
| Production | $400-800 |
| **Total** | **$750-1,500** |

*Costs vary based on usage. Enable Azure Cost Management alerts.*

---

## Troubleshooting

### "Terraform backend error"
Run the Terraform Backend Setup (Phase 2) first.

### "AKS cluster not found"
Run `terraform apply` to create the cluster first.

### "DNS not resolving"
1. Verify DNS records are created
2. Wait 5-10 minutes for DNS propagation
3. Check with: `nslookup dev.publisher.mystira.app`

### "Certificate not issued"
1. Verify DNS is resolving correctly
2. Check cert-manager logs: `kubectl logs -n cert-manager -l app=cert-manager`
3. Check certificate status: `kubectl describe certificate -n mys-dev`

### "Image pull errors"
1. Verify ACR login: `az acr login --name mysprodacr`
2. Check image exists: `az acr repository list --name mysprodacr`
3. Verify Kubernetes secret for ACR: `kubectl get secret -n mys-dev`

---

## Next Steps After Deployment

1. [ ] Set up monitoring dashboards (Azure Monitor, Application Insights)
2. [ ] Configure backup policies for databases
3. [ ] Set up CI/CD pipelines (should work automatically via GitHub Actions)
4. [ ] Review and enable Azure Front Door for production
5. [ ] Configure alert rules for critical metrics
6. [ ] Document runbooks for common operations
