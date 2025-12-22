# Mystira.App.Api Migration Guide

## Overview

This document outlines the changes required in `Mystira.App.Api` to support the hybrid data strategy (Cosmos DB + PostgreSQL) as defined in ADR-0013.

## Current State

```
Mystira.App.Api/
├── Program.cs                    # Entry point, DI configuration
├── appsettings.json             # Configuration
├── Controllers/
│   ├── AccountsController.cs
│   ├── ProfilesController.cs
│   ├── SessionsController.cs
│   └── ...
├── Middleware/
│   └── ...
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

## Required Changes

### Phase 1: Foundation

#### 1.1 Update NuGet Packages

Add to `Mystira.App.Api.csproj`:

```xml
<ItemGroup>
  <!-- PostgreSQL Support -->
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />

  <!-- Redis Caching -->
  <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.0" />
  <PackageReference Include="StackExchange.Redis" Version="2.8.0" />

  <!-- Health Checks -->
  <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.2" />
  <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.1" />

  <!-- Optional: Specification Pattern -->
  <PackageReference Include="Ardalis.Specification" Version="8.0.0" />
  <PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="8.0.0" />
</ItemGroup>
```

#### 1.2 Update appsettings.json

```json
{
  "ConnectionStrings": {
    "CosmosDb": "",
    "PostgreSQL": "",
    "Redis": ""
  },
  "DataMigration": {
    "Phase": 0,
    "Enabled": false,
    "SyncQueueType": "InMemory"
  },
  "Redis": {
    "InstanceName": "mystira:",
    "DefaultExpirationMinutes": 5
  }
}
```

#### 1.3 Create Configuration Classes

```csharp
// Configuration/DataMigrationOptions.cs
namespace Mystira.App.Api.Configuration;

public class DataMigrationOptions
{
    public const string SectionName = "DataMigration";

    public MigrationPhase Phase { get; set; } = MigrationPhase.CosmosOnly;
    public bool Enabled { get; set; } = false;
    public string SyncQueueType { get; set; } = "InMemory";
}

public enum MigrationPhase
{
    CosmosOnly = 0,
    DualWriteCosmosRead = 1,
    DualWritePostgresRead = 2,
    PostgresOnly = 3
}
```

```csharp
// Configuration/RedisOptions.cs
namespace Mystira.App.Api.Configuration;

public class RedisOptions
{
    public const string SectionName = "Redis";

    public string InstanceName { get; set; } = "mystira:";
    public int DefaultExpirationMinutes { get; set; } = 5;
}
```

#### 1.4 Update Program.cs

```csharp
// Program.cs
using Mystira.App.Api.Configuration;
using Mystira.App.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration
builder.Services.Configure<DataMigrationOptions>(
    builder.Configuration.GetSection(DataMigrationOptions.SectionName));
builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection(RedisOptions.SectionName));

// Add hybrid data services
builder.Services.AddHybridDataServices(builder.Configuration);

// Add Redis caching
builder.Services.AddRedisCache(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks()
    .AddCosmosDb(
        builder.Configuration.GetConnectionString("CosmosDb"),
        name: "cosmosdb",
        tags: new[] { "database", "cosmos" })
    .AddNpgSql(
        builder.Configuration.GetConnectionString("PostgreSQL"),
        name: "postgresql",
        tags: new[] { "database", "postgres" })
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis"),
        name: "redis",
        tags: new[] { "cache" });

var app = builder.Build();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.Run();
```

#### 1.5 Create Service Extensions

```csharp
// Extensions/HybridDataServiceExtensions.cs
namespace Mystira.App.Api.Extensions;

public static class HybridDataServiceExtensions
{
    public static IServiceCollection AddHybridDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var migrationOptions = configuration
            .GetSection(DataMigrationOptions.SectionName)
            .Get<DataMigrationOptions>() ?? new DataMigrationOptions();

