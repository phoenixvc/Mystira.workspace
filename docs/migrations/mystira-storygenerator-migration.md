# Mystira.StoryGenerator Migration Guide

**Target**: Migrate StoryGenerator to use `Mystira.Shared` infrastructure
**Prerequisites**: Mystira.Shared v0.2.0+ published to NuGet feed
**Estimated Effort**: 2 days
**Last Updated**: December 2025
**Status**: 🔄 In Progress

---

## Overview

StoryGenerator migration includes:

1. **.NET 9.0 upgrade** (required for Mystira.Shared)
2. MediatR → Wolverine migration
3. Custom `RetryPolicyService` → `Mystira.Shared.Resilience` (Polly v8)
4. In-memory stores → Redis caching
5. Contracts migration to unified package
6. **Ardalis.Specification 8.0.0** for data access
7. **Distributed locking** for LLM operations
8. **Dockerfile migration** to submodule repo (ADR-0019)

---

## Current State Analysis

### Projects in StoryGenerator

| Project | Purpose | Migration Impact |
|---------|---------|------------------|
| `Mystira.StoryGenerator.Api` | Web API | MediatR, .NET upgrade |
| `Mystira.StoryGenerator.Application` | Business logic | MediatR handlers |
| `Mystira.StoryGenerator.Domain` | Domain models | Minimal |
| `Mystira.StoryGenerator.Llm` | LLM integration | Resilience policies |
| `Mystira.StoryGenerator.RagIndexer` | RAG indexing | Custom retry → Polly |
| `Mystira.StoryGenerator.Contracts` | API contracts | → Mystira.Contracts |
| `Mystira.StoryGenerator.Web` | Blazor frontend | Design tokens |

### Current Dependencies (Api)

| Package | Version | Action |
|---------|---------|--------|
| `MediatR` | 12.1.1 | Replace with Wolverine |
| `Azure.AI.OpenAI` | 2.5.0-beta.1 | Keep |
| `Azure.Identity` | 1.17.0 | Keep |

### Custom Infrastructure to Replace

1. **RetryPolicyService** (`RagIndexer/Services/RetryPolicyService.cs`)
   - Manual exponential backoff
   - Replace with `Mystira.Shared.Resilience`

2. **In-Memory Stores** (`Api/Services/ContinuityAsync/`)
   - `InMemoryContinuityOperationStore` using `ConcurrentDictionary`
   - `ContinuityBackgroundQueue` using `Channel<T>`
   - Consider Redis for multi-instance deployments

---

## Phase 1: .NET 9.0 Upgrade

### 1.1 Update All Project Files

```xml
<!-- From -->
<TargetFramework>net8.0</TargetFramework>

<!-- To -->
<TargetFramework>net9.0</TargetFramework>
```

### 1.2 Update Package Versions

```xml
<!-- Update to .NET 9 compatible versions -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
```

### 1.3 Test Build

```bash
cd packages/story-generator
dotnet build
dotnet test
```

---

## Phase 2: Add Mystira.Shared

### 2.1 Update Mystira.StoryGenerator.Api.csproj

```xml
<!-- Remove -->
<PackageReference Include="MediatR" Version="12.1.1" />

<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.2.0" />
<PackageReference Include="Ardalis.Specification" Version="8.0.0" />
<PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="8.0.0" />
```

### 2.2 Update Mystira.StoryGenerator.Application.csproj

```xml
<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.2.0" />
<PackageReference Include="Ardalis.Specification" Version="8.0.0" />
```

### 2.3 Update Mystira.StoryGenerator.RagIndexer.csproj

```xml
<!-- Add for resilience -->
<PackageReference Include="Mystira.Shared" Version="0.2.0" />
```

### 2.4 Update Mystira.StoryGenerator.Domain.csproj

```xml
<!-- Add for specification pattern -->
<PackageReference Include="Ardalis.Specification" Version="8.0.0" />
```

---

## Phase 3: Wolverine Migration

### 3.1 Current MediatR Usage

```csharp
// Current: MediatR handler
public class GenerateStoryHandler : IRequestHandler<GenerateStoryCommand, StoryResult>
{
    public async Task<StoryResult> Handle(GenerateStoryCommand request, CancellationToken ct)
    {
        // generation logic
    }
}
```

