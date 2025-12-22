# ADR-0014: Polyglot Persistence Framework Implementation Roadmap

## Executive Summary

This roadmap details the implementation of [ADR-0014](../architecture/adr/0014-polyglot-persistence-framework-selection.md) - the Hybrid Approach using Ardalis.Specification + Custom Dual-Write + StackExchange.Redis + Polly resilience.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                         POLYGLOT PERSISTENCE STACK                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│  ┌────────────────────────────────────────────────────────────────────────────┐ │
│  │                        APPLICATION LAYER                                    │ │
│  │  ┌──────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                   Ardalis.Specification                               │  │ │
│  │  │  AccountByEmailSpec, ActiveUsersSpec, RecentSessionsSpec            │  │ │
│  │  │  (Reusable, testable, composable query definitions)                  │  │ │
│  │  └──────────────────────────────────────────────────────────────────────┘  │ │
│  │  ┌──────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                   Repository Interfaces                               │  │ │
│  │  │  IReadRepository<T>, IRepository<T> (with specification support)     │  │ │
│  │  └──────────────────────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────────────────────┘ │
│                                        │                                         │
│                                        ▼                                         │
│  ┌────────────────────────────────────────────────────────────────────────────┐ │
│  │                      INFRASTRUCTURE LAYER                                   │ │
│  │  ┌──────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                   CachedRepository<T>                                 │  │ │
│  │  │  Decorator pattern with StackExchange.Redis                          │  │ │
│  │  └─────────────────────────────────┬────────────────────────────────────┘  │ │
│  │                                    │                                        │ │
│  │  ┌──────────────────────────────────────────────────────────────────────┐  │ │
│  │  │                   DualWriteRepository<T>                              │  │ │
│  │  │  Phase-aware routing with Polly resilience                           │  │ │
│  │  └──────────────────────────┬──────────────────┬────────────────────────┘  │ │
│  │                             │                  │                            │ │
│  │              ┌──────────────▼───────┐   ┌──────▼──────────────┐            │ │
│  │              │  CosmosRepository<T> │   │ PostgresRepository<T>│            │ │
│  │              │  (EF Core Cosmos)    │   │ (EF Core Npgsql)    │            │ │
│  │              └──────────────────────┘   └─────────────────────┘            │ │
│  └────────────────────────────────────────────────────────────────────────────┘ │
│                                        │                                         │
│  ┌────────────────────────────────────────────────────────────────────────────┐ │
│  │                       RESILIENCE LAYER (Polly)                              │ │
│  │  Retry (exponential backoff) │ Circuit Breaker │ Timeout │ Fallback        │ │
│  └────────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## NuGet Packages Required

### Core Packages

```xml
<!-- Ardalis.Specification -->
<PackageReference Include="Ardalis.Specification" Version="8.0.0" />
<PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="8.0.0" />

<!-- Redis -->
<PackageReference Include="StackExchange.Redis" Version="2.8.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.0" />

<!-- Resilience -->
<PackageReference Include="Polly" Version="8.4.0" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Resilience" Version="9.0.0" />

<!-- Database -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Cosmos" Version="9.0.0" />
```

---

## Implementation Phases

### Phase 1: Ardalis.Specification Integration (Week 1-2)

#### 1.1 Add Package References

**Location**: `Mystira.App.Application/Mystira.App.Application.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Ardalis.Specification" Version="8.0.0" />
</ItemGroup>
```

**Location**: `Mystira.App.Infrastructure.Data/Mystira.App.Infrastructure.Data.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="8.0.0" />
</ItemGroup>
```

#### 1.2 Update Base Repository Interface

**Location**: `Mystira.App.Application/Ports/Data/IRepository.cs`

