# Mystira.StoryGenerator Migration Guide

**Target**: Migrate StoryGenerator to use `Mystira.Shared` infrastructure
**Prerequisites**: Mystira.Shared v0.1.0+ published to NuGet feed
**Estimated Effort**: 2 days

---

## Overview

StoryGenerator migration includes:

1. .NET 8.0 → .NET 9.0 upgrade (required for Mystira.Shared)
2. MediatR → Wolverine migration
3. Custom `RetryPolicyService` → `Mystira.Shared.Resilience`
4. In-memory stores → Redis caching (optional)
5. Contracts migration to unified package

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
<PackageReference Include="Mystira.Shared" Version="0.1.0" />
```

### 2.2 Update Mystira.StoryGenerator.Application.csproj

```xml
<!-- Add -->
<PackageReference Include="Mystira.Shared" Version="0.1.0" />
```

### 2.3 Update Mystira.StoryGenerator.RagIndexer.csproj

```xml
<!-- Add for resilience -->
<PackageReference Include="Mystira.Shared" Version="0.1.0" />
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

### 4.2 HTTP Client Resilience

```csharp
// Program.cs
builder.Services.AddMystiraResilience(builder.Configuration);

// For LLM HTTP clients
builder.Services.AddHttpClient<IOpenAIClient, OpenAIClient>()
    .AddMystiraResiliencePolicy("OpenAI", options =>
    {
        options.TimeoutSeconds = 120; // LLM calls need longer timeout
        options.RetryCount = 2;
    });
```

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
```

---

## Migration Checklist

### Pre-Migration
- [ ] Ensure Mystira.Shared is published
- [ ] Create feature branch
- [ ] Document current handler count

### Phase 1: .NET Upgrade
- [ ] Update all csproj to net9.0
- [ ] Update package versions
- [ ] Verify build and tests pass

### Phase 2: Package Setup
- [ ] Add Mystira.Shared to Api, Application, RagIndexer
- [ ] Verify build succeeds

### Phase 3: Wolverine
- [ ] Add Wolverine to Program.cs
- [ ] Convert query handlers
- [ ] Convert command handlers
- [ ] Remove MediatR package

### Phase 4: Resilience
- [ ] Delete RetryPolicyService.cs
- [ ] Add Mystira.Shared.Resilience
- [ ] Update HTTP clients with policies

### Phase 5: Caching (Optional)
- [ ] Add Redis configuration
- [ ] Replace in-memory stores
- [ ] Test multi-instance scenarios

### Phase 6: Contracts
- [ ] Update to Mystira.Contracts
- [ ] Remove Mystira.StoryGenerator.Contracts project reference

### Phase 7: Web
- [ ] Add design tokens
- [ ] Update color variables

### Post-Migration
- [ ] Run all tests
- [ ] Test API endpoints
- [ ] Load test LLM endpoints
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
| .NET 8 → 9 | Runtime upgrade | Test thoroughly |
| MediatR → Wolverine | Handler signatures | Gradual migration |
| In-memory → Redis | Requires Redis | Feature flag |

---

## Performance Considerations

1. **LLM Timeouts**: Configure longer timeouts for LLM operations
2. **Streaming**: Wolverine supports streaming responses
3. **Redis**: Improves multi-instance support for background operations

---

## Related Documentation

- [ADR-0015: Wolverine Migration](../architecture/adr/0015-event-driven-architecture-framework.md)
- [Mystira.App Migration Guide](./mystira-app-migration.md)
- [Mystira.Shared README](../../packages/shared/Mystira.Shared/README.md)
