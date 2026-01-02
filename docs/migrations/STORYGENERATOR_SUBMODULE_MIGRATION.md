# StoryGenerator Submodule Migration Instructions

This document provides instructions for migrating the StoryGenerator submodule to use the consolidated workspace packages.

## Prerequisites

The following packages have been migrated to the main workspace:

| Source Package | Target Package | Status |
|----------------|----------------|--------|
| `Mystira.StoryGenerator.Contracts` | `Mystira.Contracts.StoryGenerator` | ✅ Migrated |
| `Mystira.StoryGenerator.GraphTheory` | `Mystira.Shared.GraphTheory` | ✅ Migrated |
| `Mystira.StoryGenerator.Domain` (Graph interfaces) | `Mystira.Shared.GraphTheory` | ✅ Migrated |
| `Mystira.StoryGenerator.Domain` (Authoring types) | `Mystira.Authoring.Abstractions` | ✅ Migrated |
| `Mystira.StoryGenerator.Llm` (LLM services) | `Mystira.Ai` | ✅ Migrated |

---

## Phase 2: Update StoryGenerator Projects

### Step 1: Update Project References

Replace local project references with NuGet package references in the following files:

#### `Mystira.StoryGenerator.Api.csproj`

```xml
<!-- REMOVE these project references -->
<ProjectReference Include="..\Mystira.StoryGenerator.Contracts\..." />
<ProjectReference Include="..\Mystira.StoryGenerator.Domain\..." />
<ProjectReference Include="..\Mystira.StoryGenerator.GraphTheory\..." />
<ProjectReference Include="..\Mystira.StoryGenerator.Llm\..." />

<!-- ADD these package references -->
<PackageReference Include="Mystira.Contracts" Version="0.5.0-alpha" />
<PackageReference Include="Mystira.Shared" Version="0.5.0-alpha" />
<PackageReference Include="Mystira.Ai" Version="0.1.0-alpha" />
<PackageReference Include="Mystira.Authoring.Abstractions" Version="0.1.0-alpha" />
<PackageReference Include="Mystira.Authoring" Version="0.1.0-alpha" />
```

#### `Mystira.StoryGenerator.Application.csproj`

```xml
<!-- REMOVE -->
<ProjectReference Include="..\Mystira.StoryGenerator.Domain\..." />
<ProjectReference Include="..\Mystira.StoryGenerator.GraphTheory\..." />

<!-- ADD -->
<PackageReference Include="Mystira.Shared" Version="0.5.0-alpha" />
<PackageReference Include="Mystira.Authoring.Abstractions" Version="0.1.0-alpha" />
```

### Step 2: Update Namespace Imports

Apply these namespace changes across all `.cs` files:

| Old Namespace | New Namespace |
|---------------|---------------|
| `Mystira.StoryGenerator.Contracts.*` | `Mystira.Contracts.StoryGenerator.*` |
| `Mystira.StoryGenerator.Contracts.Chat` | `Mystira.Contracts.StoryGenerator.Chat` |
| `Mystira.StoryGenerator.Contracts.StoryConsistency` | `Mystira.Contracts.StoryGenerator.StoryConsistency` |
| `Mystira.StoryGenerator.Domain.Graph` | `Mystira.Authoring.Abstractions.Graph` or `Mystira.Shared.GraphTheory` |
| `Mystira.StoryGenerator.Domain.Stories` | `Mystira.Authoring.Abstractions.Models.Scenario` |
| `Mystira.StoryGenerator.Domain.Services` | `Mystira.Authoring.Abstractions.Services` |
| `Mystira.StoryGenerator.GraphTheory.Graph` | `Mystira.Shared.GraphTheory` |
| `Mystira.StoryGenerator.GraphTheory.Algorithms` | `Mystira.Shared.GraphTheory.Algorithms` |
| `Mystira.StoryGenerator.GraphTheory.DataFlowAnalysis` | `Mystira.Shared.GraphTheory.DataFlow` |
| `Mystira.StoryGenerator.GraphTheory.FrontierMergedGraph` | `Mystira.Shared.GraphTheory.StateSpace` |
| `Mystira.StoryGenerator.Llm.Services.LLM` | `Mystira.Ai.Providers` |

### Step 3: Update Type References

Some types have been renamed or moved:

