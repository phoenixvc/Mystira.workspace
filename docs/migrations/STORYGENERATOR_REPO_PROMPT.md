# StoryGenerator Repository Migration Prompt

Use this prompt when working on the Mystira.StoryGenerator repository to complete the package consolidation migration.

---

## Context

The Mystira.workspace has consolidated several StoryGenerator packages into shared libraries:

- `Mystira.StoryGenerator.Contracts` → `Mystira.Contracts.StoryGenerator` (NuGet)
- `Mystira.StoryGenerator.GraphTheory` → `Mystira.Shared.GraphTheory` (NuGet)
- `Mystira.StoryGenerator.Domain` → `Mystira.Authoring.Abstractions` + `Mystira.Ai.Abstractions` (NuGet)
- `Mystira.StoryGenerator.Llm` → `Mystira.Ai` (NuGet)

---

## Prompt

```
I need to migrate the Mystira.StoryGenerator repository to use consolidated packages from the Mystira.workspace.

## Task Overview

1. **Update project references** in these projects to use NuGet packages instead of local project references:
   - Mystira.StoryGenerator.Api
   - Mystira.StoryGenerator.Application
   - Mystira.StoryGenerator.Console
   - Mystira.StoryGenerator.Llm.Console
   - Mystira.StoryGenerator.RagIndexer
   - Mystira.StoryGenerator.Web

2. **Update namespace imports** in all .cs files using these mappings:
   - `Mystira.StoryGenerator.Contracts.*` → `Mystira.Contracts.StoryGenerator.*`
   - `Mystira.StoryGenerator.Domain.Stories` → `Mystira.Authoring.Abstractions.Models.Scenario`
   - `Mystira.StoryGenerator.Domain.Services` → `Mystira.Authoring.Abstractions.Services`
   - `Mystira.StoryGenerator.Domain.Graph` → `Mystira.Authoring.Abstractions.Graph` or `Mystira.Shared.GraphTheory`
   - `Mystira.StoryGenerator.GraphTheory.*` → `Mystira.Shared.GraphTheory.*`
   - `Mystira.StoryGenerator.Llm.Services.LLM` → `Mystira.Ai.Providers`

3. **Delete these local packages** (they are now in workspace NuGet packages):
   - src/Mystira.StoryGenerator.Contracts/
   - src/Mystira.StoryGenerator.Domain/
   - src/Mystira.StoryGenerator.GraphTheory/
   - src/Mystira.StoryGenerator.Llm/

4. **Update solution file** to remove deleted projects

5. **Package versions to use**:
   - Mystira.Contracts: 0.5.0-alpha
   - Mystira.Shared: 0.5.0-alpha
   - Mystira.Ai: 0.1.0-alpha
   - Mystira.Authoring.Abstractions: 0.1.0-alpha
   - Mystira.Authoring: 0.1.0-alpha

## Important Notes

- Keep Mystira.StoryGenerator.Application for now - it contains business logic specific to StoryGenerator
- The LLM service implementations (AzureOpenAIService, AnthropicAIService) are now in Mystira.Ai
- Graph algorithms (BFS, DFS, topological sort, path finding) are now in Mystira.Shared.GraphTheory
- Scenario, Scene, Branch models are now in Mystira.Authoring.Abstractions.Models.Scenario

## Verification

After migration:
1. Run `dotnet restore`
2. Run `dotnet build`
3. Run `dotnet test`
4. Verify all APIs work correctly
```

---

## Package Reference Template

Add these to `.csproj` files:

```xml
<ItemGroup>
  <!-- Contracts for API DTOs -->
  <PackageReference Include="Mystira.Contracts" Version="0.5.0-alpha" />

  <!-- Shared infrastructure and GraphTheory -->
  <PackageReference Include="Mystira.Shared" Version="0.5.0-alpha" />

  <!-- AI/LLM services -->
  <PackageReference Include="Mystira.Ai" Version="0.1.0-alpha" />

  <!-- Authoring abstractions (interfaces, commands, models) -->
  <PackageReference Include="Mystira.Authoring.Abstractions" Version="0.1.0-alpha" />

  <!-- Authoring implementations (optional, if needed) -->
  <PackageReference Include="Mystira.Authoring" Version="0.1.0-alpha" />
</ItemGroup>
```

---

## NuGet Source Configuration

Ensure `NuGet.Config` has the GitHub Packages source:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/phoenixvc/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="phoenixvc" />
      <add key="ClearTextPassword" value="%GITHUB_TOKEN%" />
    </github>
  </packageSourceCredentials>
</configuration>
```
