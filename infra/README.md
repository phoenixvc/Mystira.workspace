# Mystira Infrastructure

Infrastructure as Code, Kubernetes manifests, and deployment configurations for the Mystira platform.

## Overview

This directory contains all infrastructure components for the Mystira platform:

- **Terraform** - Cloud resource provisioning (Azure AKS, networking, databases)
- **Kubernetes** - Container orchestration and service deployment
- **Docker** - Containerization for all microservices
- **Scripts** - Deployment automation and utilities

## Infrastructure Tools: Terragrunt vs Harness GitOps

Mystira uses a **two-tier deployment model** with complementary tools for different layers:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         MYSTIRA DEPLOYMENT STACK                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                    APPLICATION LAYER (Harness GitOps)                 │  │
│  │  • Kubernetes Deployments, Services, ConfigMaps                       │  │
│  │  • Application rollouts and rollbacks                                 │  │
│  │  • GitOps sync from Git → Kubernetes                                  │  │
│  │  • Canary/Blue-Green deployments                                      │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                    ▲                                        │
│                                    │ deploys to                             │
│                                    │                                        │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                   INFRASTRUCTURE LAYER (Terragrunt)                   │  │
│  │  • AKS clusters, PostgreSQL, Redis, Storage                          │  │
│  │  • Virtual Networks, Subnets, NSGs                                    │  │
│  │  • Azure AI, Key Vault, Service Bus                                   │  │
│  │  • DNS zones, SSL certificates infrastructure                         │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Tool Comparison

| Aspect | Terragrunt/Terraform | Harness GitOps |
|--------|---------------------|----------------|
| **What it manages** | Cloud resources (Azure) | Application deployments (K8s) |
| **State** | Terraform state files | Git as source of truth |
| **Change frequency** | Infrequent (infra changes) | Frequent (app releases) |
| **Rollback** | `terraform apply` previous state | Git revert + sync |
| **Trigger** | CI/CD workflow or manual | Automatic on Git push |
| **Config location** | `infra/terraform/` | `infra/kubernetes/`, `infra/gitops/` |

### Why Two Tools? (Not Overlap)

**Terragrunt (Infrastructure)**:
- Provisions the "where" - creates AKS cluster, databases, networking
- Runs infrequently (when infrastructure changes)
- Changes require careful planning (destroy = downtime)
- CI/CD triggered: `.github/workflows/infra-*.yml`

**Harness GitOps (Applications)**:
- Deploys the "what" - your .NET apps to the infrastructure
- Runs continuously (watches Git, auto-syncs)
- Changes are safe (rolling updates, instant rollback)
- GitOps triggered: Push to `infra/kubernetes/` or `infra/gitops/applications/`

### Typical Workflow

1. **Initial Setup** (once): Terragrunt creates AKS, PostgreSQL, Redis
2. **App Development** (daily): Code changes trigger image builds
3. **App Deployment** (continuous): Harness syncs K8s manifests from Git
4. **Infrastructure Updates** (rare): Terragrunt modifies cloud resources

### When to Use Each

| Task | Tool |
|------|------|
| Add a new Azure database | Terragrunt |
| Deploy new app version | Harness GitOps |
| Scale AKS node pool | Terragrunt |
| Scale app replicas | Harness GitOps (HPA or manifest) |
| Add new subnet/NSG | Terragrunt |
| Add new K8s Service | Harness GitOps |
| Change PostgreSQL SKU | Terragrunt |
| Update app ConfigMap | Harness GitOps |

### Related Documentation

- [GitOps Setup](./gitops/README.md) - Harness agent installation and app definitions
- [Terraform Structure](./terraform/README.md) - Module organization and environments

## Structure

```
infra/
├── terraform/              # Infrastructure as Code
│   ├── modules/           # Reusable Terraform modules
│   │   ├── shared/       # Shared resources (PostgreSQL, Redis, ACR)
│   │   ├── chain/        # Chain service module
│   │   ├── publisher/    # Publisher service module
│   │   ├── story-generator/  # Story Generator module
│   │   └── front-door/   # Azure Front Door (CDN, WAF)
│   └── environments/      # Environment configurations
│       ├── dev/
│       ├── staging/
│       └── prod/
│
├── kubernetes/             # Kubernetes manifests (Kustomize)
│   ├── base/              # Base configurations
│   │   ├── cert-manager/
│   │   ├── chain/
│   │   ├── publisher/
│   │   └── story-generator/
│   └── overlays/          # Environment-specific overlays
│       ├── dev/
│       ├── staging/
│       └── prod/
│
├── docker/                 # Dockerfiles for all services
│   ├── chain/
│   ├── publisher/
│   └── story-generator/
│
└── scripts/                # Deployment and utility scripts
    └── README.md          # Script documentation
```

