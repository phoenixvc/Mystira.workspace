# ADR-0014: Polyglot Persistence Framework Selection

## Status

**Accepted** - 2025-12-24

Decision accepted to implement permanent polyglot persistence architecture. Key clarifications:
- **No full PostgreSQL migration** - Cosmos DB remains the primary document store
- Each database serves its optimal use case permanently
- PolyglotRepository provides unified access with database-appropriate routing

## Context

Following [ADR-0013](./0013-data-management-and-storage-strategy.md), we implement **permanent polyglot persistence** across Mystira services. This is NOT a migration strategy - each database is chosen for its strengths:

- **Cosmos DB**: Document storage for scenarios, content, complex nested structures (primary store)
- **PostgreSQL**: Relational data for analytics, Story-Generator, structured queries
- **Redis**: Caching layer for sessions, frequently accessed data
- **Blob Storage**: Media assets with tiered storage

### Current State

| Layer              | Current Implementation | Technology    |
| ------------------ | ---------------------- | ------------- |
| ORM                | Entity Framework Core  | EF Core 9.0   |
| Repository Pattern | Custom repositories    | Per-entity    |
| Unit of Work       | Custom UoW             | `IUnitOfWork` |
| Caching            | None                   | -             |
| Query Pattern      | Direct DbContext       | LINQ          |

### Requirements

1. **Unified Repository Interface**: Abstract database technology from business logic
2. **Polyglot Routing**: Route entities to appropriate database (Cosmos vs PostgreSQL)
3. **Cache Integration**: Transparent caching with cache-aside pattern
4. **Query Optimization**: Specification pattern for complex queries
5. **Transaction Coordination**: Cross-database consistency where needed
6. **Minimal Learning Curve**: Build on existing EF Core knowledge

## Decision Drivers

| Driver                   | Weight | Description                         |
| ------------------------ | ------ | ----------------------------------- |
| **Developer Experience** | 25%    | Easy to understand and use          |
| **Migration Support**    | 20%    | Supports gradual database migration |
| **Performance**          | 20%    | Low overhead, efficient queries     |
| **Maintainability**      | 15%    | Clean code, testable                |
| **Community/Support**    | 10%    | Active development, documentation   |
| **Cost**                 | 10%    | Licensing, operational costs        |

---

## Options Analysis

### Option 1: Enhanced Custom Repositories (Current Approach Extended)

**Description**: Extend current repository pattern with interfaces for dual-write and caching.

```
┌─────────────────────────────────────────────────────────────────┐
│                     Repository Architecture                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Application Layer                        │ │
│  │  IAccountRepository, IProfileRepository, etc.              │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              Cached Repository Decorator                    │ │
│  │  CachedAccountRepository wraps IAccountRepository          │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              Dual-Write Repository                          │ │
│  │  DualWriteAccountRepository → Cosmos + PostgreSQL          │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│              ┌───────────────┼───────────────┐                   │
│              ▼               ▼               ▼                   │
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ │
│  │ CosmosRepository │ │ PostgresRepository│ │  RedisCache     │ │
│  │ (EF Core Cosmos) │ │ (EF Core Npgsql) │ │  (IDistributed  │ │
│  │                  │ │                  │ │   Cache)        │ │
│  └──────────────────┘ └──────────────────┘ └──────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**Implementation**:

```csharp
// Decorator pattern for caching
public class CachedAccountRepository : IAccountRepository
{
    private readonly IAccountRepository _inner;
    private readonly IDistributedCache _cache;

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var cacheKey = $"account:{id}";
        var cached = await _cache.GetAsync<Account>(cacheKey, ct);
        if (cached is not null) return cached;

        var account = await _inner.GetByIdAsync(id, ct);
        if (account is not null)
            await _cache.SetAsync(cacheKey, account, ct);
        return account;
    }
}

// Dual-write for migration
public class DualWriteAccountRepository : IAccountRepository
{
    private readonly IAccountRepository _cosmos;
    private readonly IAccountRepository _postgres;
    private readonly MigrationPhase _phase;