        // Register Cosmos DB context (always available)
        var cosmosConnection = configuration.GetConnectionString("CosmosDb");
        if (!string.IsNullOrEmpty(cosmosConnection))
        {
            services.AddDbContext<MystiraAppDbContext>(options =>
                options.UseCosmos(
                    cosmosConnection,
                    "MystiraAppDb",
                    cosmosOptions => cosmosOptions.ConnectionMode(ConnectionMode.Gateway)));
        }
        else
        {
            // InMemory for local development
            services.AddDbContext<MystiraAppDbContext>(options =>
                options.UseInMemoryDatabase("MystiraAppDb"));
        }

        // Register PostgreSQL context if enabled
        var postgresConnection = configuration.GetConnectionString("PostgreSQL");
        if (migrationOptions.Enabled && !string.IsNullOrEmpty(postgresConnection))
        {
            services.AddDbContext<PostgreSqlDbContext>(options =>
                options.UseNpgsql(postgresConnection, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                }));
        }

        // Register repositories based on migration phase
        RegisterRepositories(services, migrationOptions);

        // Register background sync service if in dual-write mode
        if (migrationOptions.Phase is MigrationPhase.DualWriteCosmosRead
            or MigrationPhase.DualWritePostgresRead)
        {
            services.AddHostedService<DataSyncBackgroundService>();
            services.AddSingleton<ISyncQueue, InMemorySyncQueue>();
        }

        return services;
    }

    private static void RegisterRepositories(
        IServiceCollection services,
        DataMigrationOptions options)
    {
        switch (options.Phase)
        {
            case MigrationPhase.CosmosOnly:
                // Use existing Cosmos repositories
                services.AddScoped<IAccountRepository, CosmosAccountRepository>();
                services.AddScoped<IUserProfileRepository, CosmosUserProfileRepository>();
                services.AddScoped<IGameSessionRepository, CosmosGameSessionRepository>();
                // ... other repositories
                break;

            case MigrationPhase.DualWriteCosmosRead:
            case MigrationPhase.DualWritePostgresRead:
                // Register both implementations
                services.AddScoped<CosmosAccountRepository>();
                services.AddScoped<PgAccountRepository>();
                // Use dual-write repositories
                services.AddScoped<IAccountRepository, DualWriteAccountRepository>();
                services.AddScoped<IUserProfileRepository, DualWriteUserProfileRepository>();
                break;

            case MigrationPhase.PostgresOnly:
                // PostgreSQL only
                services.AddScoped<IAccountRepository, PgAccountRepository>();
                services.AddScoped<IUserProfileRepository, PgUserProfileRepository>();
                break;
        }
    }
}
```

```csharp
// Extensions/RedisCacheExtensions.cs
namespace Mystira.App.Api.Extensions;