## Quick Start

### Prerequisites

- **Terraform** >= 1.5.0
- **kubectl** - Kubernetes CLI
- **Azure CLI** (`az`) - For Azure deployments
- **Docker** - For local builds
- **GitHub CLI** (`gh`) - For workflow triggers

### Azure Setup

Configure Azure service principal with required permissions:

```bash
# Login to Azure
az login

# Create service principal (if not exists)
az ad sp create-for-rbac \
  --name "mystira-terraform" \
  --role Contributor \
  --scopes /subscriptions/<SUBSCRIPTION_ID>

# Set GitHub secrets (requires gh CLI)
gh secret set MYSTIRA_AZURE_CREDENTIALS --body '{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "..."
}'
```

**Important:** The service principal needs additional permissions beyond Contributor:

| Permission | Purpose |
|------------|---------|
| **User Access Administrator** | Assign RBAC roles to managed identities |
| **Application.ReadWrite.All** | Manage app registrations (Entra External ID) |
| **DelegatedPermissionGrant.ReadWrite.All** | Manage OAuth2 permission grants |
| **Storage Blob Data Contributor** | Access Terraform state storage |

The deployment workflow includes automated permission validation. For complete setup instructions, see [Azure Setup Guide](./azure-setup.md#step-2-required-permissions).

## Deployment

### Using GitHub Actions (Recommended)

Deploy infrastructure using the CI/CD workflows:

```bash
# Deploy to dev environment
gh workflow run "Infrastructure: Deploy" \
  --field environment=dev \
  --field components=all

# Deploy to staging (automatic on main push)
git push origin main

# Deploy to production (manual, requires confirmation)
gh workflow run "Deployment: Production" \
  --field confirm="DEPLOY TO PRODUCTION"
```

### Manual Deployment

For local development or troubleshooting:

```bash
# Initialize Terraform
cd terraform/environments/dev
terraform init

# Plan infrastructure changes
terraform plan -out=tfplan

# Apply changes
terraform apply tfplan

# Deploy Kubernetes manifests
kubectl apply -k ../../kubernetes/overlays/dev
```

## Environments

| Environment | Domain                  | Deployment          | Branch |
| ----------- | ----------------------- | ------------------- | ------ |
| Development | `*.dev.mystira.app`     | Manual              | `dev`  |
| Staging     | `*.staging.mystira.app` | Auto (on main push) | `main` |
| Production  | `*.mystira.app`         | Manual (protected)  | `main` |

### Service URLs

| Service           | Dev                               | Staging                               | Production                    |
| ----------------- | --------------------------------- | ------------------------------------- | ----------------------------- |
| Publisher         | `dev.publisher.mystira.app`       | `staging.publisher.mystira.app`       | `publisher.mystira.app`       |
| Chain             | `dev.chain.mystira.app`           | `staging.chain.mystira.app`           | `chain.mystira.app`           |
| Story Generator   | `dev.story-api.mystira.app`       | `staging.story-api.mystira.app`       | `story-api.mystira.app`       |
| Story Web (SWA)   | `dev.story.mystira.app`           | `staging.story.mystira.app`           | `story.mystira.app`           |

## Infrastructure Components

### Azure Resources (Terraform)

- **AKS** - Azure Kubernetes Service clusters
- **Virtual Network** - Network isolation and security
- **Azure Container Registry** - Docker image registry (shared)
- **PostgreSQL** - Managed database (shared)
- **Redis Cache** - Distributed caching (shared)
- **Azure Front Door** - CDN, WAF, DDoS protection
- **Azure DNS** - DNS zone and record management
- **Monitoring** - Application Insights and Log Analytics

### Kubernetes Deployments

All services are deployed to AKS using Kustomize:

```bash
# Lint and validate manifests
kubectl kustomize kubernetes/overlays/dev --enable-helm

# Deploy to environment
kubectl apply -k kubernetes/overlays/dev

# Check deployment status
kubectl get pods -n mys-dev
kubectl get ingress -n mys-dev
```

### SSL/TLS Certificates

Automated certificate management with cert-manager:

- **Provider**: Let's Encrypt (production)
- **Challenge**: HTTP-01 validation
- **Auto-renewal**: Certificates renew automatically
- **Wildcard support**: No (uses individual certs per service)

Cert-manager is deployed automatically via `Infrastructure: Deploy` workflow.

### Docker Images

Images are built and pushed automatically via CI/CD:

- **Registry**: `myssharedacr.azurecr.io`
- **Tags**: `dev`, `staging`, `prod`, `${SHA}`
- **Build**: Automated on push to `dev`/`main` branches

Manual build and push:

```bash
# Build image
docker build -t myssharedacr.azurecr.io/chain:dev -f docker/chain/Dockerfile .

# Login to ACR
az acr login --name myssharedacr

# Push image
docker push myssharedacr.azurecr.io/chain:dev
```

## DNS Configuration

DNS is managed via Azure DNS:

1. **DNS Zone**: `mystira.app` (created by Terraform)
2. **Name Servers**: Retrieved from Azure DNS
3. **A Records**: Auto-created for each service pointing to ingress IP

**Setup Steps:**

```bash
# Get DNS name servers
az network dns zone show \
  --name mystira.app \
  --resource-group mys-prod-core-rg-glob \
  --query nameServers -o tsv

# Update your domain registrar with these name servers
# Records are created automatically by the infrastructure workflow
```

See [Quick Access Commands](./quick-access.md) for quick reference commands.

## CI/CD Pipeline

Infrastructure workflows are fully automated:

### Workflows

- **Infrastructure: Deploy** - Deploy Terraform + Kubernetes
- **Infrastructure: Validate** - Validate configs on PRs
- **Deployment: Staging** - Auto-deploy to staging
- **Deployment: Production** - Manual prod deployment

### Pipeline Stages

```
┌────────────┐    ┌────────────┐    ┌────────────┐    ┌────────────┐
│  Validate  │───▶│  Terraform │───▶│   Build    │───▶│   Deploy   │
│  Configs   │    │   Plan     │    │   Images   │    │    K8s     │
└────────────┘    └────────────┘    └────────────┘    └────────────┘
      │                  │                  │                 │
      ▼                  ▼                  ▼                 ▼
┌────────────┐    ┌────────────┐    ┌────────────┐    ┌────────────┐
│  Security  │    │  Terraform │    │    Push    │    │ Configure  │
│   Scan     │    │   Apply    │    │ to Registry│    │    DNS     │
└────────────┘    └────────────┘    └────────────┘    └────────────┘
```

## Monitoring & Observability

- **Application Insights** - Application performance monitoring
- **Log Analytics** - Centralized logging
- **Prometheus** - Metrics collection (planned)
- **Grafana** - Dashboards (planned)

Access monitoring:

```bash
# View logs
kubectl logs -n mys-prod deployment/mys-publisher

# Check metrics in Azure Portal
az monitor app-insights component show \
  --app mystira-prod-appinsights \
  --resource-group mys-prod-core-rg-eus
```

## Security

- **Secrets**: Azure Key Vault + Kubernetes secrets
- **Network**: Virtual network isolation, network policies
- **RBAC**: Kubernetes role-based access control
- **SSL/TLS**: All traffic encrypted via cert-manager
- **WAF**: Azure Front Door Web Application Firewall
- **DDoS**: Azure DDoS Protection Standard

## Troubleshooting

### Common Issues

**Certificate not issuing:**

```bash
# Check cert-manager logs
kubectl logs -n cert-manager deployment/cert-manager

# Check certificate status
kubectl describe certificate -n mys-dev
```

**Pods not starting:**

```bash
# Check pod status
kubectl describe pod <pod-name> -n mys-dev

# Check logs
kubectl logs <pod-name> -n mys-dev

# Check events
kubectl get events -n mys-dev --sort-by='.lastTimestamp'
```

**DNS not resolving:**

```bash
# Check DNS records
az network dns record-set a list \
  --resource-group mys-prod-core-rg-glob \
  --zone-name mystira.app

# Test DNS resolution
dig publisher.mystira.app
```

For more troubleshooting tips, see [docs/infrastructure/troubleshooting-kubernetes-center.md](../docs/infrastructure/troubleshooting-kubernetes-center.md).

## Scripts

See [scripts/README.md](./scripts/README.md) for available automation scripts.

## Documentation

- [Azure Setup Guide](./azure-setup.md) - Configure Azure service principal
- [Quick Access Commands](./quick-access.md) - Common commands reference
- [Infrastructure Docs](../docs/infrastructure/) - Detailed infrastructure guides
- [Kubernetes Secrets Management](../docs/infrastructure/kubernetes-secrets-management.md)
- [SSL Certificates Guide](../docs/infrastructure/ssl-certificates-guide.md)

## License

Proprietary - All rights reserved by Phoenix VC / Mystira
