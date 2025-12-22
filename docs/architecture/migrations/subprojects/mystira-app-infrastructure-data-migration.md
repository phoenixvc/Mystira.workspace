# Mystira.App.Infrastructure.Data Migration Guide

## Overview

This document outlines the changes required in `Mystira.App.Infrastructure.Data` (Cosmos DB layer) to support the hybrid data strategy, and documents the creation of new infrastructure projects.

## Current State

```
Mystira.App.Infrastructure.Data/
├── MystiraAppDbContext.cs
├── MystiraAppDbContext.Auditing.cs
├── MystiraAppDbContext.QueryFilters.cs
├── Configuration/
│   └── EntityConfigurationExtensions.cs
├── Repositories/
│   ├── Repository.cs                    # Base repository
│   ├── AccountRepository.cs
│   ├── UserProfileRepository.cs
│   ├── GameSessionRepository.cs
│   ├── ScenarioRepository.cs
│   ├── PendingSignupRepository.cs
│   └── ...
├── Services/
│   ├── MasterDataSeederService.cs
│   ├── MediaMetadataService.cs
│   └── BadgeConfigurationLoaderService.cs
├── Specifications/
│   └── SpecificationEvaluator.cs
├── UnitOfWork/
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
└── PartitionKeyInterceptor.cs
```

## Required Changes

### Phase 1: Rename Repositories for Clarity

Rename existing repositories to indicate they are Cosmos-specific:

```
Repositories/
├── Repository.cs                 → CosmosRepository.cs
├── AccountRepository.cs          → CosmosAccountRepository.cs
├── UserProfileRepository.cs      → CosmosUserProfileRepository.cs
├── GameSessionRepository.cs      → CosmosGameSessionRepository.cs
└── ...
```

```csharp
// Repositories/CosmosAccountRepository.cs
namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Cosmos DB implementation of IAccountRepository.
/// </summary>
public class CosmosAccountRepository : CosmosRepository<Account>, IAccountRepository
{
    public CosmosAccountRepository(MystiraAppDbContext context) : base(context) { }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Where(a => a.Email.ToLower() == email.ToLower())
            .FirstOrDefaultAsync();
    }

    public async Task<Account?> GetByAuth0UserIdAsync(string auth0UserId)
    {
        return await _dbSet
            .Where(a => a.Auth0UserId == auth0UserId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet.AnyAsync(a => a.Email.ToLower() == email.ToLower());
    }
}
```

### Phase 2: Add Internal Interface for Dual-Write

```csharp
// Repositories/IInternalAccountRepository.cs
namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Internal interface exposing Cosmos-specific operations.
/// Used by hybrid layer for dual-write coordination.
/// </summary>
internal interface ICosmosAccountRepository : IAccountRepository
{
    /// <summary>
    /// Get the Cosmos DB partition key for an account.
    /// </summary>
    string GetPartitionKey(Account account);

    /// <summary>
    /// Bulk insert accounts (for migration).
    /// </summary>
    Task BulkInsertAsync(IEnumerable<Account> accounts, CancellationToken ct = default);
}
```

### Phase 3: Create New Infrastructure Projects

#### Project Structure

```
Mystira.App/src/
├── Mystira.App.Infrastructure.Data/           # Existing (Cosmos)
├── Mystira.App.Infrastructure.PostgreSQL/     # NEW
├── Mystira.App.Infrastructure.Hybrid/         # NEW
└── Mystira.App.Infrastructure.Redis/          # NEW
```

---

## New Project: Mystira.App.Infrastructure.PostgreSQL

### Project File

```xml
<!-- Mystira.App.Infrastructure.PostgreSQL.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
    <PackageReference Include="EFCore.NamingConventions" Version="8.0.3" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Mystira.App.Domain\Mystira.App.Domain.csproj" />
    <ProjectReference Include="..\Mystira.App.Application\Mystira.App.Application.csproj" />
  </ItemGroup>
</Project>
```

### DbContext

