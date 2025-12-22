# Mystira.App.Admin.Api Migration Guide

## Overview

This document outlines the changes required in `Mystira.App.Admin.Api` to support the hybrid data strategy. The Admin API manages content (scenarios, characters, badges) which will primarily remain in Cosmos DB, but requires read access to PostgreSQL for user/account data.

## Current State

```
Mystira.App.Admin.Api/
├── Program.cs
├── appsettings.json
├── Controllers/
│   ├── ScenariosController.cs
│   ├── CharactersController.cs
│   ├── BadgesController.cs
│   ├── AccountsController.cs      # Read-only access to accounts
│   └── ...
└── Services/
    └── ...
```

## Key Differences from App API

| Aspect | App API | Admin API |
|--------|---------|-----------|
| User Data | Read/Write | Read-Only |
| Content Data | Read-Only | Read/Write |
| Migration Phase | Full dual-write | Read-only from PostgreSQL |
| Redis Caching | User sessions | Content caching |

## Required Changes

### Phase 1: Foundation

#### 1.1 Update NuGet Packages

Same as App API, add to `Mystira.App.Admin.Api.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.0" />
  <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.2" />
  <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.1" />
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
    "ReadOnlyPostgresAccess": true
  },
  "Redis": {
    "InstanceName": "mystira-admin:",
    "ContentCacheMinutes": 30,
    "UserCacheMinutes": 5
  }
}
```

#### 1.3 Create Admin-Specific Configuration

```csharp
// Configuration/AdminDataMigrationOptions.cs
namespace Mystira.App.Admin.Api.Configuration;

public class AdminDataMigrationOptions
{
    public const string SectionName = "DataMigration";

    public MigrationPhase Phase { get; set; } = MigrationPhase.CosmosOnly;
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Admin API only reads from PostgreSQL, never writes user data
    /// </summary>
    public bool ReadOnlyPostgresAccess { get; set; } = true;
}
```

#### 1.4 Update Program.cs

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Bind configuration
builder.Services.Configure<AdminDataMigrationOptions>(
    builder.Configuration.GetSection(AdminDataMigrationOptions.SectionName));

// Add data services with read-only PostgreSQL access
builder.Services.AddAdminDataServices(builder.Configuration);

// Add Redis for content caching
builder.Services.AddContentCaching(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks()
    .AddCosmosDb(
        builder.Configuration.GetConnectionString("CosmosDb"),
        name: "cosmosdb",
        tags: new[] { "database", "cosmos", "content" })
    .AddNpgSql(
        builder.Configuration.GetConnectionString("PostgreSQL"),
        name: "postgresql",
        tags: new[] { "database", "postgres", "users" })
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis"),
        name: "redis",
        tags: new[] { "cache" });

var app = builder.Build();

// Map health endpoints
app.MapHealthChecks("/health");

app.Run();
```

#### 1.5 Admin-Specific Service Extensions

```csharp
// Extensions/AdminDataServiceExtensions.cs
namespace Mystira.App.Admin.Api.Extensions;

public static class AdminDataServiceExtensions
{
    public static IServiceCollection AddAdminDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(AdminDataMigrationOptions.SectionName)
            .Get<AdminDataMigrationOptions>() ?? new AdminDataMigrationOptions();

        // Cosmos DB context for content (always needed)
        services.AddDbContext<MystiraAppDbContext>(opt =>
            opt.UseCosmos(
                configuration.GetConnectionString("CosmosDb"),
                "MystiraAppDb"));

        // PostgreSQL context for user data (read-only)
        var postgresConnection = configuration.GetConnectionString("PostgreSQL");
        if (options.Enabled && !string.IsNullOrEmpty(postgresConnection))
        {
            services.AddDbContext<PostgreSqlDbContext>(opt =>
            {
                opt.UseNpgsql(postgresConnection);
                // Disable change tracking for read-only access
                opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // Register read-only PostgreSQL repository
            services.AddScoped<IReadOnlyAccountRepository, PgReadOnlyAccountRepository>();
        }

        // Content repositories (always Cosmos)
        services.AddScoped<IScenarioRepository, CosmosScenarioRepository>();
        services.AddScoped<ICharacterMapRepository, CosmosCharacterMapRepository>();
        services.AddScoped<IBadgeRepository, CosmosBadgeRepository>();
        services.AddScoped<IContentBundleRepository, CosmosContentBundleRepository>();

        // User repositories (phase-dependent)
        if (options.Phase >= MigrationPhase.DualWritePostgresRead && options.Enabled)
        {
            // Read from PostgreSQL
            services.AddScoped<IAccountQueryService, PostgresAccountQueryService>();
            services.AddScoped<IProfileQueryService, PostgresProfileQueryService>();
        }
        else
        {
            // Read from Cosmos
            services.AddScoped<IAccountQueryService, CosmosAccountQueryService>();
            services.AddScoped<IProfileQueryService, CosmosProfileQueryService>();
        }

        return services;
    }
}
```

### Phase 2: Read-Only User Access

#### 2.1 Create Read-Only Interfaces

```csharp
// Application/Ports/Data/IAccountQueryService.cs
namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Read-only access to account data for Admin API.
/// Does not include write operations.
/// </summary>
public interface IAccountQueryService
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<Account>> SearchAsync(string query, int limit = 20, CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
    Task<IEnumerable<Account>> GetRecentAsync(int count = 10, CancellationToken ct = default);
}
```

#### 2.2 Implement PostgreSQL Query Service

```csharp
// Infrastructure.PostgreSQL/Services/PostgresAccountQueryService.cs
namespace Mystira.App.Infrastructure.PostgreSQL.Services;

public class PostgresAccountQueryService : IAccountQueryService
{
    private readonly PostgreSqlDbContext _context;

