# Architecture Check

Scan the codebase for Hexagonal Architecture violations against Mystira's architectural rules.

## Arguments

- `$ARGUMENTS` - Optional scope:
  - A file or directory path to check (e.g., `src/Mystira.App.Api/`)
  - `--full` for a complete codebase scan
  - If omitted, checks recently changed files (via `git diff --name-only`)

## Instructions

### 1. Architectural Rules to Enforce

Reference: `docs/architecture/ARCHITECTURAL_RULES.md`

#### Layer Dependency Rules (STRICT)

```
ALLOWED:
  API/AdminAPI -> Application -> Domain
  Infrastructure -> Application -> Domain
  Infrastructure -> Domain

FORBIDDEN:
  Domain -> anything
  Application -> Infrastructure
  API -> Infrastructure (direct)
  API -> Domain (direct, bypass Application)
```

#### Rule 1: No Business Logic in Controllers

Scan controllers in:

- `src/Mystira.App.Api/Controllers/`
- `src/Mystira.App.Admin.Api/Controllers/`

**Violations to detect:**

- Repository interfaces injected directly into controllers
- DbContext usage in controllers
- Complex conditional logic (business decisions) in controller methods
- Any `using` statements referencing Infrastructure namespaces
- Direct entity manipulation (instead of delegating to Use Cases/MediatR)

#### Rule 2: No Services in API Layer

**Violations to detect:**

- Any files in `src/Mystira.App.Api/Services/` (known violation -- PERF-4)
- Any files in `src/Mystira.App.Admin.Api/Services/`
- Classes that contain business logic outside of the Application layer

#### Rule 3: Application Layer Must Not Reference Infrastructure

Scan `src/Mystira.App.Application/`:

**Violations to detect:**

- `using Mystira.App.Infrastructure.*` statements
- `using Microsoft.EntityFrameworkCore` statements
- Direct references to Azure SDK, Discord.Net, or other infrastructure packages
- `.csproj` ProjectReference to any Infrastructure project

#### Rule 4: Domain Layer Must Have Zero Dependencies

Scan `src/Mystira.App.Domain/`:

**Violations to detect:**

- Any `using` statements referencing Application, Infrastructure, or API namespaces
- Any NuGet package references in `.csproj` (except basic .NET BCL)
- Any framework-specific attributes (EF Core, ASP.NET)

#### Rule 5: DTOs in Contracts Only

**Violations to detect:**

- Request/Response DTO classes defined outside `src/Mystira.Contracts.App/`
- DTOs mixed into Domain or Application layers

#### Rule 6: Correct API Routing

**Violations to detect:**

- Public API controllers with `/adminapi` routes
- Admin API controllers with `/api` routes
- Missing `[Authorize]` on sensitive endpoints

### 2. Output Format

Generate a report:

```
## Architecture Compliance Report

### Summary
- Files scanned: X
- Violations found: Y
- Severity: [CLEAN | MINOR | MAJOR | CRITICAL]

### Violations

#### [CRITICAL] Layer Dependency Violation
- **File:** src/Mystira.App.Application/SomeHandler.cs:15
- **Rule:** Application must not reference Infrastructure
- **Found:** `using Mystira.App.Infrastructure.Data;`
- **Fix:** Use repository interface (port) instead of concrete implementation

#### [MAJOR] Business Logic in Controller
- **File:** src/Mystira.App.Api/Controllers/FooController.cs:42
- **Rule:** Controllers must only map DTOs and delegate to MediatR
- **Found:** Complex conditional logic computing business result
- **Fix:** Extract to a Use Case in Application layer

### Known/Tracked Violations
- PERF-4: Services in API layer (80+ files, tracked in roadmap for Wave 5)

### Recommendations
1. ...
2. ...
```

### 3. After Check

- If violations found, ask if the user wants to fix them
- For known/tracked violations, note that they're already documented
- Suggest running `dotnet build` to verify no compile-time dependency issues
