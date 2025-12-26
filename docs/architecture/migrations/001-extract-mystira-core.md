# Migration 001: Extract Mystira.Core Package

## Status: Proposed

## Summary

Extract shared foundational types from `Mystira.Contracts` and `Mystira.Shared` into a new `Mystira.Core` package to eliminate duplication and establish a single source of truth for cross-cutting types.

## Problem Statement

### Current Duplication

We have duplicate `ErrorResponse` types in two locations:

| Aspect | `Mystira.Contracts` | `Mystira.Shared` |
|--------|---------------------|------------------|
| **Namespace** | `Mystira.Contracts.App.Responses.Common` | `Mystira.Shared.Exceptions` |
| **Type** | `record` (immutable) | `class` (mutable) |
| **Framework** | net8.0 | net9.0 |
| **StatusCode** | ✅ Has property | ❌ Missing |
| **UnauthorizedErrorResponse** | ✅ Has type | ❌ Missing |
| **Timestamp type** | `DateTimeOffset` | `DateTime` |

### Issues

1. **Type Confusion**: Developers don't know which `ErrorResponse` to use
2. **Inconsistent Behavior**: Different implementations have different features
3. **Serialization Mismatch**: `class` vs `record` serialize differently
4. **Framework Split**: Different .NET versions prevent easy reference

## Proposed Solution

### New Package Structure

```
packages/
├── core/                           # NEW: Foundation layer
│   ├── Mystira.Core/               # C# base types
│   │   ├── Errors/
│   │   │   ├── ErrorResponse.cs
│   │   │   ├── ValidationErrorResponse.cs
│   │   │   ├── NotFoundErrorResponse.cs
│   │   │   ├── ForbiddenErrorResponse.cs
│   │   │   └── UnauthorizedErrorResponse.cs
│   │   ├── Results/
│   │   │   └── Result.cs
│   │   └── Mystira.Core.csproj
│   ├── Mystira.Core.Tests/
│   └── core-types/                 # TypeScript equivalent
│       ├── src/
│       │   ├── errors.ts
│       │   └── index.ts
│       └── package.json
│
├── contracts/                      # App-specific contracts
│   ├── dotnet/
│   │   └── Mystira.Contracts/      # References Mystira.Core
│   └── src/                        # TypeScript (references @mystira/core-types)
│
└── shared/                         # Infrastructure services
    └── Mystira.Shared/             # References Mystira.Core
```

### Dependency Graph

```
                    ┌─────────────────┐
                    │  Mystira.Core   │
                    │  (net8.0/net9.0)│
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
              ▼              ▼              ▼
    ┌─────────────────┐ ┌─────────────┐ ┌─────────────┐
    │Mystira.Contracts│ │Mystira.Shared│ │ Other Pkgs  │
    │   (net8.0)      │ │   (net9.0)   │ │             │
    └─────────────────┘ └──────────────┘ └─────────────┘
```

## Migration Steps

### Phase 1: Create Mystira.Core (Week 1)

#### Step 1.1: Create C# Project

```bash
# Create project structure
mkdir -p packages/core/Mystira.Core/Errors
mkdir -p packages/core/Mystira.Core/Results
mkdir -p packages/core/Mystira.Core.Tests

# Create project file
```

**Mystira.Core.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

    <PackageId>Mystira.Core</PackageId>
    <Version>0.1.0-alpha</Version>
    <Description>Core types for Mystira platform - errors, results, and foundational abstractions</Description>
  </PropertyGroup>
</Project>
```

#### Step 1.2: Move ErrorResponse to Core

Take the best of both implementations:

```csharp
namespace Mystira.Core.Errors;

/// <summary>
/// Standard error response for API errors.
/// Single source of truth for all Mystira services.
/// </summary>
public record ErrorResponse
{
    /// <summary>Human-readable error message.</summary>
    public required string Message { get; init; }

    /// <summary>Additional details about the error.</summary>
    public string? Details { get; init; }

    /// <summary>When the error occurred (UTC).</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Correlation ID for distributed tracing.</summary>
    public string? TraceId { get; init; }

    /// <summary>Unique error code (e.g., "AUTH_001", "VALIDATION_002").</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Error category (e.g., "Authentication", "Validation").</summary>
    public string? Category { get; init; }

    /// <summary>HTTP status code associated with this error.</summary>
    public int? StatusCode { get; init; }

    /// <summary>Whether the error is recoverable by the user.</summary>
    public bool IsRecoverable { get; init; } = true;

    /// <summary>Suggested action (e.g., "retry", "login").</summary>
    public string? SuggestedAction { get; init; }
}
```

#### Step 1.3: Create TypeScript Equivalent

**packages/core/core-types/src/errors.ts:**
```typescript
export interface ErrorResponse {
  message: string;
  details?: string;
  timestamp: string; // ISO 8601
  traceId?: string;
  errorCode?: string;
  category?: string;
  statusCode?: number;
  isRecoverable: boolean;
  suggestedAction?: string;
}

export interface ValidationErrorResponse extends ErrorResponse {
  errors: Record<string, string[]>;
}