```csharp
using Ardalis.Specification;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Read-only repository interface with specification support.
/// </summary>
public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class
{
    // Inherits from Ardalis:
    // - Task<T?> GetByIdAsync<TId>(TId id, CancellationToken ct)
    // - Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct)
    // - Task<List<T>> ListAsync(CancellationToken ct)
    // - Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct)
    // - Task<int> CountAsync(CancellationToken ct)
    // - Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct)
    // - Task<bool> AnyAsync(CancellationToken ct)
    // - Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct)

    // Custom additions for streaming
    IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> spec, CancellationToken ct = default);
}

/// <summary>
/// Full repository interface with write operations and specification support.
/// </summary>
public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T> where T : class
{
    // Inherits from Ardalis:
    // - Task<T> AddAsync(T entity, CancellationToken ct)
    // - Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken ct)
    // - Task UpdateAsync(T entity, CancellationToken ct)
    // - Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct)
    // - Task DeleteAsync(T entity, CancellationToken ct)
    // - Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct)
    // - Task<int> SaveChangesAsync(CancellationToken ct)
}
```

#### 1.3 Create Specification Examples

**Location**: `Mystira.App.Application/Specifications/Accounts/`

```csharp
// AccountByEmailSpec.cs
namespace Mystira.App.Application.Specifications.Accounts;

public class AccountByEmailSpec : Specification<Account>, ISingleResultSpecification<Account>
{
    public AccountByEmailSpec(string email)
    {
        Query.Where(a => a.Email.ToLower() == email.ToLower());
    }
}

// AccountByAuth0UserIdSpec.cs
public class AccountByAuth0UserIdSpec : Specification<Account>, ISingleResultSpecification<Account>
{
    public AccountByAuth0UserIdSpec(string auth0UserId)
    {
        Query.Where(a => a.Auth0UserId == auth0UserId);
    }
}

// ActiveAccountsSpec.cs
public class ActiveAccountsSpec : Specification<Account>
{
    public ActiveAccountsSpec(int? limit = null)
    {
        Query.Where(a => !a.IsDeleted)
             .OrderByDescending(a => a.LastLoginAt);

        if (limit.HasValue)
            Query.Take(limit.Value);
    }
}

// AccountsWithProfilesSpec.cs
public class AccountsWithProfilesSpec : Specification<Account>
{
    public AccountsWithProfilesSpec()
    {
        Query.Include(a => a.Profiles)
             .Where(a => !a.IsDeleted);
    }
}

// SearchAccountsSpec.cs
public class SearchAccountsSpec : Specification<Account>
{
    public SearchAccountsSpec(string searchTerm, int limit = 20)
    {
        Query.Where(a =>
                a.Email.Contains(searchTerm) ||
                a.DisplayName.Contains(searchTerm))
             .OrderBy(a => a.Email)
             .Take(limit);
    }
}
```

#### 1.4 Update Repository Implementation

**Location**: `Mystira.App.Infrastructure.Data/Repositories/CosmosRepository.cs`

```csharp
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

namespace Mystira.App.Infrastructure.Data.Repositories;

/// <summary>
/// Cosmos DB repository with Ardalis.Specification support.
/// </summary>
public class CosmosRepository<T> : RepositoryBase<T>, IRepository<T> where T : class
{
    private readonly MystiraAppDbContext _context;

    public CosmosRepository(MystiraAppDbContext context) : base(context)
    {
        _context = context;
    }

    // Custom streaming implementation
    public async IAsyncEnumerable<T> AsAsyncEnumerable(
        ISpecification<T> spec,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var query = ApplySpecification(spec);
        await foreach (var item in query.AsAsyncEnumerable().WithCancellation(ct))
        {
            yield return item;
        }
    }
}
```

#### 1.5 Update Service Layer Usage

**Before** (Direct repository methods):
```csharp
public class AccountService
{
    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _repository.GetByEmailAsync(email);
    }
}
```

