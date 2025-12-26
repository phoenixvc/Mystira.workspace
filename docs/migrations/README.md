# Mystira Migration Guides

This directory contains migration guides for adopting `Mystira.Shared` infrastructure and consolidated packages across all Mystira services.

**Last Updated**: December 2026
**Current Target Runtime**: .NET 9.0 / Node.js 22+ / Python 3.12+

> **📋 See [MIGRATION_INDEX.md](./MIGRATION_INDEX.md)** for the consolidated migration status dashboard and document hierarchy.

---

## What's New (December 2026)

### Infrastructure Updates
- **Resource Group Reorganization** (ADR-0017): Multi-tier RG strategy with service isolation
- **Microsoft Entra External ID**: Migration from Azure AD B2C for modern authentication
- **Kubernetes Standardization**: Updated to use `labels` instead of deprecated `commonLabels`
- **Dockerfile Standardization** (ADR-0019): Dockerfiles now live in submodule repos

### Package Updates
- **Ardalis.Specification 8.0.0**: New specification pattern implementation
- **Polly v8 Resilience**: Replaced legacy `IAsyncPolicy` with `ResiliencePipeline`
- **Source Generators**: Auto-generate repositories and option validators
- **Distributed Locking**: Redis-based distributed locks for concurrency control
- **Circuit Breaker Observability**: OpenTelemetry-integrated circuit breaker events

---

## Prerequisites

Before starting any migration, ensure the following packages are published:

| Package | Registry | Version | Status |
|---------|----------|---------|--------|
| `Mystira.Shared` | NuGet | 0.2.0+ | ✅ Published |
| `Mystira.Contracts` | NuGet | 0.2.0+ | ✅ Published |
| `@mystira/design-tokens` | NPM | 0.2.0+ | ✅ Published |
| `@mystira/shared-utils` | NPM | 0.2.0+ | ✅ Published |

### Runtime Requirements

| Runtime | Minimum Version | Recommended |
|---------|-----------------|-------------|
| .NET | 9.0 | 9.0.1+ |
| Node.js | 20.x | 22.x LTS |
| Python | 3.11 | 3.12+ |
| pnpm | 9.x | 9.15+ |

---

## Migration Order (Recommended)

### Phase 1: Foundation (Completed)

Infrastructure and shared packages:

- [x] ADR-0015: Wolverine adoption
- [x] ADR-0014: Polyglot persistence
- [x] ADR-0017: Resource group organization
- [x] ADR-0019: Dockerfile location standardization
- [x] Mystira.Shared.Resilience (Polly v8)
- [x] Mystira.Shared.Exceptions
- [x] Mystira.Shared.Caching (Redis + WASM)
- [x] Mystira.Shared.Messaging (Wolverine)
- [x] Mystira.Shared.Data (Ardalis.Specification 8.0.0)
- [x] Mystira.Shared.Locking (Distributed locks)
- [x] @mystira/design-tokens

### Phase 2: Service Migrations

Migrate each service independently:

| Service | Guide | Priority | Effort | Status |
|---------|-------|----------|--------|--------|
| **Mystira.App** | [mystira-app-migration.md](./mystira-app-migration.md) | High | 2-3 days | 🔄 In Progress |
| **Mystira.Admin.Api** | [mystira-admin-migration.md](./mystira-admin-migration.md) | High | 1 day | 🔄 In Progress |
| **Mystira.StoryGenerator** | [mystira-storygenerator-migration.md](./mystira-storygenerator-migration.md) | Medium | 2 days | 🔄 In Progress |
| **Mystira.Publisher** | [mystira-publisher-migration.md](./mystira-publisher-migration.md) | Medium | 0.5-1 day | 📋 Planned |
| **Mystira.Chain** | [mystira-chain-migration.md](./mystira-chain-migration.md) | Medium | 1 day | 📋 Planned |
| **Mystira.DevHub** | [mystira-devhub-migration.md](./mystira-devhub-migration.md) | Low | 0.5 day | 📋 Planned |
| **Mystira.Admin.UI** | [mystira-admin-ui-migration.md](./mystira-admin-ui-migration.md) | Low | 0.5 day | 📋 Planned |

