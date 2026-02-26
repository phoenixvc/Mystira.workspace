# Hexagonal Architecture (Ports and Adapters)

> **⚠️ Important**: For strict architectural rules and enforcement guidelines, see [ARCHITECTURAL_RULES.md](../ARCHITECTURAL_RULES.md)

## Overview

Hexagonal Architecture, also known as Ports and Adapters, is an architectural pattern that isolates the core business logic from external concerns. The application core is surrounded by adapters that handle communication with the outside world.

## Architecture Layers

```text
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│  (APIs, Controllers, PWA)               │
│         ↕                               │
│         Application Layer               │
│  (Use Cases, Application Services)      │
│         ↕                               │
│         Domain Layer                    │
│  (Entities, Value Objects, Domain Logic)│
│         ↕                               │
│      Infrastructure Layer               │
│  (Repositories, External Services)     │
└─────────────────────────────────────────┘
```

## Layer Responsibilities

### Domain Layer (`Mystira.App.Domain`)

- **Purpose**: Core business logic and domain models
- **Contains**: Entities, Value Objects, Domain Services, Domain Events
- **Dependencies**: None (pure domain logic)
- **Example**: `GameSession`, `UserProfile`, `Scenario`

### Application Layer (`Mystira.App.Application`)

- **Purpose**: Application-specific use cases and orchestration
- **Contains**: Use Cases, Application Services, DTOs (via Contracts)
- **Dependencies**: Domain, Contracts
- **Example**: `CreateGameSessionUseCase`, `GetScenariosUseCase`

### Infrastructure Layer (`Mystira.App.Infrastructure.*`)

- **Purpose**: Technical implementations and external integrations
- **Contains**: Repositories, External Services, Data Access
- **Dependencies**: Domain, Application
- **Example**: `GameSessionRepository`, `AzureBlobStorageService`

### Presentation Layer (`Mystira.App.Api`, `Mystira.App.Admin.Api`, `Mystira.App.PWA`)

- **Purpose**: User interfaces and API endpoints
- **Contains**: Controllers, Views, UI Components
- **Dependencies**: Application, Contracts
- **Example**: `GameSessionsController`, `Home.razor`

### Contracts Layer (`Mystira.Contracts.App`)

- **Purpose**: API contracts and DTOs
- **Contains**: Request/Response DTOs, API Models
- **Dependencies**: Domain
- **Example**: `CreateGameSessionRequest`, `GameSessionResponse`

## Dependency Rules

1. **Domain**: No dependencies (innermost layer)
2. **Contracts**: Depends only on Domain
3. **Application**: Depends on Domain and Contracts
4. **Infrastructure**: Depends on Domain and Application
5. **Presentation**: Depends on Application and Contracts

## Benefits

1. **Testability**: Business logic can be tested without external dependencies
2. **Flexibility**: Easy to swap implementations (e.g., different databases)
3. **Maintainability**: Clear separation of concerns
4. **Independence**: Business logic is independent of frameworks and UI

## Ports and Adapters

### Ports (Interfaces)

- Define contracts for external interactions
- Example: `IGameSessionRepository`, `IUnitOfWork`

### Adapters (Implementations)

- Implement ports for specific technologies
- Example: `GameSessionRepository`, `AzureBlobStorageService`

## Current Implementation Status

- ✅ Domain Layer: Complete
- ✅ Contracts Layer: Created (DTOs migration in progress)
- ✅ Infrastructure.Data: Repository pattern implemented
- 🔄 Application Layer: Created (use cases migration in progress)
- 🔄 Presentation Layer: Refactoring to use Application layer

## References

- [Hexagonal Architecture - Alistair Cockburn](https://alistair.cockburn.us/hexagonal-architecture/)
- [Ports and Adapters Pattern](https://herbertograca.com/2017/11/16/explicit-architecture-01-ddd-hexagonal-onion-clean-cqrs-how-i-put-it-all-together/)
