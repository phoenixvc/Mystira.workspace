# Mystira.Shared

Shared infrastructure for Mystira platform services.

## Overview

This package provides cross-cutting concerns for all Mystira .NET services:

- **Authentication**: JWT token generation, validation, and middleware
- **Authorization**: Role-based and permission-based access control
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
│   ├── JwtService.cs              # Token generation and validation
│   └── JwtMiddleware.cs           # ASP.NET Core middleware
├── Authorization/
│   ├── Permissions.cs             # Permission constants
│   ├── Roles.cs                   # Role constants
│   ├── RequirePermissionAttribute.cs
│   ├── RequireRoleAttribute.cs
│   └── PermissionHandler.cs       # Authorization handler
├── Middleware/
│   ├── TelemetryMiddleware.cs     # OpenTelemetry integration
│   ├── RequestLoggingMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
└── Extensions/
    ├── AuthenticationExtensions.cs # DI registration
    ├── AuthorizationExtensions.cs
    └── TelemetryExtensions.cs
```

## Related Documentation

- [ADR-0020: Package Consolidation Strategy](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md)
- [Authentication & Authorization Guide](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/guides/authentication-authorization-guide.md)

## License

MIT