---

## Migration Scope by Package

### Mystira.Shared (NuGet) v0.2.0+

| Namespace | What It Provides | Services Using | New in v0.2.0 |
|-----------|------------------|----------------|---------------|
| `Mystira.Shared.Auth` | JWT, Entra External ID authentication | App, Admin | ✅ Entra External ID |
| `Mystira.Shared.Resilience` | Polly v8 resilience pipelines | App, StoryGen, Admin | ✅ Polly v8 |
| `Mystira.Shared.Caching` | Redis/Memory/WASM cache | App, Admin, PWA | ✅ WASM support |
| `Mystira.Shared.Exceptions` | Error handling, Result<T> | All .NET services | |
| `Mystira.Shared.Messaging` | Wolverine integration | App, StoryGen | |
| `Mystira.Shared.Data` | Ardalis.Specification 8.0.0 | App, Admin | ✅ Specification 8.0 |
| `Mystira.Shared.Data.Polyglot` | Multi-database routing | App, Admin | |
| `Mystira.Shared.Locking` | Redis distributed locks | App, StoryGen | ✅ New |
| `Mystira.Shared.Generators` | Source generators for repos/validators | All .NET services | ✅ New |
| `Mystira.Shared.Observability` | Circuit breaker events, telemetry | All .NET services | ✅ New |

### @mystira/design-tokens (NPM) v0.2.0+

| Export | What It Provides | Services Using |
|--------|------------------|----------------|
| `colors` | Color palette (primary, neutral, semantic) | Publisher, Admin-UI, DevHub |
| `typography` | Font families, sizes, weights | Publisher, Admin-UI, DevHub |
| `spacing` | Spacing scale (0-16) | Publisher, Admin-UI, DevHub |
| `components` | Component tokens (radius, shadow) | Publisher, Admin-UI, DevHub |
| CSS variables | `variables.css` | All frontends |
| Tailwind preset | `preset.js` | DevHub, StoryGen Web |
| Dark mode | `dark-mode.css` | All frontends |

### @mystira/shared-utils (NPM) v0.2.0+

| Export | What It Provides | Services Using |
|--------|------------------|----------------|
| `retry` | Exponential backoff retry | Publisher, DevHub, Admin-UI |
| `logger` | Structured logging | All TypeScript services |
| `validateSchema` | Zod schema validation | Publisher, Admin-UI |
| `formatDate` | Date formatting utilities | All frontends |
| `debounce` / `throttle` | Rate limiting utilities | All frontends |
| `httpClient` | Pre-configured fetch wrapper | Publisher, DevHub |

---

## Key Changes Summary

### MediatR → Wolverine

| Aspect | MediatR | Wolverine |
|--------|---------|-----------|
| Handler signature | `IRequestHandler<TRequest, TResponse>` | Static method with convention |
| Dependency injection | Constructor | Method parameters |
| Pipeline behaviors | `IPipelineBehavior<,>` | Middleware chain |
| Distributed messaging | None | Azure Service Bus built-in |

### Polly v7 → Polly v8

| Aspect | Polly v7 | Polly v8 |
|--------|----------|----------|
| Policy type | `IAsyncPolicy<T>` | `ResiliencePipeline<T>` |
| Policy creation | `Policy.WrapAsync()` | `ResiliencePipelineBuilder` |
| HTTP integration | `AddPolicyHandler()` | `AddResilienceHandler()` |
| Factory method | `PolicyFactory.CreateStandardHttpPolicy()` | `ResiliencePipelineFactory.CreateStandardHttpPipeline()` |

### Custom Specifications → Ardalis.Specification 8.0.0