```csharp
// PostgreSqlDbContext.cs
namespace Mystira.App.Infrastructure.PostgreSQL;

public class PostgreSqlDbContext : DbContext
{
    public PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options)
        : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();
    public DbSet<PendingSignup> PendingSignups => Set<PendingSignup>();
    public DbSet<CompletedScenario> CompletedScenarios => Set<CompletedScenario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Use snake_case naming convention
        modelBuilder.UseSnakeCaseNamingConvention();

        // Apply configurations
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
        modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
        modelBuilder.ApplyConfiguration(new UserBadgeConfiguration());
        modelBuilder.ApplyConfiguration(new PendingSignupConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit timestamps
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

### Entity Configurations

```csharp
// Configuration/AccountConfiguration.cs
namespace Mystira.App.Infrastructure.PostgreSQL.Configuration;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasMaxLength(36)
            .ValueGeneratedNever();

        builder.Property(a => a.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Auth0UserId)
            .HasMaxLength(255);

        builder.Property(a => a.DisplayName)
            .HasMaxLength(255)
            .HasDefaultValue("");

        builder.Property(a => a.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(AccountRole.Guest);

        builder.Property(a => a.Settings)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb");

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.LastLoginAt)
            .HasDefaultValueSql("NOW()");

        // Indexes
        builder.HasIndex(a => a.Email);
        builder.HasIndex(a => a.Auth0UserId).IsUnique();
        builder.HasIndex(a => a.Role);
        builder.HasIndex(a => a.CreatedAt);

        // Soft delete filter
        builder.HasQueryFilter(a => !a.IsDeleted);

        // Relationships
        builder.HasOne(a => a.Subscription)
            .WithOne(s => s.Account)
            .HasForeignKey<Subscription>(s => s.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Profiles)
            .WithOne(p => p.Account)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

### PostgreSQL Repository

```csharp
// Repositories/PgAccountRepository.cs
namespace Mystira.App.Infrastructure.PostgreSQL.Repositories;

public class PgAccountRepository : PgRepository<Account>, IAccountRepository
{
    public PgAccountRepository(PostgreSqlDbContext context) : base(context) { }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Where(a => EF.Functions.ILike(a.Email, email))
            .FirstOrDefaultAsync();
    }

    public async Task<Account?> GetByAuth0UserIdAsync(string auth0UserId)
    {
        return await _dbSet
            .Where(a => a.Auth0UserId == auth0UserId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet.AnyAsync(a => EF.Functions.ILike(a.Email, email));
    }

    public async Task<Account> AddAsync(Account entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Account> UpdateAsync(Account entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            // Soft delete
            entity.IsDeleted = true;
            entity.DeletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }
}
```

### EF Core Migrations

```csharp
// Migrations/20251222000000_InitialCreate.cs
namespace Mystira.App.Infrastructure.PostgreSQL.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");

        migrationBuilder.CreateTable(
            name: "accounts",
            columns: table => new
            {
                id = table.Column<string>(maxLength: 36, nullable: false),
                auth0_user_id = table.Column<string>(maxLength: 255, nullable: true),
                email = table.Column<string>(maxLength: 255, nullable: false),
                display_name = table.Column<string>(maxLength: 255, nullable: false, defaultValue: ""),
                role = table.Column<string>(maxLength: 50, nullable: false, defaultValue: "Guest"),
                settings = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                created_at = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "NOW()"),
                last_login_at = table.Column<DateTimeOffset>(nullable: false, defaultValueSql: "NOW()"),
                updated_at = table.Column<DateTimeOffset>(nullable: true),
                is_deleted = table.Column<bool>(nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTimeOffset>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_accounts", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_accounts_email",
            table: "accounts",
            column: "email");

        migrationBuilder.CreateIndex(
            name: "ix_accounts_auth0_user_id",
            table: "accounts",
            column: "auth0_user_id",
            unique: true);

        // ... additional tables
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "accounts");
        // ... drop other tables
    }
}
```

---

## New Project: Mystira.App.Infrastructure.Hybrid

### Project File

```xml
<!-- Mystira.App.Infrastructure.Hybrid.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Mystira.App.Application\Mystira.App.Application.csproj" />
    <ProjectReference Include="..\Mystira.App.Infrastructure.Data\Mystira.App.Infrastructure.Data.csproj" />
    <ProjectReference Include="..\Mystira.App.Infrastructure.PostgreSQL\Mystira.App.Infrastructure.PostgreSQL.csproj" />
  </ItemGroup>
</Project>
```