    public PostgresAccountQueryService(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IEnumerable<Account>> SearchAsync(
        string query,
        int limit = 20,
        CancellationToken ct = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => EF.Functions.ILike(a.Email, $"%{query}%")
                     || EF.Functions.ILike(a.DisplayName, $"%{query}%"))
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await _context.Accounts.CountAsync(ct);
    }

    public async Task<IEnumerable<Account>> GetRecentAsync(
        int count = 10,
        CancellationToken ct = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }
}
```

### Phase 3: Content Caching

```csharp
// Extensions/ContentCachingExtensions.cs
namespace Mystira.App.Admin.Api.Extensions;

public static class ContentCachingExtensions
{
    public static IServiceCollection AddContentCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "mystira-admin:";
            });

            // Decorate content repositories with caching
            services.Decorate<IScenarioRepository, CachedScenarioRepository>();
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        return services;
    }
}
```

```csharp
// Infrastructure.Redis/CachedScenarioRepository.cs
public class CachedScenarioRepository : IScenarioRepository
{
    private readonly IScenarioRepository _inner;
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

    public async Task<Scenario?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var cacheKey = $"scenario:{id}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);

        if (cached != null)
            return JsonSerializer.Deserialize<Scenario>(cached);

        var scenario = await _inner.GetByIdAsync(id, ct);
        if (scenario != null)
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(scenario),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheDuration
                },
                ct);
        }

        return scenario;
    }

    // Write operations invalidate cache
    public async Task<Scenario> UpdateAsync(Scenario entity, CancellationToken ct = default)
    {
        var result = await _inner.UpdateAsync(entity, ct);
        await _cache.RemoveAsync($"scenario:{entity.Id}", ct);
        return result;
    }
}
```

## Admin-Specific Endpoints

### Migration Dashboard

```csharp
// Controllers/DashboardController.cs
[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : ControllerBase
{
    private readonly IAccountQueryService _accountQuery;
    private readonly IScenarioRepository _scenarios;
    private readonly IOptions<AdminDataMigrationOptions> _migrationOptions;

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats>> GetStats()
    {
        var stats = new DashboardStats
        {
            TotalAccounts = await _accountQuery.GetTotalCountAsync(),
            TotalScenarios = await _scenarios.GetTotalCountAsync(),
            CurrentMigrationPhase = _migrationOptions.Value.Phase,
            PostgresEnabled = _migrationOptions.Value.Enabled,
            RecentAccounts = await _accountQuery.GetRecentAsync(5)
        };

        return Ok(stats);
    }

    [HttpGet("migration-status")]
    public ActionResult<MigrationStatus> GetMigrationStatus()
    {
        return Ok(new MigrationStatus
        {
            Phase = _migrationOptions.Value.Phase,
            Enabled = _migrationOptions.Value.Enabled,
            ReadOnlyPostgres = _migrationOptions.Value.ReadOnlyPostgresAccess,
            DataSource = _migrationOptions.Value.Phase switch
            {
                MigrationPhase.CosmosOnly => "Cosmos DB",
                MigrationPhase.DualWriteCosmosRead => "Cosmos DB (primary)",
                MigrationPhase.DualWritePostgresRead => "PostgreSQL (primary)",
                MigrationPhase.PostgresOnly => "PostgreSQL",
                _ => "Unknown"
            }
        });
    }
}
```

## Configuration by Environment

### Local Development

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
    "ReadOnlyPostgresAccess": true
  }
}
```

### Staging (Phase 1 Testing)

```bash
az webapp config appsettings set --name mys-staging-mystira-admin-api-san \
  --settings \
    DataMigration__Phase=1 \
    DataMigration__Enabled=true \
    DataMigration__ReadOnlyPostgresAccess=true
```

## Testing Checklist

### Unit Tests

- [ ] Read-only PostgreSQL query service
- [ ] Content caching decorator
- [ ] Migration phase configuration

### Integration Tests

- [ ] Read from PostgreSQL when phase >= 2
- [ ] Read from Cosmos when phase < 2
- [ ] Cache invalidation on content updates
- [ ] No write operations to PostgreSQL

### E2E Tests

- [ ] Dashboard stats endpoint
- [ ] Migration status endpoint
- [ ] Content CRUD operations

## Dependencies

| Dependency | Version | Purpose |
|------------|---------|---------|
| `Mystira.App.Application` | * | Query interfaces |
| `Mystira.App.Infrastructure.Data` | * | Cosmos repositories |
| `Mystira.App.Infrastructure.PostgreSQL` | * | Read-only PostgreSQL access |
| `Mystira.App.Infrastructure.Redis` | * | Content caching |

## See Also

- [App API Migration](./mystira-app-api-migration.md)
- [Repository Architecture](../repository-architecture.md)
- [ADR-0013: Data Management Strategy](../../adr/0013-data-management-and-storage-strategy.md)
