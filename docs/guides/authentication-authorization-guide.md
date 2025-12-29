# Authentication & Authorization Guide

This guide provides a comprehensive overview of authentication and authorization in the Mystira platform.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Authentication Tiers](#authentication-tiers)
3. [Infrastructure Components](#infrastructure-components)
4. [Service Configuration](#service-configuration)
5. [Database Authentication](#database-authentication)
6. [Kubernetes Workload Identity](#kubernetes-workload-identity)
7. [Quick Reference](#quick-reference)

## Architecture Overview

Mystira uses a multi-tier authentication strategy:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Authentication Architecture                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐   ┌──────────────────┐   ┌──────────────────────────┐ │
│  │   Admin Tier     │   │  Consumer Tier   │   │    Service Tier          │ │
│  ├──────────────────┤   ├──────────────────┤   ├──────────────────────────┤ │
│  │ • Admin API      │   │ • Public API     │   │ • Story Generator        │ │
│  │ • Admin UI       │   │ • PWA            │   │ • Publisher              │ │
│  │ • DevHub         │   │ • Publisher      │   │ • Chain                  │ │
│  ├──────────────────┤   ├──────────────────┤   ├──────────────────────────┤ │
│  │ Microsoft        │   │ Entra            │   │ Azure Workload Identity  │ │
│  │ Entra ID         │   │ External ID      │   │ (Managed Identity)       │ │
│  └──────────────────┘   └──────────────────┘   └──────────────────────────┘ │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │                        Azure Resources                                   │ │
│  │  Key Vault │ PostgreSQL │ ACR │ Service Bus │ Application Insights      │ │
│  │            (Azure AD Auth - No Passwords!)                               │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Authentication Tiers

### Tier 1: Admin Authentication (Entra ID)

For internal admin users with enterprise SSO, MFA, and conditional access.

| Component | Authentication Method | Identity Provider |
|-----------|----------------------|-------------------|
| Admin API | JWT Bearer Token | Microsoft Entra ID |
| Admin UI | MSAL (Browser) | Microsoft Entra ID |
| DevHub | JWT Bearer Token | Microsoft Entra ID |

**Key Files:**
- Terraform: [`infra/terraform/modules/entra-id/`](../../infra/terraform/modules/entra-id/)
- ADR: [`docs/architecture/adr/0011-entra-id-authentication-integration.md`](../architecture/adr/0011-entra-id-authentication-integration.md)

### Tier 2: Consumer Authentication (Microsoft Entra External ID)

For end users with social login (Google, Discord) and self-service registration.

| Component | Authentication Method | Identity Provider |
|-----------|----------------------|-------------------|
| PWA | MSAL (Browser) | Microsoft Entra External ID |
| Public API | JWT Bearer Token | Microsoft Entra External ID |

**Key Files:**
- Terraform: [`infra/terraform/modules/entra-external-id/`](../../infra/terraform/modules/entra-external-id/)
- ADR: [`docs/architecture/adr/0010-authentication-and-authorization-strategy.md`](../architecture/adr/0010-authentication-and-authorization-strategy.md)

### Tier 3: Service Authentication (Workload Identity)

For service-to-service and service-to-Azure-resource authentication.

| Service | Authentication Method | Target Resources |
|---------|----------------------|------------------|
| Admin API | Managed Identity | PostgreSQL, Key Vault |
| Story Generator | Managed Identity | PostgreSQL, Key Vault, Redis |
| Publisher | Managed Identity | Key Vault, Service Bus |
| Chain | Managed Identity | Key Vault |

**Key Files:**
- Terraform: [`infra/terraform/modules/shared/identity/`](../../infra/terraform/modules/shared/identity/)
- Kubernetes: [`infra/kubernetes/base/service-accounts.yaml`](../../infra/kubernetes/base/service-accounts.yaml)

## Infrastructure Components

### Terraform Modules

| Module | Purpose | Location |
|--------|---------|----------|
| `entra-id` | Admin app registrations, scopes, roles | [`modules/entra-id/`](../../infra/terraform/modules/entra-id/) |
| `entra-external-id` | Consumer auth with social login | [`modules/entra-external-id/`](../../infra/terraform/modules/entra-external-id/) |
| `admin-api` | Admin API managed identity | [`modules/admin-api/`](../../infra/terraform/modules/admin-api/) |
| `shared/identity` | RBAC, workload identity federation | [`modules/shared/identity/`](../../infra/terraform/modules/shared/identity/) |
| `shared/postgresql` | Database with Azure AD auth | [`modules/shared/postgresql/`](../../infra/terraform/modules/shared/postgresql/) |
| `front-door` | TLS/SSL certificates, WAF | [`modules/front-door/`](../../infra/terraform/modules/front-door/) |
| `dns` | DNS records for custom domains | [`modules/dns/`](../../infra/terraform/modules/dns/) |

### Kubernetes Resources

| Resource | Purpose | Location |
|----------|---------|----------|
| Namespace | `mystira` namespace | [`kubernetes/base/namespace.yaml`](../../infra/kubernetes/base/namespace.yaml) |
| ServiceAccounts | Workload identity for pods | [`kubernetes/base/service-accounts.yaml`](../../infra/kubernetes/base/service-accounts.yaml) |

## Service Configuration

### Admin API (.NET)

**appsettings.json:**
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<from-terraform-output>",
    "ClientId": "<from-terraform-output>",
    "Audience": "api://mystira-admin-api-dev"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=<server>.postgres.database.azure.com;Database=adminapi;Username=mys-dev-admin-api-identity-san;Ssl Mode=Require"
  }
}
```

**Program.cs:**
```csharp
// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("ContentModerator", policy =>
        policy.RequireClaim("extension_role", "Moderator", "Admin"));
});

// PostgreSQL with Azure AD auth
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.UseAzureADAuthentication(new DefaultAzureCredential());
    options.UseNpgsql(dataSourceBuilder.Build());
});
```

**Required NuGet Packages:**
```xml
<PackageReference Include="Microsoft.Identity.Web" Version="2.x" />
<PackageReference Include="Azure.Identity" Version="1.x" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.x" />
```

### Admin UI (React)

**authConfig.ts:**
```typescript
import { Configuration } from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_TENANT_ID}`,
    redirectUri: import.meta.env.VITE_REDIRECT_URI,
  },
  cache: {
    cacheLocation: 'sessionStorage',
  },
};

export const loginRequest = {
  scopes: ['api://mystira-admin-api-dev/Admin.Read'],
};
```