**After** (Specification-based):
```csharp
public class AccountService
{
    private readonly IReadRepository<Account> _repository;

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var spec = new AccountByEmailSpec(email);
        return await _repository.FirstOrDefaultAsync(spec, ct);
    }

    public async Task<List<Account>> SearchAsync(string term, CancellationToken ct)
    {
        var spec = new SearchAccountsSpec(term);
        return await _repository.ListAsync(spec, ct);
    }
}
```

| Task | Location | Priority | Effort |
|------|----------|----------|--------|
| Add Ardalis.Specification packages | Application, Infrastructure.Data | P0 | 0.5 day |
| Update IRepository interface | Application/Ports/Data | P0 | 0.5 day |
| Create Account specifications | Application/Specifications | P0 | 1 day |
| Create UserProfile specifications | Application/Specifications | P1 | 1 day |
| Create GameSession specifications | Application/Specifications | P2 | 1 day |
| Update CosmosRepository | Infrastructure.Data | P0 | 1 day |
| Update service layer usage | Application/Services | P1 | 2 days |

---

### Phase 2: Polly Resilience Layer (Week 2-3)

#### 2.1 Add Resilience Packages

**Location**: `Mystira.App.Infrastructure.Hybrid/Mystira.App.Infrastructure.Hybrid.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Polly" Version="8.4.0" />
  <PackageReference Include="Microsoft.Extensions.Resilience" Version="9.0.0" />
</ItemGroup>
```

#### 2.2 Create Resilience Strategies

**Location**: `Mystira.App.Infrastructure.Hybrid/Resilience/`

```csharp
// ResilienceStrategies.cs
namespace Mystira.App.Infrastructure.Hybrid.Resilience;

public static class ResilienceStrategies
{
    public static ResiliencePipeline<T> CreateDatabasePipeline<T>(ILogger logger)
    {
        return new ResiliencePipelineBuilder<T>()
            // Retry with exponential backoff
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry {AttemptNumber} after {Delay}ms due to {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
                    return default;
                }
            })
            // Circuit breaker
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    logger.LogError("Circuit breaker opened for {Duration}", args.BreakDuration);
                    return default;
                }
            })
            // Timeout
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10)
            })
            .Build();
    }

    public static ResiliencePipeline CreateCachePipeline(ILogger logger)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(50),
                ShouldHandle = new PredicateBuilder().Handle<RedisConnectionException>()
            })
            .AddTimeout(TimeSpan.FromMilliseconds(500))
            .Build();
    }
}

// ResilienceServiceExtensions.cs
public static class ResilienceServiceExtensions
{
    public static IServiceCollection AddResiliencePipelines(this IServiceCollection services)
    {
        services.AddResiliencePipeline("database", (builder, context) =>
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<ResiliencePipeline>>();
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .AddTimeout(TimeSpan.FromSeconds(10));
        });

        services.AddResiliencePipeline("cache", (builder, context) =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(50)
            })
            .AddTimeout(TimeSpan.FromMilliseconds(500));
        });

        return services;
    }
}
```

#### 2.3 Integrate with Repositories

```csharp
// ResilientDualWriteRepository.cs
public class ResilientDualWriteRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly IRepository<T> _cosmosRepo;
    private readonly IRepository<T> _postgresRepo;
    private readonly ResiliencePipeline _dbPipeline;
    private readonly IOptions<MigrationOptions> _options;
    private readonly ILogger<ResilientDualWriteRepository<T>> _logger;

    public ResilientDualWriteRepository(
        [FromKeyedServices("cosmos")] IRepository<T> cosmosRepo,
        [FromKeyedServices("postgres")] IRepository<T> postgresRepo,
        [FromKeyedServices("database")] ResiliencePipeline dbPipeline,
        IOptions<MigrationOptions> options,
        ILogger<ResilientDualWriteRepository<T>> logger)
    {
        _cosmosRepo = cosmosRepo;
        _postgresRepo = postgresRepo;
        _dbPipeline = dbPipeline;
        _options = options;
        _logger = logger;
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct)
    {
        return await _dbPipeline.ExecuteAsync(async token =>
        {
            var repo = GetReadRepository();
            return await repo.FirstOrDefaultAsync(spec, token);
        }, ct);
    }

    private IRepository<T> GetReadRepository() => _options.Value.Phase switch
    {
        MigrationPhase.CosmosOnly or MigrationPhase.DualWriteCosmosRead => _cosmosRepo,
        _ => _postgresRepo
    };
}
```

