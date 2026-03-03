# ADR-0023: Shared Infrastructure Interfaces

## Status

**Accepted** - 2026-01-19

## Context

Multiple Mystira applications share common data access patterns. Clean Architecture principles suggest that each Application layer should define its own ports (interfaces), but this leads to interface duplication across applications with identical contracts.

### Current State

Infrastructure interfaces are defined in `Mystira.Shared.Data.Repositories`:

- `IRepository<TEntity, TKey>` - Generic repository contract
- `IUnitOfWork` - Transaction management contract

Domain-specific repository interfaces (e.g., `IAccountRepository`, `IScenarioRepository`) are defined in each application's `Ports/Data/` directory and extend the shared `IRepository<TEntity, TKey>`.

### The Clean Architecture Perspective

Strict Clean Architecture advocates that:

1. Application layer should define its own ports
2. Infrastructure layer implements those ports
3. No direct coupling between Application and shared packages

This would mean each app defines its own `IUnitOfWork` in `Application/Ports/Data/`.

### The Practical Reality

In a workspace with multiple applications sharing the same infrastructure patterns:

1. `IUnitOfWork` contracts are identical across all apps
2. `IRepository<TEntity, TKey>` contracts are identical across all apps
3. Defining the same interface in every app adds maintenance burden
4. Changes to the pattern require updates across all apps

## Decision

Infrastructure interfaces (`IUnitOfWork`, `IRepository<TEntity, TKey>`) are defined in `Mystira.Shared.Data.Repositories` rather than each application's Ports directory.

Domain-specific repository interfaces remain in each application's `Ports/Data/` for domain-specific methods that extend the shared contracts.

### Structure

```
Mystira.Shared/
└── Data/
    └── Repositories/
        ├── IRepository.cs        # Generic repository contract
        └── IUnitOfWork.cs        # Transaction management contract

MyApp.Application/
└── Ports/
    └── Data/
        └── IAccountRepository.cs  # Domain-specific, extends IRepository<Account, Guid>
```

## Rationale

1. **Workspace Consistency**: All Mystira apps use the same infrastructure patterns, ensuring consistent behavior
2. **Reduced Duplication**: Single source of truth for infrastructure contracts
3. **Easier Updates**: Pattern changes only need to happen in one place
4. **Already Established**: `IRepository<TEntity, TKey>` was already in `Mystira.Shared`, adding `IUnitOfWork` maintains consistency

## Trade-offs

### Accepted Trade-offs

| Trade-off                                               | Mitigation                                               |
| ------------------------------------------------------- | -------------------------------------------------------- |
| Deviates from strict Clean Architecture                 | Documented as intentional architectural decision         |
| All apps coupled to shared package's interface versions | Versioning strategy via NuGet ensures controlled updates |
| Harder to customize transaction semantics per-app       | Domain-specific needs can extend the base interface      |

### When This Doesn't Apply

If an application needs fundamentally different transaction semantics (e.g., distributed transactions, saga patterns), it should define its own `IUnitOfWork` variant in its `Ports/Data/` directory.

## Consequences

### Positive

1. **Single Source of Truth**: One definition of infrastructure contracts
2. **Consistency**: All apps behave the same way with transactions
3. **Less Boilerplate**: No need to copy-paste identical interfaces
4. **Easier Onboarding**: New developers learn one pattern

### Negative

1. **Coupling**: Apps depend on shared package for core abstractions
2. **Version Coordination**: Shared package updates affect all consumers
3. **Less Flexibility**: Custom transaction behavior requires more work

## Related ADRs

- [ADR-0007: NuGet Feed Strategy for Shared Libraries](./0007-nuget-feed-strategy-for-shared-libraries.md)
- [ADR-0014: Polyglot Persistence Framework Selection](./0014-polyglot-persistence-framework-selection.md)
- [ADR-0022: Shared Package Dependency Update Strategy](./0022-shared-package-dependency-update-strategy.md)

## References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Ardalis.Specification](https://github.com/ardalis/Specification) - Used for repository specifications
