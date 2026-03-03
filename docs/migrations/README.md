# Mystira Migration Guides

This directory contains migration guides for adopting `Mystira.Shared` infrastructure patterns and consolidated packages across all Mystira services.

**Last Updated**: February 2026
**Current Runtime**: .NET 10.0 / Node.js 24+ / Python 3.12+

> **See [MIGRATION_INDEX.md](./MIGRATION_INDEX.md)** for the consolidated migration status dashboard.

---

## Current State

The monorepo migration is **complete** — all 7 service repositories are inlined, .NET 10 is deployed, and all internal packages use `<ProjectReference>` (no NuGet publishing for internal packages).

The remaining migration work is **framework-level** — adopting shared infrastructure patterns (Wolverine, Polly v8, Ardalis.Specification, etc.) within each service.

---

## Service Migration Status

| Service                    | Guide                                                                        | Status            |
| -------------------------- | ---------------------------------------------------------------------------- | ----------------- |
| **Mystira.App**            | [mystira-app-migration.md](./mystira-app-migration.md)                       | ✅ 100% Complete  |
| **Mystira.Admin.Api**      | [mystira-admin-migration.md](./mystira-admin-migration.md)                   | 📋 Ready to start |
| **Mystira.StoryGenerator** | [mystira-storygenerator-migration.md](./mystira-storygenerator-migration.md) | 🔄 ~30%           |
| **Mystira.Publisher**      | [mystira-publisher-migration.md](./mystira-publisher-migration.md)           | 📋 Planned        |
| **Mystira.Chain**          | [mystira-chain-migration.md](./mystira-chain-migration.md)                   | 📋 Planned        |
| **Mystira.DevHub**         | [mystira-devhub-migration.md](./mystira-devhub-migration.md)                 | 📋 Planned        |
| **Mystira.Admin.UI**       | [mystira-admin-ui-migration.md](./mystira-admin-ui-migration.md)             | 📋 Planned        |

## Package Migration Guides

| Package                 | Guide                                                                        | Status      |
| ----------------------- | ---------------------------------------------------------------------------- | ----------- |
| Infrastructure Packages | [mystira-infrastructure-migration.md](./mystira-infrastructure-migration.md) | ✅ Complete |
| Mystira.Shared          | [mystira-shared-migration.md](../guides/mystira-shared-migration.md)         | ✅ Complete |
| Contracts               | [contracts-migration.md](../guides/contracts-migration.md)                   | ✅ Complete |

---

## Key Framework Changes

### MediatR → Wolverine

| Aspect                | MediatR                                | Wolverine                     |
| --------------------- | -------------------------------------- | ----------------------------- |
| Handler signature     | `IRequestHandler<TRequest, TResponse>` | Static method with convention |
| Dependency injection  | Constructor                            | Method parameters             |
| Distributed messaging | None                                   | Azure Service Bus built-in    |

### Polly v7 → Polly v8

| Aspect           | Polly v7             | Polly v8                 |
| ---------------- | -------------------- | ------------------------ |
| Policy type      | `IAsyncPolicy<T>`    | `ResiliencePipeline<T>`  |
| HTTP integration | `AddPolicyHandler()` | `AddResilienceHandler()` |

### Custom Queries → Ardalis.Specification 9.3.1

| Aspect             | Before                  | After                                  |
| ------------------ | ----------------------- | -------------------------------------- |
| Repository pattern | Custom `IRepository<T>` | `IReadRepository<T>`, `IRepository<T>` |
| Single result      | Custom query handlers   | `ISingleResultSpecification<T>`        |

---

## Related Documentation

- [2026 Roadmap](../../ROADMAP.md) - Single source of truth for planning
- [Architecture ADRs](../architecture/adr/) - Decision records
- [ADR-0024: True Monorepo Migration](../architecture/adr/0024-true-monorepo-migration.md)
- [Operations Runbooks](../operations/runbooks/) - Rollback procedures
