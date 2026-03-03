# Mystira.Infrastructure.Data

Data infrastructure package for the Mystira platform.

## Features

- Entity Framework Core repositories with Cosmos DB and PostgreSQL providers
- Ardalis.Specification integration for composable query patterns
- Polyglot persistence support (ADR-0013/0014)
- Redis caching with distributed cache patterns
- Unit of Work pattern implementation

## Installation

```bash
dotnet add package Mystira.Infrastructure.Data
```

## Usage

```csharp
services.AddMystiraDataInfrastructure(configuration);
```

## Dependencies

- `Mystira.Domain` - Core domain types
- `Mystira.Application` - Application layer contracts
- `Mystira.Shared` - Shared infrastructure

## License

MIT
