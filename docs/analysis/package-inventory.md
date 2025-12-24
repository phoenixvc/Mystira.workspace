# Mystira Package Inventory

**Last Updated**: 2025-12-24
**Purpose**: Analyze packages for potential consolidation and optimal placement

---

## Consolidation Status (ADR-0020)

| Status | Package | Action |
|--------|---------|--------|
| ✅ Complete | `@mystira/contracts` | Unified TypeScript contracts |
| ✅ Complete | `Mystira.Contracts` | Unified .NET contracts |
| ✅ Complete | `@mystira/shared-utils` | Moved to workspace level |
| ✅ Complete | `Mystira.Shared` | .NET shared auth/infrastructure |
| ⏳ Deprecated | `@mystira/app-contracts` | Use `@mystira/contracts/app` |
| ⏳ Deprecated | `@mystira/story-generator-contracts` | Use `@mystira/contracts/story-generator` |
| ⏳ Deprecated | `Mystira.App.Contracts` | Use `Mystira.Contracts.App` |
| ⏳ Deprecated | `Mystira.StoryGenerator.Contracts` | Use `Mystira.Contracts.StoryGenerator` |
| ⏳ Deprecated | `Mystira.App.Shared` | Use `Mystira.Shared` |

---

## Package Inventory Summary

### NPM Packages (Workspace Level)

| Package | Location | Status | Notes |
|---------|----------|--------|-------|
| `@mystira/contracts` | `packages/contracts` | ✅ Active | Unified API types with subpath exports |
| `@mystira/shared-utils` | `packages/shared-utils` | ✅ Active | Retry, logging, validation utilities |

### NPM Packages (Submodules)

| Package | Current Location | Type | Could Combine? | Optimal Destination | Notes |
|---------|------------------|------|----------------|---------------------|-------|
| `@mystira/app` | `packages/app` (submodule) | NPM | ❌ Keep separate | Submodule | Core app - linked with contracts |
| `@mystira/app-contracts` | `packages/app` (submodule) | NPM | ⏳ Deprecated | Use `@mystira/contracts/app` | Migrating to unified contracts |
| `@mystira/story-generator` | `packages/story-generator` (submodule) | NPM | ❌ Keep separate | Submodule | Core service - linked with contracts |
| `@mystira/story-generator-contracts` | `packages/story-generator` (submodule) | NPM | ⏳ Deprecated | Use `@mystira/contracts/story-generator` | Migrating to unified contracts |
| `@mystira/publisher` | `packages/publisher` (submodule) | NPM | ❌ Keep separate | Submodule | Independent service |

### NuGet Packages (Workspace Level)

| Package | Location | Status | Notes |
|---------|----------|--------|-------|
| `Mystira.Contracts` | `packages/contracts/dotnet` | ✅ Active | Unified .NET contracts |
| `Mystira.Shared` | `packages/shared/Mystira.Shared` | ✅ Active | Auth, authorization, telemetry |

### NuGet Packages (Submodules)

| Package | Current Location | Type | Could Combine? | Optimal Destination | Notes |
|---------|------------------|------|----------------|---------------------|-------|
| `Mystira.App.Domain` | `packages/app/src/` | NuGet | ❌ Keep | Submodule | Core domain - consumed by Admin.Api via NuGet |
| `Mystira.App.Application` | `packages/app/src/` | NuGet | ❌ Keep | Submodule | Application layer - consumed by Admin.Api |
| `Mystira.App.Shared` | `packages/app/src/` | NuGet | ⏳ Deprecated | Use `Mystira.Shared` | Migrating to workspace-level shared |
| `Mystira.App.Contracts` | `packages/app/src/` | NuGet | ⏳ Deprecated | Use `Mystira.Contracts.App` | Migrating to unified contracts |
| `Mystira.StoryGenerator.Contracts` | `packages/story-generator/src/` | NuGet | ⏳ Deprecated | Use `Mystira.Contracts.StoryGenerator` | Migrating to unified contracts |

### Docker Images