| Task | Location | Priority | Effort |
|------|----------|----------|--------|
| Add Polly packages | Infrastructure.Hybrid | P0 | 0.5 day |
| Create resilience strategies | Infrastructure.Hybrid/Resilience | P0 | 1 day |
| Create pipeline registration | Infrastructure.Hybrid/Extensions | P0 | 0.5 day |
| Integrate with dual-write repository | Infrastructure.Hybrid | P0 | 1 day |
| Add health check integration | Infrastructure.Hybrid | P1 | 0.5 day |
| Add metrics for resilience events | Infrastructure.Hybrid | P2 | 1 day |

---

### Phase 3: Enhanced Caching Layer (Week 3-4)

#### 3.1 Create Cache Configuration

**Location**: `Mystira.App.Infrastructure.Redis/Configuration/`

```csharp
// RedisCacheOptions.cs
namespace Mystira.App.Infrastructure.Redis.Configuration;

public class RedisCacheOptions
{
    public const string SectionName = "Redis";

    public string InstanceName { get; set; } = "mystira:";
    public int DefaultExpirationMinutes { get; set; } = 5;
    public int SlideExpirationMinutes { get; set; } = 2;
    public bool EnableCompression { get; set; } = true;
    public int CompressionThresholdBytes { get; set; } = 1024;

    // Per-entity cache configuration
    public Dictionary<string, EntityCacheConfig> EntityConfigs { get; set; } = new()
    {
        ["Account"] = new() { ExpirationMinutes = 10, EnableCache = true },
        ["UserProfile"] = new() { ExpirationMinutes = 5, EnableCache = true },
        ["Scenario"] = new() { ExpirationMinutes = 60, EnableCache = true },
        ["GameSession"] = new() { ExpirationMinutes = 2, EnableCache = false }
    };
}

public class EntityCacheConfig
{
    public int ExpirationMinutes { get; set; } = 5;
    public bool EnableCache { get; set; } = true;
    public bool UseSliding { get; set; } = false;
}
```

#### 3.2 Create Generic Cached Repository

