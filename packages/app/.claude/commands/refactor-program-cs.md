# Refactor Program.cs

Extract service registrations from Program.cs into well-organized extension methods, following the established pattern in `src/Mystira.App.Api/Configuration/`.

## Arguments

- `$ARGUMENTS` - Optional:
  - A specific section to extract (e.g., `authentication`, `cors`, `swagger`, `rate-limiting`)
  - `--analyze` to only report what should be extracted without making changes
  - If omitted, analyzes Program.cs and extracts all remaining inline registrations

## Instructions

### 1. Review Current State

Read `src/Mystira.App.Api/Program.cs` and the existing extension methods in `src/Mystira.App.Api/Configuration/`:

Already extracted (do not duplicate):

- `AuthenticationExtensions.cs` - JWT/auth configuration
- `CorsExtensions.cs` - CORS policy setup
- `DatabaseExtensions.cs` - Cosmos DB / EF Core setup
- `RateLimitingExtensions.cs` - Rate limiting configuration
- `RepositoryExtensions.cs` - Repository DI registrations
- `SwaggerExtensions.cs` - Swagger/OpenAPI configuration
- `UseCaseExtensions.cs` - Use case DI registrations

### 2. Identify Remaining Inline Registrations

Look for any service registrations still directly in Program.cs that should be extracted:

- Logging configuration
- Health check registration
- Middleware pipeline configuration
- Any `builder.Services.Add*` calls not covered by existing extensions
- Any `app.Use*` middleware calls that could be grouped

### 3. Extension Method Pattern

Follow the established pattern:

```csharp
namespace Mystira.App.Api.Configuration;

public static class {Feature}Extensions
{
    public static IServiceCollection Add{Feature}(this IServiceCollection services, IConfiguration configuration)
    {
        // Service registrations
        return services;
    }

    // For middleware pipeline extensions:
    public static WebApplication Use{Feature}(this WebApplication app)
    {
        // Middleware configuration
        return app;
    }
}
```

### 4. Rules

- Each extension file should be single-responsibility (one logical group of services)
- Extension methods should be in the `Mystira.App.Api.Configuration` namespace
- Program.cs should read like a high-level overview: `builder.Services.AddAuth(config)`, `builder.Services.AddDatabase(config)`, etc.
- Do NOT move business logic -- only DI/middleware configuration
- Preserve the exact middleware ordering (order matters in ASP.NET Core pipeline)

### 5. After Refactoring

- Verify build: `dotnet build src/Mystira.App.Api/Mystira.App.Api.csproj`
- Verify Program.cs is clean and readable
- List all Configuration extension files and their responsibilities