| Image | Current Location | Could Combine? | Optimal Destination | Notes |
|-------|------------------|----------------|---------------------|-------|
| `publisher` | `infra/docker/publisher/` | ❌ Keep | Move to submodule (ADR-0019) | Independent service |
| `chain` | `infra/docker/chain/` | ❌ Keep | Move to submodule (ADR-0019) | Independent service |
| `story-generator` | `infra/docker/story-generator/` | ❌ Keep | Move to submodule (ADR-0019) | Independent service |
| `admin-api` | Submodule | ❌ Keep | Submodule | Already in correct location |

### Services (Non-Package)

| Service | Current Location | Could Combine? | Optimal Destination | Notes |
|---------|------------------|----------------|---------------------|-------|
| Mystira.App.Api | `packages/app` | ❌ Keep | Submodule | Public API - Azure App Service |
| Mystira.App.PWA | `packages/app` | ❌ Keep | Submodule | Blazor WASM - Static Web App |
| Mystira.StoryGenerator.Api | `packages/story-generator` | ❌ Keep | Submodule | API service - Kubernetes |
| Mystira.StoryGenerator.Web | `packages/story-generator` | ❌ Keep | Submodule | Blazor WASM - Static Web App |
| Mystira.Admin.Api | `packages/admin-api` | ❌ Keep | Separate submodule | Security isolation required |
| Mystira.Admin.UI | `packages/admin-ui` | ❌ Keep | Separate submodule | Security isolation required |
| Mystira.Chain | `packages/chain` | ❌ Keep | Submodule | Blockchain - Kubernetes |
| Mystira.DevHub | `packages/devhub` | ❌ Keep | Submodule | Developer portal |

---

## Consolidation Recommendations

### High Priority - Should Consolidate

#### 1. Create `@mystira/contracts` (Unified TypeScript Types)

**Combine:**
- `@mystira/app-contracts`
- `@mystira/story-generator-contracts`

**Benefits:**
- Single source of truth for API types
- Simplified dependency management
- Consistent versioning across all TypeScript consumers
- Reduces duplication in client apps

**Implementation:**
```
packages/
└── contracts/           # NEW workspace package
    ├── package.json     # @mystira/contracts
    ├── src/
    │   ├── app/         # App API types
    │   ├── story-generator/  # Story Generator types
    │   └── index.ts     # Unified exports
    └── tsconfig.json
```

#### 2. Create `Mystira.Contracts` (Unified NuGet Package)

**Combine:**
- `Mystira.App.Contracts`
- `Mystira.StoryGenerator.Contracts`

**Benefits:**
- Single NuGet package for all API contracts
- Simplified reference management in consuming projects
- Consistent versioning across .NET consumers

**Implementation:**
```
packages/
└── contracts/           # NEW workspace package (or separate submodule)
    └── src/
        └── Mystira.Contracts/
            ├── App/     # App contracts
            ├── StoryGenerator/  # Story Generator contracts
            └── Mystira.Contracts.csproj
```

#### 3. Move `@mystira/shared-utils` to Workspace

**Current:** Inside `packages/publisher` submodule
**Optimal:** Workspace-level `packages/shared-utils`

**Benefits:**
- Truly shared across all packages
- Independent versioning from publisher
- Cleaner dependency graph

---

### Medium Priority - Consider Consolidating

#### 4. Create `Mystira.Shared` NuGet Package

**Evaluate combining:**
- `Mystira.App.Shared`
- Common infrastructure utilities

**Benefits:**
- Shared cross-cutting concerns
- Reduces duplication

**Considerations:**
- May increase coupling between services
- Need to carefully separate app-specific from truly shared code

---

### Keep Separate (No Consolidation)

| Package | Reason |
|---------|--------|
| `Mystira.App.Domain` | Core domain logic - tightly coupled to App |
| `Mystira.App.Application` | Application layer - tightly coupled to App |
| `Mystira.App.Infrastructure.*` | Infrastructure implementations - service-specific |
| Service containers | Each has unique runtime requirements |
| Admin API/UI | Security isolation required |

---

## Recommended Package Structure

