# Infrastructure Guide

This guide explains the infrastructure organization, deployment models, and coordination between services in the Mystira workspace.

## Overview

The Mystira workspace uses a **hybrid infrastructure approach** with infrastructure code located in multiple places, each optimized for the specific deployment model of the services:

```
Mystira.workspace/
├── infra/                        # Containerized services (Kubernetes)
│   ├── terraform/               # Infrastructure as Code (Terraform)
│   │   ├── modules/
│   │   │   ├── chain/          # Chain service infrastructure
│   │   │   ├── publisher/      # Publisher service infrastructure
│   │   │   ├── story-generator/# Story-Generator service infrastructure
│   │   │   ├── dns/            # DNS zone management
│   │   │   └── shared/         # Shared infrastructure modules
│   │   │       ├── postgresql/ # Shared PostgreSQL database
│   │   │       ├── redis/      # Shared Redis cache
│   │   │       └── monitoring/ # Shared monitoring/logging
│   │   └── environments/       # Environment-specific configs
│   ├── kubernetes/              # Kubernetes manifests
│   │   ├── base/               # Base configurations
│   │   └── overlays/           # Environment overlays
│   └── docker/                  # Dockerfiles
│
└── packages/app/
    └── infrastructure/           # Azure PaaS services (Bicep)
        ├── main.bicep          # Main orchestration template
        ├── modules/            # Bicep modules
        └── params.*.json       # Environment parameters
```

## Infrastructure Organization

### 1. Containerized Services (`infra/`)

**Location**: `infra/`  
**Tool**: Terraform + Kubernetes  
**Services**: Chain, Publisher

#### Components

- **Terraform Modules** (`infra/terraform/modules/`):
  - `chain/` - Chain service infrastructure (VMs, networking, storage)
  - `publisher/` - Publisher service infrastructure
  - `story-generator/` - Story-Generator service infrastructure
  - `dns/` - DNS zone and record management
  - `shared/` - Shared infrastructure modules:
    - `postgresql/` - Shared PostgreSQL database
    - `redis/` - Shared Redis cache
    - `monitoring/` - Shared monitoring and logging

- **Kubernetes Manifests** (`infra/kubernetes/`):
  - Base configurations for each service
  - Environment-specific overlays (dev, staging, prod)
  - Cert-Manager for SSL/TLS

- **Dockerfiles** (`infra/docker/`):
  - Container images for Chain and Publisher services

#### Deployment Model

```
Code → Docker Image → Azure Container Registry → Kubernetes (AKS) → Service
```

#### Deployment Workflow

- CI/CD: `.github/workflows/chain-ci.yml` and `publisher-ci.yml`
- Infrastructure: `.github/workflows/infra-deploy.yml`
- Orchestration: Kubernetes (StatefulSets, Deployments, Services, Ingress)

### 2. Azure PaaS Services (`packages/app/infrastructure/`)

**Location**: `packages/app/infrastructure/`  
**Tool**: Azure Bicep  
**Services**: App (API, Admin API, PWA)

#### Components

- **Main Template** (`main.bicep`):
  - Orchestrates all App infrastructure
  - Environment-agnostic template

- **Modules** (`modules/`):
  - `app-service.bicep` - Azure App Service configuration
  - `cosmos-db.bicep` - Cosmos DB setup
  - `storage.bicep` - Azure Blob Storage
  - `key-vault.bicep` - Secrets management
  - `static-web-app.bicep` - PWA hosting
  - `log-analytics.bicep` - Monitoring
  - `application-insights.bicep` - APM
  - Plus: DNS, bots, communication services, alerts, dashboards

- **Parameters** (`params.*.json`):
  - `params.dev.json` - Development environment
  - `params.staging.json` - Staging environment
  - `params.prod.json` - Production environment

#### Deployment Model

```
Code → Azure App Service / Static Web App → Service
```

#### Deployment Workflow

