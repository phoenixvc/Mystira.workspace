# Mystira.Application

Application layer for the Mystira platform containing CQRS commands/queries, handlers, validators, specifications, and application services.

## Overview

This package provides the application logic layer following Clean Architecture principles:

- **CQRS Pattern**: Commands and Queries with Wolverine handlers
- **Specifications**: Composable query patterns using Ardalis.Specification
- **Validators**: FluentValidation for input validation
- **Mappers**: Mapperly for compile-time object mapping
- **Services**: Application-level services (scoring, badge awarding, etc.)

## Dependencies

- `Mystira.Domain` - Core domain types and entities
- `Mystira.Contracts` - API contracts (requests/responses)
- `Mystira.Shared` - Shared utilities and CQRS base types

## Structure

```
Mystira.Application/
├── CQRS/                    # Command/Query handlers
│   ├── Attribution/
│   ├── Avatars/
│   ├── Badges/
│   ├── ContentBundles/
│   ├── Contributors/
│   ├── GameSessions/
│   └── ...
├── Services/                # Application services
├── Specifications/          # Query specifications
├── Validators/              # FluentValidation validators
├── Mappers/                 # Mapperly mappers
├── Interfaces/              # Shared interfaces
└── DependencyInjection.cs   # Service registration
```

## Usage

```csharp
// In your startup/program.cs
services.AddApplicationServices();

// Wolverine will auto-discover handlers
```

## Version

0.5.0-alpha - Migrated from Mystira.App.Application to workspace