```csharp
// CachedRepository.cs
namespace Mystira.App.Infrastructure.Redis;

public class CachedRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly IRepository<T> _inner;
    private readonly IDistributedCache _cache;
    private readonly ResiliencePipeline _cachePipeline;
    private readonly IOptions<RedisCacheOptions> _options;
    private readonly ILogger<CachedRepository<T>> _logger;
    private readonly string _entityName;
    private readonly EntityCacheConfig _entityConfig;

    public CachedRepository(
        IRepository<T> inner,
        IDistributedCache cache,
        [FromKeyedServices("cache")] ResiliencePipeline cachePipeline,
        IOptions<RedisCacheOptions> options,
        ILogger<CachedRepository<T>> logger)
    {
        _inner = inner;
        _cache = cache;
        _cachePipeline = cachePipeline;
        _options = options;
        _logger = logger;
        _entityName = typeof(T).Name;
        _entityConfig = options.Value.EntityConfigs.GetValueOrDefault(_entityName)
            ?? new EntityCacheConfig();
    }

    private string GetCacheKey(string suffix)
        => $"{_options.Value.InstanceName}{_entityName.ToLower()}:{suffix}";

    public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken ct = default) where TId : notnull
    {
        if (!_entityConfig.EnableCache)
            return await _inner.GetByIdAsync(id, ct);

        var cacheKey = GetCacheKey(id.ToString()!);

        // Try cache first with resilience
        var cached = await TryGetFromCacheAsync<T>(cacheKey, ct);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {Key}", cacheKey);
            return cached;
        }

        // Get from database
        _logger.LogDebug("Cache miss for {Key}", cacheKey);
        var entity = await _inner.GetByIdAsync(id, ct);

        // Cache the result
        if (entity is not null)
            await SetCacheAsync(cacheKey, entity, ct);

        return entity;
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        // Only cache single-result specs with deterministic cache keys
        if (!_entityConfig.EnableCache || spec is not ICacheableSpecification cacheable)
            return await _inner.FirstOrDefaultAsync(spec, ct);

        var cacheKey = GetCacheKey($"spec:{cacheable.CacheKey}");
        var cached = await TryGetFromCacheAsync<T>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var entity = await _inner.FirstOrDefaultAsync(spec, ct);
        if (entity is not null)
            await SetCacheAsync(cacheKey, entity, ct);

        return entity;
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        var result = await _inner.AddAsync(entity, ct);
        await InvalidateCacheAsync(entity.Id, ct);
        return result;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        await _inner.UpdateAsync(entity, ct);
        await InvalidateCacheAsync(entity.Id, ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        await _inner.DeleteAsync(entity, ct);
        await InvalidateCacheAsync(entity.Id, ct);
    }

    private async Task<TResult?> TryGetFromCacheAsync<TResult>(string key, CancellationToken ct)
    {
        try
        {
            return await _cachePipeline.ExecuteAsync(async token =>
            {
                var data = await _cache.GetStringAsync(key, token);
                return data is null ? default : JsonSerializer.Deserialize<TResult>(data);
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for {Key}, falling back to database", key);
            return default;
        }
    }

    private async Task SetCacheAsync<TValue>(string key, TValue value, CancellationToken ct)
    {
        try
        {
            await _cachePipeline.ExecuteAsync(async token =>
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_entityConfig.ExpirationMinutes)
                };

                if (_entityConfig.UseSliding)
                    options.SlidingExpiration = TimeSpan.FromMinutes(_options.Value.SlideExpirationMinutes);

                await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, token);
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for {Key}", key);
        }
    }

    private async Task InvalidateCacheAsync(string id, CancellationToken ct)
    {
        try
        {
            await _cache.RemoveAsync(GetCacheKey(id), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache invalidation failed for {Id}", id);
        }
    }

    // Delegate other methods to inner repository...
    public Task<List<T>> ListAsync(CancellationToken ct = default) => _inner.ListAsync(ct);
    public Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default) => _inner.ListAsync(spec, ct);
    // etc...
}

// ICacheableSpecification.cs
public interface ICacheableSpecification
{
    string CacheKey { get; }
}

// Example cacheable specification
public class AccountByEmailSpec : Specification<Account>, ISingleResultSpecification<Account>, ICacheableSpecification
{
    public string CacheKey { get; }

    public AccountByEmailSpec(string email)
    {
        CacheKey = $"email:{email.ToLower()}";
        Query.Where(a => a.Email.ToLower() == email.ToLower());
    }
}
```

| Task | Location | Priority | Effort |
|------|----------|----------|--------|
| Create cache configuration options | Infrastructure.Redis/Configuration | P0 | 0.5 day |
| Implement generic CachedRepository | Infrastructure.Redis | P0 | 2 days |
| Add ICacheableSpecification interface | Application/Specifications | P1 | 0.5 day |
| Update specifications for caching | Application/Specifications | P1 | 1 day |
| Add cache metrics | Infrastructure.Redis | P2 | 1 day |
| Add cache warming on startup | Infrastructure.Redis | P3 | 1 day |

---

### Phase 4: Complete Stack Integration (Week 4-5)

#### 4.1 DI Registration

**Location**: `Mystira.App.Api/Extensions/PolyglotDataServiceExtensions.cs`