    public async Task<Account> CreateAsync(Account account, CancellationToken ct)
    {
        // Always write to both during migration
        await _postgres.CreateAsync(account, ct);
        await _cosmos.CreateAsync(account, ct);
        return account;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _phase switch
        {
            MigrationPhase.CosmosOnly => await _cosmos.GetByIdAsync(id, ct),
            MigrationPhase.DualWriteCosmosRead => await _cosmos.GetByIdAsync(id, ct),
            MigrationPhase.DualWritePostgresRead => await _postgres.GetByIdAsync(id, ct),
            MigrationPhase.PostgresOnly => await _postgres.GetByIdAsync(id, ct),
            _ => await _cosmos.GetByIdAsync(id, ct)
        };
    }
}
```

**Pros**:

- Full control over implementation
- No external dependencies
- Builds on existing codebase
- Lightweight, minimal overhead

**Cons**:

- Significant development effort
- Need to implement all patterns from scratch
- Testing complexity
- Missing battle-tested edge case handling

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Developer Experience | 4 | 1.00 |
| Migration Support | 5 | 1.00 |
| Performance | 5 | 1.00 |
| Maintainability | 3 | 0.45 |
| Community/Support | 2 | 0.20 |
| Cost | 5 | 0.50 |
| **Total** | | **4.15** |

---

### Option 2: Ardalis.Specification + Custom Infrastructure

**Description**: Use [Ardalis.Specification](https://github.com/ardalis/Specification) for query pattern, build custom infrastructure around it.

```
┌─────────────────────────────────────────────────────────────────┐
│                Ardalis.Specification Architecture               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Specification Layer                      │ │
│  │  AccountByEmailSpec, ActiveAccountsSpec, etc.              │ │
│  │  (Reusable query definitions)                              │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │           IReadRepository<T> / IRepository<T>               │ │
│  │  (Generic repository with specification support)           │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              EfRepository<T> Implementation                 │ │
│  │  (Works with any EF Core provider)                         │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│              ┌───────────────┴───────────────┐                   │
│              ▼                               ▼                   │
│  ┌──────────────────────────┐  ┌──────────────────────────┐     │
│  │   CosmosDbContext        │  │   PostgresDbContext      │     │
│  └──────────────────────────┘  └──────────────────────────┘     │
└─────────────────────────────────────────────────────────────────┘
```

**Implementation**:

```csharp
// Specification definition
public class AccountByEmailSpec : Specification<Account>, ISingleResultSpecification<Account>
{
    public AccountByEmailSpec(string email)
    {
        Query.Where(a => a.Email.ToLower() == email.ToLower());
    }
}

// Repository usage
public class AccountService
{
    private readonly IReadRepository<Account> _repository;

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var spec = new AccountByEmailSpec(email);
        return await _repository.FirstOrDefaultAsync(spec, ct);
    }
}
```

**Pros**:

- Clean specification pattern
- Works with any EF Core provider
- Strong community (Steve Smith / Ardalis)
- Well-documented
- Easy to test specifications

**Cons**:

- Doesn't solve dual-write problem
- No built-in caching
- Need to build infrastructure around it

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Developer Experience | 5 | 1.25 |
| Migration Support | 2 | 0.40 |
| Performance | 4 | 0.80 |
| Maintainability | 5 | 0.75 |
| Community/Support | 4 | 0.40 |
| Cost | 5 | 0.50 |
| **Total** | | **4.10** |

---

### Option 3: ABP Framework (Volo.Abp)

**Description**: Use [ABP Framework](https://abp.io/) infrastructure modules for repository, unit of work, and caching.

```
┌─────────────────────────────────────────────────────────────────┐
│                    ABP Framework Stack                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                    Application Layer                        │ │
│  │  Inherits from ApplicationService, uses built-in repos    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │           ABP Repository Abstractions                       │ │
│  │  IRepository<TEntity, TKey>, IReadOnlyRepository           │ │
│  │  Built-in specifications, paging, filtering                │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│                              ▼                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              ABP EF Core Module                             │ │
│  │  AbpDbContext, auto unit of work, auditing                 │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              ABP Caching Module                             │ │
│  │  IDistributedCache<T>, Redis integration                   │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**:

