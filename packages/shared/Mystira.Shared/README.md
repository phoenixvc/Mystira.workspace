# Mystira.Shared

Shared infrastructure for Mystira platform services.

## Overview

This package provides cross-cutting concerns for all Mystira .NET services:

- **Authentication**: JWT token generation, validation, and middleware
- **Authorization**: Role-based and permission-based access control
- **Resilience**: Polly-based retry, circuit breaker, and timeout policies
- **Exceptions**: Standardized error handling, Result<T> pattern, ProblemDetails
- **Caching**: Distributed caching with Redis support
- **Messaging**: Wolverine integration for unified in-process and distributed messaging
- **Middleware**: Telemetry, logging, and request/response handling
- **Extensions**: Dependency injection helpers for easy integration

## Installation

```bash
dotnet add package Mystira.Shared
```

Or via NuGet Package Manager:
```
Install-Package Mystira.Shared
```

## Usage

### Configure Services

```csharp
// Program.cs
using Mystira.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Mystira authentication (Microsoft Entra ID)
builder.Services.AddMystiraAuthentication(builder.Configuration);

// Add Mystira authorization policies
builder.Services.AddMystiraAuthorization();

// Add telemetry and logging
builder.Services.AddMystiraTelemetry(builder.Configuration);

var app = builder.Build();

// Use authentication/authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseMystiraTelemetry();
app.UseMystiraExceptionHandling();
```

### Resilience (HTTP Clients)

```csharp
using Mystira.Shared.Resilience;

// Add resilient HTTP client with retry, circuit breaker, and timeout
builder.Services.AddResilientHttpClient<IScenarioApiClient, ScenarioApiClient>(
    "ScenarioApi",
    client => client.BaseAddress = new Uri("https://api.mystira.app"));

// For long-running operations (e.g., LLM calls)
builder.Services.AddLongRunningHttpClient<ILlmApiClient, LlmApiClient>(
    "LlmApi");
```

### Caching

```csharp
using Mystira.Shared.Extensions;

// Add distributed caching (Redis or in-memory fallback)
builder.Services.AddMystiraCaching(builder.Configuration);

// Use ICacheService
public class ScenarioService
{
    private readonly ICacheService _cache;

    public async Task<Scenario> GetScenarioAsync(string id)
    {
        return await _cache.GetOrCreateAsync(
            $"scenario:{id}",
            async ct => await _repository.GetByIdAsync(id, ct));
    }
}
```

### Messaging (Wolverine)

```csharp
using Mystira.Shared.Extensions;

// Add Wolverine messaging
builder.AddMystiraMessaging();

// Handler (convention-based, no interfaces needed)
public static class CreateAccountHandler
{
    public static async Task<AccountDto> HandleAsync(
        CreateAccountCommand command,
        IAccountRepository repo)
    {
        // Handler implementation
    }
}
```

### Configuration

```json
// appsettings.json
{
  "Mystira": {
    "Authentication": {
      "Authority": "https://login.microsoftonline.com/{tenant-id}",
      "ClientId": "{client-id}",
      "Audience": "api://{client-id}",
      "ValidateIssuer": true,
      "ValidateAudience": true
    },
    "Telemetry": {
      "ServiceName": "mystira-api",
      "EnableTracing": true,
      "EnableMetrics": true
    }
  }
}
```

### Protect Controllers

```csharp
using Microsoft.AspNetCore.Authorization;
using Mystira.Shared.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ScenariosController : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.ScenariosRead)]
    public async Task<IActionResult> GetScenarios() { ... }

    [HttpPost]
    [RequirePermission(Permissions.ScenariosWrite)]
    public async Task<IActionResult> CreateScenario() { ... }

    [HttpDelete("{id}")]
    [RequireRole(Roles.Admin)]
    public async Task<IActionResult> DeleteScenario(Guid id) { ... }
}
```

## Migration from Mystira.App.Shared

This package replaces `Mystira.App.Shared`. To migrate:

1. Update package reference:
   ```xml
   <!-- Before -->
   <PackageReference Include="Mystira.App.Shared" Version="x.x.x" />

   <!-- After -->
   <PackageReference Include="Mystira.Shared" Version="0.1.0" />
   ```

2. Update using statements:
   ```csharp
   // Before
   using Mystira.App.Shared.Authentication;
   using Mystira.App.Shared.Authorization;

   // After
   using Mystira.Shared.Authentication;
   using Mystira.Shared.Authorization;
   ```

3. Update service registration (method names unchanged):
   ```csharp
   // Same API, just new namespace
   services.AddMystiraAuthentication(configuration);
   services.AddMystiraAuthorization();
   ```

## Package Contents

```
Mystira.Shared/
├── Authentication/
│   ├── JwtOptions.cs              # JWT configuration options
│   └── JwtService.cs              # Token generation and validation
├── Authorization/
│   ├── Permissions.cs             # Permission constants
│   ├── RequirePermissionAttribute.cs
│   └── PermissionHandler.cs       # Authorization handler
├── Resilience/                    # Wave 1
│   ├── ResilienceOptions.cs       # Retry/circuit breaker config
│   ├── PolicyFactory.cs           # Polly policy creation
│   └── ResilienceExtensions.cs    # DI registration
├── Exceptions/                    # Wave 1
│   ├── ErrorResponse.cs           # Standard error responses
│   ├── Result.cs                  # Result<T> pattern
│   ├── MystiraException.cs        # Base exception types
│   └── GlobalExceptionHandler.cs  # IExceptionHandler
├── Caching/                       # Wave 2
│   ├── CacheOptions.cs            # Cache configuration
│   ├── ICacheService.cs           # Cache abstraction
│   └── DistributedCacheService.cs # Redis/memory implementation
├── Messaging/                     # Wave 3 (Wolverine)
│   ├── MessagingOptions.cs        # Messaging configuration
│   └── ICommand.cs                # Message marker interfaces
├── Middleware/
│   └── TelemetryMiddleware.cs     # OpenTelemetry integration
└── Extensions/
    ├── AuthenticationExtensions.cs
    ├── AuthorizationExtensions.cs
    ├── CachingExtensions.cs
    ├── ExceptionExtensions.cs
    ├── MessagingExtensions.cs
    └── TelemetryExtensions.cs
```

## Related Documentation

- [ADR-0020: Package Consolidation Strategy](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md)
- [Authentication & Authorization Guide](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/guides/authentication-authorization-guide.md)

## License

MIT