```csharp
namespace Mystira.App.Api.Extensions;

public static class PolyglotDataServiceExtensions
{
    public static IServiceCollection AddPolyglotDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Bind configuration
        services.Configure<MigrationOptions>(
            configuration.GetSection(MigrationOptions.SectionName));
        services.Configure<RedisCacheOptions>(
            configuration.GetSection(RedisCacheOptions.SectionName));

        // 2. Add resilience pipelines
        services.AddResiliencePipelines();

        // 3. Add database contexts
        services.AddDbContext<MystiraAppDbContext>(opt =>
            opt.UseCosmos(configuration.GetConnectionString("CosmosDb"), "MystiraAppDb"));

        services.AddDbContext<PostgreSqlDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

        // 4. Register base repositories (keyed services for DI)
        services.AddKeyedScoped<IRepository<Account>, CosmosAccountRepository>("cosmos");
        services.AddKeyedScoped<IRepository<Account>, PgAccountRepository>("postgres");

        // 5. Get migration options to determine registration
        var migrationOptions = configuration
            .GetSection(MigrationOptions.SectionName)
            .Get<MigrationOptions>() ?? new MigrationOptions();

        // 6. Register appropriate implementation based on phase
        if (migrationOptions.Phase is MigrationPhase.DualWriteCosmosRead
            or MigrationPhase.DualWritePostgresRead)
        {
            // Dual-write with caching
            services.AddScoped<IRepository<Account>>(sp =>
            {
                var dualWrite = new ResilientDualWriteRepository<Account>(
                    sp.GetRequiredKeyedService<IRepository<Account>>("cosmos"),
                    sp.GetRequiredKeyedService<IRepository<Account>>("postgres"),
                    sp.GetRequiredKeyedService<ResiliencePipeline>("database"),
                    sp.GetRequiredService<IOptions<MigrationOptions>>(),
                    sp.GetRequiredService<ILogger<ResilientDualWriteRepository<Account>>>());

                return new CachedRepository<Account>(
                    dualWrite,
                    sp.GetRequiredService<IDistributedCache>(),
                    sp.GetRequiredKeyedService<ResiliencePipeline>("cache"),
                    sp.GetRequiredService<IOptions<RedisCacheOptions>>(),
                    sp.GetRequiredService<ILogger<CachedRepository<Account>>>());
            });
        }
        else if (migrationOptions.Phase == MigrationPhase.PostgresOnly)
        {
            // PostgreSQL only with caching
            services.AddScoped<IRepository<Account>>(sp =>
            {
                var postgres = sp.GetRequiredKeyedService<IRepository<Account>>("postgres");
                return new CachedRepository<Account>(
                    postgres,
                    sp.GetRequiredService<IDistributedCache>(),
                    sp.GetRequiredKeyedService<ResiliencePipeline>("cache"),
                    sp.GetRequiredService<IOptions<RedisCacheOptions>>(),
                    sp.GetRequiredService<ILogger<CachedRepository<Account>>>());
            });
        }
        else
        {
            // Cosmos only with caching
            services.AddScoped<IRepository<Account>>(sp =>
            {
                var cosmos = sp.GetRequiredKeyedService<IRepository<Account>>("cosmos");
                return new CachedRepository<Account>(
                    cosmos,
                    sp.GetRequiredService<IDistributedCache>(),
                    sp.GetRequiredKeyedService<ResiliencePipeline>("cache"),
                    sp.GetRequiredService<IOptions<RedisCacheOptions>>(),
                    sp.GetRequiredService<ILogger<CachedRepository<Account>>>());
            });
        }

        return services;
    }
}
```

#### 4.2 Simplified Registration with Scrutor

```csharp
// Using Scrutor for decorator pattern
services.AddScoped<IRepository<Account>, CosmosAccountRepository>();
services.Decorate<IRepository<Account>, ResilientDualWriteRepository<Account>>();
services.Decorate<IRepository<Account>, CachedRepository<Account>>();
```

