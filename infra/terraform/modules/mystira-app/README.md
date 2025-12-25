# Mystira.App Infrastructure Module

This Terraform module deploys the infrastructure for [Mystira.App](https://github.com/phoenixvc/Mystira.App), converted from the original Bicep templates.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Mystira.App                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────────┐   │
│  │  Static Web App  │    │   App Service    │    │     Cosmos DB        │   │
│  │  (Blazor WASM)   │───▶│   (API Backend)  │───▶│   (Serverless)       │   │
│  │                  │    │   .NET 9.0       │    │                      │   │
│  │  • PWA           │    │                  │    │  Containers:         │   │
│  │  • Offline       │    │  • REST API      │    │  • UserProfiles      │   │
│  │  • IndexedDB     │    │  • CQRS/MediatR  │    │  • Accounts          │   │
│  └────────┬─────────┘    │  • Managed ID    │    │  • Scenarios         │   │
│           │              └────────┬─────────┘    │  • GameSessions      │   │
│           │                       │              │  • ContentBundles    │   │
│           │              ┌────────▼─────────┐    │  • PendingSignups    │   │
│           │              │    Key Vault     │    │  • CompassTrackings  │   │
│           │              │    (Secrets)     │    └──────────────────────┘   │
│           │              └────────┬─────────┘                               │
│           │                       │                                         │
│           │              ┌────────▼─────────┐    ┌──────────────────────┐   │
│           └─────────────▶│ Storage Account  │    │  Communication Svc   │   │
│                          │  (Media Blobs)   │    │  (Email)             │   │
│                          └──────────────────┘    └──────────────────────┘   │
│                                                                              │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                        Monitoring                                     │   │
│  │  Log Analytics Workspace  ◄────  Application Insights                │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Resources Created

| Resource                   | Purpose                      | Notes                                             |
| -------------------------- | ---------------------------- | ------------------------------------------------- |
| **Cosmos DB**              | Document database            | Serverless mode, 7 containers                     |
| **App Service**            | API backend                  | Linux, .NET 9.0, System Managed Identity          |
| **Static Web App**         | Blazor WASM PWA              | Deployed to fallback region (not available in ZA) |
| **Storage Account**        | Media blobs                  | With CORS support                                 |
| **Key Vault**              | Secrets management           | Stores connection strings, JWT keys               |
| **Application Insights**   | APM & monitoring             | Connected to Log Analytics                        |
| **Log Analytics**          | Centralized logging          | 30-day retention (configurable)                   |
| **Communication Services** | Email (optional)             | Azure-managed email                               |
| **Azure Bot**              | Teams integration (optional) | For Teams channel                                 |

## Usage

```hcl
module "mystira_app" {
  source = "../../modules/mystira-app"

  environment         = "dev"
  location            = "southafricanorth"
  fallback_location   = "eastus2"  # For Static Web App
  resource_group_name = azurerm_resource_group.mystira_app.name
  project_name        = "mystira"
  org                 = "mys"

  # Cosmos DB
  cosmos_db_serverless = true

  # App Service
  app_service_sku = "B1"
  dotnet_version  = "9.0"

  # Static Web App
  enable_static_web_app = true
  static_web_app_sku    = "Free"

  # Storage
  storage_sku = "Standard_LRS"
  cors_allowed_origins = [
    "https://app.mystira.app",
    "http://localhost:5000"
  ]

  # Communication Services
  enable_communication_services = true
  sender_email                  = "DoNotReply@mystira.app"

  # Monitoring
  log_retention_days = 30
  daily_quota_gb     = 1

  tags = {
    Environment = "dev"
    Project     = "Mystira"
  }
}
```

## Importing Existing Resources

If resources already exist from Bicep deployments, import them into Terraform state:

```bash
# Get your subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
RG_NAME="mys-dev-core-rg-san"  # Using shared core resource group

# Import Cosmos DB
terraform import 'module.mystira_app.azurerm_cosmosdb_account.main[0]' \
  /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.DocumentDB/databaseAccounts/mys-dev-mystira-cosmos-san

# Import Storage Account
terraform import 'module.mystira_app.azurerm_storage_account.main[0]' \
  /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Storage/storageAccounts/mysdevmystirastsan

# Import App Service Plan
terraform import 'module.mystira_app.azurerm_service_plan.main' \
  /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Web/serverFarms/mys-dev-mystira-asp-san

# Import App Service
terraform import 'module.mystira_app.azurerm_linux_web_app.api' \
  /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Web/sites/mys-dev-mystira-api-san

# Import Key Vault
terraform import 'module.mystira_app.azurerm_key_vault.main' \
  /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.KeyVault/vaults/mys-dev-app-kv-san

# Import Static Web App
terraform import 'module.mystira_app.azurerm_static_web_app.main[0]' \
  /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Web/staticSites/mys-dev-mystira-swa-eus2

# Import Log Analytics (only if NOT using shared monitoring)
terraform import 'module.mystira_app.azurerm_log_analytics_workspace.main[0]' \
  /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.OperationalInsights/workspaces/mys-dev-mystira-law-san

# Import Application Insights (only if NOT using shared monitoring)
terraform import 'module.mystira_app.azurerm_application_insights.main[0]' \
  /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG_NAME/providers/Microsoft.Insights/components/mys-dev-mystira-ai-san
```

## Cosmos DB Containers

### Infrastructure-Managed Containers (7)

These containers are created by Terraform. Partition keys match the DbContext `ToJsonProperty` mappings.

| Container        | Partition Key | Purpose                                  |
| ---------------- | ------------- | ---------------------------------------- |
| UserProfiles     | `/id`         | User profile data                        |
| Accounts         | `/id`         | User accounts                            |
| Scenarios        | `/id`         | Game scenarios                           |
| GameSessions     | `/accountId`  | Active game sessions                     |
| ContentBundles   | `/id`         | Downloadable content                     |
| PendingSignups   | `/email`      | Unverified registrations                 |
| CompassTrackings | `/id`         | Analytics tracking (Axis mapped to "id") |

### EF Core Auto-Created Containers (13+)

Additional containers are created automatically by EF Core `EnsureCreatedAsync()` at application startup:

| Container                | Partition Key | Purpose                    |
| ------------------------ | ------------- | -------------------------- |
| CharacterMaps            | `/id`         | Character mapping data     |
| BadgeConfigurations      | `/id`         | Badge definitions          |
| Badges                   | `/id`         | Badge instances            |
| BadgeImages              | `/id`         | Badge image assets         |
| CompassAxes              | `/id`         | Compass axis definitions   |
| ArchetypeDefinitions     | `/id`         | Archetype master data      |
| EchoTypeDefinitions      | `/id`         | Echo type master data      |
| FantasyThemeDefinitions  | `/id`         | Fantasy theme master data  |
| AgeGroupDefinitions      | `/id`         | Age group master data      |
| PlayerScenarioScores     | `/profileId`  | Player scores per scenario |
| MediaAssets              | `/mediaType`  | Media asset metadata       |
| MediaMetadataFiles       | `/id`         | Media metadata             |
| AvatarConfigurationFiles | `/id`         | Avatar configurations      |

## Naming Convention

Resources follow the pattern: `[org]-[env]-[project]-[type]-[region]`

Example: `mys-dev-mystira-cosmos-san`

- `mys` = Organization (Mystira)
- `dev` = Environment
- `mystira` = Project
- `cosmos` = Resource type
- `san` = Region code (South Africa North)

## Notes

1. **Static Web App Location**: Azure Static Web Apps are not available in South Africa North. The module deploys to a fallback region (default: East US 2).

2. **Serverless Cosmos DB**: Recommended for dev/staging to minimize costs. Production can use provisioned throughput.

3. **Key Vault Soft Delete**: Enabled with 7-day retention. Production enables purge protection.

4. **Managed Identity**: App Service uses System Assigned Managed Identity for secure access to Key Vault and other resources.

5. **Container Migration**: If importing existing containers deployed with old Bicep templates, partition keys may differ. Cosmos DB partition keys cannot be changed after creation - containers must be recreated with correct keys. The EF Core DbContext `ToJsonProperty` mappings are the source of truth.