- Complete framework with all patterns built-in
- Multi-tenancy support
- Excellent documentation
- Active development
- Commercial support available

**Cons**:

- Heavyweight, requires significant refactoring
- Opinionated, may conflict with existing patterns
- Overkill for just persistence layer
- Learning curve

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Developer Experience | 3 | 0.75 |
| Migration Support | 4 | 0.80 |
| Performance | 4 | 0.80 |
| Maintainability | 4 | 0.60 |
| Community/Support | 5 | 0.50 |
| Cost | 3 | 0.30 |
| **Total** | | **3.75** |

---

### Option 4: Hybrid - Ardalis.Specification + Custom Dual-Write + StackExchange.Redis

**Description**: Combine best-of-breed libraries with targeted custom infrastructure.

```
┌─────────────────────────────────────────────────────────────────┐
│                    Hybrid Architecture                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                Ardalis.Specification                        │ │
│  │  Query patterns, composable specifications                 │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │           Custom Polyglot Repository Layer                  │ │
│  │  PolyglotRepository<T> : IRepository<T>                    │ │
│  │  - Migration phase aware                                    │ │
│  │  - Cache-aside pattern                                      │ │
│  │  - Health checks per backend                                │ │
│  └────────────────────────────────────────────────────────────┘ │
│                              │                                   │
│              ┌───────────────┼───────────────┐                   │
│              ▼               ▼               ▼                   │
│  ┌────────────────┐ ┌────────────────┐ ┌────────────────┐       │
│  │ EF Core Cosmos │ │ EF Core Npgsql │ │ StackExchange  │       │
│  │                │ │                │ │ .Redis         │       │
│  └────────────────┘ └────────────────┘ └────────────────┘       │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              Polly Resilience Layer                         │ │
│  │  Retry, circuit breaker, timeout                           │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**NuGet Packages**:

```xml
<PackageReference Include="Ardalis.Specification" Version="8.0.0" />
<PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.8.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.0.0" />
<PackageReference Include="Polly" Version="8.4.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
```

**Implementation**:

```csharp
// Polyglot repository with all features
public class PolyglotRepository<T> : IRepository<T> where T : class, IEntity
{
    private readonly IDbContextFactory<CosmosDbContext> _cosmosFactory;
    private readonly IDbContextFactory<PostgresDbContext> _postgresFactory;
    private readonly IDistributedCache _cache;
    private readonly IOptions<MigrationOptions> _options;
    private readonly ResiliencePipeline _resilience;

