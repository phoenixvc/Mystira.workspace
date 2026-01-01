# Mystira.Infrastructure Migration Guide

**Target**: Migrate from `Mystira.App.Infrastructure.*` to `Mystira.Infrastructure.*` packages
**Prerequisites**: .NET 9.0 SDK, access to GitHub Packages NuGet feed
**Last Updated**: January 2026
**Status**: Complete

---

## Overview

This guide covers migrating from the `Mystira.App.Infrastructure.*` packages (previously in the `packages/app` submodule) to the new consolidated `Mystira.Infrastructure.*` NuGet packages.

### Package Mapping

| Old Package | New Package | Version |
|-------------|-------------|---------|
| `Mystira.App.Infrastructure.Data` | `Mystira.Infrastructure.Data` | 0.5.0-alpha |
| `Mystira.App.Infrastructure.Azure` | `Mystira.Infrastructure.Azure` | 0.5.0-alpha |
| `Mystira.App.Infrastructure.Discord` | `Mystira.Infrastructure.Discord` | 0.5.0-alpha |
| `Mystira.App.Infrastructure.Teams` | `Mystira.Infrastructure.Teams` | 0.5.0-alpha |
| `Mystira.App.Infrastructure.WhatsApp` | `Mystira.Infrastructure.WhatsApp` | 0.5.0-alpha |
| `Mystira.App.Infrastructure.Payments` | `Mystira.Infrastructure.Payments` | 0.5.0-alpha |

---

## Quick Start

### Step 1: Update NuGet Sources

Ensure your `nuget.config` includes the GitHub Packages feed:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/phoenixvc/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

### Step 2: Update Package References

```xml
<!-- Before -->
<ProjectReference Include="../Mystira.App.Infrastructure.Data/Mystira.App.Infrastructure.Data.csproj" />
<ProjectReference Include="../Mystira.App.Infrastructure.Azure/Mystira.App.Infrastructure.Azure.csproj" />

<!-- After -->
<PackageReference Include="Mystira.Infrastructure.Data" Version="0.5.0-alpha" />
<PackageReference Include="Mystira.Infrastructure.Azure" Version="0.5.0-alpha" />
```

### Step 3: Update Namespace Imports

```csharp
// Before
using Mystira.App.Infrastructure.Data;
using Mystira.App.Infrastructure.Data.Polyglot;
using Mystira.App.Infrastructure.Azure;

// After
using Mystira.Infrastructure.Data;
using Mystira.Infrastructure.Data.Polyglot;
using Mystira.Infrastructure.Azure;
```

---

## Package Details

### Mystira.Infrastructure.Data

Provides polyglot persistence with support for Cosmos DB and PostgreSQL.

**Dependencies:**
- `Mystira.Domain` (0.5.0-alpha)
- `Mystira.Application` (0.5.0-alpha)
- `Mystira.Shared` (0.4.*)
- `Microsoft.EntityFrameworkCore.Cosmos` (9.0.0)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.2)
- `Ardalis.Specification.EntityFrameworkCore` (9.0.0)

**Key Components:**

| Component | Namespace |
|-----------|-----------|
| `PolyglotRepository<T>` | `Mystira.Infrastructure.Data.Polyglot` |
| `CosmosDbContext` | `Mystira.Infrastructure.Data.Cosmos` |
| `PostgresDbContext` | `Mystira.Infrastructure.Data.Postgres` |
| `PolyglotDbContextFactory` | `Mystira.Infrastructure.Data.Polyglot` |

**Registration:**

```csharp
// Program.cs
builder.Services.AddPolyglotPersistence<CosmosDbContext, PostgresDbContext>(builder.Configuration);
builder.Services.AddPolyglotRepository<Scenario>();
builder.Services.AddPolyglotRepository<Account>();
```

**Configuration:**

```json
{
  "Polyglot": {
    "Primary": "CosmosDb",
    "Fallback": "PostgreSql",
    "SyncEnabled": true
  },
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=...",
    "DatabaseName": "mystira"
  },
  "PostgreSql": {
    "ConnectionString": "Host=localhost;Database=mystira;..."
  }
}
```

---

### Mystira.Infrastructure.Azure

Azure service integrations including Blob Storage, Queue Storage, and Key Vault.

**Dependencies:**
- `Mystira.Application` (0.5.0-alpha)
- `Azure.Storage.Blobs` (12.23.0)
- `Azure.Storage.Queues` (12.21.0)
- `Azure.Security.KeyVault.Secrets` (4.7.0)

**Key Components:**

| Component | Interface | Description |
|-----------|-----------|-------------|
| `AzureBlobStorageService` | `IBlobStorageService` | Blob storage operations |
| `AzureQueueService` | `IQueueService` | Queue messaging |
| `AzureKeyVaultService` | `ISecretService` | Secret management |

**Registration:**

```csharp
builder.Services.AddAzureInfrastructure(builder.Configuration);
```

---

### Mystira.Infrastructure.Discord

Discord bot integration for notifications and commands.

**Dependencies:**
- `Mystira.Application` (0.5.0-alpha)
- `Discord.Net` (3.17.0)

**Key Components:**

| Component | Interface |
|-----------|-----------|
| `DiscordNotificationService` | `IDiscordNotificationService` |
| `DiscordBotClient` | `IDiscordClient` |

**Registration:**

```csharp
builder.Services.AddDiscordIntegration(builder.Configuration);
```

---

