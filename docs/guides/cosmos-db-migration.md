# Cosmos DB Migration Guide

This guide documents the process for migrating data between Azure Cosmos DB accounts, specifically the migration from the legacy `prodwusappmystiracosmos` account to the new environment-specific accounts.

## Overview

### Source Account (Legacy)
- **Account Name:** `prodwusappmystiracosmos`
- **Resource Group:** `prod-wus-rg-mystira`
- **Database:** `MystiraAppDb`

### Destination Accounts

| Environment | Account Name | Resource Group |
|------------|--------------|----------------|
| Production | `mys-prod-core-cosmos-san` | `mys-prod-core-rg-san` |
| Development | `mys-dev-core-cosmos-san` | `mys-dev-core-rg-san` |

## Migration Architecture

The migration is handled by the `MigrationService` in the Mystira.DevHub.Services project. It supports:

- **Typed migrations** for known entity types (Scenarios, ContentBundles, MediaAssets)
- **Generic container migrations** for any Cosmos DB container
- **Blob Storage migrations** for Azure Storage containers
- **Master data seeding** from JSON fixture files
- **Dry-run mode** for previewing migrations
- **Bulk operations** for high-performance data transfer
- **Retry logic** with exponential backoff

### Supported Containers

| Container | Partition Key | Migration Type |
|-----------|---------------|----------------|
| Scenarios | `/id` | Typed |
| ContentBundles | `/id` | Typed |
| MediaAssets | `/id` | Typed |
| UserProfiles | `/id` | Generic |
| GameSessions | `/id` | Generic |
| Accounts | `/id` | Generic |
| CompassTrackings | `/id` | Generic |
| CharacterMaps | `/id` | Generic |
| CharacterMapFiles | `/id` | Generic |
| CharacterMediaMetadataFiles | `/id` | Generic |
| AvatarConfigurationFiles | `/id` | Generic |
| BadgeConfigurations | `/id` | Generic |

## Quick Start

### Prerequisites