public static class RedisCacheExtensions
{
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        var redisOptions = configuration
            .GetSection(RedisOptions.SectionName)
            .Get<RedisOptions>() ?? new RedisOptions();

        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = redisOptions.InstanceName;
            });

            // Also register IConnectionMultiplexer for advanced scenarios
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnection));
        }
        else
        {
            // Fallback to in-memory cache for local development
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
```

### Phase 2: Controller Updates

#### 2.1 Add Migration Metrics

```csharp
// Controllers/AccountsController.cs
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _repository;
    private readonly TelemetryClient _telemetry;
    private readonly IOptions<DataMigrationOptions> _migrationOptions;

    public AccountsController(
        IAccountRepository repository,
        TelemetryClient telemetry,
        IOptions<DataMigrationOptions> migrationOptions)
    {
        _repository = repository;
        _telemetry = telemetry;
        _migrationOptions = migrationOptions;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id)
    {
        var stopwatch = Stopwatch.StartNew();

        var account = await _repository.GetByIdAsync(id.ToString());

        stopwatch.Stop();

        // Track migration metrics
        _telemetry.TrackMetric("Account.GetById.LatencyMs", stopwatch.ElapsedMilliseconds);
        _telemetry.TrackMetric("DataMigration.Phase", (int)_migrationOptions.Value.Phase);

        if (account is null)
            return NotFound();

        return Ok(AccountDto.FromEntity(account));
    }
}
```

### Phase 3: Add Migration Endpoints (Admin Only)

```csharp
// Controllers/Admin/MigrationController.cs
[ApiController]
[Route("api/admin/migration")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class MigrationController : ControllerBase
{
    private readonly IOptions<DataMigrationOptions> _options;
    private readonly ISyncQueue _syncQueue;
    private readonly ILogger<MigrationController> _logger;

    [HttpGet("status")]
    public ActionResult<MigrationStatusDto> GetStatus()
    {
        return Ok(new MigrationStatusDto
        {
            CurrentPhase = _options.Value.Phase,
            Enabled = _options.Value.Enabled,
            SyncQueueType = _options.Value.SyncQueueType,
            SyncQueueDepth = _syncQueue?.GetQueueLengthAsync().Result ?? 0
        });
    }

    [HttpGet("sync-queue")]
    public async Task<ActionResult<IEnumerable<SyncItem>>> GetSyncQueue()
    {
        if (_syncQueue is null)
            return Ok(Array.Empty<SyncItem>());

        var items = await _syncQueue.GetAllAsync();
        return Ok(items);
    }

    [HttpPost("retry-failed")]
    public async Task<ActionResult> RetryFailedItems()
    {
        if (_syncQueue is null)
            return BadRequest("Sync queue not enabled");

        await _syncQueue.RetryFailedAsync();
        _logger.LogInformation("Retrying failed sync items");
        return Ok();
    }
}
```

## Configuration by Environment

### Local Development (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "CosmosDb": "",
    "PostgreSQL": "",
    "Redis": ""
  },
  "DataMigration": {
    "Phase": 0,
    "Enabled": false
  }
}
```

### Dev Environment (Azure App Settings)

```bash
az webapp config appsettings set --name mys-dev-mystira-api-san \
  --resource-group mys-dev-core-rg-san \
  --settings \
    DataMigration__Phase=0 \
    DataMigration__Enabled=false
```

### Staging Environment (Phase 1 Testing)

```bash
az webapp config appsettings set --name mys-staging-mystira-api-san \
  --resource-group mys-staging-core-rg-san \
  --settings \
    DataMigration__Phase=1 \
    DataMigration__Enabled=true \
    DataMigration__SyncQueueType=Redis
```

### Production Environment (After Validation)

```bash
az webapp config appsettings set --name mys-prod-mystira-api-san \
  --resource-group mys-prod-core-rg-san \
  --settings \
    DataMigration__Phase=1 \
    DataMigration__Enabled=true \
    DataMigration__SyncQueueType=Redis
```

## Testing Checklist

### Unit Tests

- [ ] `DataMigrationOptions` configuration binding
- [ ] `HybridDataServiceExtensions` DI registration
- [ ] Repository selection by migration phase

### Integration Tests

- [ ] Dual-write to Cosmos + PostgreSQL
- [ ] Read from correct source based on phase
- [ ] Sync queue processing
- [ ] Fallback behavior when secondary fails

### E2E Tests

- [ ] Health check endpoints
- [ ] Migration status endpoint
- [ ] Account CRUD operations in each phase

## Rollback Procedure

If issues occur during migration:

1. **Set phase back to CosmosOnly**:
   ```bash
   az webapp config appsettings set --name <app-name> \
     --settings DataMigration__Phase=0
   ```

2. **Restart the application**:
   ```bash
   az webapp restart --name <app-name> --resource-group <rg>
   ```

3. **Verify health checks**:
   ```bash
   curl https://<app-name>.azurewebsites.net/health
   ```

## Dependencies

| Dependency | Version | Purpose |
|------------|---------|---------|
| `Mystira.App.Application` | * | Repository interfaces |
| `Mystira.App.Infrastructure.Data` | * | Cosmos repositories |
| `Mystira.App.Infrastructure.PostgreSQL` | * | PostgreSQL repositories (new) |
| `Mystira.App.Infrastructure.Hybrid` | * | Dual-write coordination (new) |
| `Mystira.App.Infrastructure.Redis` | * | Caching layer (new) |

## See Also

- [ADR-0013: Data Management Strategy](../../adr/0013-data-management-and-storage-strategy.md)
- [ADR-0014: Polyglot Persistence Frameworks](../../adr/0014-polyglot-persistence-framework-selection.md)
- [Repository Architecture](../repository-architecture.md)
- [PostgreSQL Schema](../user-domain-postgresql-migration.md)