### Dual-Write Repository

See [Repository Architecture](../repository-architecture.md) for full implementation.

---

## New Project: Mystira.App.Infrastructure.Redis

### Project File

```xml
<!-- Mystira.App.Infrastructure.Redis.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.0" />
    <PackageReference Include="StackExchange.Redis" Version="2.8.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Mystira.App.Application\Mystira.App.Application.csproj" />
  </ItemGroup>
</Project>
```

### Cache Service

```csharp
// RedisCacheService.cs
namespace Mystira.App.Infrastructure.Redis;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken ct = default);
}

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redis)
    {
        _cache = cache;
        _redis = redis;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        var data = await _cache.GetStringAsync(key, ct);
        return data is null ? null : JsonSerializer.Deserialize<T>(data, _jsonOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null,
        CancellationToken ct = default) where T : class
    {
        var data = JsonSerializer.Serialize(value, _jsonOptions);
        await _cache.SetStringAsync(key, data, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
        }, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken ct = default)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern);

        foreach (var key in keys)
        {
            await _cache.RemoveAsync(key!, ct);
        }
    }
}
```

---

## Migration Steps

### Step 1: Rename Existing Repositories

```bash
# In Infrastructure.Data project
git mv Repositories/AccountRepository.cs Repositories/CosmosAccountRepository.cs
git mv Repositories/UserProfileRepository.cs Repositories/CosmosUserProfileRepository.cs
git mv Repositories/Repository.cs Repositories/CosmosRepository.cs
# ... etc
```

### Step 2: Create New Projects

```bash
# Create new projects
dotnet new classlib -n Mystira.App.Infrastructure.PostgreSQL -o src/Mystira.App.Infrastructure.PostgreSQL
dotnet new classlib -n Mystira.App.Infrastructure.Hybrid -o src/Mystira.App.Infrastructure.Hybrid
dotnet new classlib -n Mystira.App.Infrastructure.Redis -o src/Mystira.App.Infrastructure.Redis

# Add to solution
dotnet sln add src/Mystira.App.Infrastructure.PostgreSQL
dotnet sln add src/Mystira.App.Infrastructure.Hybrid
dotnet sln add src/Mystira.App.Infrastructure.Redis
```

### Step 3: Run EF Core Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate \
  --project src/Mystira.App.Infrastructure.PostgreSQL \
  --startup-project src/Mystira.App.Api \
  --context PostgreSqlDbContext

# Apply migration
dotnet ef database update \
  --project src/Mystira.App.Infrastructure.PostgreSQL \
  --startup-project src/Mystira.App.Api \
  --context PostgreSqlDbContext
```

## Testing Checklist

### Unit Tests

- [ ] Cosmos repositories work unchanged
- [ ] PostgreSQL repositories CRUD operations
- [ ] Cache service get/set/remove

### Integration Tests

- [ ] EF Core migrations apply correctly
- [ ] PostgreSQL schema matches design
- [ ] Both contexts work in same application

## Dependencies Summary

```
Mystira.App.Api
├── Mystira.App.Application
├── Mystira.App.Infrastructure.Data (Cosmos)
├── Mystira.App.Infrastructure.PostgreSQL (new)
├── Mystira.App.Infrastructure.Hybrid (new)
└── Mystira.App.Infrastructure.Redis (new)

Mystira.App.Infrastructure.Hybrid
├── Mystira.App.Infrastructure.Data
└── Mystira.App.Infrastructure.PostgreSQL

Mystira.App.Infrastructure.Data
├── Mystira.App.Application
└── Mystira.App.Domain

Mystira.App.Infrastructure.PostgreSQL
├── Mystira.App.Application
└── Mystira.App.Domain
```

## See Also

- [App API Migration](./mystira-app-api-migration.md)
- [Domain Migration](./mystira-app-domain-migration.md)
- [Repository Architecture](../repository-architecture.md)
- [PostgreSQL Schema](../user-domain-postgresql-migration.md)
