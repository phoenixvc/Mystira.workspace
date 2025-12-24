# Kubernetes Manifests

This directory contains Kubernetes manifests for deploying Mystira services to AKS.

## Directory Structure

```
kubernetes/
├── base/                    # Base manifests (shared across environments)
│   ├── kustomization.yaml
│   ├── namespace.yaml
│   └── service-accounts.yaml
└── overlays/                # Environment-specific overlays (future)
    ├── dev/
    ├── staging/
    └── prod/
```

## Azure Workload Identity

The ServiceAccounts in this directory are configured for Azure Workload Identity, which allows pods to authenticate to Azure services without storing credentials.

### Service Accounts

| ServiceAccount | Service | Terraform Module | Notes |
|----------------|---------|------------------|-------|
| `admin-api-sa` | Admin API | `modules/admin-api` | ✅ Ready |
| `story-generator-sa` | Story Generator API | `modules/story-generator` | ✅ Ready |
| `publisher-sa` | Publisher | `modules/publisher` | ✅ Ready |
| `chain-sa` | Chain | `modules/chain` | ✅ Ready |

**Notes:**
- Admin UI is a browser-based SPA and doesn't need a ServiceAccount (it uses MSAL authentication in the browser).
- Story Generator deploys the **API** component to Kubernetes. The Blazor WASM frontend (if needed) would use Static Web App (no ServiceAccount required).

### Prerequisites

1. **AKS cluster with OIDC issuer enabled** (configured in Terraform)
2. **Workload identity enabled** on the AKS cluster (configured in Terraform)
3. **Federated credentials** created for each managed identity (configured in Terraform)
4. **Managed identities** created for each service (see table above)

### Setup Steps

1. **Get managed identity client IDs from Terraform:**

   ```bash
   cd infra/terraform/environments/dev
   terraform output -json
   ```

2. **Update ServiceAccount annotations with client IDs:**

   ```bash
   # Replace placeholders in service-accounts.yaml
   export ADMIN_API_CLIENT_ID="<from-terraform-output>"
   export STORY_GENERATOR_CLIENT_ID="<from-terraform-output>"
   export PUBLISHER_CLIENT_ID="<from-terraform-output>"
   export CHAIN_CLIENT_ID="<from-terraform-output>"

   envsubst < base/service-accounts.yaml | kubectl apply -f -
   ```

   Or use kustomize with patches:

   ```bash
   kubectl apply -k overlays/dev
   ```

3. **Deploy workloads with ServiceAccount:**

   ```yaml
   apiVersion: apps/v1
   kind: Deployment
   metadata:
     name: story-generator
     namespace: mystira
   spec:
     template:
       spec:
         serviceAccountName: story-generator-sa
         containers:
           - name: story-generator
             # Azure SDK will automatically use workload identity
             env:
               - name: AZURE_CLIENT_ID
                 valueFrom:
                   fieldRef:
                     fieldPath: metadata.annotations['azure.workload.identity/client-id']
   ```

### Pod Configuration

For workload identity to work, pods must have:

1. **ServiceAccount** with `azure.workload.identity/client-id` annotation
2. **Label**: `azure.workload.identity/use: "true"` (on ServiceAccount or Pod)

The Azure SDK automatically detects and uses workload identity when running in an AKS pod with these configurations.

## Applying Manifests

### Using kubectl

```bash
# Create namespace and service accounts
kubectl apply -f base/namespace.yaml
kubectl apply -f base/service-accounts.yaml
```

### Using Kustomize

```bash
# Apply base manifests
kubectl apply -k base/

# Apply environment-specific overlay (when available)
kubectl apply -k overlays/dev/
```

## Verification

Check that ServiceAccounts are created:

```bash
kubectl get serviceaccounts -n mystira

# Verify workload identity annotation
kubectl get serviceaccount story-generator-sa -n mystira -o yaml
```

## Troubleshooting

### Token not injected

If the Azure AD token is not being injected:

1. Verify OIDC issuer is enabled on AKS:
   ```bash
   az aks show -n <cluster-name> -g <resource-group> --query "oidcIssuerProfile"
   ```

2. Verify federated credential exists:
   ```bash
   az identity federated-credential list --identity-name <identity-name> -g <resource-group>
   ```

3. Check pod events:
   ```bash
   kubectl describe pod <pod-name> -n mystira
   ```

### Authentication failures

If Azure SDK authentication fails:

1. Verify client ID is correct in ServiceAccount annotation
2. Check that federated credential subject matches: `system:serviceaccount:mystira:<service-account-name>`
3. Ensure pod has the correct label: `azure.workload.identity/use: "true"`

## PostgreSQL Azure AD Authentication

Services can authenticate to PostgreSQL using Azure AD tokens instead of passwords. This is configured via the shared PostgreSQL module.

### Connection String Format

For Azure AD authentication, use this connection string format:

```
Host=<server>.postgres.database.azure.com;Database=<database>;Username=<identity-name>;Ssl Mode=Require
```

The Azure SDK automatically obtains tokens via workload identity when running in AKS.

### .NET Configuration

For .NET applications using Npgsql:

```csharp
// In appsettings.json or environment variable
"ConnectionStrings": {
  "DefaultConnection": "Host=mys-dev-core-db.postgres.database.azure.com;Database=adminapi;Username=mys-dev-admin-api-identity-san;Ssl Mode=Require"
}

// In Program.cs, configure Npgsql to use Azure AD
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.UseAzureADAuthentication(new DefaultAzureCredential());
    options.UseNpgsql(dataSourceBuilder.Build());
});
```

## Related Documentation

- [Azure Workload Identity](https://azure.github.io/azure-workload-identity/)
- [AKS Workload Identity](https://learn.microsoft.com/en-us/azure/aks/workload-identity-overview)
- [Terraform Identity Module](../terraform/modules/shared/identity/README.md)
- [Terraform Admin API Module](../terraform/modules/admin-api/README.md)
- [PostgreSQL Azure AD Auth](https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/how-to-configure-sign-in-azure-ad-authentication)