### 3.2 Target Wolverine Handler

```csharp
// Target: Wolverine convention-based handler
public static class GenerateStoryHandler
{
    public static async Task<StoryResult> Handle(
        GenerateStoryCommand command,
        ILlmService llmService,
        ILogger<GenerateStoryHandler> logger,
        CancellationToken ct)
    {
        // generation logic - dependencies injected as parameters
    }
}
```

### 3.3 Update Program.cs

```csharp
// Remove
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GenerateStoryCommand).Assembly));

// Add
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(GenerateStoryCommand).Assembly);

    // Configure for long-running LLM operations
    opts.DefaultExecutionTimeout = TimeSpan.FromMinutes(5);
});
```

### 3.4 Update Command/Query Markers

```csharp
// From
public record GenerateStoryCommand(...) : IRequest<StoryResult>;

// To
using Mystira.Shared.Messaging;
public record GenerateStoryCommand(...) : ICommand<StoryResult>;
```

---

## Phase 4: Resilience Migration

### 4.1 Replace RetryPolicyService

```csharp
// Current: Custom retry (RagIndexer/Services/RetryPolicyService.cs)
public async Task<T> ExecuteWithRetryAsync<T>(
    Func<Task<T>> operation, string operationName,
    int maxRetries = 3, int delayMs = 1000)
{
    int attempt = 0;
    while (true) {
        attempt++;
        try { return await operation(); }
        catch (Exception ex) when (attempt < maxRetries) {
            await Task.Delay(delayMs * attempt);
        }
    }
}
```

```csharp
// Target: Use Polly via Mystira.Shared.Resilience
using Mystira.Shared.Resilience;

public class RagIndexingService
{
    private readonly ResiliencePipeline _pipeline;

    public RagIndexingService(PolicyFactory policyFactory)
    {
        _pipeline = policyFactory.CreateStandardPipeline("RagIndexing");
    }

    public async Task<T> IndexAsync<T>(Func<Task<T>> operation, CancellationToken ct)
    {
        return await _pipeline.ExecuteAsync(async token => await operation(), ct);
    }
}
```

### 4.2 HTTP Client Resilience (Polly v8)

```csharp
// Program.cs
builder.Services.AddMystiraResilience(builder.Configuration);

// For LLM HTTP clients with extended timeouts
builder.Services.AddResilientHttpClientV8<IOpenAIClient, OpenAIClient>(
    "OpenAI",
    client => client.BaseAddress = new Uri("https://api.openai.com"),
    options =>
    {
        options.TimeoutSeconds = 120; // LLM calls need longer timeout
        options.MaxRetries = 2;
        options.LongRunningTimeoutSeconds = 300; // For very long generations
    });

// Or use standard resilience handler
builder.Services.AddHttpClient<IOpenAIClient, OpenAIClient>()
    .AddStandardResilienceHandler()
    .Configure(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
    });
```

### 4.3 Breaking Changes (Polly v7 → v8)

| Before (v7) | After (v8) |
|-------------|------------|
| `IAsyncPolicy<T>` | `ResiliencePipeline<T>` |
| `PolicyFactory.CreateStandardPipeline()` | `ResiliencePipelineFactory.CreateStandardHttpPipeline()` |

---

## Phase 5: Caching (Optional)

### 5.1 Replace In-Memory Stores

```csharp
// Current: ConcurrentDictionary
private readonly ConcurrentDictionary<string, ContinuityOperation> _operations = new();

// Target: Redis-backed store
using Mystira.Shared.Caching;

public class RedisContinuityOperationStore : IContinuityOperationStore
{
    private readonly ICacheService _cache;

    public async Task<ContinuityOperation?> GetAsync(string id, CancellationToken ct)
    {
        return await _cache.GetAsync<ContinuityOperation>($"continuity:{id}", ct);
    }

    public async Task SetAsync(string id, ContinuityOperation operation, CancellationToken ct)
    {
        await _cache.SetAsync($"continuity:{id}", operation, TimeSpan.FromHours(1), ct);
    }
}
```

### 5.2 Registration