---

## Testing Strategy

### Unit Tests

```csharp
// SpecificationTests.cs
[TestClass]
public class AccountSpecificationTests
{
    [TestMethod]
    public void AccountByEmailSpec_SetsCorrectFilter()
    {
        var spec = new AccountByEmailSpec("test@example.com");
        var evaluator = new SpecificationEvaluator();

        var accounts = new List<Account>
        {
            new() { Email = "test@example.com" },
            new() { Email = "other@example.com" }
        };

        var result = evaluator.GetQuery(accounts.AsQueryable(), spec).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("test@example.com", result[0].Email);
    }
}

// CachedRepositoryTests.cs
[TestClass]
public class CachedRepositoryTests
{
    private Mock<IRepository<Account>> _innerMock;
    private Mock<IDistributedCache> _cacheMock;
    private CachedRepository<Account> _sut;

    [TestInitialize]
    public void Setup()
    {
        _innerMock = new Mock<IRepository<Account>>();
        _cacheMock = new Mock<IDistributedCache>();
        // ... setup
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsCachedValue_WhenInCache()
    {
        var account = new Account { Id = "123", Email = "test@example.com" };
        _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>(), default))
            .ReturnsAsync(JsonSerializer.Serialize(account));

        var result = await _sut.GetByIdAsync("123");

        Assert.IsNotNull(result);
        Assert.AreEqual("test@example.com", result.Email);
        _innerMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), default), Times.Never);
    }
}
```

### Integration Tests

```csharp
[TestClass]
public class PolyglotRepositoryIntegrationTests
{
    [TestMethod]
    public async Task DualWrite_WritesToBothDatabases()
    {
        // Use testcontainers for Cosmos emulator and PostgreSQL
        await using var cosmos = new CosmosDbContainer().Build();
        await using var postgres = new PostgreSqlContainer().Build();

        // Create dual-write repository
        // Verify both databases have the entity
    }
}
```

---

## Monitoring & Observability

### Metrics to Track

```csharp
// PolyglotMetrics.cs
public static class PolyglotMetrics
{
    private static readonly Counter<long> CacheHits = Meter.CreateCounter<long>("polyglot_cache_hits");
    private static readonly Counter<long> CacheMisses = Meter.CreateCounter<long>("polyglot_cache_misses");
    private static readonly Counter<long> RetryAttempts = Meter.CreateCounter<long>("polyglot_retry_attempts");
    private static readonly Counter<long> CircuitBreakerTrips = Meter.CreateCounter<long>("polyglot_circuit_breaker_trips");
    private static readonly Histogram<double> QueryDuration = Meter.CreateHistogram<double>("polyglot_query_duration_ms");

    public static void RecordCacheHit(string entity) => CacheHits.Add(1, new("entity", entity));
    public static void RecordCacheMiss(string entity) => CacheMisses.Add(1, new("entity", entity));
    public static void RecordRetry(string database) => RetryAttempts.Add(1, new("database", database));
    public static void RecordCircuitBreaker(string database) => CircuitBreakerTrips.Add(1, new("database", database));
    public static void RecordQueryDuration(string entity, double ms) => QueryDuration.Record(ms, new("entity", entity));
}
```

---

## Summary Timeline

| Week | Phase | Key Deliverables |
|------|-------|------------------|
| 1-2 | Ardalis.Specification | Interface updates, specifications, repository updates |
| 2-3 | Polly Resilience | Retry/circuit breaker/timeout pipelines |
| 3-4 | Enhanced Caching | Generic cached repository, cache configuration |
| 4-5 | Integration | DI registration, testing, metrics |

---

## Related Documents

- [ADR-0014: Polyglot Persistence Framework Selection](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [Repository Architecture](../architecture/migrations/repository-architecture.md)
- [Hybrid Data Strategy Roadmap](./hybrid-data-strategy-roadmap.md)