- CI/CD: `.github/workflows/mystira-app-*-cicd-*.yml` (in `packages/app/.github/workflows/`)
- Infrastructure: `.github/workflows/infrastructure-deploy-*.yml`
- Services: Azure PaaS (App Services, Cosmos DB, Storage, etc.)

## Service Details

### Chain Service

**Infrastructure**: `infra/terraform/modules/chain/`  
**Deployment**: Kubernetes (AKS)  
**Resources**:

- Virtual Machines (for chain nodes)
- Network Security Groups
- Storage (for chain data)
- Kubernetes StatefulSet

**Access**:

- Internal: Kubernetes service
- External: Ingress with DNS (`chain.mystira.app`)

### Publisher Service

**Infrastructure**: `infra/terraform/modules/publisher/`  
**Deployment**: Kubernetes (AKS)  
**Resources**:

- Kubernetes Deployment
- Service and Ingress
- ConfigMaps and Secrets

**Access**:

- Internal: Kubernetes service
- External: Ingress with DNS (`publisher.mystira.app`)

### App Services

**Infrastructure**: `packages/app/infrastructure/`  
**Deployment**: Azure PaaS  
**Resources**:

- **App Services**: API and Admin API hosting
- **Static Web App**: PWA hosting
- **Cosmos DB**: NoSQL database
- **Azure Storage**: Blob storage for media
- **Key Vault**: Secrets management
- **Application Insights**: Monitoring and APM
- **Log Analytics**: Centralized logging
- **Azure Communication Services**: Email/SMS/WhatsApp
- **Azure Bot**: Teams/Discord integration

**Access**:

- API: `api.{env}.mystira.app` or `api.mystira.app` (prod)
- Admin API: `admin.{env}.mystira.app` or `admin.mystira.app` (prod)
- PWA: `{env}.mystira.app` or `mystira.app` (prod)

### Story-Generator Service

**Infrastructure**: `infra/terraform/modules/story-generator/`  
**Deployment**: Kubernetes (AKS) or Azure PaaS  
**Resources**:

- PostgreSQL database (shared or dedicated)
- Redis cache (shared or dedicated)
- Kubernetes Deployment or App Service
- Application Insights integration
- Key Vault for secrets

**Access**:

- Internal: Kubernetes service (`mystira-story-generator`)
- External: Ingress with DNS (`story-generator.mystira.app`)
- Kubernetes manifests: `infra/kubernetes/base/story-generator/`

**Deployment**:

- Kubernetes Deployment with HorizontalPodAutoscaler
- Uses shared PostgreSQL and Redis resources
- Environment-specific configurations via Kustomize overlays

## Shared Resources

### DNS Management

**Location**: `infra/terraform/modules/dns/`  
**Domain**: `mystira.app`

**DNS Records**:

- `publisher.mystira.app` → Publisher service ingress
- `chain.mystira.app` → Chain service ingress
- `api.{env}.mystira.app` → App API service
- `admin.{env}.mystira.app` → App Admin API service
- `{env}.mystira.app` → App PWA (Static Web App)

**Coordination**:

- Central DNS module in `infra/terraform/modules/dns/`
- Both App Bicep and Terraform modules reference the same DNS zone
- DNS zone lives in separate resource group: `mys-prod-mystira-rg-glob`

### Networking

**Current State**:

- Chain/Publisher: VNet and subnets managed by Terraform
- App: Uses Azure PaaS (no explicit VNet configuration required)

**Future Consideration**:

- If App services need VNet integration, coordinate through shared VNet module

### Monitoring & Logging

**Shared Monitoring** (`infra/terraform/modules/shared/monitoring/`):

- Log Analytics Workspace for centralized logging
- Application Insights for APM
- Metric alerts and action groups

**App Services**:

- Application Insights + Log Analytics (via Bicep)
- Can integrate with shared monitoring workspace

**Chain/Publisher/Story-Generator**:

