# Create Use Case

Generate a CQRS Command or Query with Handler following Mystira's Application layer patterns.

## Arguments

- `$ARGUMENTS` - Format: `<Name> [--type command|query] [--description "..."]`
  - Name: Use case name in PascalCase (e.g., CreateGameSession, GetUserBadges)
  - --type: Explicitly set command (write) or query (read). Auto-detected from name prefix if omitted.
  - --description: Brief description of what this use case does

## Instructions

### 1. Determine Type

Auto-detect from name prefix if `--type` not specified:
- **Command** (write): Create*, Update*, Delete*, Add*, Remove*, Link*, Unlink*
- **Query** (read): Get*, Find*, Search*, List*, Validate*, Count*

### 2. Identify the Entity

Extract the entity name from the use case name:
- `CreateGameSession` -> Entity: `GameSession`
- `GetUserBadges` -> Entity: `UserBadge`
- `LinkProfilesToAccount` -> Entity: `Account`

### 3. Generate Files

#### For Commands:

```
src/Mystira.App.Application/CQRS/{Entity}s/Commands/{Name}Command.cs
src/Mystira.App.Application/CQRS/{Entity}s/Commands/{Name}CommandHandler.cs
```

**Command pattern (Wolverine — plain DTO, no MediatR interfaces):**
```csharp
namespace Mystira.App.Application.CQRS.{Entity}s.Commands;

public record {Name}Command(/* input properties */);
```

**Handler pattern (Wolverine — static method with DI via parameters):**
```csharp
namespace Mystira.App.Application.CQRS.{Entity}s.Commands;

public static class {Name}CommandHandler
{
    public static async Task<{ResultType}> Handle(
        {Name}Command command,
        I{Entity}Repository {entity}Repository,
        CancellationToken cancellationToken)
    {
        // Implementation — dependencies are injected as method parameters by Wolverine
    }
}
```

#### For Queries:

```
src/Mystira.App.Application/CQRS/{Entity}s/Queries/{Name}Query.cs
src/Mystira.App.Application/CQRS/{Entity}s/Queries/{Name}QueryHandler.cs
```

**Query pattern (Wolverine — plain DTO):**
```csharp
namespace Mystira.App.Application.CQRS.{Entity}s.Queries;

public record {Name}Query(/* input properties */);
```

**Handler pattern (Wolverine — static method with DI via parameters):**
```csharp
namespace Mystira.App.Application.CQRS.{Entity}s.Queries;

public static class {Name}QueryHandler
{
    public static async Task<{ResultType}> Handle(
        {Name}Query query,
        I{Entity}Repository {entity}Repository,
        CancellationToken cancellationToken)
    {
        // Implementation — dependencies are injected as method parameters by Wolverine
    }
}
```

**Cacheable Query (for reference/lookup data):**
```csharp
public record {Name}Query(/* input */) : ICacheableQuery
{
    public string CacheKey => $"{Entity}:{Id}";
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(15);
}
```

### 4. Check for Required Ports

- Verify the repository interface exists in `src/Mystira.App.Application/Ports/`
- If not, create `I{Entity}Repository.cs` with the required method signature
- Check that an implementation exists in `src/Mystira.App.Infrastructure.Data/Repositories/`

### 5. Validation

- The handler MUST NOT reference Infrastructure directly (no EF Core DbContext, no Azure SDK)
- The handler MUST only use port interfaces (repository abstractions)
- The handler SHOULD validate business rules via the Domain entity methods
- Commands that modify state SHOULD return the created/updated entity or its ID
- Queries SHOULD return DTOs or domain models, not EF entities with tracking

### 6. After Generation

- Verify build: `dotnet build src/Mystira.App.Application/Mystira.App.Application.csproj`
- Suggest adding corresponding tests
- Suggest adding the controller endpoint if not yet created