    public async Task<T?> GetByIdAsync(Guid id, ISpecification<T>? spec, CancellationToken ct)
    {
        // 1. Check cache
        var cached = await GetFromCacheAsync(id, ct);
        if (cached is not null) return cached;

        // 2. Get from appropriate database based on migration phase
        var entity = await _resilience.ExecuteAsync(async token =>
        {
            return _options.Value.Phase switch
            {
                MigrationPhase.PostgresOnly => await GetFromPostgresAsync(id, spec, token),
                _ => await GetFromCosmosAsync(id, spec, token)
            };
        }, ct);

        // 3. Cache result
        if (entity is not null)
            await SetCacheAsync(id, entity, ct);

        return entity;
    }
}
```

**Pros**:

- Best-of-breed for each concern
- Ardalis.Specification for queries (proven pattern)
- StackExchange.Redis for high-performance caching
- Polly for resilience
- Custom dual-write exactly as needed
- Incremental adoption

**Cons**:

- Multiple dependencies to manage
- Custom integration code needed
- Testing complexity

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Developer Experience | 4 | 1.00 |
| Migration Support | 5 | 1.00 |
| Performance | 5 | 1.00 |
| Maintainability | 4 | 0.60 |
| Community/Support | 4 | 0.40 |
| Cost | 5 | 0.50 |
| **Total** | | **4.50** |

---

## Decision Matrix Summary

| Option             | DevEx | Migration | Perf | Maintain | Community | Cost | **Total** |
| ------------------ | ----- | --------- | ---- | -------- | --------- | ---- | --------- |
| 1. Custom Extended | 4     | 5         | 5    | 3        | 2         | 5    | **4.15**  |
| 2. Ardalis.Spec    | 5     | 2         | 4    | 5        | 4         | 5    | **4.10**  |
| 3. ABP Framework   | 3     | 4         | 4    | 4        | 5         | 3    | **3.75**  |
| 4. Hybrid Approach | 4     | 5         | 5    | 4        | 4         | 5    | **4.50**  |

---

## Recommendation

### **Option 4: Hybrid Approach (Permanent Polyglot)**

Implement a hybrid solution combining:

1. **Ardalis.Specification** for query patterns
2. **Custom PolyglotRepository** for database-appropriate routing (NOT migration)
3. **StackExchange.Redis** for distributed caching
4. **Polly** for resilience patterns
5. **Wolverine EF Core** for saga/outbox persistence

### Database Routing Strategy

| Entity Type | Primary Store | Rationale |
|-------------|---------------|-----------|
| Scenarios, Content Bundles | Cosmos DB | Complex nested documents, flexible schema |
| User Profiles, Sessions | Cosmos DB | Document-oriented, low-latency reads |
| Analytics, Metrics | PostgreSQL | Relational queries, aggregations |
| Story Generator data | PostgreSQL | Structured LLM responses, embeddings |
| Saga/Outbox state | PostgreSQL | Transactional consistency for Wolverine |

### Implementation in Mystira.Shared

All infrastructure is consolidated in `Mystira.Shared`:

```
Mystira.Shared/
├── Data/
│   ├── Repositories/       # IRepository, RepositoryBase ✅
│   ├── Specifications/     # ISpecification, BaseSpecification ✅
│   ├── Entities/          # Entity, AuditableEntity ✅
│   └── Polyglot/          # PolyglotRepository, routing config ⏳
├── Caching/               # ICacheService, Redis ✅
├── Resilience/            # Polly policies ✅
└── Messaging/             # Wolverine integration ✅
```

### Implementation Status

| Component | Package | Status |
|-----------|---------|--------|
| Specification Pattern | `Mystira.Shared.Data.Specifications` | ✅ Complete |
| Repository Base | `Mystira.Shared.Data.Repositories` | ✅ Complete |
| Redis Caching | `Mystira.Shared.Caching` | ✅ Complete |
| Polly Resilience | `Mystira.Shared.Resilience` | ✅ Complete |
| Wolverine Messaging | `Mystira.Shared.Messaging` | ✅ Complete |
| Ardalis.Specification | NuGet integration | ✅ Complete |
| PolyglotRepository | `Mystira.Shared.Data.Polyglot` | ✅ Complete |
| Wolverine EF Core | NuGet integration | ✅ Complete |

---

## Consequences

### Positive

- **Database-appropriate storage**: Each data type uses optimal database technology
- **Clean separation of concerns**: Unified interface hides persistence complexity
- **High performance with caching**: Redis cache-aside pattern for hot data
- **Battle-tested components**: Ardalis.Specification, Polly, StackExchange.Redis
- **Minimal vendor lock-in**: Standard abstractions allow swapping implementations

### Negative

- Initial setup complexity for PolyglotRepository
- Multiple packages to maintain
- Need to coordinate database updates for same entity

### Risks & Mitigations

| Risk                               | Mitigation                                   |
| ---------------------------------- | -------------------------------------------- |
| Ardalis.Spec version conflicts     | Pin versions, test upgrades                  |
| Redis cache stampede               | Use cache locks for expensive queries        |
| Cross-database consistency         | Use Wolverine saga/outbox for coordination   |
| Testing complexity                 | Use in-memory providers for unit tests       |
| Wrong database choice for entity   | Document routing rationale, review in PRs    |

---

## References

- [Ardalis.Specification](https://github.com/ardalis/Specification)
- [The .NET Architect's Guide to Polyglot Persistence](https://developersvoice.com/blog/scalability/dotnet-polyglot-persistence-guide/)
- [Martin Fowler: Polyglot Persistence](https://martinfowler.com/bliki/PolyglotPersistence.html)
- [Onion-DDD-Tooling-DotNET](https://github.com/josecuellar/Onion-DDD-Tooling-DotNET)
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
- [Polly Resilience](https://github.com/App-vNext/Polly)