- Integrated with shared Log Analytics workspace
- Application Insights per service
- Unified monitoring dashboards

**Integration**: All services reference the shared monitoring module for consistent logging and observability.

### Secrets Management

**App Services**:

- Azure Key Vault (via Bicep)
- Stores: JWT keys, Discord tokens, Bot credentials, WhatsApp config

**Chain/Publisher**:

- Kubernetes Secrets
- Can reference Azure Key Vault via CSI driver (future enhancement)

**Coordination**: Consider cross-service secret access if needed

## Deployment Workflows

### Containerized Services (Chain/Publisher)

**Workflow**: `.github/workflows/infra-deploy.yml`

**Steps**:

1. Plan Terraform changes
2. Apply Terraform (if changes detected)
3. Build and push Docker images
4. Deploy to Kubernetes

**Trigger**:

- Push to `main` branch
- Manual workflow dispatch
- Path-based triggers for infrastructure changes

### App Services

**Infrastructure**: `.github/workflows/infrastructure-deploy-{env}.yml` (in `packages/app/.github/workflows/`)

**Code Deployment**:

- API: `.github/workflows/mystira-app-api-cicd-{env}.yml`
- Admin API: `.github/workflows/mystira-app-admin-api-cicd-{env}.yml`
- PWA: `.github/workflows/mystira-app-pwa-cicd-{env}.yml`

**Trigger**:

- Push to environment branches
- Manual workflow dispatch
- After infrastructure deployment

### Coordination

**Current State**: Independent deployments

**Future Enhancement**: Create workspace-level orchestration workflow that:

- Coordinates infrastructure deployments
- Ensures proper ordering (DNS → Infrastructure → Services)
- Handles cross-service dependencies

## Environments

### Development

**Resource Groups** (per [ADR-0008: Azure Resource Naming Conventions](../architecture/adr/0008-azure-resource-naming-conventions.md)):

- `mystira-dev-rg` (legacy, kept as-is per ADR-0008)
- `mys-dev-mystira-rg-euw` (new convention for future resources)

**Resources**:

- AKS cluster: `mystira-dev-aks` (legacy, kept as-is per ADR-0008)
- App Services: `mys-dev-mystira-api-euw`, `mys-dev-mystira-adminapi-euw` (new convention)
- Static Web App: `mys-dev-mystira-swa-euw` (new convention)
- Cosmos DB: `mys-dev-mystira-cosmos-euw` (new convention)

### Staging

**Resource Groups**:

- `mystira-staging-rg` (legacy, kept as-is per ADR-0008)
- `mys-staging-mystira-rg-euw` (new convention for future resources)

**Resources**: Similar structure with `staging` suffix

### Production

**Resource Groups**:

- `mystira-prod-rg` (legacy, kept as-is per ADR-0008)
- `mys-prod-mystira-rg-euw` (new convention for future resources)
- `mys-prod-mystira-rg-glob` (DNS zone, new convention)

**Resources**: Production-grade with higher SKUs and redundancy

**Note**: See [ADR-0008](../architecture/adr/0008-azure-resource-naming-conventions.md) for complete naming conventions and legacy resource mapping.

## Getting Started

### Prerequisites

**For Containerized Services (Terraform + K8s)**:

- Terraform >= 1.5
- kubectl
- Azure CLI
- Docker

**For App Services (Bicep)**:

- Azure CLI
- Bicep CLI (or use Azure CLI which includes Bicep)

### Initial Setup

1. **Authenticate with Azure**:

   ```bash
   az login
   az account set --subscription <subscription-id>
   ```

2. **Deploy Containerized Services**:

   ```bash
   cd infra/terraform/environments/dev
   terraform init
   terraform plan
   terraform apply
   ```