**Environment Variables:**
```bash
VITE_AZURE_CLIENT_ID=<admin-ui-client-id>
VITE_AZURE_TENANT_ID=<tenant-id>
VITE_REDIRECT_URI=https://dev.admin.mystira.app/auth/callback
```

### PWA (Consumer App)

**External ID Configuration:**
```typescript
export const externalIdConfig = {
  auth: {
    clientId: import.meta.env.VITE_EXTERNAL_ID_CLIENT_ID,
    authority: 'https://mystiradev.ciamlogin.com/<tenant-id>/v2.0',
    knownAuthorities: ['mystiradev.ciamlogin.com'],
    redirectUri: import.meta.env.VITE_REDIRECT_URI,
  },
};
```

## Database Authentication

### PostgreSQL Azure AD Authentication

Mystira uses Azure AD authentication for PostgreSQL, eliminating the need for password management.

**How It Works:**

```
┌─────────────┐    ┌──────────────┐    ┌─────────────────┐    ┌────────────┐
│   AKS Pod   │───▶│   Workload   │───▶│   Azure AD      │───▶│ PostgreSQL │
│             │    │   Identity   │    │   Token         │    │            │
│ DefaultAz.. │    │   Federation │    │   Exchange      │    │   (AAD)    │
└─────────────┘    └──────────────┘    └─────────────────┘    └────────────┘
```

1. Pod runs with ServiceAccount linked to Managed Identity
2. Azure SDK (DefaultAzureCredential) requests token from workload identity
3. Token is exchanged for PostgreSQL access token
4. Npgsql uses token to authenticate (no password!)

**Terraform Configuration:**
```hcl
module "shared_postgresql" {
  source = "../../modules/shared/postgresql"

  # Enable Azure AD authentication
  aad_auth_enabled = true
  aad_admin_identities = {
    "admin-api" = {
      principal_id   = module.admin_api.identity_principal_id
      principal_name = "mys-dev-admin-api-identity-san"
      principal_type = "ServicePrincipal"
    }
  }
}
```

**Connection String (no password):**
```
Host=mys-dev-core-db.postgres.database.azure.com;Database=adminapi;Username=mys-dev-admin-api-identity-san;Ssl Mode=Require
```

**Key Files:**
- PostgreSQL Module: [`modules/shared/postgresql/`](../../infra/terraform/modules/shared/postgresql/)
- PostgreSQL README: [`modules/shared/postgresql/README.md`](../../infra/terraform/modules/shared/postgresql/README.md)

