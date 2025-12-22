# Deployment Types Guide

This guide covers the different deployment options available in the Mystira platform.

## Table of Contents

1. [Deployment Overview](#deployment-overview)
2. [Azure Kubernetes Service (AKS)](#azure-kubernetes-service-aks)
3. [Azure App Service](#azure-app-service)
4. [Azure Static Web Apps](#azure-static-web-apps)
5. [Container Registry](#container-registry)
6. [Comparison Matrix](#comparison-matrix)
7. [Choosing the Right Option](#choosing-the-right-option)

## Deployment Overview

Mystira uses multiple deployment strategies based on service requirements:

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          Mystira Deployment Architecture                         │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────┐ │
│  │                    Azure Kubernetes Service (AKS)                           │ │
│  │                                                                             │ │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐          │ │
│  │  │ Admin API   │ │ Publisher   │ │ Story Gen   │ │ Chain       │          │ │
│  │  │ (.NET 9)    │ │ (Node.js)   │ │ (.NET 9)    │ │ (Python)    │          │ │
│  │  │             │ │             │ │             │ │             │          │ │
│  │  │ Deployment  │ │ Deployment  │ │ Deployment  │ │ StatefulSet │          │ │
│  │  │ + Service   │ │ + Service   │ │ + Service   │ │ + Service   │          │ │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘          │ │
│  │                                                                             │ │
│  │  Workload Identity │ HPA │ Ingress (NGINX) │ Network Policies              │ │
│  └────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────┐ │
│  │                       Azure App Service                                     │ │
│  │                                                                             │ │
│  │  ┌─────────────────────────────────────────────────────────────────────┐   │ │
│  │  │  Mystira.App API (.NET 9)                                           │   │ │
│  │  │  • Linux App Service Plan                                            │   │ │
│  │  │  • System-Assigned Managed Identity                                  │   │ │
│  │  │  • Key Vault Integration                                             │   │ │
│  │  │  • Custom Domain + SSL                                               │   │ │
│  │  └─────────────────────────────────────────────────────────────────────┘   │ │
│  └────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────┐ │
│  │                     Azure Static Web Apps                                   │ │
│  │                                                                             │ │
│  │  ┌─────────────┐ ┌─────────────┐                                          │ │
│  │  │ Admin UI    │ │ PWA         │                                          │ │
│  │  │ (React)     │ │ (Blazor)    │                                          │ │
│  │  │             │ │             │                                          │ │
│  │  │ GitHub      │ │ GitHub      │                                          │ │
│  │  │ Integration │ │ Integration │                                          │ │
│  │  └─────────────┘ └─────────────┘                                          │ │
│  └────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

## Azure Kubernetes Service (AKS)

### Overview

AKS hosts the core microservices that require:
- Container orchestration
- Auto-scaling
- Service mesh capabilities
- Workload identity integration

### Services Deployed to AKS

| Service | Language | Workload Type | Replicas |
|---------|----------|---------------|----------|
| Admin API | .NET 9 | Deployment | 1-3 |
| Story Generator | .NET 9 | Deployment | 1-5 |
| Publisher | Node.js | Deployment | 1-5 |
| Chain | Python | StatefulSet | 1-3 |

### AKS Configuration

```hcl
# environments/dev/main.tf
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mys-dev-core-aks-san"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = "mys-dev-core"

  default_node_pool {
    name           = "default"
    node_count     = 2
    vm_size        = "Standard_D2s_v3"
    vnet_subnet_id = azurerm_subnet.aks.id
  }

  identity {
    type = "SystemAssigned"
  }

  # Enable OIDC issuer for workload identity
  oidc_issuer_enabled       = true
  workload_identity_enabled = true

  network_profile {
    network_plugin = "azure"
    network_policy = "calico"
    service_cidr   = "172.16.0.0/16"
    dns_service_ip = "172.16.0.10"
  }
}
```

### Production AKS Configuration

```hcl
# environments/prod/main.tf
resource "azurerm_kubernetes_cluster" "main" {
  name                = "mys-prod-core-aks-san"
  kubernetes_version  = "1.28"

  default_node_pool {
    name                = "system"
    node_count          = 3
    vm_size             = "Standard_D4s_v3"
    vnet_subnet_id      = azurerm_subnet.aks.id
    enable_auto_scaling = true
    min_count           = 3
    max_count           = 10
    zones               = ["1", "2", "3"]  # Availability zones
  }

  azure_active_directory_role_based_access_control {
    managed            = true
    azure_rbac_enabled = true
  }
}

# Dedicated node pool for Chain workloads
resource "azurerm_kubernetes_cluster_node_pool" "chain" {
  name                  = "chain"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = "Standard_D4s_v3"
  node_count            = 3
  enable_auto_scaling   = true
  min_count             = 3
  max_count             = 6
  zones                 = ["1", "2", "3"]

  node_labels = {
    "workload" = "chain"
  }

  node_taints = [
    "workload=chain:NoSchedule"
  ]
}
```

### Kubernetes Manifests

**Namespace:**
```yaml
# infra/kubernetes/base/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: mystira
```

**ServiceAccount with Workload Identity:**
```yaml
# infra/kubernetes/base/service-accounts.yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: admin-api-sa
  namespace: mystira
  labels:
    azure.workload.identity/use: "true"
  annotations:
    azure.workload.identity/client-id: "${ADMIN_API_CLIENT_ID}"
```

**Deployment Example:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: admin-api
  namespace: mystira
spec:
  replicas: 2
  selector:
    matchLabels:
      app: admin-api
  template:
    metadata:
      labels:
        app: admin-api
        azure.workload.identity/use: "true"
    spec:
      serviceAccountName: admin-api-sa
      containers:
        - name: admin-api
          image: myssharedacr.azurecr.io/admin-api:latest
          ports:
            - containerPort: 8080
          resources:
            requests:
              memory: "256Mi"
              cpu: "250m"
            limits:
              memory: "512Mi"
              cpu: "500m"
          livenessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 30
            periodSeconds: 10
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 8080
            initialDelaySeconds: 5
            periodSeconds: 5
```

**Key Files:**
- AKS Config: [`environments/dev/main.tf`](../../infra/terraform/environments/dev/main.tf)
- K8s Manifests: [`infra/kubernetes/`](../../infra/kubernetes/)
- ServiceAccounts: [`infra/kubernetes/base/service-accounts.yaml`](../../infra/kubernetes/base/service-accounts.yaml)

## Azure App Service

### Overview

App Service is used for the Mystira.App component, which is a monolithic .NET application with simpler deployment requirements.

### Resources Created

| Resource | Type | Purpose |
|----------|------|---------|
| App Service Plan | Linux | Compute hosting |
| Linux Web App | .NET 9 | API backend |
| Key Vault | Secrets | Configuration and secrets |
| Application Insights | Monitoring | Telemetry |

### Configuration

```hcl
# modules/mystira-app/main.tf
resource "azurerm_service_plan" "main" {
  name                = "${local.name_prefix}-asp-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.app_service_sku  # B1, B2, P1v2, P2v2, etc.
}

resource "azurerm_linux_web_app" "api" {
  name                = "${local.name_prefix}-api-${local.region_code}"
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.main.id

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "9.0"
    }
    always_on = true
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"                = "Production"
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.main.connection_string

    # Key Vault references (passwordless)
    "ConnectionStrings__CosmosDb" = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault.main.vault_uri}secrets/cosmos-connection-string/)"
  }
}
```

### App Service SKUs

| SKU | vCPU | Memory | Use Case | Cost |
|-----|------|--------|----------|------|
| F1 | Shared | 1 GB | Free tier, testing | Free |
| B1 | 1 | 1.75 GB | Development | ~$13/month |
| B2 | 2 | 3.5 GB | Light production | ~$26/month |
| P1v2 | 1 | 3.5 GB | Production | ~$73/month |
| P2v2 | 2 | 7 GB | High traffic | ~$146/month |

### Key Vault Integration

App Service uses managed identity for passwordless Key Vault access:

```hcl
# Grant App Service access to Key Vault
resource "azurerm_key_vault_access_policy" "app_service" {
  key_vault_id = azurerm_key_vault.main.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_web_app.api.identity[0].principal_id

  secret_permissions = ["Get", "List"]
}
```

**Key Files:**
- Mystira App Module: [`modules/mystira-app/`](../../infra/terraform/modules/mystira-app/)
- Dev Config: [`environments/dev/mystira-app.tf`](../../infra/terraform/environments/dev/mystira-app.tf)

## Azure Static Web Apps

### Overview

Static Web Apps host frontend applications (React, Blazor WASM) with global CDN distribution.

### Features

- **Global CDN**: Content served from edge locations
- **Free SSL**: Automatic HTTPS certificates
- **GitHub Integration**: Direct deployment from repository
- **API Backend**: Optional Azure Functions integration
- **Custom Domains**: Supports custom domain names

### Configuration

```hcl
# modules/mystira-app/main.tf
resource "azurerm_static_web_app" "main" {
  name                = "${local.name_prefix}-swa-${local.region_code}"
  location            = "westus2"  # Not available in all regions
  resource_group_name = var.resource_group_name
  sku_tier            = "Standard"  # or "Free"
  sku_size            = "Standard"
}

# Custom domain
resource "azurerm_static_web_app_custom_domain" "main" {
  static_web_app_id = azurerm_static_web_app.main.id
  domain_name       = "app.mystira.app"
  validation_type   = "cname-delegation"
}
```

### Static Web App SKUs

| SKU | Features | Cost |
|-----|----------|------|
| Free | 100 GB bandwidth, 2 custom domains | Free |
| Standard | 100 GB bandwidth, 5 custom domains, Auth | ~$9/month |

### Deployment via GitHub Actions

```yaml
# .github/workflows/azure-static-web-apps.yml
name: Deploy Static Web App

on:
  push:
    branches: [main]
    paths:
      - 'packages/admin-ui/**'

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Build and Deploy
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "packages/admin-ui"
          output_location: "dist"
```

## Container Registry

### Azure Container Registry (ACR)

All container images are stored in a shared ACR:

```hcl
# environments/dev/main.tf
resource "azurerm_container_registry" "shared" {
  name                = "myssharedacr"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"  # or "Standard", "Premium"
  admin_enabled       = false
}
```

### Image Tagging Strategy

| Environment | Tag Pattern | Example |
|-------------|-------------|---------|
| Dev | `dev-<sha>` | `admin-api:dev-abc1234` |
| Staging | `staging-<sha>` | `admin-api:staging-abc1234` |
| Prod | `v<version>`, `latest` | `admin-api:v1.2.3` |

### ACR Access

**AKS to ACR (Pull):**
```hcl
# modules/shared/identity/main.tf
resource "azurerm_role_assignment" "aks_acr_pull" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = var.aks_principal_id
}
```

**CI/CD to ACR (Push):**
```hcl
resource "azurerm_role_assignment" "cicd_acr_push" {
  scope                = var.acr_id
  role_definition_name = "AcrPush"
  principal_id         = var.cicd_principal_id
}
```

### Building and Pushing Images

```bash
# Login to ACR
az acr login --name myssharedacr

# Build image
docker build -t myssharedacr.azurecr.io/admin-api:dev-$(git rev-parse --short HEAD) .

# Push image
docker push myssharedacr.azurecr.io/admin-api:dev-$(git rev-parse --short HEAD)
```

**Key Files:**
- ACR Config: [`environments/dev/main.tf`](../../infra/terraform/environments/dev/main.tf)
- Identity Module: [`modules/shared/identity/`](../../infra/terraform/modules/shared/identity/)

## Comparison Matrix

| Feature | AKS | App Service | Static Web App |
|---------|-----|-------------|----------------|
| **Best For** | Microservices | Monolithic apps | SPAs, Static sites |
| **Scaling** | HPA, Cluster autoscaler | Manual/Auto scale rules | Automatic |
| **Cost** | Higher (VMs) | Medium | Low/Free |
| **Complexity** | High | Medium | Low |
| **Workload Identity** | Yes | Yes (System-assigned) | No |
| **Custom Domains** | Via Ingress | Yes | Yes |
| **SSL** | Cert-manager/Front Door | Managed | Managed |
| **Container Support** | Native | Yes (Web App for Containers) | No |
| **Stateful Workloads** | StatefulSet | Limited | No |
| **Multi-language** | Any | Limited to supported stacks | Any (static) |

## Choosing the Right Option

### Use AKS When:

- Running multiple microservices
- Need container orchestration
- Require horizontal pod autoscaling
- Running stateful workloads (databases, blockchain)
- Need service mesh capabilities
- Require workload identity for Azure resources

### Use App Service When:

- Single application deployment
- Simpler operational requirements
- Built-in scaling is sufficient
- Don't need container orchestration
- Want managed platform updates

### Use Static Web Apps When:

- Frontend-only applications (React, Vue, Blazor WASM)
- Static site generation (Hugo, Jekyll)
- Need global CDN distribution
- Want GitHub-integrated deployments
- Cost optimization for static content

## Quick Reference

### Deployment Commands

**AKS:**
```bash
# Get credentials
az aks get-credentials -n mys-dev-core-aks-san -g mys-dev-core-rg-san

# Deploy manifests
kubectl apply -k infra/kubernetes/base/

# Check pods
kubectl get pods -n mystira
```

**App Service:**
```bash
# Deploy via Azure CLI
az webapp deployment source config-zip \
  -n mys-dev-app-api-san \
  -g mys-dev-core-rg-san \
  --src app.zip

# View logs
az webapp log tail -n mys-dev-app-api-san -g mys-dev-core-rg-san
```

**Container Registry:**
```bash
# List repositories
az acr repository list -n myssharedacr -o table

# List tags
az acr repository show-tags -n myssharedacr --repository admin-api -o table
```

## Related Documentation

- [Kubernetes README](../../infra/kubernetes/README.md)
- [Authentication Guide](./authentication-authorization-guide.md)
- [Networking Guide](./networking-guide.md)
- [Mystira App Module](../../infra/terraform/modules/mystira-app/README.md)
- [ADR-0001: Infrastructure Organization](../architecture/adr/0001-infrastructure-organization-hybrid-approach.md)
- [Azure AKS Docs](https://learn.microsoft.com/en-us/azure/aks/)
- [Azure App Service Docs](https://learn.microsoft.com/en-us/azure/app-service/)
- [Azure Static Web Apps Docs](https://learn.microsoft.com/en-us/azure/static-web-apps/)