### Current State
```
packages/
├── app/                          # Submodule (Mystira.App)
│   └── src/
│       ├── Mystira.App.Contracts/  ← Could extract
│       └── ...
├── story-generator/              # Submodule (Mystira.StoryGenerator)
│   └── src/
│       ├── Mystira.StoryGenerator.Contracts/  ← Could extract
│       └── ...
├── publisher/                    # Submodule (Mystira.Publisher)
│   └── packages/
│       └── shared-utils/         ← Could move to workspace
├── admin-api/                    # Submodule
├── admin-ui/                     # Submodule
├── chain/                        # Submodule
└── devhub/                       # Submodule
```

### Proposed State (After Consolidation)
```
packages/
├── contracts/                    # NEW - Workspace native
│   ├── package.json              # @mystira/contracts (NPM)
│   ├── src/
│   │   ├── typescript/           # TypeScript types
│   │   │   ├── app/
│   │   │   ├── story-generator/
│   │   │   └── index.ts
│   │   └── dotnet/               # .NET contracts
│   │       └── Mystira.Contracts/
│   │           ├── App/
│   │           └── StoryGenerator/
│   └── Mystira.Contracts.csproj  # NuGet package
│
├── shared-utils/                 # NEW - Workspace native
│   ├── package.json              # @mystira/shared-utils
│   └── src/
│
├── app/                          # Submodule (unchanged structure)
├── story-generator/              # Submodule (unchanged structure)
├── publisher/                    # Submodule (remove shared-utils)
├── admin-api/                    # Submodule
├── admin-ui/                     # Submodule
├── chain/                        # Submodule
└── devhub/                       # Submodule
```

---

## Migration Impact

### If We Consolidate Contracts

| Impact Area | Changes Required |
|-------------|------------------|
| **NPM Consumers** | Update imports from `@mystira/app-contracts` → `@mystira/contracts/app` |
| **NuGet Consumers** | Update package reference to `Mystira.Contracts` |
| **Changesets Config** | Update linked packages |
| **Release Workflow** | Simplify - single contracts package instead of multiple |
| **Submodule Dispatch** | Update to trigger `contracts-publish` instead of separate events |

### Versioning After Consolidation

```json
// .changeset/config.json
{
  "linked": [
    ["@mystira/contracts"],  // Unified contracts
    ["@mystira/app"],
    ["@mystira/story-generator"],
    ["@mystira/publisher", "@mystira/shared-utils"]
  ]
}
```

---

## Decision Matrix

| Package | Keep Separate | Consolidate | Move to Workspace |
|---------|:-------------:|:-----------:|:-----------------:|
| `@mystira/app` | ✅ | | |
| `@mystira/app-contracts` | | ✅ | ✅ |
| `@mystira/story-generator` | ✅ | | |
| `@mystira/story-generator-contracts` | | ✅ | ✅ |
| `@mystira/publisher` | ✅ | | |
| `@mystira/shared-utils` | | | ✅ |
| `Mystira.App.Domain` | ✅ | | |
| `Mystira.App.Application` | ✅ | | |
| `Mystira.App.Contracts` | | ✅ | ✅ |
| `Mystira.StoryGenerator.Contracts` | | ✅ | ✅ |

---

## Infrastructure Pattern Analysis (Phase 4)

Detailed code-level analysis of App and StoryGenerator submodules identified shared patterns that could be consolidated into `Mystira.Shared`.

### Pattern Comparison

| Pattern | App | StoryGenerator | Consolidation Opportunity |
|---------|-----|----------------|---------------------------|
| **Repository** | 23 implementations with generic `Repository<T>` base | None (service-based) | Keep App-specific |
| **Polly Resilience** | `Microsoft.Extensions.Http.Polly` (11 clients) | Custom `RetryPolicyService` | 🔴 High - Standardize |
| **Caching** | `IMemoryCache` with MediatR pipeline | `ConcurrentDictionary` stores | 🟡 Medium - Add Redis |
| **Error Handling** | `ExceptionDetailsHelper` + try-catch | Response object pattern | 🟡 Medium - ProblemDetails |
| **Base Entities** | 3-level hierarchy (Entity→Auditable→SoftDeletable) | No ORM entities | Keep App-specific |
| **HTTP Clients** | 11 typed clients + `BaseApiClient` | 2 LLM service clients | 🟢 Low - Patterns differ |

