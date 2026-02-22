# Create API Endpoint

Scaffold a new API endpoint following Mystira's Hexagonal Architecture. Generate all required files across the correct layers.

## Arguments

- `$ARGUMENTS` - Format: `<EntityName> <Action> [--admin]`
  - EntityName: The domain entity (e.g., GameSession, UserProfile, Scenario)
  - Action: The operation (e.g., Create, Update, Delete, GetById, GetAll)
  - --admin: Optional flag to generate in AdminAPI instead of public API

## Instructions

### 1. Determine Routing

- If `--admin` flag is present: generate in `src/Mystira.App.Admin.Api/Controllers/` with route prefix `/adminapi/`
- Otherwise: generate in `src/Mystira.App.Api/Controllers/` with route prefix `/api/`

**Routing Rule:**
- `/api/*` = User acting on their OWN resources (requires auth, scoped to user)
- `/adminapi/*` = System-level or other users' data (requires admin role)

### 2. Generate Files (in this order)

#### a) Request/Response DTOs in `src/Mystira.Contracts.App/`

```
src/Mystira.Contracts.App/Requests/{Action}{Entity}Request.cs
src/Mystira.Contracts.App/Responses/{Entity}Response.cs  (if not already exists)
```

- Request DTOs: Only the fields needed for this action
- Response DTOs: Full entity representation (reuse existing if present)
- Use `System.ComponentModel.DataAnnotations` for validation attributes

#### b) CQRS Command/Query in `src/Mystira.App.Application/`

For write operations (Create, Update, Delete):
```
src/Mystira.App.Application/CQRS/{Entity}s/Commands/{Action}{Entity}Command.cs
src/Mystira.App.Application/CQRS/{Entity}s/Commands/{Action}{Entity}CommandHandler.cs
```

For read operations (Get, GetAll, Search):
```
src/Mystira.App.Application/CQRS/{Entity}s/Queries/{Action}{Entity}Query.cs
src/Mystira.App.Application/CQRS/{Entity}s/Queries/{Action}{Entity}QueryHandler.cs
```

**Wolverine handler pattern (NOT MediatR):**

Command/Query is a plain DTO class:
```csharp
namespace Mystira.App.Application.CQRS.{Entity}s.Commands;

public record {Action}{Entity}Command(/* input properties */);
```

Handler uses Wolverine's static method convention with DI via parameters:
```csharp
namespace Mystira.App.Application.CQRS.{Entity}s.Commands;

public static class {Action}{Entity}CommandHandler
{
    public static async Task<{ResultType}> Handle(
        {Action}{Entity}Command command,
        I{Entity}Repository {entity}Repository,
        CancellationToken cancellationToken)
    {
        // Implementation — dependencies are injected as method parameters
    }
}
```

- Do NOT use `IRequest<T>`, `IRequestHandler<,>`, or `using MediatR;`
- Wolverine discovers handlers by convention (static `Handle` method)
- Dependencies are injected as method parameters, not via constructor
- Use async/await for all I/O

#### c) Controller Method

Add the action method to the existing controller, or create a new controller if none exists for this entity.

```csharp
[ApiController]
[Route("[api|adminapi]/[controller]")]
[Authorize]
public class {Entity}sController : ControllerBase
{
    private readonly IMessageBus _bus;

    public {Entity}sController(IMessageBus bus)
    {
        _bus = bus;
    }

    // Action method with proper HTTP verb, route, and Swagger attributes
    // Dispatches via: var result = await _bus.InvokeAsync<ResultType>(command);
}
```

- Use `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` as appropriate
- Add `[ProducesResponseType]` attributes for Swagger
- Map Request DTO to Command/Query, dispatch via `IMessageBus`
- Return appropriate status codes (200, 201, 204, 404)

#### d) Register in DI (if needed)

Check `src/Mystira.App.Api/Configuration/UseCaseExtensions.cs` or `Program.cs` for registration.
Wolverine auto-discovers handlers by convention, so usually no registration needed.

### 3. Validation Rules

- NEVER put business logic in the controller
- NEVER call repositories directly from the controller
- ALWAYS go through Wolverine: Controller -> IMessageBus -> Command/Query -> Handler -> Repository
- ALWAYS add `[Authorize]` (or `[AllowAnonymous]` only if explicitly needed)
- ALWAYS validate input at the controller level with DataAnnotations

### 4. After Generation

- Verify the solution builds: `dotnet build Mystira.App.sln`
- List the files created so the user can review them
- Suggest adding tests in the corresponding test project