| Old Type | New Type/Location |
|----------|-------------------|
| `IDirectedGraph<TNode, TEdge>` | `Mystira.Shared.GraphTheory.IDirectedGraph<TNode, TEdgeLabel>` |
| `DirectedGraph<TNode, TEdge>` | `Mystira.Shared.GraphTheory.DirectedGraph<TNode, TEdgeLabel>` |
| `SceneStateNode` | `Mystira.Shared.GraphTheory.StateSpace.StateNode<TState>` |
| `SceneTransition` | `Mystira.Shared.GraphTheory.StateSpace.StateTransition<TState, TLabel>` |
| `ILLMService` | `Mystira.Ai.Abstractions.ILLMService` |
| `ILlmServiceFactory` | `Mystira.Ai.Abstractions.ILlmServiceFactory` |
| `Scenario` | `Mystira.Authoring.Abstractions.Models.Scenario.Scenario` |
| `Scene` | `Mystira.Authoring.Abstractions.Models.Scenario.Scene` |

---

## Phase 3: Delete Duplicate Packages

After updating all references, delete these directories from the submodule:

```bash
# Delete from packages/story-generator/src/
rm -rf Mystira.StoryGenerator.Contracts
rm -rf Mystira.StoryGenerator.Domain
rm -rf Mystira.StoryGenerator.GraphTheory
rm -rf Mystira.StoryGenerator.Llm

# Keep these (they reference the new packages)
# Mystira.StoryGenerator.Api
# Mystira.StoryGenerator.Application
# Mystira.StoryGenerator.Console
# Mystira.StoryGenerator.Llm.Console
# Mystira.StoryGenerator.RagIndexer
# Mystira.StoryGenerator.Web
```

### Update Solution File

Remove deleted projects from `Mystira.StoryGenerator.sln`:

```bash
# Remove project references for deleted packages
dotnet sln remove src/Mystira.StoryGenerator.Contracts/Mystira.StoryGenerator.Contracts.csproj
dotnet sln remove src/Mystira.StoryGenerator.Domain/Mystira.StoryGenerator.Domain.csproj
dotnet sln remove src/Mystira.StoryGenerator.GraphTheory/Mystira.StoryGenerator.GraphTheory.csproj
dotnet sln remove src/Mystira.StoryGenerator.Llm/Mystira.StoryGenerator.Llm.csproj
```

---

## Quick Reference: Search & Replace Patterns

### Using sed (for bulk updates)

```bash
# Contracts namespace
find . -name "*.cs" -exec sed -i 's/Mystira\.StoryGenerator\.Contracts/Mystira.Contracts.StoryGenerator/g' {} \;

# Domain.Stories -> Authoring.Abstractions.Models.Scenario
find . -name "*.cs" -exec sed -i 's/Mystira\.StoryGenerator\.Domain\.Stories/Mystira.Authoring.Abstractions.Models.Scenario/g' {} \;

# Domain.Graph -> Authoring.Abstractions.Graph (for scenario-specific)
find . -name "*.cs" -exec sed -i 's/Mystira\.StoryGenerator\.Domain\.Graph/Mystira.Authoring.Abstractions.Graph/g' {} \;

# Domain.Services -> Authoring.Abstractions.Services
find . -name "*.cs" -exec sed -i 's/Mystira\.StoryGenerator\.Domain\.Services/Mystira.Authoring.Abstractions.Services/g' {} \;

# GraphTheory.Graph -> Shared.GraphTheory
find . -name "*.cs" -exec sed -i 's/Mystira\.StoryGenerator\.GraphTheory\.Graph/Mystira.Shared.GraphTheory/g' {} \;

# GraphTheory.Algorithms -> Shared.GraphTheory.Algorithms
find . -name "*.cs" -exec sed -i 's/Mystira\.StoryGenerator\.GraphTheory\.Algorithms/Mystira.Shared.GraphTheory.Algorithms/g' {} \;
```

---

## Verification Checklist

After migration, verify:

- [ ] All projects build successfully
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] No duplicate type definitions remain
- [ ] Solution file updated to remove deleted projects
- [ ] NuGet restore works with new package references

---

## Rollback Plan

If issues arise:

1. Revert package reference changes in `.csproj` files
2. Restore deleted project directories from git
3. Restore solution file from git
4. Run `dotnet restore`

---

## Related Documentation

- [StoryGenerator Migration Plan](./STORY_GENERATOR_MIGRATION_PLAN.md)
- [Mystira.Contracts Migration](../guides/contracts-migration.md)
- [Mystira.Shared Migration](../guides/mystira-shared-migration.md)