### Key Findings

#### App (packages/app)

**Repository Pattern** (`Mystira.App.Infrastructure.Data/Repositories/`):
- Generic `Repository<T>` with specification support
- 23 specialized repositories (GameSession, UserProfile, Account, etc.)
- UnitOfWork pattern for transactions
- Well-designed but App-specific

**Resilience** (`Mystira.App.PWA/Program.cs:69-172`):
```csharp
// Same policy duplicated for 11 HTTP clients
IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy(string clientName)
{
    var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    var circuitBreaker = HttpPolicyExtensions.HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    var timeout = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));
    return Policy.WrapAsync(timeout, retryPolicy, circuitBreaker);
}
```

**Caching** (`Mystira.App.Application/Behaviors/QueryCachingBehavior.cs`):
- MediatR pipeline behavior for query caching
- Uses `IMemoryCache` (single-instance limitation)
- No Redis/distributed cache support

#### StoryGenerator (packages/story-generator)

**Custom Retry** (`Mystira.StoryGenerator.RagIndexer/Services/RetryPolicyService.cs`):
```csharp
public async Task<T> ExecuteWithRetryAsync<T>(
    Func<Task<T>> operation, string operationName,
    int maxRetries = 3, int delayMs = 1000)
{
    int attempt = 0;
    while (true) {
        attempt++;
        try { return await operation(); }
        catch (Exception ex) when (attempt < maxRetries) {
            await Task.Delay(delayMs * attempt); // Manual exponential backoff
        }
    }
}
```

**In-Memory State** (`Mystira.StoryGenerator.Api/Services/ContinuityAsync/`):
- `InMemoryContinuityOperationStore` using `ConcurrentDictionary`
- `ContinuityBackgroundQueue` using `Channel<T>`
- Not suitable for multi-instance deployments

### Submodule Initialization Status

| Submodule | Status | Analysis |
|-----------|--------|----------|
| `packages/app` | ✅ Initialized | Full analysis completed |
| `packages/story-generator` | ✅ Initialized | Full analysis completed |
| `packages/publisher` | ✅ Initialized | React + Zustand + Axios + comprehensive design tokens |
| `packages/admin-api` | ✅ Initialized | .NET 9 API with Redis caching, JWT + Entra ID auth |
| `packages/devhub` | ✅ Initialized | Tauri desktop app with Rust backend, Zustand stores |
| `packages/admin-ui` | ✅ Initialized | React + Bootstrap 5 + Zustand + React Query |
| `packages/chain` | ✅ Initialized | Blockchain service |

---

## Design System Analysis (Phase 4e)

### Current State: Fragmented Design Tokens

| Package | Framework | Primary Color | Token System |
|---------|-----------|---------------|--------------|
| Publisher | Custom CSS | `#9333ea` (Purple) | ✅ Comprehensive (variables.css) |
| App/PWA | Custom CSS | `#7c3aed` (Purple) | 🟡 Partial (app.css) |
| DevHub | Tailwind | `#0ea5e9` (Sky Blue) | 🟡 Tailwind defaults |
| Story Generator | Tailwind | `#3b82f6` (Blue) | 🟡 Tailwind defaults |
| Admin UI | Bootstrap 5 | `#4e73df` (Blue) | ❌ Bootstrap defaults |

### Publisher's Token System (Reference Implementation)

Publisher has the most comprehensive design token system at `/packages/publisher/src/styles/variables.css`:

```css
/* Color System */
--color-primary-50 through --color-primary-900 (purple gradient)
--color-neutral-50 through --color-neutral-900
--color-success, --color-warning, --color-danger, --color-info

/* Typography */
--font-family-sans: 'Inter', system-ui, sans-serif
--font-family-mono: 'Fira Code', monospace
--font-size-xs through --font-size-4xl (8 scales)
--font-weight-normal/medium/semibold/bold

/* Spacing */
--spacing-0 through --spacing-16 (11-point scale)

/* Components */
--radius-sm/md/lg/xl/full
--shadow-sm/md/lg/xl
--transition-fast/base/slow
--z-dropdown/sticky/modal/tooltip/toast
```