```csharp
// Program.cs
builder.Services.AddMystiraCaching(builder.Configuration);
builder.Services.AddScoped<IContinuityOperationStore, RedisContinuityOperationStore>();
```

---

## Phase 6: Contracts Migration

### 6.1 Replace Mystira.StoryGenerator.Contracts

The `Mystira.StoryGenerator.Contracts` project should be deprecated in favor of `Mystira.Contracts.StoryGenerator`:

```xml
<!-- From -->
<ProjectReference Include="..\Mystira.StoryGenerator.Contracts\..." />

<!-- To -->
<PackageReference Include="Mystira.Contracts" Version="0.1.0" />
```

### 6.2 Update Imports

```csharp
// From
using Mystira.StoryGenerator.Contracts;

// To
using Mystira.Contracts.StoryGenerator;
```

---

## Phase 7: Web (Blazor) Design Tokens

### 7.1 Update Tailwind Config (if applicable)

```javascript
// tailwind.config.js
const mystiraPreset = require('@mystira/design-tokens/tailwind/preset');

module.exports = {
  presets: [mystiraPreset],
  content: ['./Pages/**/*.razor', './Shared/**/*.razor'],
};
```

### 7.2 Or Import CSS Variables

```css
/* app.css */
@import '@mystira/design-tokens/css/variables.css';
@import '@mystira/design-tokens/css/dark-mode.css';
```

---

## Phase 8: Distributed Locking

### 8.1 Setup

```csharp
// Program.cs
builder.Services.AddMystiraDistributedLocking(builder.Configuration);
```

### 8.2 Usage for LLM Operations

```csharp
public class StoryGenerationService
{
    private readonly IDistributedLockService _lockService;

    public async Task<StoryResult> GenerateAsync(string sessionId, CancellationToken ct)
    {
        // Prevent duplicate generations for the same session
        return await _lockService.ExecuteWithLockAsync(
            $"story:generation:{sessionId}",
            async token =>
            {
                // Only one generation per session at a time
                return await DoGenerateAsync(sessionId, token);
            },
            expiry: TimeSpan.FromMinutes(10), // Long timeout for LLM
            wait: TimeSpan.FromSeconds(5),
            ct);
    }
}
```

---

## Phase 9: Ardalis.Specification 8.0.0

### 9.1 Create Specification Classes

```csharp
using Ardalis.Specification;

namespace Mystira.StoryGenerator.Domain.Specifications;

public sealed class StoryByIdSpec : Specification<Story>, ISingleResultSpecification<Story>
{
    public StoryByIdSpec(string storyId)
    {
        Query
            .Where(s => s.Id == storyId)
            .Include(s => s.Chapters)
            .AsNoTracking();
    }
}

public sealed class StoriesBySessionSpec : Specification<Story>
{
    public StoriesBySessionSpec(string sessionId)
    {
        Query
            .Where(s => s.SessionId == sessionId)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking();
    }
}
```

---

## Phase 10: Dockerfile Migration (ADR-0019)

Move Dockerfile from workspace to submodule repo:

### 10.1 Create Dockerfile in Submodule

```dockerfile
# packages/story-generator/Dockerfile (new location)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj", "src/Mystira.StoryGenerator.Api/"]
# ... other project references
RUN dotnet restore "src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj"
COPY . .
RUN dotnet build "src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Mystira.StoryGenerator.Api/Mystira.StoryGenerator.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mystira.StoryGenerator.Api.dll"]
```

### 10.2 Add CI/CD Workflow

```yaml
# .github/workflows/ci.yml (in Mystira.StoryGenerator repo)
name: StoryGenerator CI

on:
  push:
    branches: [main, dev]
  pull_request:

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - run: dotnet restore
      - run: dotnet build --configuration Release --no-restore
      - run: dotnet test --configuration Release --no-build

  docker:
    needs: build
    if: github.ref == 'refs/heads/dev'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@v3
        with:
          registry: myssharedacr.azurecr.io
          username: ${{ secrets.ACR_USERNAME }}
          password: ${{ secrets.ACR_PASSWORD }}
      - uses: docker/build-push-action@v5
        with:
          push: true
          tags: myssharedacr.azurecr.io/story-generator:${{ github.sha }}
      - name: Trigger workspace deployment
        uses: peter-evans/repository-dispatch@v2
        with:
          token: ${{ secrets.WORKSPACE_PAT }}
          repository: phoenixvc/Mystira.workspace
          event-type: story-generator-deploy
          client-payload: '{"sha": "${{ github.sha }}"}'
```