### Mystira.Infrastructure.Teams

Microsoft Teams integration for notifications.

**Dependencies:**
- `Mystira.Application` (0.5.0-alpha)
- `Microsoft.Bot.Builder` (4.24.0)

**Registration:**

```csharp
builder.Services.AddTeamsIntegration(builder.Configuration);
```

---

### Mystira.Infrastructure.WhatsApp

WhatsApp Business API integration.

**Dependencies:**
- `Mystira.Application` (0.5.0-alpha)
- `Twilio` (7.7.1)

**Registration:**

```csharp
builder.Services.AddWhatsAppIntegration(builder.Configuration);
```

---

### Mystira.Infrastructure.Payments

Payment processing with Stripe and PayFast.

**Dependencies:**
- `Mystira.Application` (0.5.0-alpha)
- `Stripe.net` (47.2.0)

**Key Components:**

| Component | Interface |
|-----------|-----------|
| `StripePaymentService` | `IPaymentService` |
| `PayFastPaymentService` | `IPaymentService` |

**Registration:**

```csharp
builder.Services.AddPaymentProcessing(builder.Configuration);
```

---

## Breaking Changes

### Namespace Changes

All namespaces have been updated from `Mystira.App.Infrastructure.*` to `Mystira.Infrastructure.*`:

```csharp
// Before
using Mystira.App.Infrastructure.Data.Polyglot;
using Mystira.App.Infrastructure.Azure.Storage;
using Mystira.App.Infrastructure.Discord;

// After
using Mystira.Infrastructure.Data.Polyglot;
using Mystira.Infrastructure.Azure.Storage;
using Mystira.Infrastructure.Discord;
```

### Domain/Application References

Infrastructure packages now reference:
- `Mystira.Domain` instead of `Mystira.App.Domain`
- `Mystira.Application` instead of `Mystira.App.Application`

Ensure you've migrated to these packages first. See:
- [Mystira.Domain Migration](./mystira-domain-migration.md)
- [Mystira.Application Migration](./mystira-application-migration.md)

### Polyglot Deprecation in Mystira.Shared

The polyglot interfaces in `Mystira.Shared.Data.Polyglot` are now **deprecated**. Use:

| Deprecated | Replacement |
|------------|-------------|
| `Mystira.Shared.Data.Polyglot.IPolyglotRepository<T>` | `Mystira.Application.Ports.Data.IPolyglotRepository<T>` |
| `Mystira.Shared.Data.Polyglot.DatabaseTargetAttribute` | `Mystira.Application.Ports.Data.DatabaseTargetAttribute` |

**Implementation**: Use `Mystira.Infrastructure.Data.Polyglot.PolyglotRepository<T>`.

---

## Migration Checklist

### Pre-Migration

- [ ] Ensure .NET 9.0 SDK is installed
- [ ] Configure GitHub Packages NuGet feed access
- [ ] Verify `Mystira.Domain` and `Mystira.Application` are already migrated
- [ ] Create a feature branch for migration

### Package Updates

- [ ] Replace `Mystira.App.Infrastructure.Data` with `Mystira.Infrastructure.Data`
- [ ] Replace `Mystira.App.Infrastructure.Azure` with `Mystira.Infrastructure.Azure`
- [ ] Replace `Mystira.App.Infrastructure.Discord` with `Mystira.Infrastructure.Discord` (if used)
- [ ] Replace `Mystira.App.Infrastructure.Teams` with `Mystira.Infrastructure.Teams` (if used)
- [ ] Replace `Mystira.App.Infrastructure.WhatsApp` with `Mystira.Infrastructure.WhatsApp` (if used)
- [ ] Replace `Mystira.App.Infrastructure.Payments` with `Mystira.Infrastructure.Payments` (if used)

### Code Updates

- [ ] Update all `using Mystira.App.Infrastructure.*` to `using Mystira.Infrastructure.*`
- [ ] Update any references to `Mystira.Shared.Data.Polyglot` interfaces to use `Mystira.Application.Ports.Data`
- [ ] Verify DI registration calls match new extension method names

### Verification

- [ ] Run `dotnet build` and fix any compilation errors
- [ ] Run all unit tests
- [ ] Run integration tests
- [ ] Verify database connectivity (Cosmos DB, PostgreSQL)
- [ ] Test payment processing in sandbox environment

---

## Troubleshooting

### Package Not Found

**Error**: `Unable to find package 'Mystira.Infrastructure.Data'`

**Solution**: Ensure GitHub Packages is configured in your `nuget.config` with valid credentials.

### Namespace Conflicts

If you have both old and new packages referenced, you may see ambiguous reference errors. Remove all `Mystira.App.Infrastructure.*` project references before adding the new package references.

### EF Core Migration Issues

If using Entity Framework migrations, you may need to regenerate migrations after updating to the new packages:

```bash
# Remove old migrations
rm -rf Migrations/

# Create new initial migration
dotnet ef migrations add InitialCreate --context CosmosDbContext
dotnet ef migrations add InitialCreate --context PostgresDbContext
```

---

## Related Documentation

- [MIGRATION_INDEX.md](./MIGRATION_INDEX.md) - Migration status dashboard
- [Mystira.Shared Migration Guide](../guides/mystira-shared-migration.md)
- [ADR-0020: Package Consolidation Strategy](../architecture/adr/0020-package-consolidation-strategy.md)
- [ADR-0014: Polyglot Persistence](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