### Frontend State Management Comparison

| Package | State Manager | HTTP Client | Form Validation |
|---------|---------------|-------------|-----------------|
| Publisher | Zustand v5 | Axios | Zod + React Hook Form |
| Admin-UI | Zustand v5 | Axios | Zod + React Hook Form |
| DevHub | Zustand v4 | Tauri IPC | N/A (CLI-based) |
| App/PWA | Blazor state | HttpClient | DataAnnotations |
| Story Generator | Blazor state | HttpClient | DataAnnotations |

### Proposed Design Token Package

```
packages/
└── design-tokens/
    ├── package.json           # @mystira/design-tokens
    ├── src/
    │   ├── colors.ts          # Unified color palette
    │   ├── typography.ts      # Font system
    │   ├── spacing.ts         # Spacing scale
    │   └── index.ts
    ├── css/
    │   └── variables.css      # CSS custom properties
    └── tailwind/
        └── preset.js          # Tailwind preset for DevHub, StoryGen
```

### Proposed Mystira.Shared Namespaces

Based on comprehensive analysis, the following namespaces should be added to `Mystira.Shared`:

#### High Priority (⭐⭐⭐)

| Namespace | Description | Source Patterns |
|-----------|-------------|-----------------|
| `Mystira.Shared.Data` | Repository + Specifications | App: `IRepository<T>`, `Repository<T>`, `ISpecification<T>` |
| `Mystira.Shared.Resilience` | Polly pipelines | App: `CreateResiliencePolicy()`, StoryGen: `RetryPolicyService` |
| `Mystira.Shared.Caching` | Redis + distributed cache | New (IMemoryCache → IDistributedCache) |
| `Mystira.Shared.Exceptions` | Error/Result patterns | App: `ErrorResponse`, `ValidationErrorResponse` |

#### Medium Priority (⭐⭐)

| Namespace | Description | Source Patterns |
|-----------|-------------|-----------------|
| `Mystira.Shared.Domain` | Base entities | App: `Entity`, `AuditableEntity`, `SoftDeletableEntity` |
| `Mystira.Shared.Http` | HTTP client config | App: `BaseApiClient`, handler patterns |
| `Mystira.Shared.Validation` | FluentValidation pipeline | App: MediatR validation behaviors |
| `Mystira.Shared.Api` | Response wrappers | Contracts: `ApiResponse<T>`, `ApiError` |
| `Mystira.Shared.Migration` | Dual-write helpers | New (for future migrations) |

### NuGet Package Dependencies

| Package | Version | Used By | Purpose |
|---------|---------|---------|---------|
| `Microsoft.Extensions.Http.Polly` | 9.0.0 | App.PWA | HTTP resilience policies |
| `Microsoft.EntityFrameworkCore` | 9.0.0 | App.Infrastructure | ORM |
| `Microsoft.EntityFrameworkCore.Cosmos` | 9.0.0 | App.Api, Admin.Api | Cosmos DB |
| `FluentValidation` | 11.11.0 | App.Application | Input validation |
| `MediatR` | 12.4.1 | App.Application | CQRS pattern ⚠️ **Migrating to Wolverine** |
| `Wolverine` | - | **Planned** | Unified messaging (replaces MediatR) |
| `StackExchange.Redis` | - | **Not used** | Need for distributed cache |
| `Polly` | via Http.Polly | App.PWA | Resilience policies |

### Consolidation Roadmap

#### .NET Backend Consolidation

| Phase | Action | Package | Status |
|-------|--------|---------|--------|
| 4a | Add `Mystira.Shared.Resilience` | `Mystira.Shared` | ✅ Complete |
| 4b | Add `Mystira.Shared.Exceptions` | `Mystira.Shared` | ✅ Complete |
| 4c | Add `Mystira.Shared.Caching` | `Mystira.Shared` | ✅ Complete |
| 4d | Add `Mystira.Shared.Data` | `Mystira.Shared` | ✅ Complete |
| 4h | **Migrate MediatR → Wolverine** | All .NET services | ⏳ Planned |