## Kubernetes Workload Identity

### ServiceAccount Configuration

Each service has a dedicated ServiceAccount with workload identity annotations:

```yaml
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

### Deployment Configuration

Pods must reference the ServiceAccount and have the workload identity label:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: admin-api
  namespace: mystira
spec:
  template:
    metadata:
      labels:
        azure.workload.identity/use: "true"
    spec:
      serviceAccountName: admin-api-sa
      containers:
        - name: admin-api
          # Azure SDK automatically uses workload identity
```

### Available ServiceAccounts

| ServiceAccount | Service | Managed Identity |
|----------------|---------|------------------|
| `admin-api-sa` | Admin API | `mys-dev-admin-api-identity-san` |
| `story-generator-sa` | Story Generator | `mys-dev-story-identity-san` |
| `publisher-sa` | Publisher | `mys-dev-publisher-identity` |
| `chain-sa` | Chain | `mys-dev-chain-identity-san` |

**Key Files:**
- ServiceAccounts: [`kubernetes/base/service-accounts.yaml`](../../infra/kubernetes/base/service-accounts.yaml)
- Kubernetes README: [`kubernetes/README.md`](../../infra/kubernetes/README.md)

## Quick Reference

### Getting Configuration Values

After `terraform apply`, get authentication configuration:

```bash
cd infra/terraform/environments/dev

# Get all outputs
terraform output -json

# Specific values
terraform output entra_admin_api_client_id
terraform output entra_admin_ui_client_id
terraform output entra_tenant_id
terraform output admin_api_identity_client_id
terraform output postgresql_aad_connection_template
```

### Applying Kubernetes ServiceAccounts

```bash
# Set environment variables from Terraform
export ADMIN_API_CLIENT_ID=$(terraform output -raw admin_api_identity_client_id)
export STORY_GENERATOR_CLIENT_ID=$(terraform output -raw story_generator_identity_client_id)
export PUBLISHER_CLIENT_ID=$(terraform output -raw publisher_identity_client_id)
export CHAIN_CLIENT_ID=$(terraform output -raw chain_identity_client_id)

# Apply with substitution
envsubst < infra/kubernetes/base/service-accounts.yaml | kubectl apply -f -
```

### Testing Authentication

**Admin API (get token):**
```bash
# Using Azure CLI
az account get-access-token --resource api://mystira-admin-api-dev

# Test API
curl -H "Authorization: Bearer $TOKEN" https://dev.admin-api.mystira.app/api/health
```

**PostgreSQL (Azure AD):**
```bash
# Get token for PostgreSQL
TOKEN=$(az account get-access-token --resource https://ossrdbms-aad.database.windows.net --query accessToken -o tsv)

# Connect with psql
PGPASSWORD=$TOKEN psql "host=mys-dev-core-db.postgres.database.azure.com dbname=adminapi user=mys-dev-admin-api-identity-san sslmode=require"
```

## Troubleshooting

### Token Not Injected in Pod

1. Verify OIDC issuer is enabled:
   ```bash
   az aks show -n <cluster> -g <rg> --query "oidcIssuerProfile"
   ```

2. Verify federated credential:
   ```bash
   az identity federated-credential list --identity-name <identity> -g <rg>
   ```

3. Check pod events:
   ```bash
   kubectl describe pod <pod> -n mystira
   ```

### PostgreSQL Authentication Fails

1. Verify identity is an AD admin:
   ```bash
   az postgres flexible-server ad-admin list --resource-group <rg> --server-name <server>
   ```

2. Check connection string format (no password, correct username)

3. Ensure app is using `DefaultAzureCredential`

### MSAL Authentication Issues

1. Check redirect URI matches exactly (including trailing slashes)
2. Verify client ID and tenant ID
3. Check browser console for CORS errors

## Related Documentation

- [ADR-0010: Authentication Strategy](../architecture/adr/0010-authentication-and-authorization-strategy.md)
- [ADR-0011: Entra ID Integration](../architecture/adr/0011-entra-id-authentication-integration.md)
- [Kubernetes README](../../infra/kubernetes/README.md)
- [PostgreSQL Module](../../infra/terraform/modules/shared/postgresql/README.md)
- [Identity Module](../../infra/terraform/modules/shared/identity/README.md)
- [Admin API Module](../../infra/terraform/modules/admin-api/README.md)
- [Azure Workload Identity Docs](https://azure.github.io/azure-workload-identity/)
- [Microsoft Identity Platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