export interface NotFoundErrorResponse extends ErrorResponse {
  resourceType?: string;
  resourceId?: string;
}
```

### Phase 2: Update References (Week 2)

#### Step 2.1: Update Mystira.Contracts

```xml
<!-- Mystira.Contracts.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\core\Mystira.Core\Mystira.Core.csproj" />
</ItemGroup>
```

**Replace local ErrorResponse with re-export:**
```csharp
// Mystira.Contracts/App/Responses/Common/ErrorResponse.cs
// DEPRECATED: Use Mystira.Core.Errors.ErrorResponse directly

global using Mystira.Core.Errors;

namespace Mystira.Contracts.App.Responses.Common;

[Obsolete("Use Mystira.Core.Errors.ErrorResponse instead")]
public record ErrorResponseLegacy : Mystira.Core.Errors.ErrorResponse;
```

#### Step 2.2: Update Mystira.Shared

```xml
<!-- Mystira.Shared.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\core\Mystira.Core\Mystira.Core.csproj" />
</ItemGroup>
```

**Update GlobalExceptionHandler:**
```csharp
using Mystira.Core.Errors;

public class GlobalExceptionHandler
{
    public ErrorResponse HandleException(Exception ex)
    {
        return ErrorResponse.FromException(ex);
    }
}
```

#### Step 2.3: Update TypeScript contracts

**packages/contracts/package.json:**
```json
{
  "dependencies": {
    "@mystira/core-types": "workspace:*"
  }
}
```

**Re-export from contracts:**
```typescript
// packages/contracts/src/app/types.ts
export type { ErrorResponse, ValidationErrorResponse } from '@mystira/core-types';
```

### Phase 3: Deprecation & Cleanup (Week 3)

#### Step 3.1: Add Obsolete Attributes

```csharp
// In Mystira.Shared
namespace Mystira.Shared.Exceptions;

[Obsolete("Use Mystira.Core.Errors.ErrorResponse instead. Will be removed in v1.0.")]
public class ErrorResponse { /* ... */ }
```

#### Step 3.2: Update Consuming Applications

```csharp
// Before
using Mystira.Shared.Exceptions;
// or
using Mystira.Contracts.App.Responses.Common;

// After
using Mystira.Core.Errors;
```

#### Step 3.3: Remove Duplicates

After all consumers are updated:

1. Delete `Mystira.Shared/Exceptions/ErrorResponse.cs`
2. Delete `Mystira.Contracts/App/Responses/Common/ErrorResponse.cs`

## Update Solution File

Add to `Mystira.sln`:

```
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Mystira.Core", "packages\core\Mystira.Core\Mystira.Core.csproj", "{NEW-GUID}"
EndProject
```

## Breaking Changes

### C# Consumers

| Before | After |
|--------|-------|
| `Mystira.Shared.Exceptions.ErrorResponse` | `Mystira.Core.Errors.ErrorResponse` |
| `class ErrorResponse` | `record ErrorResponse` |
| `DateTime Timestamp` | `DateTimeOffset Timestamp` |

### TypeScript Consumers

| Before | After |
|--------|-------|
| `@mystira/contracts` ApiError | `@mystira/core-types` ErrorResponse |

## Rollback Plan

If issues arise:

1. Revert the `<ProjectReference>` changes
2. Keep both packages temporarily
3. Add `[Obsolete]` warnings instead of removing

## Testing Strategy

1. **Unit Tests**: All error types have construction/serialization tests
2. **Integration Tests**: Verify API responses match expected shape
3. **Contract Tests**: Pact tests between services

## Timeline

| Phase | Duration | Tasks |
|-------|----------|-------|
| Phase 1 | Week 1 | Create Mystira.Core package |
| Phase 2 | Week 2 | Update references, add deprecation warnings |
| Phase 3 | Week 3 | Remove duplicates, update docs |

## Decision Record

**Chosen approach**: Extract to `Mystira.Core`

**Alternatives considered**:

1. **Keep Contracts as source of truth**: Would require Shared to reference Contracts, creating awkward dependency
2. **Keep Shared as source of truth**: Contracts should be lighter weight, shouldn't depend on infrastructure
3. **OpenAPI-first generation**: More complex tooling, overkill for current scale

## Appendix: Files to Create

```
packages/core/
├── Mystira.Core/
│   ├── Errors/
│   │   ├── ErrorResponse.cs
│   │   ├── ValidationErrorResponse.cs
│   │   ├── NotFoundErrorResponse.cs
│   │   ├── ForbiddenErrorResponse.cs
│   │   └── UnauthorizedErrorResponse.cs
│   ├── Results/
│   │   ├── Result.cs
│   │   └── ResultExtensions.cs
│   ├── Mystira.Core.csproj
│   └── README.md
├── Mystira.Core.Tests/
│   ├── Errors/
│   │   └── ErrorResponseTests.cs
│   └── Mystira.Core.Tests.csproj
└── core-types/
    ├── src/
    │   ├── errors.ts
    │   ├── results.ts
    │   └── index.ts
    ├── package.json
    ├── tsconfig.json
    └── README.md
```

## Questions for Team

1. Should `Mystira.Core` be published to NuGet, or kept as internal package?
2. Do we need backwards-compatible type aliases during migration?
3. Should we add JSON serialization tests to ensure API compatibility?
