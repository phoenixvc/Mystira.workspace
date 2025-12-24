# Mystira Migration Guides

This directory contains migration guides for adopting `Mystira.Shared` infrastructure and consolidated packages across all Mystira services.

---

## Prerequisites

Before starting any migration, ensure the following packages are published:

| Package | Registry | Version |
|---------|----------|---------|
| `Mystira.Shared` | NuGet | 0.1.0+ |
| `Mystira.Contracts` | NuGet | 0.1.0+ |
| `@mystira/design-tokens` | NPM | 0.1.0+ |
| `@mystira/shared-utils` | NPM | 0.1.0+ |

---

## Migration Order (Recommended)

### Phase 1: Foundation PR (Current)

Get the `Mystira.Shared` infrastructure merged first:

- [x] ADR-0015: Wolverine adoption
- [x] ADR-0014: Polyglot persistence
- [x] Mystira.Shared.Resilience
- [x] Mystira.Shared.Exceptions
- [x] Mystira.Shared.Caching
- [x] Mystira.Shared.Messaging
- [x] Mystira.Shared.Data (Repository, Specification, Polyglot)
- [x] @mystira/design-tokens

### Phase 2: Service Migrations (Separate PRs)

Migrate each service independently after Phase 1 is merged:

| Service | Guide | Priority | Effort |
|---------|-------|----------|--------|
| **Mystira.App** | [mystira-app-migration.md](./mystira-app-migration.md) | High | 2-3 days |
| **Mystira.Admin** | [mystira-admin-migration.md](./mystira-admin-migration.md) | High | 1 day |
| **Mystira.StoryGenerator** | [mystira-storygenerator-migration.md](./mystira-storygenerator-migration.md) | Medium | 2 days |
| **Mystira.Publisher** | [mystira-publisher-migration.md](./mystira-publisher-migration.md) | Low | 0.5-1 day |

---

## Migration Scope by Package

### Mystira.Shared (NuGet)

| Namespace | What It Provides | Services Using |
|-----------|------------------|----------------|
| `Mystira.Shared.Auth` | JWT, Entra ID authentication | App, Admin |
| `Mystira.Shared.Resilience` | Polly HTTP policies | App, StoryGen, Admin |
| `Mystira.Shared.Caching` | Redis/Memory cache | App, Admin |
| `Mystira.Shared.Exceptions` | Error handling, Result<T> | All .NET services |
| `Mystira.Shared.Messaging` | Wolverine integration | App, StoryGen |
| `Mystira.Shared.Data` | Repository, Specification | App, Admin |
| `Mystira.Shared.Data.Polyglot` | Multi-database routing | App, Admin |

### @mystira/design-tokens (NPM)

| Export | What It Provides | Services Using |
|--------|------------------|----------------|
| `colors` | Color palette (primary, neutral, semantic) | Publisher, Admin-UI, DevHub |
| `typography` | Font families, sizes, weights | Publisher, Admin-UI, DevHub |
| `spacing` | Spacing scale (0-16) | Publisher, Admin-UI, DevHub |
| `components` | Component tokens (radius, shadow) | Publisher, Admin-UI, DevHub |
| CSS variables | `variables.css` | All frontends |
| Tailwind preset | `preset.js` | DevHub, StoryGen Web |

---

## Key Changes Summary

### MediatR → Wolverine

| Aspect | MediatR | Wolverine |
|--------|---------|-----------|
| Handler signature | `IRequestHandler<TRequest, TResponse>` | Static method with convention |
| Dependency injection | Constructor | Method parameters |
| Pipeline behaviors | `IPipelineBehavior<,>` | Middleware chain |
| Distributed messaging | None | Azure Service Bus built-in |

### Custom → Mystira.Shared.Resilience

| Aspect | Before | After |
|--------|--------|-------|
| Retry policy | Custom per-service | `PolicyFactory.CreateStandardHttpPolicy()` |
| Circuit breaker | Manual or missing | Included in standard policy |
| Configuration | Hardcoded | `appsettings.json` |

### IMemoryCache → Redis

| Aspect | Before | After |
|--------|--------|-------|
| Cache type | `IMemoryCache` | `ICacheService` (Redis/Memory) |
| Multi-instance | Not supported | Fully supported |
| Cache-aside | Manual | `GetOrSetAsync()` |

---

## Rollback Strategy

Each migration guide includes rollback instructions. General strategy:

1. **Feature Flags**: Use configuration to toggle new infrastructure
2. **Gradual Migration**: Run old and new side-by-side during transition
3. **Separate PRs**: Each service migrated independently
4. **Backward Compatibility**: Old packages remain available

---

## Testing Requirements

Before merging any migration PR:

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] API contract tests pass
- [ ] Performance benchmarks (no regression)
- [ ] Visual regression tests (frontend)

---

## Timeline

| Week | Activity |
|------|----------|
| 1 | Merge foundation PR, publish packages |
| 2 | Migrate Mystira.Admin (simplest .NET) |
| 3-4 | Migrate Mystira.App (largest) |
| 5 | Migrate Mystira.StoryGenerator |
| 6 | Migrate Mystira.Publisher (frontend) |
| 7 | Cleanup deprecated packages |

---

## Related Documentation

- [ADR-0014: Polyglot Persistence](../architecture/adr/0014-polyglot-persistence-framework-selection.md)
- [ADR-0015: Wolverine Migration](../architecture/adr/0015-event-driven-architecture-framework.md)
- [ADR-0020: Package Consolidation](../architecture/adr/0020-package-consolidation-strategy.md)
- [Package Inventory](../analysis/package-inventory.md)