---

## Migration Checklist

### Pre-Migration
- [ ] Ensure Mystira.Shared v0.2.0+ is published
- [ ] Create feature branch
- [ ] Document current handler count
- [ ] Backup Key Vault secrets

### Phase 1: .NET 9.0 Upgrade
- [ ] Update all csproj to net9.0
- [ ] Update package versions
- [ ] Add Ardalis.Specification packages
- [ ] Verify build and tests pass

### Phase 2: Package Setup
- [ ] Add Mystira.Shared to Api, Application, RagIndexer, Domain
- [ ] Verify build succeeds

### Phase 3: Wolverine
- [ ] Add Wolverine to Program.cs
- [ ] Convert query handlers
- [ ] Convert command handlers
- [ ] Remove MediatR package

### Phase 4: Resilience (Polly v8)
- [ ] Delete RetryPolicyService.cs
- [ ] Add Mystira.Shared.Resilience
- [ ] Replace `IAsyncPolicy` with `ResiliencePipeline`
- [ ] Update HTTP clients with extended timeouts for LLM

### Phase 5: Caching
- [ ] Add Redis configuration
- [ ] Replace in-memory stores
- [ ] Test multi-instance scenarios

### Phase 6: Contracts
- [ ] Update to Mystira.Contracts
- [ ] Remove Mystira.StoryGenerator.Contracts project reference

### Phase 7: Web (Design Tokens)
- [ ] Add design tokens
- [ ] Add dark mode support
- [ ] Update color variables

### Phase 8: Distributed Locking
- [ ] Add distributed locking configuration
- [ ] Implement locks for LLM operations
- [ ] Test concurrent generation scenarios

### Phase 9: Specification Pattern
- [ ] Create specification classes for data access
- [ ] Update repositories to use Ardalis.Specification

### Phase 10: Dockerfile Migration
- [ ] Create Dockerfile in submodule repo
- [ ] Add CI/CD workflow
- [ ] Remove Dockerfile from workspace

### Post-Migration
- [ ] Run all unit tests
- [ ] Run integration tests
- [ ] Test API endpoints
- [ ] Load test LLM endpoints
- [ ] Verify distributed lock behavior
- [ ] Create PR

---

## Handler Conversion Reference

| Handler | Type | Status |
|---------|------|--------|
| `GenerateStoryHandler` | Command | ⬜ Pending |
| `GetStoryQueryHandler` | Query | ⬜ Pending |
| `GenerateContinuityHandler` | Command | ⬜ Pending |
| `IndexDocumentHandler` | Command | ⬜ Pending |
| ... | ... | ... |

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| .NET 8 → 9 | Runtime upgrade | Test thoroughly in staging |
| MediatR → Wolverine | Handler signatures | Gradual migration |
| Polly v7 → v8 | Policy API changes | Use new ResiliencePipeline API |
| In-memory → Redis | Requires Redis | Feature flag |
| Custom specs → Ardalis | Query patterns change | Gradual migration |

---

## Performance Considerations

1. **LLM Timeouts**: Configure longer timeouts for LLM operations (5+ minutes)
2. **Streaming**: Wolverine supports streaming responses
3. **Redis**: Improves multi-instance support for background operations
4. **Distributed Locking**: Use appropriate lock durations for LLM operations
5. **Polly v8**: Lower memory allocation than v7

---

## Related Documentation

- [ADR-0015: Wolverine Migration](../architecture/adr/0015-event-driven-architecture-framework.md)
- [ADR-0019: Dockerfile Location Standardization](../adr/ADR-0019-dockerfile-location-standardization.md)
- [Ardalis.Specification 8.0.0 Guide](../architecture/specifications/ardalis-specification-migration.md)
- [Mystira.App Migration Guide](./mystira-app-migration.md)
- [Mystira.Shared Migration Guide](../guides/mystira-shared-migration.md)
- [Mystira.Shared README](../../packages/shared/Mystira.Shared/README.md)
