# Migration 001: Unified Type System with OpenAPI + Mystira.Core

## Status: Proposed

## Summary

Establish a unified type system using:
1. **OpenAPI specs** as the single source of truth for API contracts (generates both C# and TypeScript)
2. **Mystira.Core** for internal/shared types not exposed via APIs (Result, domain errors, etc.)

All .NET projects target **net9.0**.

## Problem Statement

### Current Duplication

| Aspect | `Mystira.Contracts` | `Mystira.Shared` |
|--------|---------------------|------------------|
| **Namespace** | `Mystira.Contracts.App.Responses.Common` | `Mystira.Shared.Exceptions` |
| **Type** | `record` (immutable) | `class` (mutable) |
| **Framework** | net8.0 | net9.0 |
| **StatusCode** | ✅ Has | ❌ Missing |

### Issues

1. Types drift apart between C# and TypeScript
2. No single source of truth
3. Manual synchronization is error-prone
4. Mixed .NET versions cause compatibility issues

## Proposed Solution: Hybrid Approach

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        SINGLE SOURCE OF TRUTH                        │
├─────────────────────────────────────────────────────────────────────┤
│  packages/api-spec/                                                  │
│  └── openapi/                                                        │
│      ├── app-api.yaml          # App API contracts                   │
│      ├── story-generator.yaml  # Story Generator API                 │
│      └── common/                                                     │
│          ├── errors.yaml       # ErrorResponse, ValidationError      │
│          └── pagination.yaml   # PagedResponse, etc.                 │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    │         CODE GENERATION        │
                    ▼                               ▼
        ┌───────────────────┐           ┌───────────────────┐
        │  C# (NSwag)       │           │  TypeScript       │
        │  Mystira.Contracts│           │  @mystira/contracts│
        │  (net9.0)         │           │                   │
        └───────────────────┘           └───────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                        INTERNAL TYPES                                │
├─────────────────────────────────────────────────────────────────────┤
│  packages/core/                                                      │
│  ├── Mystira.Core/          # C# internal types (net9.0)            │
│  │   ├── Results/           # Result<T>, Error handling             │
│  │   ├── Domain/            # Domain primitives                      │
│  │   └── Extensions/        # Common extensions                      │
│  └── @mystira/core-types/   # TypeScript equivalents                 │
└─────────────────────────────────────────────────────────────────────┘
```

### What Goes Where?

| Type | Location | Reason |
|------|----------|--------|
| `ErrorResponse` | **api-spec/** (OpenAPI) | Exposed in API responses |
| `ValidationErrorResponse` | **api-spec/** (OpenAPI) | Exposed in API responses |
| `StoryRequest` | **api-spec/** (OpenAPI) | API request body |
| `Result<T>` | **Mystira.Core** | Internal only, not serialized to API |
| `DomainException` | **Mystira.Core** | Internal error handling |
| `ISpecification<T>` | **Mystira.Core** | Internal query pattern |

### New Package Structure

```
packages/
├── api-spec/                      # NEW: OpenAPI specifications
│   ├── openapi/
│   │   ├── app-api.yaml
│   │   ├── story-generator.yaml
│   │   └── common/
│   │       ├── errors.yaml
│   │       ├── pagination.yaml
│   │       └── health.yaml
│   ├── scripts/
│   │   ├── generate-csharp.sh
│   │   └── generate-typescript.sh
│   └── package.json
│
├── core/                          # NEW: Internal shared types
│   ├── Mystira.Core/              # C# (net9.0)
│   │   ├── Results/
│   │   │   ├── Result.cs
│   │   │   └── ResultExtensions.cs
│   │   ├── Domain/
│   │   │   ├── Entity.cs
│   │   │   └── ValueObject.cs
│   │   └── Mystira.Core.csproj
│   ├── Mystira.Core.Tests/
│   └── core-types/                # TypeScript internal types
│       └── package.json
│
├── contracts/                     # GENERATED from OpenAPI
│   ├── dotnet/
│   │   └── Mystira.Contracts/     # Generated C# (net9.0)
│   └── src/                       # Generated TypeScript
│
└── shared/                        # Infrastructure (net9.0)
    └── Mystira.Shared/            # References Mystira.Core
```

## Phase 1: OpenAPI Specification (Week 1)

### Step 1.1: Create OpenAPI Directory Structure

```bash
mkdir -p packages/api-spec/openapi/common
mkdir -p packages/api-spec/scripts
```

### Step 1.2: Define Common Error Types

**packages/api-spec/openapi/common/errors.yaml:**
```yaml
openapi: 3.1.0
info:
  title: Mystira Common Types - Errors
  version: 1.0.0

components:
  schemas:
    ErrorResponse:
      type: object
      required:
        - message
        - isRecoverable
      properties:
        message:
          type: string
          description: Human-readable error message
        details:
          type: string
          nullable: true
          description: Additional error details
        timestamp:
          type: string
          format: date-time
          description: When the error occurred (UTC)
        traceId:
          type: string
          nullable: true
          description: Correlation ID for distributed tracing
        errorCode:
          type: string
          nullable: true
          description: Unique error code (e.g., "AUTH_001")
        category:
          type: string
          nullable: true
          description: Error category (e.g., "Authentication")
        statusCode:
          type: integer
          nullable: true
          description: HTTP status code
        isRecoverable:
          type: boolean
          default: true
          description: Whether the error is recoverable
        suggestedAction:
          type: string
          nullable: true
          description: Suggested action (e.g., "retry", "login")

    ValidationErrorResponse:
      allOf:
        - $ref: '#/components/schemas/ErrorResponse'
        - type: object
          required:
            - errors
          properties:
            errors:
              type: object
              additionalProperties:
                type: array
                items:
                  type: string
              description: Field-level validation errors

    NotFoundErrorResponse:
      allOf:
        - $ref: '#/components/schemas/ErrorResponse'
        - type: object
          properties:
            resourceType:
              type: string
              nullable: true
            resourceId:
              type: string
              nullable: true

    UnauthorizedErrorResponse:
      allOf:
        - $ref: '#/components/schemas/ErrorResponse'
        - type: object
          properties:
            authScheme:
              type: string
              nullable: true

    ForbiddenErrorResponse:
      allOf:
        - $ref: '#/components/schemas/ErrorResponse'
        - type: object
          properties:
            requiredPermission:
              type: string
              nullable: true
```

### Step 1.3: Create App API Spec

**packages/api-spec/openapi/app-api.yaml:**
```yaml
openapi: 3.1.0
info:
  title: Mystira App API
  version: 1.0.0
  description: Main Mystira application API

servers:
  - url: https://api.mystira.io/v1

paths:
  /health:
    get:
      operationId: getHealth
      summary: Health check endpoint
      responses:
        '200':
          description: Service is healthy
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/HealthCheckResult'

components:
  schemas:
    # Import common types
    ErrorResponse:
      $ref: './common/errors.yaml#/components/schemas/ErrorResponse'
    ValidationErrorResponse:
      $ref: './common/errors.yaml#/components/schemas/ValidationErrorResponse'

    # App-specific types
    HealthCheckResult:
      type: object
      required:
        - status
        - timestamp
      properties:
        status:
          type: string
          enum: [Healthy, Degraded, Unhealthy]
        timestamp:
          type: string
          format: date-time
        checks:
          type: array
          items:
            $ref: '#/components/schemas/HealthCheckEntry'

    HealthCheckEntry:
      type: object
      required:
        - name
        - status
      properties:
        name:
          type: string
        status:
          type: string
          enum: [Healthy, Degraded, Unhealthy]
        description:
          type: string
          nullable: true
        duration:
          type: string
          nullable: true
```

### Step 1.4: Create Generation Scripts

**packages/api-spec/scripts/generate-csharp.sh:**
```bash
#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$SCRIPT_DIR/../.."
OUTPUT_DIR="$ROOT_DIR/contracts/dotnet/Mystira.Contracts.Generated"

echo "Generating C# contracts from OpenAPI..."

# Using NSwag
npx nswag openapi2csclient \
  /input:"$SCRIPT_DIR/../openapi/app-api.yaml" \
  /output:"$OUTPUT_DIR/AppApiClient.cs" \
  /namespace:Mystira.Contracts.Generated \
  /generateClientInterfaces:true \
  /generateDtoTypes:true \
  /dateType:System.DateTimeOffset \
  /dateTimeType:System.DateTimeOffset \
  /arrayType:System.Collections.Generic.IReadOnlyList \
  /generateOptionalParameters:true

echo "C# contracts generated at $OUTPUT_DIR"
```

**packages/api-spec/scripts/generate-typescript.sh:**
```bash
#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$SCRIPT_DIR/../.."
OUTPUT_DIR="$ROOT_DIR/contracts/src/generated"

echo "Generating TypeScript contracts from OpenAPI..."

npx openapi-typescript "$SCRIPT_DIR/../openapi/app-api.yaml" \
  --output "$OUTPUT_DIR/app-api.ts"

npx openapi-typescript "$SCRIPT_DIR/../openapi/story-generator.yaml" \
  --output "$OUTPUT_DIR/story-generator.ts"

echo "TypeScript contracts generated at $OUTPUT_DIR"
```

### Step 1.5: Package Configuration

**packages/api-spec/package.json:**
```json
{
  "name": "@mystira/api-spec",
  "version": "1.0.0",
  "private": true,
  "scripts": {
    "generate": "npm run generate:csharp && npm run generate:typescript",
    "generate:csharp": "bash scripts/generate-csharp.sh",
    "generate:typescript": "bash scripts/generate-typescript.sh",
    "validate": "npx @redocly/cli lint openapi/*.yaml",
    "preview": "npx @redocly/cli preview-docs openapi/app-api.yaml"
  },
  "devDependencies": {
    "@redocly/cli": "^1.25.0",
    "nswag": "^14.0.0",
    "openapi-typescript": "^7.0.0"
  }
}
```

## Phase 2: Create Mystira.Core (Week 1-2)

### Step 2.1: Core Project (net9.0)

**packages/core/Mystira.Core/Mystira.Core.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

    <PackageId>Mystira.Core</PackageId>
    <Version>0.1.0-alpha</Version>
    <Authors>Mystira Team</Authors>
    <Description>Core internal types for Mystira platform - Result pattern, domain primitives</Description>
  </PropertyGroup>
</Project>
```

### Step 2.2: Result Pattern (Internal Only)

**packages/core/Mystira.Core/Results/Result.cs:**
```csharp
namespace Mystira.Core.Results;

/// <summary>
/// Represents the result of an operation that can fail.
/// Internal use only - not exposed via APIs.
/// </summary>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed Result");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result");

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
        => IsSuccess ? onSuccess(_value!) : onFailure(_error!);
}

/// <summary>
/// Represents an error in the Result pattern.
/// </summary>
public sealed record Error(string Code, string Message, Exception? Exception = null)
{
    public static Error NotFound(string resource, string? id = null)
        => new("NOT_FOUND", id is null ? $"{resource} not found" : $"{resource} '{id}' not found");

    public static Error Validation(string message)
        => new("VALIDATION", message);

    public static Error Unauthorized(string? reason = null)
        => new("UNAUTHORIZED", reason ?? "Authentication required");

    public static Error Forbidden(string? permission = null)
        => new("FORBIDDEN", permission is null ? "Access denied" : $"Missing permission: {permission}");

    public static Error Conflict(string message)
        => new("CONFLICT", message);

    public static Error Internal(string message, Exception? ex = null)
        => new("INTERNAL", message, ex);
}
```

### Step 2.3: TypeScript Core Types

**packages/core/core-types/src/results.ts:**
```typescript
export type Result<T, E = Error> =
  | { success: true; value: T }
  | { success: false; error: E };

export interface Error {
  code: string;
  message: string;
}

export const Result = {
  success: <T>(value: T): Result<T> => ({ success: true, value }),
  failure: <E = Error>(error: E): Result<never, E> => ({ success: false, error }),

  map: <T, U, E>(result: Result<T, E>, fn: (value: T) => U): Result<U, E> =>
    result.success ? Result.success(fn(result.value)) : result,

  flatMap: <T, U, E>(result: Result<T, E>, fn: (value: T) => Result<U, E>): Result<U, E> =>
    result.success ? fn(result.value) : result,
};
```

## Phase 3: Update Existing Projects to net9.0 (Week 2)

### Step 3.1: Update Mystira.Contracts

**packages/contracts/dotnet/Mystira.Contracts/Mystira.Contracts.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <!-- ... rest of config ... -->
  </PropertyGroup>

  <!-- Generated types from OpenAPI -->
  <ItemGroup>
    <Compile Include="..\Mystira.Contracts.Generated\**\*.cs" LinkBase="Generated" />
  </ItemGroup>
</Project>
```

### Step 3.2: Update Mystira.Shared

Already net9.0, just add reference to Core:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\core\Mystira.Core\Mystira.Core.csproj" />
</ItemGroup>
```

### Step 3.3: Remove Duplicate ErrorResponse

1. Delete `Mystira.Shared/Exceptions/ErrorResponse.cs` (use generated from OpenAPI)
2. Delete manual `Mystira.Contracts/App/Responses/Common/ErrorResponse.cs` (use generated)
3. Update `GlobalExceptionHandler` to use generated types

## Phase 4: CI/CD Integration (Week 3)

### Step 4.1: Add Generation to Build Pipeline

**.github/workflows/generate-contracts.yml:**
```yaml
name: Generate API Contracts

on:
  push:
    paths:
      - 'packages/api-spec/openapi/**'
  workflow_dispatch:

jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-node@v4
        with:
          node-version: '20'

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Install dependencies
        run: cd packages/api-spec && npm ci

      - name: Validate OpenAPI specs
        run: cd packages/api-spec && npm run validate

      - name: Generate contracts
        run: cd packages/api-spec && npm run generate

      - name: Commit generated files
        uses: stefanzweifel/git-auto-commit-action@v5
        with:
          commit_message: "chore: regenerate API contracts from OpenAPI"
          file_pattern: "packages/contracts/**"
```

## Dependency Graph (Final)

```
                     ┌──────────────────┐
                     │   api-spec/      │
                     │   (OpenAPI YAML) │
                     └────────┬─────────┘
                              │ generates
              ┌───────────────┴───────────────┐
              ▼                               ▼
   ┌─────────────────────┐        ┌─────────────────────┐
   │ Mystira.Contracts   │        │ @mystira/contracts  │
   │ (Generated C#)      │        │ (Generated TS)      │
   │ net9.0              │        │                     │
   └─────────────────────┘        └─────────────────────┘
              │                               │
              │ references                    │ imports
              ▼                               ▼
   ┌─────────────────────┐        ┌─────────────────────┐
   │   Mystira.Core      │        │ @mystira/core-types │
   │   (Internal types)  │        │ (Internal types)    │
   │   net9.0            │        │                     │
   └─────────────────────┘        └─────────────────────┘
              │
              │ references
              ▼
   ┌─────────────────────┐
   │   Mystira.Shared    │
   │   (Infrastructure)  │
   │   net9.0            │
   └─────────────────────┘
```

## Benefits of Hybrid Approach

| Benefit | Description |
|---------|-------------|
| **Single Source of Truth** | OpenAPI spec defines all API types once |
| **Auto-sync** | C# and TypeScript always match |
| **API Documentation** | OpenAPI gives free Swagger/Redoc docs |
| **Contract Testing** | Can validate against spec |
| **Internal Flexibility** | Mystira.Core can have C#-specific patterns (Result<T>) |
| **net9.0 Everywhere** | Consistent runtime, latest features |

## Tools Used

| Tool | Purpose |
|------|---------|
| **NSwag** | Generate C# clients/types from OpenAPI |
| **openapi-typescript** | Generate TypeScript types from OpenAPI |
| **Redocly CLI** | Validate and preview OpenAPI specs |

## Migration Checklist

- [ ] Create `packages/api-spec/` structure
- [ ] Write OpenAPI specs for common types
- [ ] Write OpenAPI specs for App API
- [ ] Write OpenAPI specs for Story Generator API
- [ ] Set up NSwag for C# generation
- [ ] Set up openapi-typescript for TS generation
- [ ] Create `packages/core/Mystira.Core/`
- [ ] Move Result<T> to Core
- [ ] Update Mystira.Contracts to net9.0
- [ ] Update Mystira.Shared to reference Core
- [ ] Remove duplicate ErrorResponse files
- [ ] Add CI/CD generation workflow
- [ ] Update consuming applications

## Questions for Team

1. Should we use NSwag or Kiota for C# generation?
2. Do we need runtime validation (e.g., Zod for TypeScript)?
3. Should OpenAPI specs be versioned separately?