1. **Azure CLI** - Install from [Microsoft Docs](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
2. **.NET 9.0 SDK** - Install from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
3. **Azure Access** - You need read access to the source account and write access to the destination account

### One-Time Migration (Recommended)

Use the provided migration scripts for a complete migration:

#### Linux/macOS

```bash
# Clone the repository (if not already done)
cd Mystira.Devhub

# Run migration to dev environment (dry-run first)
./scripts/migrate-cosmos-db.sh --environment dev --dry-run

# If dry-run looks good, run the actual migration
./scripts/migrate-cosmos-db.sh --environment dev

# For production (be careful!)
./scripts/migrate-cosmos-db.sh --environment prod --dry-run
./scripts/migrate-cosmos-db.sh --environment prod
```

#### Windows (PowerShell)

```powershell
# Clone the repository (if not already done)
cd Mystira.Devhub

# Run migration to dev environment (dry-run first)
.\scripts\migrate-cosmos-db.ps1 -Environment dev -DryRun

# If dry-run looks good, run the actual migration
.\scripts\migrate-cosmos-db.ps1 -Environment dev

# For production (be careful!)
.\scripts\migrate-cosmos-db.ps1 -Environment prod -DryRun
.\scripts\migrate-cosmos-db.ps1 -Environment prod
```

### Script Options

| Option | Description |
|--------|-------------|
| `-e, --environment` | Target environment: `dev` or `prod` (required) |
| `-t, --type` | Migration type (default: `all`). Options: `scenarios`, `bundles`, `media-metadata`, `user-profiles`, `game-sessions`, `accounts`, `compass-trackings`, `character-maps`, `badge-configurations`, `blobs`, `master-data`, `all` |
| `-d, --dry-run` | Preview mode - counts items without migrating |
| `-v, --verbose` | Enable verbose output |

### Environment Variables

| Variable | Description |
|----------|-------------|
| `SOURCE_COSMOS_CONNECTION` | Source Cosmos DB connection string (auto-detected from Azure if not set) |
| `DEST_COSMOS_CONNECTION` | Destination Cosmos DB connection string (auto-detected from Azure if not set) |
| `SOURCE_STORAGE_CONNECTION` | Source Azure Storage connection string (for blob migration) |
| `DEST_STORAGE_CONNECTION` | Destination Azure Storage connection string (for blob migration) |

## Manual Migration via CLI

For more control, you can use the CLI directly:

### Build the CLI

```bash
cd Mystira.DevHub.CLI
dotnet build --configuration Release
```

### Run a Migration

```bash
# Create a JSON command file
cat > migration-command.json << 'EOF'
{
    "command": "migration.run",
    "args": {
        "type": "all",
        "sourceCosmosConnection": "YOUR_SOURCE_CONNECTION_STRING",
        "destCosmosConnection": "YOUR_DEST_CONNECTION_STRING",
        "sourceDatabaseName": "MystiraAppDb",
        "destDatabaseName": "MystiraAppDb",
        "dryRun": false,
        "maxRetries": 3,
        "useBulkOperations": true
    }
}
EOF

# Run the migration
cat migration-command.json | dotnet run --configuration Release --no-build
```

### Migration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `type` | string | (required) | Migration type to run |
| `sourceCosmosConnection` | string | - | Source Cosmos DB connection string |
| `destCosmosConnection` | string | - | Destination Cosmos DB connection string |
| `sourceDatabaseName` | string | `MystiraDb` | Source database name |
| `destDatabaseName` | string | `MystiraAppDb` | Destination database name |
| `dryRun` | boolean | `false` | Preview mode |
| `maxRetries` | number | `3` | Maximum retries for failed items |
| `useBulkOperations` | boolean | `true` | Use bulk API for better performance |

## Using the DevHub UI

The Mystira DevHub desktop application provides a graphical interface for migrations:

1. Launch the DevHub application
2. Navigate to **Migration** in the sidebar
3. Enter source and destination connection strings
4. Select the resources to migrate
5. Click **Preview** for a dry-run
6. Click **Migrate** to start the migration

## Migration Workflow Best Practices

### Pre-Migration Checklist

- [ ] Verify you have access to both source and destination accounts
- [ ] Run a dry-run to understand the scope of migration
- [ ] Ensure destination account has sufficient RU/s provisioned
- [ ] Back up the destination database if it contains existing data
- [ ] Notify relevant stakeholders of the migration window

### During Migration

- [ ] Monitor the migration progress via logs
- [ ] Check Azure Monitor for throttling/rate-limiting
- [ ] If errors occur, review the error list and retry failed items

### Post-Migration Verification

- [ ] Compare document counts between source and destination
- [ ] Spot-check critical documents (e.g., recent game sessions)
- [ ] Test application functionality against new database
- [ ] Update application configuration to use new connection string

## Troubleshooting

### Common Issues

#### "Failed to retrieve connection string"

**Cause:** Missing Azure access or wrong resource group/account name.

**Solution:**
1. Verify you're logged in: `az account show`
2. Check subscription: `az account set --subscription <subscription-id>`
3. Verify resource exists: `az cosmosdb show --name <account-name> --resource-group <rg-name>`

#### Rate Limiting (429 Errors)

**Cause:** Source or destination account has insufficient throughput.

**Solution:**
1. Increase RU/s on the destination account temporarily
2. Reduce `useBulkOperations` batch size
3. Run migration during off-peak hours

#### Partition Key Mismatch

**Cause:** Destination container has different partition key.

**Solution:**
1. Delete the destination container (if empty)
2. Let the migration recreate it with correct partition key
3. Or manually create with matching partition key path

### Getting Help

- Review the migration logs in the CLI output
- Check Azure Monitor for Cosmos DB metrics
- Open an issue in the repository with relevant logs

## Extending the Migration Service

To add support for new containers:

### For Generic Containers (Dynamic Schema)

Use the existing `MigrateContainerAsync` method in `MigrationCommands.cs`:

```csharp
case "my-new-container":
    results.Add(await _migrationService.MigrateContainerAsync(
        sourceCosmosConnection,
        destCosmosConnection,
        args.SourceDatabaseName,
        args.DestDatabaseName,
        "MyNewContainer",
        "/partitionKeyPath",  // Adjust as needed
        migrationOptions));
    break;
```

### For Typed Containers (Strong Schema)

1. Add the entity model to `Mystira.App.Domain.Models`
2. Add a typed migration method in `MigrationService.cs`
3. Wire it up in `MigrationCommands.cs`

## Security Considerations

- Connection strings contain sensitive credentials
- Never commit connection strings to source control
- Use environment variables or Azure Key Vault in production
- Delete the `migration-command.json` file after use
- The legacy `appsettings.json` with hardcoded credentials should be rotated

## Related Resources

- [Azure Cosmos DB Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/)
- [Cosmos DB Bulk Executor](https://docs.microsoft.com/en-us/azure/cosmos-db/bulk-executor-overview)
- [Azure Data Migration Guide](https://docs.microsoft.com/en-us/azure/dms/)