3. **Deploy App Infrastructure**:
   ```bash
   cd packages/app/infrastructure
   az deployment group create \
     --resource-group mys-dev-mystira-rg-san \
     --template-file main.bicep \
     --parameters @params.dev.json \
     --parameters jwtRsaPrivateKey="<key>" jwtRsaPublicKey="<key>"
   ```

### Manual Deployment

See individual README files:

- `infra/README.md` - Containerized services
- `packages/app/infrastructure/README.md` - App services

## Best Practices

### 1. Environment Parity

- Use the same infrastructure structure across environments
- Only vary SKUs, replica counts, and resource sizes
- Keep configuration differences minimal

### 2. Secrets Management

- Never commit secrets to repositories
- Use Azure Key Vault or GitHub Secrets
- Rotate secrets regularly
- Use different keys per environment

### 3. Infrastructure Changes

- Always run `terraform plan` or `az deployment group what-if` before applying
- Review changes in PR before merging
- Use preview workflows for validation
- Test in dev before staging/prod

### 4. Service Dependencies

- Document cross-service dependencies
- Use DNS names for service discovery
- Avoid hardcoding IPs or internal addresses
- Consider service mesh for complex microservices

### 5. Monitoring

- Set up alerts for all critical services
- Monitor resource utilization
- Track deployment success rates
- Review logs regularly

## Troubleshooting

### Common Issues

**1. DNS not resolving**

- Check DNS zone configuration
- Verify DNS records are created
- Check propagation (may take time)

**2. Services can't communicate**

- Verify network security groups (for VMs)
- Check Kubernetes service endpoints
- Review firewall rules

**3. Deployment failures**

- Check Azure quotas and limits
- Verify service principal permissions
- Review Terraform/Bicep error messages

**4. Secrets not accessible**

- Verify Key Vault access policies
- Check service principal permissions
- Ensure secrets are properly referenced

### Getting Help

- Check service-specific README files
- Review GitHub Actions workflow logs
- Consult Azure Portal for resource status
- See `infra/AZURE_SETUP.md` for Azure configuration

## Shared Infrastructure Modules

### Shared PostgreSQL (`infra/terraform/modules/shared/postgresql/`)

Provides a shared PostgreSQL Flexible Server for all services:

- Centralized database management
- Cost-efficient multi-tenant database hosting
- Configurable databases per service
- Private DNS zone and VNet integration

### Shared Redis (`infra/terraform/modules/shared/redis/`)

Provides a shared Redis cache for all services:

- Centralized caching layer
- Configurable capacity and SKU
- VNet integration for Premium tier
- TLS enforcement

### Shared Monitoring (`infra/terraform/modules/shared/monitoring/`)

Provides centralized monitoring and logging:

- Log Analytics Workspace
- Application Insights
- Metric alerts and action groups
- Unified observability across services

**Usage**: Services can reference these shared modules or use dedicated instances as needed.

## Future Improvements

1. **Shared Secrets**: Cross-service Key Vault access coordination
2. **Service Mesh**: Consider for advanced microservices communication
3. **Infrastructure as Code Consistency**: Further standardize patterns
4. **Coordination Workflows**: Workspace-level orchestration workflows
5. **Database Read Replicas**: Add read replica support for PostgreSQL

## Related Documentation

- [Architecture Overview](./ARCHITECTURE.md)
- [ADR-0001: Infrastructure Organization](./architecture/adr/0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0002: Documentation Location Strategy](./architecture/adr/0002-documentation-location-strategy.md)
- [ADR-0005: Service Networking and Communication](./architecture/adr/0005-service-networking-and-communication.md) - Network topology and service communication patterns
- [ADR-0006: Admin API Repository Extraction](./architecture/adr/0006-admin-api-repository-extraction.md) - Repository structure changes
- [Environment Variables](./ENVIRONMENT.md)
- [Azure Setup Guide](../infra/AZURE_SETUP.md)
- [App Infrastructure README](../packages/app/infrastructure/README.md)
- [Infra README](../infra/README.md)