#### Wolverine Migration (ADR-0015 Accepted)

See [ADR-0015](../architecture/adr/0015-event-driven-architecture-framework.md) for full details.

| Phase | Action | Services | Status |
|-------|--------|----------|--------|
| W1 | Add Wolverine alongside MediatR | App.Application | ✅ Complete (Mystira.Shared.Messaging) |
| W2 | Migrate command/query handlers | App, StoryGenerator | ⏳ Planned |
| W3 | Add Azure Service Bus for events | All .NET services | ⏳ Planned |
| W4 | Remove MediatR dependency | All .NET services | ⏳ Planned |

#### Frontend/TypeScript Consolidation

| Phase | Action | Package | Status |
|-------|--------|---------|--------|
| 4e | Create `@mystira/design-tokens` | NPM | ✅ Complete |
| 4f | Standardize Tailwind preset | NPM | ✅ Complete (in design-tokens) |
| 4g | Consider `@mystira/ui-react` | NPM | 🔍 Evaluate |

See [ADR-0020](../architecture/adr/0020-package-consolidation-strategy.md#comprehensive-consolidation-matrix) for detailed implementation plans.

---

## Next Steps

### Completed ✅
1. ~~**Create `packages/contracts`** workspace package~~ ✅
2. ~~**Migrate TypeScript types** from submodules~~ ✅
3. ~~**Migrate NuGet contracts** from submodules~~ ✅
4. ~~**Move `shared-utils`** to workspace level~~ ✅
5. ~~**Update Changesets config** for new package structure~~ ✅
6. ~~**Update CI/CD workflows** for unified contracts publishing~~ ✅
7. ~~**Create `Mystira.Shared`** with auth infrastructure~~ ✅
8. ~~**Analyze App and StoryGenerator for consolidation**~~ ✅
9. ~~**Initialize all submodules**~~ ✅ (admin-api, admin-ui, publisher, devhub, chain)
10. ~~**Analyze design tokens and UI/UX assets**~~ ✅

### Phase 4: .NET Backend Consolidation (In Progress)
11. **Phase 4a**: Add `Mystira.Shared.Resilience` - Polly policies
12. **Phase 4b**: Add `Mystira.Shared.Exceptions` - Error/Result patterns
13. **Phase 4c**: Add `Mystira.Shared.Caching` - Redis distributed cache
14. **Phase 4d**: Add `Mystira.Shared.Data` - Repository + Specifications
15. **Phase 4h**: Migrate MediatR → Wolverine (ADR-0015 Accepted)

### Wolverine Migration (ADR-0015 Accepted)
16. **Phase W1**: Add Wolverine alongside MediatR in App.Application
17. **Phase W2**: Migrate command/query handlers to Wolverine convention
18. **Phase W3**: Add Azure Service Bus transport for distributed events
19. **Phase W4**: Remove MediatR dependency from all services

### Phase 4: Frontend/TypeScript Consolidation (In Progress)
20. **Phase 4e**: Create `@mystira/design-tokens` - Unified color/typography/spacing
21. **Phase 4f**: Create Tailwind preset for DevHub, Story Generator
22. **Phase 4g**: Evaluate `@mystira/ui-react` for shared components

### Phase 5: Cleanup (Pending)
23. Migrate services to use `Mystira.Shared` namespaces
24. Migrate frontends to use `@mystira/design-tokens`
25. Cleanup deprecated packages and workflows

---

## Related Documentation

- [Publishing Flow](../cicd/publishing-flow.md)
- [Package Releases Guide](../guides/package-releases.md)
- [ADR-0007: NuGet Feed Strategy](../architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md)
- [ADR-0009: App Segregation Strategy](../architecture/adr/0009-further-app-segregation-strategy.md)
- [ADR-0015: Event-Driven Architecture (Wolverine)](../architecture/adr/0015-event-driven-architecture-framework.md)
- [ADR-0020: Package Consolidation Strategy](../architecture/adr/0020-package-consolidation-strategy.md)