| Aspect | Before | After |
|--------|--------|-------|
| Query interface | `IRequest<T>` (MediatR) | `Specification<T>` |
| Repository pattern | Custom `IRepository<T>` | `IReadRepository<T>`, `IRepository<T>` |
| Single result | Custom query handlers | `ISingleResultSpecification<T>` |
| Pagination | Manual Skip/Take | Built-in `Skip()`, `Take()` |

### IMemoryCache → ICacheService (Redis/WASM)

| Aspect | Before | After |
|--------|--------|-------|
| Cache type | `IMemoryCache` | `ICacheService` (Redis/Memory/WASM) |
| Multi-instance | Not supported | Fully supported |
| Cache-aside | Manual | `GetOrSetAsync()` |
| WASM support | None | IndexedDB-backed cache |

### Azure AD B2C → Microsoft Entra External ID

| Aspect | Before | After |
|--------|--------|-------|
| Identity provider | Azure AD B2C | Microsoft Entra External ID |
| Terraform module | `aadb2c` | `entra-external-id` |
| Mobile support | Limited | Native mobile flows |
| User flows | B2C user flows | External ID authentication methods |

### Source Generators (New)

| Feature | Description |
|---------|-------------|
| `[GenerateRepository]` | Auto-generates repository implementations |
| `[GenerateValidator]` | Auto-generates IOptions validators |
| Compile-time | No runtime reflection overhead |

### Distributed Locking (New)

| Feature | Description |
|---------|-------------|
| `IDistributedLockService` | Redis-backed distributed locks |
| `ExecuteWithLockAsync()` | Scoped lock execution |
| `TryAcquireAsync()` | Non-blocking lock attempt |
| Lock extension | Extend lock duration during long operations |

---

## Rollback Strategy

Each migration guide includes rollback instructions. General strategy:

1. **Feature Flags**: Use configuration to toggle new infrastructure
2. **Gradual Migration**: Run old and new side-by-side during transition
3. **Separate PRs**: Each service migrated independently
4. **Backward Compatibility**: Old packages remain available
5. **Database Migrations**: Always reversible with down migrations

---

## Testing Requirements

Before merging any migration PR:

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] API contract tests pass
- [ ] Performance benchmarks (no regression)
- [ ] Visual regression tests (frontend)
- [ ] Specification tests for Ardalis.Specification migrations
- [ ] Distributed lock contention tests
- [ ] Circuit breaker state change tests

---

## Infrastructure Migration Checklist

For each environment (dev → staging → prod):

- [ ] Resource groups created per ADR-0017
- [ ] Key Vaults migrated to service-specific RGs
- [ ] Kubernetes manifests using `labels` (not `commonLabels`)
- [ ] Dockerfiles moved to submodule repos (ADR-0019)
- [ ] Microsoft Entra External ID configured (if applicable)
- [ ] Service Bus in core-rg with proper RBAC
- [ ] PostgreSQL AAD auth enabled

---

## Related Documentation

### Architecture Decision Records
- [ADR-0014: Polyglot Persistence](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [ADR-0015: Wolverine Migration](../architecture/adr/0015-event-driven-architecture-framework.md)
- [ADR-0017: Resource Group Organization](../architecture/adr/0017-resource-group-organization-strategy.md)
- [ADR-0019: Dockerfile Location Standardization](../adr/ADR-0019-dockerfile-location-standardization.md)
- [ADR-0020: Package Consolidation](../architecture/adr/0020-package-consolidation-strategy.md)

### Technical Guides
- [Ardalis.Specification 8.0.0 Migration](../architecture/specifications/ardalis-specification-migration.md)
- [Mystira.Shared Migration Guide](../guides/mystira-shared-migration.md)
- [Package Inventory](../analysis/package-inventory.md)
- [Caching Strategies (WASM + Redis)](../architecture/caching-strategies.md)

### Infrastructure
- [Azure Resources Migration Summary](../../MIGRATION_SUMMARY.md)
- [Terraform Environments](../../infra/terraform/environments/)
- [Kubernetes Manifests](../../infra/k8s/)
